# 파서가 음수 값을 읽지 못함

## 재현
    dotnet run --project src/EquipMonitor.CheckRules -- data/samples/negative-values.log
    → FormatException: bad value temp='-3.5' ...

냉동 챔버(DEV-03)는 영하 온도를 보고한다. `POST /ingest` 도 같은 라인에서 422 를 돌려준다.

## 인수조건
- [ ] `temp=-3.5`, `temp=-4` 를 숫자로 파싱
- [ ] `tests/EquipMonitor.Tests/ParserTests.cs` 에 음수 케이스 추가
- [ ] 위 명령이 `-- 0 alarm(s)` 로 정상 종료
