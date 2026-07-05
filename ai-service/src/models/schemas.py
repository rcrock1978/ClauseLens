"""Pydantic schemas for the AI service. Mirrors contracts/api-v1.yaml."""
from __future__ import annotations

from datetime import datetime
from typing import Literal

from pydantic import BaseModel, Field


ConfidenceLevel = Literal["high", "medium", "low"]


class PlaybookRuleSummary(BaseModel):
    rule_id: str
    clause_type: str
    condition: str
    standard_language: str
    severity: int = Field(ge=0, le=3)


class ClauseAnalysisRequest(BaseModel):
    clause_text: str
    clause_type: str
    applicable_rules: list[PlaybookRuleSummary]


class ClauseAnalysisResult(BaseModel):
    is_compliant: bool
    matched_rule_id: str | None
    matched_rule_ids: list[str]
    highest_severity: int
    confidence: ConfidenceLevel
    confidence_score: float = Field(ge=0.0, le=1.0)
    rationale: str


class RedlineRequest(BaseModel):
    risk_flag_id: str
    clause_text: str
    matched_rule_standard_language: str
    rule_guideline: str


class RedlineResult(BaseModel):
    suggested_text: str
    rationale: str
    citations: str
    confidence: ConfidenceLevel
    confidence_score: float = Field(ge=0.0, le=1.0)


class ObligationsRequest(BaseModel):
    clause_text: str


class ObligationItem(BaseModel):
    description: str
    responsible_party: str
    due_date: datetime | None
    trigger_condition: str | None


class ObligationsResult(BaseModel):
    obligations: list[ObligationItem]


class CompareRequest(BaseModel):
    clause_text: str
    standard_language: str


class DiffSpan(BaseModel):
    start: int
    length: int
    operation: Literal["equal", "insert", "delete"]


class CompareResult(BaseModel):
    spans: list[DiffSpan]
