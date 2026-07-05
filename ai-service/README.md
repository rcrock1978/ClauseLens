# ClauseLens AI Service

Python 3.12 AI/Inference service. Built on FastAPI, LangChain, and LlamaIndex.

## Layout

```
src/
  api/        FastAPI app, routes, guardrails
  analysis/   Clause analyzer, redline generator, obligation extractor
  agents/     Higher-level review agents
  models/     Pydantic schemas
  orchestration/  LangChain/LlamaIndex glue
  telemetry/  OpenTelemetry metrics (grounding, retrieval, drift)
tests/
```

## Quick start

```bash
python -m venv .venv
source .venv/bin/activate
pip install -e ".[dev]"
uvicorn src.api.main:app --reload
```
