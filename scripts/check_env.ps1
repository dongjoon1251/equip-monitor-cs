# 수업 전 환경 점검. 전부 OK 면 exit 0. 사용: pwsh scripts/check_env.ps1  (Windows PowerShell 5.1 도 가능)
# Windows PowerShell 5.1: powershell -ExecutionPolicy Bypass -File scripts\check_env.ps1
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
$codeHint = if ($env:OS -ne 'Windows_NT') { "VS Code 에서 Cmd+Shift+P -> 'Shell Command: Install 'code' command in PATH'" } else { "VS Code 설치 후 터미널 재시작" }
Check "code on PATH" ([bool]$code) $codeHint
if ($code) {
    $ext = & code --list-extensions 2>$null
    # VS Code 1.11x 부터 Copilot Chat 은 VS Code 에 내장 -> 확장 목록에 안 나온다. 설치본의 내장 확장 폴더도 확인.
    $codePath = $code.Source; $item = Get-Item $codePath -ErrorAction SilentlyContinue
    if ($item -and $item.LinkType) { $codePath = if ([IO.Path]::IsPathRooted("$($item.Target)")) { "$($item.Target)" } else { Join-Path (Split-Path $codePath -Parent) "$($item.Target)" } }
    $codeDir = Split-Path $codePath -Parent
    $builtin = [bool](@((Join-Path (Join-Path (Join-Path $codeDir "..") "extensions") "copilot"),
                        (Join-Path (Join-Path (Join-Path (Join-Path (Join-Path $codeDir "..") "resources") "app") "extensions") "copilot")) | Where-Object { Test-Path $_ })
    $installed = (@($ext) | ForEach-Object { "$_".ToLower() }) -contains "github.copilot-chat"
    Check "GitHub Copilot Chat (확장 또는 VS Code 내장)" ($installed -or $builtin) "VS Code 를 최신으로 업데이트하거나, 확장 탭에서 'GitHub Copilot Chat' 설치"
}
# gh 로그인은 선택: 실습 이슈 생성(seed)·PR 생성에만 쓰고, 둘 다 GitHub 웹으로 대신할 수 있다 -> 실패해도 WARN.
if (Get-Command gh -ErrorAction SilentlyContinue) {
    & gh auth status 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { Write-Host "OK   gh auth" } else { Write-Host "WARN gh auth  -> (선택) gh auth login - 이슈 생성(seed)·PR 생성에 사용, 안 되면 GitHub 웹에서 직접 해도 됩니다" }
}
& dotnet build (Join-Path (Split-Path $PSScriptRoot -Parent) "EquipMonitor.sln") -v q --nologo 2>$null | Out-Null
Check "dotnet build" ($LASTEXITCODE -eq 0) "dotnet build 오류를 확인하세요"
if ($results -contains $false) { exit 1 } else { Write-Host "모두 OK — 이 화면을 캡처해 제출하세요"; exit 0 }
