<!--
  Sync Impact Report
  ==================
  Version change: (template) → 1.0.0
  Bump rationale: Initial constitution fill — no prior version.
  Modified principles: N/A (all new)
  Added sections:
    - Principle I: Spec-First Development
    - Principle II: Clean Architecture & Domain-Driven Design
    - Principle III: Contract-First Integration
    - Principle IV: Observability & Auditability
    - Principle V: Grounded AI with Human Oversight
    - Quality & Security Constraints
    - Development Workflow
  Removed sections: N/A
  Templates requiring updates:
    - .specify/templates/plan-template.md ✅ No change needed — Constitution Check section already exists and references constitution.md.
    - .specify/templates/spec-template.md ✅ No change needed — structure is generic and compatible.
    - .specify/templates/tasks-template.md ✅ No change needed — task categorization is independent of principles.
    - .opencode/commands/ — all command files reference the constitution path generically; no update required.
  Follow-up TODOs: None.
-->

# ClauseLens Constitution

## Core Principles

### I. Spec-First Development

Every feature MUST start with an executable specification (`spec.md`).
The SDD lifecycle is strictly enforced:

    speckit.specify → [review gate] → speckit.plan → [review gate] → speckit.tasks → speckit.implement

No code is written without an approved spec. No spec ships without passing
its review gate. Acceptance criteria from the spec are the source of truth
for all tests and validation. Deviations from the spec MUST be documented
and re-reviewed.

Rationale: Prevents ad-hoc development, makes scope auditable, and keeps
the agent and human team aligned on a single source of truth.

### II. Clean Architecture & Domain-Driven Design

Dependency rules are strict and non-negotiable:

- **Domain layer** — entities, value objects, aggregates, domain events.
  Zero framework or infrastructure dependencies.
- **Application layer** — CQRS commands/queries, MediatR handlers, ports
  (interfaces), DTOs, validators. Depends only on Domain.
- **Infrastructure layer** — EF Core, message bus, caching, AI adapters.
  Implements ports defined in Application.
- **Presentation layer** — ASP.NET Core controllers/minimal APIs, Angular
  frontend. Depends only on Application.

Bounded contexts (Document Intake, Clause Analysis, Playbook & Rules,
Review Workflow, Audit) define service boundaries. Ubiquitous language
from the PRD MUST be maintained across all artifacts. Anti-Corruption
Layers MUST protect domain purity when integrating external systems.

Rationale: Keeps the system maintainable as it grows from MVP to
thousands of tenants. Enforces testability and independent deployability
of each bounded context.

### III. Contract-First Integration

APIs (OpenAPI), event schemas, and MCP tool schemas are defined as
contracts before any implementation begins. Contracts are the interface
that enables parallel work across:

- .NET 10 backend services
- Angular frontend (responsive web/PWA)
- Python 3.12 AI/Inference service

Backward-compatible changes to contracts are preferred. Breaking changes
MUST go through a version bump and be communicated across all consuming
teams before implementation.

Rationale: Contracts-first decouples delivery across three language
ecosystems and allows front-end, back-end, and AI work to proceed in
parallel against agreed boundaries.

### IV. Observability & Auditability

All services MUST produce structured logs and distributed traces:

- **Structured logging** via Serilog (.NET) with correlation IDs across
  services. Logs are written to daily-rotated JSON files at
  `logs/{yyyy-MM-dd}.json` — one JSON object per line with fields:
  timestamp, level, correlationId, service, message, exception, metadata.
- **Distributed tracing** via OpenTelemetry across gateway → services →
  data → AI service → model/tool calls.
- **Append-only audit log** for all security-relevant and state-changing
  actions. Audit logs are tamper-evident and retained per compliance policy.
- **AI observability** — per-call token usage, cost, latency, retrieval
  hits, grounding/eval scores, and drift metrics.

Rationale: A multi-tenant SaaS cannot debug, bill, or audit without
structured, centralized observability. Daily JSON files ensure offline
analysis and long-term retention independent of the cloud backend.

### V. Grounded AI with Human Oversight

All AI-generated outputs MUST be grounded in retrieved context (playbook
clauses, contract terms) with explicit citations. Core rules:

- Eval gates in CI enforce minimum thresholds for relevance, faithfulness,
  and task success before any AI change ships.
- Human-in-the-loop checkpoints are required for high-impact actions
  (e.g., approving redlines, modifying playbooks).
- Prompt-injection defense, PII redaction, and output moderation are
  non-negotiable guardrails applied before every model call and before
  surfacing results.
- No customer data is used for training without explicit consent.
- Model routing (cheap model first), caching, and token budgets are used
  to control cost without sacrificing quality.

Rationale: Legal use cases demand accuracy and accountability. Ungrounded
AI output is worse than no output. Eval gates and human oversight protect
both the user and the product's reputation.

## Quality & Security Constraints

### Multi-Tenant Isolation

Tenant key on every table + row-level security. No cross-tenant data access
by construction. Tenant-isolation tests MUST pass in CI.

### Performance Budgets

- API reads: p95 < 200 ms
- API writes: p95 < 400 ms
- AI first-token: < 1.5 s, grounded answer: < 6 s (streamed)

### Security Gates

OWASP API Top 10, SAST/SCA/secret-scan, and signed container images
MUST pass in CI before deployment. Secrets never in code or images.

## Development Workflow

### SDD Lifecycle

The canonical workflow with mandatory review gates:

```
speckit.specify → [approve/reject] → speckit.plan → [approve/reject]
→ speckit.tasks → speckit.implement
```

Rejecting a gate aborts the workflow until the concern is resolved.

### CI/CD Gates

Every PR MUST pass:
1. Build + unit/integration tests
2. AI eval thresholds (when AI changes are involved)
3. SAST/SCA/secret-scan
4. Tenant-isolation tests
5. Performance budget checks

### Constitution Compliance

Every `plan.md` MUST include a Constitution Check section evaluating
whether the proposed design violates any principle. Violations MUST be
documented with justification and a simpler alternative that was rejected.
Unjustified violations are a gate failure.

## Governance

This Constitution is the highest source of truth for engineering decisions
in the ClauseLens project. It supersedes any tool-specific guidance or
ad-hoc conventions.

### Amendment Procedure

1. Propose the change with documented rationale.
2. Obtain team approval (review gate).
3. Bump the version according to the rules below.
4. Update the constitution and propagate changes to dependent templates.
5. Run `speckit.agent-context.update` to refresh referenced guidance.

### Versioning Policy

- **MAJOR**: Backward-incompatible principle removal or redefinition.
- **MINOR**: New principle, new section, or materially expanded guidance.
- **PATCH**: Clarifications, wording refinements, typo fixes.

### Compliance Review

- Every plan gate MUST check constitution compliance.
- Complexity MUST be justified when a principle is violated.
- All PRs/reviews MUST verify constitution compliance for the changed areas.
- Use `.specify/scripts/bash/check-prerequisites.sh` to verify environment
  compliance.

**Version**: 1.0.0 | **Ratified**: 2026-06-30 | **Last Amended**: 2026-07-01
