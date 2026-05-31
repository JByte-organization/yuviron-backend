#!/usr/bin/env bash

# Helpers for GitHub Actions logs, annotations, and job summaries.
# Keep workflow YAML readable: GitHub prints every `run:` block when a step is opened.

set -Eeuo pipefail

GHA_STAGE=""
GHA_OWNER=""
GHA_FAIL_DETAIL=""

gha_init_summary() {
  {
    echo "## Backend deploy"
    echo
    echo "| Field | Value |"
    echo "|---|---|"
    echo "| Environment | ${DEPLOY_ENV} |"
    echo "| ASP.NET environment | ${ASPNET_ENV} |"
    echo "| Branch | ${GITHUB_REF_NAME} |"
    echo "| Commit | \`${GITHUB_SHA}\` |"
    echo "| Run | [${GITHUB_RUN_ID}.${GITHUB_RUN_ATTEMPT}](${GITHUB_SERVER_URL}/${GITHUB_REPOSITORY}/actions/runs/${GITHUB_RUN_ID}) |"
    echo
    echo "### Stages"
    echo
    echo "| Stage | Status | Likely owner | Detail |"
    echo "|---|---|---|---|"
  } >> "$GITHUB_STEP_SUMMARY"

  echo "::notice title=Backend deploy::Starting ${DEPLOY_ENV} deploy for ${GITHUB_SHA}"
}

gha_begin_stage() {
  GHA_STAGE="$1"
  GHA_OWNER="$2"
  GHA_FAIL_DETAIL="$3"

  trap 'gha_fail_stage "$?"' ERR
  echo "::group::${GHA_STAGE}"
}

gha_pass_stage() {
  local detail="$1"

  echo "::endgroup::"
  trap - ERR
  echo "| ${GHA_STAGE} | OK | ${GHA_OWNER} | ${detail} |" >> "$GITHUB_STEP_SUMMARY"
}

gha_fail_stage() {
  local code="$1"

  echo "::endgroup::"
  echo "::error title=${GHA_STAGE} failed::${GHA_FAIL_DETAIL}"
  echo "| ${GHA_STAGE} | FAILED | ${GHA_OWNER} | ${GHA_FAIL_DETAIL} |" >> "$GITHUB_STEP_SUMMARY"
  exit "$code"
}

gha_begin_group() {
  echo "::group::$1"
}

gha_end_group() {
  echo "::endgroup::"
}

gha_warn() {
  local title="$1"
  local message="$2"

  echo "::warning title=${title}::${message}"
}
