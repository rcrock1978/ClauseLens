"""Confidence calibration: numeric score -> High/Medium/Low bucket (per FR-012)."""
from __future__ import annotations

from typing import Literal

ConfidenceLevel = Literal["high", "medium", "low"]


def calibrate_confidence(score: float) -> ConfidenceLevel:
    if score >= 0.85:
        return "high"
    if score >= 0.65:
        return "medium"
    return "low"
