# Research: AI-Powered Contract Review System

## Technology Decisions

### Backend: .NET 10 / C# 14 with Clean Architecture

- **Decision**: ASP.NET Core with MediatR (CQRS), EF Core, MassTransit
- **Rationale**: Matches the PRD stack requirement. .NET 10 provides native
  AOT compilation, better container density, and long-term LTS support.
  MediatR decouples controllers from use cases. EF Core provides
  migrations-first workflow with SQL Server.
- **Alternatives considered**: Node.js/Express (rejected — PRD mandates .NET),
  Go (rejected — team expertise mismatch)

### AI Service: Python 3.12 with LangChain + Semantic Kernel

- **Decision**: Hybrid AI orchestration — Semantic Kernel (.NET) for
  integration into the backend, LangChain/LlamaIndex (Python) for the
  dedicated AI service
- **Rationale**: Semantic Kernel provides native .NET integration for
  simple AI flows (classification, routing). LangChain/LlamaIndex in
  Python provides the richest ecosystem for RAG, agent orchestration, and
  evaluation.
- **Alternatives considered**: Pure .NET AI stack (rejected — Python
  ecosystem for LLM/RAG is more mature), Pure Python backend (rejected —
  PRD mandates .NET for core services)

### Frontend: Angular (Responsive Web/PWA)

- **Decision**: Angular with PWA capabilities
- **Rationale**: Matches the PRD tech stack decision. Angular provides
  strong typing, dependency injection, and a structured module system
  suitable for enterprise legal applications. PWA enables offline
  document review.
- **Alternatives considered**: React (rejected — PRD specifies Angular),
  Blazor (rejected — PRD specifies Angular for frontend)

### Infrastructure: Azure Cloud with Kubernetes

- **Decision**: AKS with Terraform IaC, Helm/Kustomize, GitHub Actions CI/CD
- **Rationale**: PRD mandates Azure. AKS provides Kubernetes orchestration.
  Terraform enables reproducible environments. GitHub Actions aligns
  with the portfolio's CI/CD standard.
- **Alternatives considered**: AWS EKS (rejected — PRD mandates Azure),
  Docker Compose-only (rejected — not suitable for multi-tenant SaaS scale)

### Database: SQL Server + Azure AI Search + Redis

- **Decision**: CQRS with SQL Server write model, read models, and Azure
  AI Search for hybrid vector/keyword retrieval
- **Rationale**: SQL Server fits PRD persistence requirement. Azure AI
  Search provides integrated hybrid search (vector + keyword) needed for
  RAG. Redis caches hot read models and query results.
- **Alternatives considered**: PostgreSQL + pgvector (rejected — PRD
  mandates SQL Server + Azure AI Search)

### Authentication: OpenID Connect (Entra ID / Auth0)

- **Decision**: OIDC/OAuth2 with Entra ID or Auth0, RBAC/ABAC for authorization
- **Rationale**: PRD mandates OIDC standards. Entra ID is native to Azure.
  Auth0 provides a simpler developer experience.
- **Alternatives considered**: Custom JWT (rejected — security compliance
  requirements), AWS Cognito (rejected — Azure-first stack)

### AI Confidence Scoring & Low-Confidence Routing

- **Decision**: Emit per-flag confidence (High/Medium/Low); auto-route
  Low-confidence flags to "Needs Discussion" instead of "Flagged"
- **Rationale**: Aligns with Constitution Principle V (grounded AI with
  human oversight). Reviewers see uncertainty explicitly rather than
  treating low-confidence output as a false-positive risk. Eval gates
  in CI can enforce confidence calibration thresholds per release.
- **Alternatives considered**: Single high-confidence threshold with
  silent suppression (rejected — hides uncertainty from reviewers);
  always-surface with no routing distinction (rejected — drowns
  reviewers in low-quality flags).

### Data Retention & Tenant Offboarding (GDPR + SOC 2)

- **Decision**: Configurable per-tenant retention (default 7 years for
  contracts and audit log; playbook retained while tenant is active).
  Offboarding = 30-day soft delete then hard delete. Right-to-erasure
  honored within 30 days.
- **Rationale**: SOC 2 expects long audit retention; GDPR right-to-erase
  demands a finite SLA. Configurable per-tenant retention is the only
  way to satisfy both without a per-customer policy engine. Soft
  delete gives operators a recovery window.
- **Alternatives considered**: Fixed 7-year with immediate offboarding
  delete (rejected — no operator recovery, harsh for legal mistakes);
  indefinite retention with manual purge (rejected — no SLA on
  erasure, GDPR exposure).

### Reviewer Assignment Model (Primary + Secondary)

- **Decision**: One primary Reviewer (decides); zero to two secondary
  Reviewers (comments only, non-blocking).
- **Rationale**: Matches typical legal-review patterns (lead + advisors)
  without requiring a consensus/quorum engine for MVP. Preserves
  single-decision state machine; secondaries add observability without
  complicating the data model.
- **Alternatives considered**: Single reviewer only (rejected — does
  not match real legal workflows); full panel with majority vote
  (rejected — adds quorum/aggregation logic not needed at MVP scale).

### Reviewer SLA & Reassignment

- **Decision**: 7 business-day SLA on primary Reviewer. Owner is
  auto-nudged at day 3; Owner may reassign at day 7 with mandatory
  reason recorded in audit log.
- **Rationale**: Legal review legitimately takes days, not hours, so
  no sub-day timeout. A 7-day hard cap with a 3-day nudge prevents
  silent stalls while keeping the contract moving. Reassignment reason
  is captured for audit (Principle IV).
- **Alternatives considered**: No SLA (rejected — violates SC-004
  cycle-time budget); 3-day hard timeout with auto-reassign (rejected
  — too aggressive, may reassign valid in-progress reviews).

### Tenant Bootstrap & First Admin Provisioning

- **Decision**: Self-service signup — first Admin registers with email
  + password, verifies email before first login, then invites all
  subsequent users in the tenant.
- **Rationale**: Standard B2B SaaS bootstrap. Works without an external
  IdP, which is explicitly out of scope for MVP. No human sales
  intervention required, enabling product-led growth. Email
  verification is the minimum bar to claim tenant ownership.
- **Alternatives considered**: Operator-provisioned only (rejected —
  blocks self-serve GTM); invite-only from an existing Admin (rejected
  — chicken-and-egg for the very first tenant); magic-link only
  (rejected — diverges from the OIDC/Auth0 stack already chosen).

## Architecture Patterns

### CQRS with MediatR

Commands mutate state via aggregate methods and publish domain events.
Queries read from denormalized read models. Pipeline behaviors handle
validation, logging, performance monitoring, and caching cross-cutting.

### Event-Driven with Outbox

Domain events are published reliably via the transactional outbox pattern
(EF Core + MassTransit). Integration events are published to the message
bus for cross-service communication.

### Contract-First Development

OpenAPI specs, event schemas, and MCP tool schemas are defined before
implementation. This enables parallel work across .NET backend, Angular
frontend, and Python AI service.

### Multi-Tenant Row-Level Security

Every table includes a TenantId column. Row-level security policies
enforce tenant isolation at the database level. Application-layer
authorization reinforces this.

## Key Risks and Mitigations

| Risk | Mitigation |
|------|-----------|
| LLM hallucination in risk flags | Grounded RAG with citations, eval thresholds in CI, human-in-the-loop review |
| Low-confidence false positives | Per-flag High/Medium/Low confidence score; Low auto-routes to "Needs Discussion" |
| Multi-tenant data leakage | Row-level security, tenant key everywhere, isolation tests in CI |
| Document parsing quality | Support PDF/DOCX only for MVP, reject complex formatting gracefully |
| AI service latency | Response streaming, model routing (cheap model first), prompt caching |
| Reviewer stalls blocking SC-004 | 7 business-day SLA with day-3 nudge and day-7 reassign |
| GDPR / SOC 2 retention conflict | Configurable per-tenant retention; 30-day soft delete on offboarding; 30-day right-to-erasure SLA |
| Cloud cost overruns | Right-sized K8s resources, scale-to-zero for non-prod, FinOps dashboards |
