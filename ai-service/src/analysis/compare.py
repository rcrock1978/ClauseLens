"""Side-by-side comparison via token-level diff (per US4)."""
from __future__ import annotations

from difflib import SequenceMatcher

from ..models.schemas import CompareResult, DiffSpan


async def compare_to_standard(clause_text: str, standard_language: str) -> CompareResult:
    sm = SequenceMatcher(a=clause_text, b=standard_language, autojunk=False)
    spans: list[DiffSpan] = []
    for tag, i1, i2, j1, j2 in sm.get_opcodes():
        if tag == "equal":
            spans.append(DiffSpan(start=i1, length=i2 - i1, operation="equal"))
        elif tag == "delete":
            spans.append(DiffSpan(start=i1, length=i2 - i1, operation="delete"))
        elif tag == "insert":
            spans.append(DiffSpan(start=i1, length=0, operation="insert"))
    return CompareResult(spans=spans)
