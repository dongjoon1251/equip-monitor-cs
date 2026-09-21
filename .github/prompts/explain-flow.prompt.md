---
mode: agent
description: 텔레메트리 수신부터 Alarm 저장까지의 호출 경로를 파일:함수 순서로 설명
---
`POST /telemetry` 요청이 들어와 Alarm 이 저장될 때까지의 호출 경로를 `파일경로:함수명` 순서로 나열하고, 각 단계가 하는 일을 한 줄씩 설명해줘. 코드는 수정하지 마.

#file:src/EquipMonitor/Program.cs #file:src/EquipMonitor/Api/TelemetryEndpoints.cs #file:src/EquipMonitor/Alarms/AlarmEngine.cs #file:src/EquipMonitor/Store/InMemoryStore.cs
