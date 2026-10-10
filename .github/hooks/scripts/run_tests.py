"""postToolUse hook: 파일을 고친 직후 전체 테스트를 돌려 결과를 모델에게 돌려준다."""
import json
import os
import subprocess
import sys
from pathlib import Path

LOG = Path(".github/hooks/hook.log")
env = {**os.environ, "TZ": "UTC0"}  # CI 와 같은 UTC 에서 검증 (mac/Linux 에서만 적용, Windows .NET 은 TZ 를 무시)
try:
    r = subprocess.run(["dotnet", "test", "--no-restore", "--nologo", "-v", "q"], capture_output=True, text=True, env=env, timeout=55)
except FileNotFoundError:  # dotnet 이 없는 환경(예: cloud agent)에서는 조용히 넘어감
    sys.exit(0)
lines = [l.strip() for l in r.stdout.splitlines() if l.strip()]
last = next((l for l in reversed(lines) if any(k in l for k in ("Passed!", "Failed!", "통과!", "실패!"))), (lines or ["(no output)"])[-1])
with LOG.open("a", encoding="utf-8") as f:
    f.write(f"postToolUse dotnet test: {last}\n")
if r.returncode != 0:
    fails = [l for l in lines if l.startswith("Failed ") or l.startswith("실패 ")][:5]
    msg = ("[Harness] 방금 변경 후 dotnet test 가 실패한다: " + last + "\n" + "\n".join(fails)
           + "\n이번 작업과 관계없는 실패면 고치지 말고 보고만 하라.")
else:
    msg = "[Harness] 방금 변경 후 dotnet test 통과: " + last
print(json.dumps({"additionalContext": msg}, ensure_ascii=False))
