"""Verify the AI observability metrics endpoint is reachable.

Used by the GitHub Actions `eval-gates` job (per T145a).
"""
from __future__ import annotations

import os
import sys

import httpx


def main() -> int:
    base = os.getenv("AI_METRICS_URL", "http://ai-service:8000")
    try:
        r = httpx.get(f"{base}/health", timeout=5.0)
        r.raise_for_status()
    except Exception as exc:
        print(f"metrics endpoint unreachable: {exc}", file=sys.stderr)
        return 1
    print(f"metrics endpoint OK: {r.json()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
