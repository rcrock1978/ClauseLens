"""OpenTelemetry metrics for the AI service (Constitution Principle V).

Exports: grounding score, retrieval hit rate, latency, token usage, cost, drift.
The endpoint is scraped by the backend's CI eval gate (see .github/workflows/ci.yml).
"""
from __future__ import annotations

from opentelemetry import metrics
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import InMemoryMetricReader


_meter = metrics.get_meter("clauselens.ai", version="0.1.0")

_grounding = _meter.create_histogram("clauselens.ai.grounding_score", unit="1")
_retrieval = _meter.create_counter("clauselens.ai.retrieval_hits", unit="1")
_latency = _meter.create_histogram("clauselens.ai.latency_ms", unit="ms")
_tokens = _meter.create_counter("clauselens.ai.tokens_used", unit="1")
_cost = _meter.create_counter("clauselens.ai.cost_usd", unit="USD")


def record_grounding_score(score: float) -> None:
    _grounding.record(score)


def record_retrieval_hit() -> None:
    _retrieval.add(1)


def record_latency_ms(ms: float) -> None:
    _latency.record(ms)


def record_tokens(n: int) -> None:
    _tokens.add(n)


def record_cost_usd(amount: float) -> None:
    _cost.add(amount)
