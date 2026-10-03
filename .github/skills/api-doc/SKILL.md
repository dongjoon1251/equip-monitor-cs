---
name: api-doc
description: 지정한 Minimal API 엔드포인트 파일의 엔드포인트를 표로 문서화한다. "/api-doc <라우터 파일>" 로 직접 실행한다.
argument-hint: "[src/EquipMonitor/Api/<파일>Endpoints.cs]"
disable-model-invocation: true
---
# API 문서 만들기

인자로 받은 라우터 파일과 `src/EquipMonitor/Models/Models.cs` **두 파일만** 읽는다.

엔드포인트마다 아래 표 한 줄을 만든다.

| 메서드 · 경로 | 요청(쿼리·바디) | 응답 모델 | 오류 코드 | 예시 `curl` |
|---|---|---|---|---|

## 규칙
- 오류 코드는 코드에 있는 `Results.NotFound` · `Results.UnprocessableEntity` 등만 적는다(추측 금지).
- 시각 필드는 UTC `Z` 형식 예시를 쓴다.
- `curl` 예시 값은 `data/samples/` 의 실제 값(예: 장비 ID `DEV-01`)을 쓴다.
- 코드는 수정하지 않는다.
