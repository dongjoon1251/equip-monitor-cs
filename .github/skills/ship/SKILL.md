---
name: ship
description: 머지된 Jira 작업을 로컬 CI 로 확인하고, 릴리스 파일을 만들어 GitHub Release 로 올린 뒤 Jira 를 Done 으로 닫는다. "/ship <Jira 키> <버전 태그>" 로 직접 실행한다.
disable-model-invocation: true
---
# 배포 — 머지 확인 → 로컬 CI → 패키징 → Release → Jira Done

입력: Jira 키와 버전 태그(예: `/ship KAN-5 v0.1.0`). 없으면 한 번 묻는다. 이 저장소는 GitHub Actions 를 쓰지 않으므로 CI · CD 를 모두 이 컴퓨터에서 한다.

## 절차
1. **머지 확인** — GitHub MCP 로 제목에 그 Jira 키가 들어간 PR 이 `main` 에 머지됐는지 본다. 머지 전이면 멈추고 "PR 을 먼저 사람이 머지하세요" 라고 알린다.
2. **main 최신화** — `git status --porcelain` 이 비어 있는지 확인하고 `git switch main` → `git pull`.
3. **로컬 CI** — `bash scripts/ci.sh` (Windows PowerShell: `scripts/ci.ps1`). 하나라도 실패하면 배포하지 않고 멈춘다. Jira 에는 `배포 보류: 로컬 CI 실패 (<요약>)` 코멘트만 남긴다.
4. **패키징** — `bash scripts/package.sh <태그>` 로 릴리스 아카이브와 `SHA256SUMS` 를 만든다. 스크립트가 없으면 멈추고 실습 7-2 를 먼저 하라고 안내한다.
5. **승인 받기** — 태그 이름, 올릴 파일 목록, 대상 커밋을 표로 보여 주고 "이대로 태그를 만들고 Release 를 올릴까요?" 라고 묻는다. 승인 전에는 태그 · Release 를 만들지 않는다.
6. **Release** — `git tag <태그>` → `git push origin <태그>` → `gh release create <태그> <아카이브> SHA256SUMS --title <태그> --generate-notes`.
7. **(선택) 컨테이너** — `Dockerfile` 이 있고 `docker` 가 있으면 `docker build -t equip-monitor:<태그> .` 후 컨테이너를 띄워 `/health` 를 확인하고 정리한다. 없으면 건너뛰었다고만 적는다.
8. **Jira Done** — Atlassian MCP 로 `배포 완료: Release <태그> (<Release 링크>)` 코멘트를 남기고 상태를 `Done` 으로 바꾼다.
9. **보고** — 표 한 장: 머지된 PR · 로컬 CI 결과 · Release 링크 · 컨테이너 확인 · Jira 상태.

## 규칙
- 터미널 명령 · Jira 쓰기는 하나씩 실행해 승인 카드에서 사람이 볼 수 있게 한다.
- 강제 push, 기존 태그 덮어쓰기, PR 머지는 하지 않는다(머지는 사람, hook 이 막는다).
- 어느 단계든 실패하면 Jira 를 Done 으로 바꾸지 않는다.
