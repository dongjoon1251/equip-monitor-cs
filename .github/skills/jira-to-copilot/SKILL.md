---
name: jira-to-copilot
description: Jira 작업의 담당자를 GitHub Copilot(GitHub Copilot for Jira)으로 바꿔 cloud agent 에게 구현을 맡긴다. "/jira-to-copilot <Jira 키>" 로 직접 실행하고, "/req-to-jira --copilot …" 의 마지막 단계에서도 쓴다.
disable-model-invocation: true
---
# Jira 작업 → GitHub Copilot 에게 맡기기

입력: Jira 키(예: `KAN-7`). 없으면 한 번 묻는다. 구현은 이 Skill 이 하지 않는다 — GitHub 의 Copilot cloud agent 가 한다.

## 절차
1. **작업 확인** — Atlassian MCP 로 작업을 읽는다. 아래가 모두 있어야 맡긴다. 하나라도 없으면 맡기지 말고 무엇이 빠졌는지 보고하고 끝낸다.
   - 설명에 Confluence 링크 · 인수조건 · `## Copilot 작업 지시` 절(저장소 `<owner>/<repo>`)
   - 상태가 Done 이 아님 · 담당자가 비어 있거나 사람이 아직 손대지 않음
2. **Copilot 계정 찾기** — Atlassian MCP 의 사용자 검색으로 `GitHub Copilot` 을 찾는다. 0 개거나 여러 개면 맡기지 말고 "GitHub Copilot for Jira 앱이 설치 · 연결되지 않았거나 이름이 다릅니다. Jira 화면의 Assignee 에서 직접 고르세요" 라고 안내하고 끝낸다.
3. **멈추고 승인 받기** — 표로 보여 주고 "이대로 Copilot 에게 맡길까요?" 라고 묻는다. 승인 전에는 쓰지 않는다.
   | Jira 키 · 제목 | 저장소 | 기준 브랜치 | 담당자 (지금 → 바꿀 값) |
   기준 브랜치는 `main` 이다. 맡기기 전에 `main` 에 CI/CD 워크플로가 있어야 PR 에서 Tests 가 돈다는 점을 한 줄로 덧붙인다.
4. **담당자 변경** — Atlassian MCP 로 담당자를 2번의 GitHub Copilot 계정으로 바꾼다(쓰기 1번).
5. **코멘트** — `Copilot 에게 맡김: 저장소 <owner>/<repo>, 기준 main. 진행 상황과 PR 은 GitHub 에서 확인.` (쓰기 1번)
6. **다시 읽어 확인** — 작업을 다시 읽어 담당자가 GitHub Copilot 인지, 최근 코멘트에 Copilot 앱의 시작 · 오류 메시지가 있는지 본다.
   - 담당자가 되돌아갔거나 오류 코멘트가 있으면: 그 내용을 그대로 보고하고 "Jira 화면에서 Assignee → GitHub Copilot 으로 직접 맡기세요" 라고 안내한다.
7. **보고** — 표 한 장: Jira 키 · 담당자 · 코멘트 결과 · 다음 확인 위치(GitHub **Pull requests** 의 `copilot/…` Draft PR, PR 제목에 Jira 키가 없으면 붙이기).

## 규칙
- 쓰기(담당자 변경 · 코멘트)는 한 번에 하나씩 부르고, 승인 카드에서 사람이 볼 수 있게 한다.
- 코드와 저장소 파일은 수정하지 않는다. PR 생성 · 리뷰 · 머지는 하지 않는다.
- Copilot 계정을 이름으로 추측하지 않는다 — 검색 결과가 정확히 하나일 때만 쓴다.
