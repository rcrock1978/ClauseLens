"""Obligation extractor (per US5)."""
from __future__ import annotations

from ..models.schemas import ObligationItem, ObligationsResult
from ..models.obligation import ExtractedObligation


async def extract_obligations(clause_text: str) -> ObligationsResult:
    """Heuristic: a sentence containing 'shall' or 'must' is treated as an
    obligation. The first capitalized noun phrase is the responsible party.
    A real implementation would use the LLM with structured output.
    """
    obligations: list[ObligationItem] = []
    for sentence in _split_sentences(clause_text):
        lowered = sentence.lower()
        if "shall" in lowered or "must" in lowered:
            party = _extract_party(sentence)
            obligations.append(
                ObligationItem(
                    description=sentence.strip(),
                    responsible_party=party,
                    due_date=None,
                    trigger_condition=None,
                )
            )
    return ObligationsResult(obligations=obligations)


def _split_sentences(text: str) -> list[str]:
    parts = re.split(r"(?<=[.!?])\s+", text)
    return [p for p in parts if p.strip()]


_PARTY_RE = re.compile(r"\b([A-Z][a-zA-Z]+(?:\s+[A-Z][a-zA-Z]+)?)\s+shall\b")


def _extract_party(sentence: str) -> str:
    m = _PARTY_RE.search(sentence)
    return m.group(1) if m else "Unspecified"
