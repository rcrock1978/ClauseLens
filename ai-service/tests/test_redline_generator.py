"""Tests for the redline generator (per FR-004)."""
from __future__ import annotations

import pytest

from clauselens.analysis.redline_generator import generate_redline


@pytest.mark.asyncio
async def test_redline_returns_text_and_rationale_and_citations() -> None:
    r = await generate_redline(
        clause_text="Provider shall have unlimited liability.",
        matched_rule_standard_language="Liability shall be capped at fees paid in 12 months.",
        rule_guideline="Cap liability to contract value",
    )
    assert r.suggested_text
    assert r.rationale
    assert r.citations
    assert r.confidence in ("high", "medium", "low")
    assert 0.0 <= r.confidence_score <= 1.0
