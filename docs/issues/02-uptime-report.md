# 장비별 가동률 리포트 API `GET /report/uptime`

## 배경
`Report/Uptime.cs` 에 계산 로직은 있으나 API 로 노출되지 않았다. 데이터 분석팀이 일 단위 가동률을 조회하려 한다.

## 인수조건
- [ ] `GET /report/uptime?from=YYYY-MM-DD&to=YYYY-MM-DD[&device_id=]` — 양 끝 날짜 포함
- [ ] 응답: 장비별 `{device_id, from, to, durations_s: {RUN, IDLE, DOWN, MAINT}, uptime_ratio}` 배열
- [ ] `device_id` 지정 시 해당 장비만, 없는 장비면 404
- [ ] 날짜 구간은 `Report.Uptime.DayRange` 를 사용
- [ ] `tests/EquipMonitor.Tests/ApiTests.cs` 에 리포트 테스트 추가 (고정 타임스탬프)

## 참고
- 라우터는 `src/EquipMonitor/Api/ReportEndpoints.cs` 로 분리하고 `Program.cs` 에 등록
