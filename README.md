# ClauseLens

AI-powered contract review system. Flags risky clauses against your firm's
playbook, suggests grounded redlines, and coordinates the review workflow.

> Greenfield SpecKit project. The authoritative source of truth for the
> feature is `specs/001-contract-review-system/spec.md`. Architecture and
> data model live in `specs/001-contract-review-system/plan.md` and
> `data-model.md`. Tasks live in `specs/001-contract-review-system/tasks.md`.

## Repository layout

```
backend/         .NET 10 / C# 14 (Clean Architecture, DDD, CQRS, MediatR)
frontend/        Angular 18 (PWA)
ai-service/      Python 3.12 (FastAPI + LangChain + LlamaIndex)
infra/           Terraform, Docker, Helm/Kustomize
specs/           SpecKit feature specs, plans, tasks, design artifacts
.github/         CI workflows
```

## Local development

```bash
docker compose up --build
# API:        http://localhost:5000
# Frontend:   http://localhost:4200
# AI service: http://localhost:8000
```

See `specs/001-contract-review-system/quickstart.md` for the validation
scenarios and end-to-end runbook.

## Constitutional principles

This project follows the **ClauseLens Constitution** at
`.specify/memory/constitution.md` — Spec-First, Clean Architecture +
DDD, Contract-First Integration, Observability & Auditability, and
Grounded AI with Human Oversight.
