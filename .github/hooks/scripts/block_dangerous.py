"""preToolUse hook: 되돌릴 수 없는 터미널 명령(강제 push, reset --hard, 재귀 삭제)을 막는다."""
import json
import re
import sys
from pathlib import Path

LOG = Path(".github/hooks/hook.log")
TERMINAL = ("bash", "powershell", "shell", "terminal")  # 터미널 명령 도구만 검사 (파일 수정·질문 도구는 통과)
DANGEROUS = [
    (r"git\s+push\b.*(--force|\s-f\b)", "강제 push"),
    (r"git\s+reset\b.*--hard", "git reset --hard"),
    (r"git\s+clean\s+-\w*f", "git clean -f"),
    (r"\brm\s+-\w*(rf|fr)", "rm -rf"),
    (r"Remove-Item\b.*-Recurse", "Remove-Item -Recurse"),
]


def strings_in(value):
    """도구 인자 어디에 있든 문자열을 모은다."""
    if isinstance(value, str):
        try:
            value = json.loads(value)
        except ValueError:
            return [value]
    if isinstance(value, dict):
        return [s for v in value.values() for s in strings_in(v)]
    if isinstance(value, list):
        return [s for v in value for s in strings_in(v)]
    return []


event = json.load(sys.stdin)
tool = event.get("toolName") or event.get("tool_name") or ""
args = event.get("toolArgs", event.get("tool_input"))
text = "\n".join(strings_in(args)) if any(t in tool.lower() for t in TERMINAL) else ""
hit = next((name for pat, name in DANGEROUS if re.search(pat, text, re.IGNORECASE)), None)
with LOG.open("a", encoding="utf-8") as f:
    f.write(f"preToolUse {tool} deny={bool(hit)} {hit or ''}\n")
if hit:
    print(json.dumps({
        "permissionDecision": "deny",
        "permissionDecisionReason": f"{hit} 는 되돌릴 수 없는 명령이라 Harness 가 막았다. 실행하지 말고 사람에게 확인을 요청하라.",
    }, ensure_ascii=False))
