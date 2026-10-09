# GitHub 라벨·실습 이슈 생성 (gh CLI 필요, 한 번만 실행, repo 루트에서 실행). Windows PowerShell 5.1 기준(pwsh 불필요).
# 사용: powershell -ExecutionPolicy Bypass -File scripts\seed.ps1 [-WithInjection] [-InjectionFile <path>]   (mac/Linux: pwsh scripts/seed.ps1)
param([switch]$WithInjection, [string]$InjectionFile = "docs/issues/04-log-format-question.md")
$root = Join-Path $PSScriptRoot ".."
$labels = @{ feature = "0E8A16"; bug = "D73A4A"; practice = "1D76DB"; demo = "5319E7" }
$plan = @(
    @{ file = "docs/issues/01-sustained-alarm.md";  labels = "feature,practice" },
    @{ file = "docs/issues/02-uptime-report.md";    labels = "feature,practice" },
    @{ file = "docs/issues/03-negative-values.md";  labels = "bug,practice" })
if ($WithInjection) {
    if (-not (Test-Path $InjectionFile)) {
        [Console]::Error.WriteLine("인젝션 데모 이슈 파일이 없습니다: $InjectionFile")
        [Console]::Error.WriteLine("강사용 solution repo 의 docs/issues/04-log-format-question.md 경로를 -InjectionFile 로 지정하세요")
        exit 2
    }
    $InjectionFile = (Resolve-Path $InjectionFile).Path
    $plan += @{ file = $InjectionFile; labels = "demo" }
}
function Gh { & gh @args; if ($LASTEXITCODE -ne 0) { [Console]::Error.WriteLine("gh 명령 실패: gh $args"); exit 1 } }
foreach ($l in $labels.GetEnumerator()) { Gh label create $l.Key --color $l.Value --force | Out-Null }
foreach ($p in $plan) {
    $path = if ([IO.Path]::IsPathRooted($p.file)) { $p.file } else { Join-Path $root $p.file }
    $title = (Get-Content $path -TotalCount 1 -Encoding UTF8) -replace '^#\s*', ''
    Gh issue create --title $title --body-file $path --label $p.labels
}
