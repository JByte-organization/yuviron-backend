#!/usr/bin/env python3
"""Generate environment-specific appsettings JSON for Yuviron backend services.

WHY THIS FILE EXISTS
--------------------
appsettings.{env}.json files contain secrets and are gitignored — they are never
committed. This script is the single source of truth for their structure. It is
called by the CI workflow ("Generate appsettings" step in deploy.yml) immediately
after source sync, before docker compose build.

Previously this logic lived as an inline Python heredoc inside the workflow YAML,
which made it untestable, hard to review in PRs, and easy to break accidentally.

OUTPUT
------
  src/Yuviron.Api/appsettings.{ASPNET_ENV}.json        (API service)
  src/Yuviron.MediaWorker/appsettings.{ASPNET_ENV}.json (MediaWorker service)

Files are written atomically (write to .tmp → os.replace) with mode 0600
so secrets are never visible in a half-written state or world-readable.

────────────────────────────────────────────────────────────────────────────────
HOW TO MAKE COMMON CHANGES
────────────────────────────────────────────────────────────────────────────────

▸ Add a non-secret config value (e.g. a new limit, timeout, or feature flag)
    Edit _api_config() or _worker_config() directly. No other files need changing.

    Example — adding a new section to the API config:
        "RateLimits": {
            "MaxRequestsPerMinute": 100 if is_prod else 1000,
        },

▸ Make a value differ between dev and prod
    Use the `is_prod` bool that is already in scope inside _api_config():
        "FeatureFlags": {
            "EnableExperimentalSearch": not is_prod,
        },

▸ Add a new SECRET
    Three steps:
    1. Read it here with _env("MY_NEW_SECRET") and place it in the config dict.
    2. Add MY_NEW_SECRET to the `env:` block of the "Generate appsettings" step
       in .github/workflows/deploy.yml:
           MY_NEW_SECRET: ${{ secrets.MY_NEW_SECRET }}
    3. Create the GitHub Secret:
       Repository → Settings → Secrets and variables → Actions → New repository secret.
       Do this for every environment (dev + prod) if they use different secrets.

▸ Remove a secret
    1. Delete the _env("OLD_SECRET") call from the config dict here.
    2. Delete the corresponding line from the `env:` block in deploy.yml.
    The GitHub Secret itself can stay — unused secrets are harmless.

▸ Add config for a new service (e.g. a new microservice)
    1. Add a new _xyz_config() function below following the same pattern.
    2. Add a _write_atomic(...) call in main() pointing to the new service path.
    3. List any new required env vars in the REQUIRED ENV VARS section below.

────────────────────────────────────────────────────────────────────────────────
REQUIRED ENV VARS (injected by deploy.yml, never set manually on the server)
────────────────────────────────────────────────────────────────────────────────
  DEPLOY_ENV              "dev" or "prod"
  ASPNET_ENV              "Development" or "Production"
  JWT_SECRET
  EMAIL_USERNAME
  EMAIL_PASSWORD
  SEED_ADMIN_EMAIL        / SEED_ADMIN_PASSWORD
  SEED_MANAGER_EMAIL      / SEED_MANAGER_PASSWORD
  SEED_USER_EMAIL         / SEED_USER_PASSWORD
  SEED_PREMIUM_EMAIL      / SEED_PREMIUM_PASSWORD
  JAMENDO_CLIENT_ID
  STREAM_SECRET
  STRIPE_SECRET_KEY
  STRIPE_WEBHOOK_SECRET
"""
from __future__ import annotations

import json
import os
import stat
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent


def _env(name: str) -> str:
    """Read a required env var; exit with a clear error if it is missing or empty."""
    value = os.environ.get(name, "")
    if not value:
        raise SystemExit(f"ERROR: required env var '{name}' is not set or empty")
    return value


def _write_atomic(path: Path, data: dict) -> None:
    """Write a JSON file atomically (tmp → rename) with mode 0600."""
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, tmp = tempfile.mkstemp(dir=path.parent, prefix=f".{path.name}.")
    try:
        os.chmod(tmp, stat.S_IRUSR | stat.S_IWUSR)
        with os.fdopen(fd, "w", encoding="utf-8") as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
            f.write("\n")
        os.replace(tmp, path)
    except Exception:
        try:
            os.unlink(tmp)
        except OSError:
            pass
        raise
    print(f"  Written: {path.relative_to(ROOT)}")


def _api_config(is_prod: bool) -> dict:
    cors = [
        "https://yuviron.com",
        "https://backoffice.yuviron.com",
        "https://admin.yuviron.com",
    ]
    if not is_prod:
        cors += [
            "https://dev.yuviron.com",
            "https://dev-backoffice.yuviron.com",
            "https://dev-admin.yuviron.com",
            "http://localhost:3000",
        ]

    return {
        "CorsSettings": {"AllowedOrigins": cors},
        "JwtSettings": {
            "Secret": _env("JWT_SECRET"),
            "Issuer": "YuvironApi",
            "Audience": "YuvironClient",
            "ExpiryMinutes": 60,
        },
        "Email": {
            "Host": "smtp.gmail.com",
            "Port": 587,
            "Username": _env("EMAIL_USERNAME"),
            "Password": _env("EMAIL_PASSWORD"),
        },
        "SeedUsers": {
            "Admin":   {"Email": _env("SEED_ADMIN_EMAIL"),   "Password": _env("SEED_ADMIN_PASSWORD")},
            "Manager": {"Email": _env("SEED_MANAGER_EMAIL"), "Password": _env("SEED_MANAGER_PASSWORD")},
            "User":    {"Email": _env("SEED_USER_EMAIL"),    "Password": _env("SEED_USER_PASSWORD")},
            "Premium": {"Email": _env("SEED_PREMIUM_EMAIL"), "Password": _env("SEED_PREMIUM_PASSWORD")},
        },
        "JamendoApi": {"ClientId": _env("JAMENDO_CLIENT_ID")},
        "StreamSecurity": {"SecretKey": _env("STREAM_SECRET")},
        "ArtistLimits": {
            "FreeUserMaxProfiles": 1,
            "PremiumUserMaxProfiles": 5,
        },
        "AudioSettings": {
            "HlsQualities": [128, 320],
            "FfmpegAudioFilters": "-af loudnorm=I=-14:LRA=11:TP=-1.5",
        },
        "AdSettings": {
            "CooldownMinutes": 15 if is_prod else 2,
        },
        "FileAccess": {
            "PublicFolders": ["covers/", "avatars/", "banners/", "ads/"],
        },
        "Stripe": {
            "SecretKey": _env("STRIPE_SECRET_KEY"),
            "WebhookSecret": _env("STRIPE_WEBHOOK_SECRET"),
        },
    }


def _worker_config() -> dict:
    return {
        "JamendoApi": {"ClientId": _env("JAMENDO_CLIENT_ID")},
    }


def main() -> None:
    deploy_env = _env("DEPLOY_ENV")
    aspnet_env = _env("ASPNET_ENV")
    is_prod = deploy_env == "prod"

    _write_atomic(ROOT / "src" / "Yuviron.Api" / f"appsettings.{aspnet_env}.json", _api_config(is_prod))
    _write_atomic(ROOT / "src" / "Yuviron.MediaWorker" / f"appsettings.{aspnet_env}.json", _worker_config())


if __name__ == "__main__":
    main()
