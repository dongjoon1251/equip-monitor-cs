# 온도 85°C 이상이 5분 지속되면 WARNING 알람

## 배경
현재 `temp-critical`(95°C 즉시)만 있어 서서히 과열되는 장비를 놓친다. 운영팀 요청: 85°C 이상이 **5분 이상 지속**되면 WARNING.

## 인수조건
- [ ] `temp >= 85.0` 상태가 300초 이상 지속된 시점의 텔레메트리에서 WARNING 알람 1건 발생 (룰 이름 `temp-sustained`)
- [ ] 4분 59초 지속은 발생하지 않음
- [ ] 지속 중에는 재알람하지 않음 (1건만)
- [ ] 85°C 미만으로 회복한 뒤 다시 5분 지속되면 **다시** 알람
- [ ] 판단 기준 시간은 텔레메트리 `ts` (벽시계 사용 금지)
- [ ] `Alarms/Rules.cs` 에 룰 추가 + `DefaultRules.All` 등록 + `tests/EquipMonitor.Tests/RulesTests.cs` 경계 테스트
- [ ] `dotnet run --project src/EquipMonitor.CheckRules -- data/samples/overheat.log` 출력에 09:07, 09:15 두 건이 보임

## 참고
- `data/samples/overheat.log` 가 재현 데이터
