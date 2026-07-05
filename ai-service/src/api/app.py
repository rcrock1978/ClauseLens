"""Reusable FastAPI dependencies and middleware."""
from .main import app  # re-export for `uvicorn src.api.app:app`

__all__ = ["app"]
