---
applyTo: "tests/**"
---
# 테스트 규약
- xUnit `[Fact]`/`[Theory]` 스타일. 파일명 `<모듈>Tests.cs`(예: `RulesTests.cs`), 메서드명은 동작을 서술하는 PascalCase(`ThresholdRule_FiresAtLimitNotBelow`).
- 시간은 `TestData.cs` 의 `T0`, `At(minutes)`, `Tele(...)` 로 만든다. `DateTime.Now` 금지.
- 경계값은 3종(미만 / 정확히 경계 / 초과)을 반드시 나눠 검증한다.
- `InMemoryStore` 는 싱글턴이 아니다. 단위 테스트는 `new InMemoryStore()` 로 격리한다. API 테스트는 `IClassFixture<WebApplicationFactory<Program>>` 을 쓰고 생성자에서 `InMemoryStore.Reset()` 을 호출한다.
- 네트워크·파일시스템 접근 금지. API 는 `WebApplicationFactory<Program>` 이 만든 `HttpClient` 로만 호출.
- 하나의 테스트는 하나의 동작만 검증한다.
