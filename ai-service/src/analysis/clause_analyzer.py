"""Per-clause analysis: applies playbook rules, returns confidence-scored result.

Per FR-003 + FR-012: emits a confidence level and a numeric score; per FR-009a:
aggregates multiple matching rules into a single flag with highest severity and
all matching rule IDs.
"""
from __future__ import annotations

from ..models.schemas import ClauseAnalysisRequest, ClauseAnalysisResult, PlaybookRuleSummary, ConfidenceLevel
from .confidence import calibrate_confidence


async def analyze_clause(
    clause_text: str,
    clause_type: str,
    applicable_rules: list[PlaybookRuleSummary],
) -> ClauseAnalysisResult:
    # Heuristic: match if the rule's condition substring appears in the clause.
    matches: list[PlaybookRuleSummary] = []
    for rule in applicable_rules:
        if rule.clause_type.lower() != clause_type.lower():
            continue
        if rule.condition and rule.condition.lower() in clause_text.lower():
            matches.append(rule)

    if not matches:
        return ClauseAnalysisResult(
            is_compliant=False,
            matched_rule_id=None,
            matched_rule_ids=[],
            highest_severity=0,
            confidence="high",
            confidence_score=0.95,
            rationale="No playbook rule matched this clause; routing to Unreviewed.",
        )

    primary = max(matches, key=lambda r: r.severity)
    highest = primary.severity
    rationale = (
        f"Clause matches {len(matches)} rule(s); the highest-severity rule is "
        f"'{primary.condition}' (severity {primary.severity})."
    )
    score = 0.9 if highest >= 2 else 0.75 if highest == 1 else 0.6
    confidence: ConfidenceLevel = calibrate_confidence(score)

    return ClauseAnalysisResult(
        is_compliant=False,
        matched_rule_id=primary.rule_id,
        matched_rule_ids=[m.rule_id for m in matches],
        highest_severity=highest,
        confidence=confidence,
        confidence_score=score,
        rationale=rationale,
    )
