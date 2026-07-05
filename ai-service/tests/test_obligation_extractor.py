"""Tests for the obligation extractor (per US5)."""
from __future__ import annotations

import pytest

from clauselens.analysis.obligation_extractor import extract_obligations


@pytest.mark.asyncio
async def test_extracts_shall_obligations() -> None:
    text = (
        "Provider shall deliver the report by Q4. "
        "Customer must pay the invoice within 30 days. "
        "This clause has no obligation."
    )
    r = await extract_obligations(text)
    descriptions = [o.description for o in r.obligations]
    assert any("deliver the report" in d for d in descriptions)
    assert any("pay the invoice" in d for d in descriptions)
    assert not any("no obligation" in d for d in descriptions)
