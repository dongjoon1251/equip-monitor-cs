#!/usr/bin/env bash
# Actions 에서 Jira 를 갱신하는 도우미. 시크릿 JIRA_BASE_URL · JIRA_EMAIL · JIRA_API_TOKEN 이 없으면 아무것도 하지 않는다.
#   keys "<텍스트>"            텍스트에서 실제로 있는 Jira 키만 뽑는다
#   comment KEY "<내용>"       코멘트 추가
#   move KEY "<상태 이름>"     그 상태로 가는 전환 실행 (없으면 경고만)
#   label KEY add|remove NAME  라벨 추가 · 삭제
#   search "<JQL>"             JQL 결과의 키 목록
set -euo pipefail

if [ -z "${JIRA_BASE_URL:-}" ] || [ -z "${JIRA_EMAIL:-}" ] || [ -z "${JIRA_API_TOKEN:-}" ]; then
  echo "::notice::Jira 시크릿(JIRA_BASE_URL · JIRA_EMAIL · JIRA_API_TOKEN)이 없어 Jira 갱신을 건너뜀" >&2
  exit 0
fi
BASE=${JIRA_BASE_URL%/}
api() { curl -fsS -u "$JIRA_EMAIL:$JIRA_API_TOKEN" -H 'Content-Type: application/json' "$@"; }

cmd=$1; shift
case "$cmd" in
  keys)
    # UTF-8 같은 글자도 키 모양이라, 실제로 조회되는 키만 남긴다
    for k in $(grep -oE '\b[A-Z][A-Z0-9]+-[0-9]+\b' <<<"$*" | sort -u); do
      api "$BASE/rest/api/2/issue/$k?fields=key" >/dev/null 2>&1 && echo "$k"
    done ;;
  comment)
    jq -n --arg b "$2" '{body: $b}' | api -X POST "$BASE/rest/api/2/issue/$1/comment" -d @- >/dev/null ;;
  move)
    id=$(api "$BASE/rest/api/2/issue/$1/transitions" \
      | jq -r --arg t "$2" '[.transitions[] | select(.to.name == $t or .name == $t) | .id][0] // empty')
    if [ -n "$id" ]; then
      jq -n --arg id "$id" '{transition: {id: $id}}' | api -X POST "$BASE/rest/api/2/issue/$1/transitions" -d @- >/dev/null
    else
      echo "::warning::$1: '$2' 로 가는 전환이 없어 상태는 그대로 둠"
    fi ;;
  label)
    jq -n --arg op "$2" --arg n "$3" '{update: {labels: [{($op): $n}]}}' | api -X PUT "$BASE/rest/api/2/issue/$1" -d @- >/dev/null ;;
  search)
    api -G "$BASE/rest/api/3/search/jql" --data-urlencode "jql=$1" --data-urlencode "fields=key" | jq -r '.issues[].key' ;;
  *)
    echo "알 수 없는 명령: $cmd" >&2; exit 2 ;;
esac
