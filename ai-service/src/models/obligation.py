"""Per-tenant obligation model used by the obligation extractor."""
from __future__ import annotations

from datetime import datetime
from pydantic import BaseModel


class ExtractedObligation(BaseModel):
    description: str
    responsible_party: str
    due_date: datetime | None
    trigger_condition: str | None
