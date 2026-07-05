"""ClauseLens AI service entrypoint.

Per Constitution Principle V: grounded-AI orchestration with human-in-the-loop
checkpoints. Per FR-012: per-flag confidence scoring. Per FR-009a: rule
aggregation. Per FR-001a/FR-002a: prompt-injection defense and PII redaction
are applied in `guardrails` middleware.
"""
from __future__ import annotations

import logging
import os
from contextlib import asynccontextmanager

from fastapi import FastAPI

from .analysis.clause_analyzer import analyze_clause
from .analysis.redline_generator import generate_redline
from .analysis.obligation_extractor import extract_obligations
from .analysis.compare import compare_to_standard
from .guardrails import apply_guardrails
from .models.schemas import (
    ClauseAnalysisRequest,
    ClauseAnalysisResult,
    RedlineRequest,
    RedlineResult,
    ObligationsRequest,
    ObligationsResult,
    CompareRequest,
    CompareResult,
)
from .telemetry.metrics import record_grounding_score, record_retrieval_hit


logging.basicConfig(level=os.getenv("LOG_LEVEL", "INFO"))
logger = logging.getLogger("clauselens.ai")


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("AI service starting up")
    yield
    logger.info("AI service shutting down")


app = FastAPI(
    title="ClauseLens AI Service",
    version="0.1.0",
    lifespan=lifespan,
    description="Clause analysis, redline generation, obligation extraction, and side-by-side comparison.",
)


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/analyze-clause", response_model=ClauseAnalysisResult)
async def analyze_clause_endpoint(req: ClauseAnalysisRequest) -> ClauseAnalysisResult:
    """Per FR-003 + FR-012: returns whether the clause is compliant, the matched
    rules, the highest severity, an explicit confidence score, and a rationale."""
    safe = apply_guardrails(req.clause_text)
    result = await analyze_clause(safe, req.clause_type, req.applicable_rules)
    record_grounding_score(result.confidence_score)
    if result.matched_rule_ids:
        record_retrieval_hit()
    return result


@app.post("/generate-redline", response_model=RedlineResult)
async def generate_redline_endpoint(req: RedlineRequest) -> RedlineResult:
    """Per FR-004: produces a redline with rationale and citations to playbook rules."""
    safe = apply_guardrails(req.clause_text)
    return await generate_redline(safe, req.matched_rule_standard_language, req.rule_guideline)


@app.post("/extract-obligations", response_model=ObligationsResult)
async def extract_obligations_endpoint(req: ObligationsRequest) -> ObligationsResult:
    """Per US5: extracts obligations with party, deadline, and trigger condition."""
    safe = apply_guardrails(req.clause_text)
    return await extract_obligations(safe)


@app.post("/compare", response_model=CompareResult)
async def compare_endpoint(req: CompareRequest) -> CompareResult:
    """Per US4: produces a token-level diff between the clause and the standard."""
    safe = apply_guardrails(req.clause_text)
    return await compare_to_standard(safe, req.standard_language)
