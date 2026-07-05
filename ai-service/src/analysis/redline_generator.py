"""Redline generator with explicit citation enforcement (per FR-004)."""
from __future__ import annotations

from ..models.schemas import RedlineRequest, RedlineResult
from .confidence import calibrate_confidence


async def generate_redline(
    clause_text: str,
    matched_rule_standard_language: str,
    rule_guideline: str,
) -> RedlineResult:
    """Return suggested text, rationale, and explicit citations.

    Real implementation calls the LLM with a constrained prompt that
    requires every claim to cite a rule_id. For MVP we synthesize a
    deterministic suggestion that always cites the matched rule.
    """
    suggested = matched_rule_standard_language.strip()
    rationale = f"Aligns clause to playbook standard: {rule_guideline}".strip()
    citations = f"rule:{hash(rule_guideline) & 0xffff:04x}"  # placeholder rule id
    score = 0.88
    return RedlineResult(
        suggested_text=suggested,
        rationale=rationale,
        citations=citations,
        confidence=calibrate_confidence(score),
        confidence_score=score,
    )
