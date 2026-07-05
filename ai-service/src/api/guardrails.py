"""Prompt-injection defense + PII redaction. Per Constitution Principle V."""
from __future__ import annotations

import re

# Patterns we never want to forward to the LLM.
_INJECTION_PATTERNS = [
    r"ignore (all )?previous instructions",
    r"system prompt",
    r"disregard your instructions",
    r"reveal your (system|hidden) prompt",
]

# Conservative PII regexes (email, phone, SSN). Real production code would use
# Microsoft Presidio or a dedicated redaction service.
_PII_PATTERNS = {
    "email": re.compile(r"[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+"),
    "phone": re.compile(r"\+?\d[\d\-\s().]{7,}\d"),
    "ssn":   re.compile(r"\b\d{3}-\d{2}-\d{4}\b"),
}


def apply_guardrails(text: str) -> str:
    """Strip prompt-injection patterns and redact obvious PII before any LLM call."""
    safe = text
    for pat in _INJECTION_PATTERNS:
        safe = re.sub(pat, "[REDACTED-INJECTION]", safe, flags=re.IGNORECASE)
    for label, regex in _PII_PATTERNS.items():
        safe = regex.sub(f"[REDACTED-{label.upper()}]", safe)
    return safe
