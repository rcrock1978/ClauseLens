# Implementation Plan: AI-Powered Contract Review System

**Branch**: `001-contract-review-system` | **Date**: 2026-07-01 | **Spec**: specs/001-contract-review-system/spec.md

**Input**: Feature specification from `specs/001-contract-review-system/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

**Clarifications incorporated**: 10 total (5 from the first session,
5 from the second session). New FRs added: FR-012 (AI confidence),
FR-013 (retention), FR-007a (primary+secondary reviewers),
FR-007b (reviewer SLA), FR-007c (self-service signup).

## Summary

ClauseLens is a multi-tenant SaaS platform that lets legal and ops teams
upload contracts (PDF/DOCX), automatically segment them into clauses, flag
risks against a firm's playbook, suggest redlines with citations, and
manage a review workflow with per-clause sign-off. Built with .NET 10
backend services, an Angular responsive web frontend, and a Python 3.12
AI service for LLM orchestration, RAG, and clause analysis.

## Technical Context

**Language/Version**: .NET 10 / C# 14 (backend services), Python 3.12
(AI/Inference service), TypeScript (Angular frontend)

**Primary Dependencies**: ASP.NET Core (minimal APIs + controllers),
MediatR (CQRS), FluentValidation, Entity Framework Core (SQL Server),
MassTransit (Azure Service Bus / RabbitMQ), Serilog (structured logging),
OpenTelemetry (distributed tracing), Semantic Kernel (.NET), LangChain /
LlamaIndex (Python), Angular (PWA frontend)

**Storage**: SQL Server (primary operational store with CQRS read models),
Azure AI Search (hybrid vector + keyword search for playbook/contract
retrieval), Redis (caching layer)

**Testing**: xUnit (.NET unit/integration/contract tests), pytest (Python
AI service), Playwright (E2E for Angular frontend), ArchUnitNET (layering
enforcement)

**Target Platform**: Linux containers (Docker), Kubernetes (AKS), responsive
web/PWA via modern browsers

**Project Type**: Web application — multi-service with Angular frontend,
.NET backend services, and Python AI service

**Performance Goals**: API reads p95 < 200 ms, API writes p95 < 400 ms,
AI first-token < 1.5 s, grounded answer < 6 s (streamed)

**Constraints**: Multi-tenant isolation (row-level security), 50 pages /
25 MB document upload limit, SOC 2 / GDPR readiness, English-language MVP,
external integrations (DMS, DocuSign, SSO) deferred

**Scale/Scope**: SMB multi-tenant SaaS targeting 10k+ users, with
horizontal scaling via Kubernetes HPA and CQRS read models

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Principle I — Spec-First Development**: ✅ PASS — Spec exists
(specs/001-contract-review-system/spec.md) created via speckit.specify
workflow. All requirements trace to spec sections.

**Principle II — Clean Architecture & DDD**: ✅ PASS — Plan follows
Clean Architecture layering. Bounded contexts (Document Intake, Clause
Analysis, Playbook & Rules, Review Workflow, Audit) map to service
boundaries. No violations at plan stage.

**Principle III — Contract-First Integration**: ✅ PASS — Contracts
(OpenAPI, event schemas, MCP tools) will be defined in contracts/ before
implementation, enabling parallel .NET, Angular, Python delivery.

**Principle IV — Observability & Auditability**: ✅ PASS — Audit trail
is a functional requirement (FR-008). Structured logging with daily JSON
rotation and OpenTelemetry tracing are planned.

**Principle V — Grounded AI with Human Oversight**: ✅ PASS — Redlines
include citations (FR-004). Per-flag confidence (High/Medium/Low) is
emitted (FR-012); Low confidence auto-routes to "Needs Discussion"
rather than masquerading as a flag. Human-in-the-loop via review
workflow. Eval gates planned in CI.

**Re-check post-Phase 1**: All principles still PASS. New FRs map to
existing layering — `Tenant`/`User` onboarding lives in Application +
Infrastructure (Auth/email-verify), `Retention` is an Infrastructure
background job consuming domain events, `Reviewer SLA` is an
Application scheduling concern driven by a domain event on assignment.
No additional bounded contexts required.

**Result**: ALL GATES PASS. No violations. Complexity Tracking not required.

## Project Structure

### Documentation (this feature)

```text
specs/001-contract-review-system/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── ClauseLens.sln
├── src/
│   ├── ClauseLens.Domain/              # Entities, value objects, aggregates, domain events
│   ├── ClauseLens.Application/         # CQRS commands/queries, MediatR handlers, ports, DTOs
│   ├── ClauseLens.Infrastructure/      # EF Core, MassTransit, Redis, AI adapters, Serilog
│   └── ClauseLens.Api/                 # ASP.NET Core minimal APIs, middleware, auth
├── tests/
│   ├── ClauseLens.Domain.Tests/
│   ├── ClauseLens.Application.Tests/
│   └── ClauseLens.Integration.Tests/

frontend/
├── src/
│   ├── app/                            # Angular modules, components, pages
│   ├── core/                           # Auth, HTTP interceptors, guards
│   └── shared/                         # Common components, models
└── tests/

ai-service/
├── src/
│   ├── api/                            # FastAPI / gRPC endpoints
│   ├── analysis/                       # Clause analysis, RAG pipeline
│   ├── agents/                         # AI agents (review, extraction)
│   └── models/                         # Pydantic schemas
└── tests/

infra/
├── terraform/                          # IaC for Azure resources
├── docker/                             # Dockerfiles per service
└── k8s/                                # Helm/Kustomize manifests

scripts/
└── check-prerequisites.sh
```

**Structure Decision**: Multi-project monorepo with three top-level
directories: `backend/` (.NET), `frontend/` (Angular), `ai-service/`
(Python). `infra/` holds all infrastructure-as-code. This matches the
three-language architecture required by the PRD and Constitution
Principle III (Contract-First Integration).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations. Complexity Tracking not required.

## Phase 1 Design Artifacts

- `research.md` — Technology and pattern decisions, including the five
  decisions added in the second clarification session (AI confidence,
  retention/offboarding, reviewer model, reviewer SLA, tenant bootstrap).
- `data-model.md` — 12 entities. `RiskFlag` and `Redline` now carry
  `ConfidenceLevel`; `ReviewTask` now models `PrimaryReviewerId` +
  `SecondaryReviewerIds` + `SlaDeadline` + `ReassignmentReason`;
  `Tenant` carries retention settings and lifecycle state; `User` carries
  `EmailVerifiedAt` and `InvitedById`.
- `contracts/api-v1.yaml` — Adds `tenants/signup`, `tenants/{id}/offboard`,
  `gdpr/erasure`, `auth/verify-email`, `users/invite`,
  `contracts/{id}/review/reassign`. RiskFlag/Redline now expose
  `confidence`; ReviewTask body changes from `reviewerId` to
  `primaryReviewerId` + `secondaryReviewerIds`.
- `quickstart.md` — Scenarios 4-8 cover review SLA reassign, tenant
  signup, email verification, audit, and offboarding.
