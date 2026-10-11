---
name: pr-create
description: 현재 feat/<Jira 키> 브랜치의 커밋으로 PR 템플릿에 맞는 제목 · 설명 초안을 보여 주고, 사람이 승인하면 push 하고 PR 을 만든다. "/pr-create" 로 직접 실행한다.
disable-model-invocation: true
---
# PR 만들기 — 초안 → 승인 → push → PR

## 절차
1. **범위 확인** — `git status --porcelain` 이 비어 있는지 본다(커밋 안 된 변경이 있으면 멈추고 사람에게 알린다). `git log main..HEAD --oneline` 과 `git diff main...HEAD --stat` 으로 PR 에 들어갈 커밋 · 파일을 확인한다.
2. **Jira 키** — 브랜치 이름 `feat/<Jira 키>` 에서 키를 찾는다(없으면 커밋 메시지에서, 그래도 없으면 한 번 묻는다). Atlassian MCP 로 그 작업의 제목과 Confluence 링크를 읽는다.
3. **초안** — `.github/pull_request_template.md` 의 칸을 그대로 채운다.
   - 제목: `<Jira 키> <Conventional Commits 한 줄>` (예: `KAN-5 feat: 알람 웹훅 알림`)
   - 이슈: 커밋 메시지에 나온 GitHub 이슈마다 `Closes #N` 한 줄씩
   - Jira: `<키> — <Jira URL>`, Confluence 링크
   - 변경 요약: "무엇을 · 왜" 3줄 이내, 파일 나열 금지
   - 테스트 방법: 실제로 실행한 명령과 결과만. 실행하지 않은 것은 "미실행"
4. **멈추고 승인 받기** — 제목과 설명을 채팅에 보여 주고 "이대로 push 하고 PR 을 만들까요?" 라고 묻는다. 고칠 점을 말하면 반영해 다시 보여 준다. 승인 전에는 push 하지 않는다.
5. **push** — `git push -u origin <현재 브랜치>`.
6. **PR 생성** — GitHub MCP 의 PR 생성 도구로 base `main`, head 현재 브랜치, 3번의 제목 · 설명으로 만든다. GitHub MCP 에 PR 생성 도구가 없으면 `gh pr create --base main --title "<제목>" --body-file <임시 파일>` 로 대신한다.
7. **Jira 기록** — Atlassian MCP 로 그 Jira 작업에 `PR 열림: <PR 링크>` 코멘트를 남긴다(상태는 그대로 In Progress).
8. **보고** — PR 번호 · 링크와 Jira 코멘트 결과. push 할 때 로컬 CI(pre-push hook)가 실패하면 PR 을 만들지 말고 실패 내용을 보고한다.

## 규칙
- 코드는 수정하지 않는다. 커밋 메시지 · diff 에 없는 내용을 지어내지 않는다.
- 머지 · 리뷰 요청 · 강제 push 는 하지 않는다(머지는 hook 이 막는다).
- push 와 PR 생성은 각각 한 번씩 불러 승인 카드에서 사람이 볼 수 있게 한다.
