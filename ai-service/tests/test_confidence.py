"""Confidence calibration tests (per FR-012 + Principle V eval gate)."""
from clauselens.analysis.confidence import calibrate_confidence


def test_high_band() -> None:
    assert calibrate_confidence(0.95) == "high"
    assert calibrate_confidence(0.85) == "high"


def test_medium_band() -> None:
    assert calibrate_confidence(0.80) == "medium"
    assert calibrate_confidence(0.65) == "medium"


def test_low_band() -> None:
    assert calibrate_confidence(0.64) == "low"
    assert calibrate_confidence(0.10) == "low"
