---
name: explain-flow
description: 텔레메트리 수신부터 Alarm 저장까지의 호출 경로를 파일:함수 순서로 설명한다. "/explain-flow" 로 직접 실행한다.
disable-model-invocation: true
---
# 텔레메트리 → 알람 호출 경로 설명

`POST /telemetry` 요청이 들어와 Alarm 이 저장될 때까지의 호출 경로를 `파일경로:함수명` 순서로 나열하고, 각 단계가 하는 일을 한 줄씩 설명한다.

## 읽을 파일 (이 4개만 읽는다 — 코드베이스 전체 검색 금지)
- `src/EquipMonitor/Program.cs`
- `src/EquipMonitor/Api/TelemetryEndpoints.cs`
- `src/EquipMonitor/Alarms/AlarmEngine.cs`
- `src/EquipMonitor/Store/InMemoryStore.cs`

## 규칙
- 코드는 수정하지 않는다.
- 위 파일에 없는 내용(예: DB 저장)은 추측하지 말고 "확인 안 됨"이라고 쓴다.
