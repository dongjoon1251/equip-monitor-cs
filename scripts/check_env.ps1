# 수업 전 환경 점검. 전부 OK 면 exit 0. 사용: pwsh scripts/check_env.ps1  (Windows PowerShell 5.1 도 가능)
$results = @()
function Check($name, [bool]$ok, $hint) {
    $mark = if ($ok) { "OK  " } else { "FAIL" }
    Write-Host "$mark $name" -NoNewline; if (-not $ok) { Write-Host "  -> $hint" -NoNewline }; Write-Host ""
    $script:results += $ok
}
$sdks = & dotnet --list-sdks 2>$null
Check ".NET SDK 8+" ([bool]($sdks -match '^(8|9|1\d)\.')) ".NET 8 SDK 설치: https://dotnet.microsoft.com/download/dotnet/8.0"
Check "git on PATH" ([bool](Get-Command git -ErrorAction SilentlyContinue)) "Git 설치"
Check "gh on PATH"  ([bool](Get-Command gh  -ErrorAction SilentlyContinue)) "GitHub CLI 설치: https://cli.github.com"
$code = Get-Command code -ErrorAction SilentlyContinue
$codeHint = if ($env:OS -ne 'Windows_NT') { "VS Code 에서 Cmd+Shift+P -> 'Shell Command: Install ''code'' command in PATH'" } else { "VS Code 설치 후 터미널 재시작" }
Check "code on PATH" ([bool]$code) $codeHint
if ($code) {
    $ext = & code --list-extensions 2>$null
    Check "GitHub Copilot Chat 확장" ((@($ext) | ForEach-Object { "$_".ToLower() }) -contains "github.copilot-chat") "VS Code 확장 탭에서 'GitHub Copilot Chat' 설치"
}
if (Get-Command gh -ErrorAction SilentlyContinue) { & gh auth status 2>$null | Out-Null; Check "gh auth" ($LASTEXITCODE -eq 0) "gh auth login" }
& dotnet build (Join-Path (Split-Path $PSScriptRoot -Parent) "EquipMonitor.sln") -v q --nologo 2>$null | Out-Null
Check "dotnet build" ($LASTEXITCODE -eq 0) "dotnet build 오류를 확인하세요"
if ($results -contains $false) { exit 1 } else { Write-Host "모두 OK — 이 화면을 캡처해 제출하세요"; exit 0 }
