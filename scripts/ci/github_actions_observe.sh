#!/usr/bin/env bash

# Helpers for GitHub Actions logs, annotations, and job summaries.
# Keep workflow YAML readable: GitHub prints every `run:` block when a step is opened.

set -Eeuo pipefail

GHA_STAGE=""
GHA_AREA=""
GHA_OWNER=""
GHA_FAIL_DETAIL=""
GHA_META_FILE="${RUNNER_TEMP}/yuviron-deploy-summary-meta.md"
GHA_STAGE_FILE="${RUNNER_TEMP}/yuviron-deploy-summary-stages.md"

gha_init_summary() {
  {
    echo "| Field | Value |"
    echo "|---|---|"
    echo "| Environment | ${DEPLOY_ENV} |"
    echo "| ASP.NET environment | ${ASPNET_ENV} |"
    echo "| Branch | ${GITHUB_REF_NAME} |"
    echo "| Commit | \`${GITHUB_SHA}\` |"
    echo "| Run | [${GITHUB_RUN_ID}.${GITHUB_RUN_ATTEMPT}](${GITHUB_SERVER_URL}/${GITHUB_REPOSITORY}/actions/runs/${GITHUB_RUN_ID}) |"
  } > "$GHA_META_FILE"
  : > "$GHA_STAGE_FILE"

  echo "::notice title=Backend deploy::Starting ${DEPLOY_ENV} deploy for ${GITHUB_SHA}"
}

gha_begin_stage() {
  GHA_STAGE="$1"
  GHA_AREA="$2"
  GHA_OWNER="$3"
  GHA_FAIL_DETAIL="$4"

  trap 'gha_fail_stage "$?"' ERR
  echo "::group::${GHA_STAGE}"
}

gha_pass_stage() {
  local detail="$1"

  echo "::endgroup::"
  trap - ERR
  gha_record_stage "${GHA_STAGE}" "OK" "${GHA_AREA}" "${GHA_OWNER}" "${detail}"
}

gha_fail_stage() {
  local code="$1"

  echo "::endgroup::"
  echo "::error title=${GHA_STAGE} failed::${GHA_FAIL_DETAIL}"
  gha_record_stage "${GHA_STAGE}" "FAILED" "${GHA_AREA}" "${GHA_OWNER}" "${GHA_FAIL_DETAIL}"
  exit "$code"
}

gha_record_stage() {
  local stage="$1"
  local status="$2"
  local area="$3"
  local owner="$4"
  local detail="$5"

  echo "| ${stage} | ${status} | ${area} | ${owner} | ${detail} |" >> "$GHA_STAGE_FILE"
}

gha_render_summary() {
  {
    echo "## Backend deploy"
    echo
    cat "$GHA_META_FILE"
    echo
    echo "### Stages"
    echo
    echo "| Stage | Status | Area | Likely owner | Detail |"
    echo "|---|---|---|---|---|"
    if [ -s "$GHA_STAGE_FILE" ]; then
      cat "$GHA_STAGE_FILE"
    else
      echo "| Deploy | UNKNOWN | CI/GitHub | GitHub Actions | No stage data was recorded. Check the early setup logs. |"
    fi
    echo
    echo "_Job summary generated at run-time._"
  } >> "$GITHUB_STEP_SUMMARY"
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
