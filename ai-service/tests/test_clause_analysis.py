"""Tests for the clause analyzer (per US2 + FR-009a)."""
from __future__ import annotations

import pytest

from clauselens.analysis.clause_analyzer import analyze_clause
from clauselens.models.schemas import PlaybookRuleSummary


@pytest.mark.asyncio
async def test_compliant_clause_routes_to_unreviewed() -> None:
    rules = [
        PlaybookRuleSummary(
            rule_id="r1", clause_type="indemnification",
            condition="unlimited liability",
            standard_language="Cap liability.",
            severity=3,
        )
    ]
    r = await analyze_clause(
        "Standard NDA — mutual confidentiality.", "indemnification", rules
    )
    assert r.is_compliant is False
    assert r.matched_rule_ids == []


@pytest.mark.asyncio
async def test_aggregates_multiple_matching_rules_per_fr_009a() -> None:
    rules = [
        PlaybookRuleSummary(rule_id="r1", clause_type="liability", condition="unlimited", standard_language="Cap", severity=2),
        PlaybookRuleSummary(rule_id="r2", clause_type="liability", condition="uncapped", standard_language="Cap", severity=3),
        PlaybookRuleSummary(rule_id="r3", clause_type="liability", condition="indemnify", standard_language="Cap", severity=1),
    ]
    text = "Provider shall have unlimited uncapped liability and indemnify the customer."
    r = await analyze_clause(text, "liability", rules)
    assert set(r.matched_rule_ids) == {"r1", "r2", "r3"}
    assert r.highest_severity == 3
