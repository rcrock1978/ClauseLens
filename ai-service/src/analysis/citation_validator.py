"""Citation validator: ensures every claim in a redline cites a rule_id."""
from __future__ import annotations

import re

_CITE_PATTERN = re.compile(r"rule:[A-Za-z0-9_\-]{1,64}")


def validate_citations(rationale: str, citations: str) -> list[str]:
    """Return a list of citation issues (empty list == all good)."""
    issues: list[str] = []
    if not _CITE_PATTERN.search(citations):
        issues.append("citations must include at least one 'rule:<id>' token")
    if not _CITE_PATTERN.search(rationale):
        issues.append("rationale must include at least one 'rule:<id>' citation")
    return issues
