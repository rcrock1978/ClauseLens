---
description: "Task list for AI-Powered Contract Review System"
---

# Tasks: AI-Powered Contract Review System

**Input**: Design documents from `/specs/001-contract-review-system/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Test tasks are included because the spec implies acceptance-driven validation (SC-001..SC-006, FR-001..FR-013) and the plan names xUnit + pytest + Playwright + ArchUnitNET as the testing stack. Tests are written first and must fail before implementation per Constitution Principle I.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- Backend (.NET): `backend/src/`, `backend/tests/`
- Frontend (Angular): `frontend/src/`, `frontend/tests/`
- AI service (Python): `ai-service/src/`, `ai-service/tests/`
- Infra: `infra/terraform/`, `infra/docker/`, `infra/k8s/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, monorepo layout, and CI scaffolding

- [X] T001 Create monorepo directory structure (backend/, frontend/, ai-service/, infra/) per plan.md
- [X] T002 Initialize backend .NET 10 solution in backend/ClauseLens.sln with projects ClauseLens.Domain, ClauseLens.Application, ClauseLens.Infrastructure, ClauseLens.Api
- [X] T003 Initialize frontend Angular workspace in frontend/ with PWA support
- [X] T004 Initialize ai-service Python 3.12 project in ai-service/ with pyproject.toml and src layout
- [X] T005 [P] Configure backend linting (dotnet format, EditorConfig) and analyzers in backend/.editorconfig
- [X] T006 [P] Configure frontend linting (ESLint, Prettier, strict tsconfig) in frontend/
- [X] T007 [P] Configure Python linting (ruff, black, mypy) in ai-service/pyproject.toml
- [X] T008 Create docker-compose.yml at repo root orchestrating backend, frontend, ai-service, sqlserver, redis, azurite
- [X] T009 [P] Add GitHub Actions workflow ci.yml running backend tests, frontend tests, ai-service tests, and linters

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T010 Implement Tenant aggregate (TenantId, Name, Status, SoftDeleteScheduledAt, RetentionYears) in backend/src/ClauseLens.Domain/Entities/Tenant.cs
- [X] T011 Implement User aggregate (Email, EmailVerifiedAt, Role, Status, InvitedById) in backend/src/ClauseLens.Domain/Entities/User.cs
- [X] T012 [P] Implement Playbook + PlaybookRule aggregates in backend/src/ClauseLens.Domain/Entities/Playbook.cs
- [X] T013 [P] Implement Contract + Clause aggregates in backend/src/ClauseLens.Domain/Entities/Contract.cs and Clause.cs
- [X] T014 [P] Implement RiskFlag, Redline, Obligation entities in backend/src/ClauseLens.Domain/Entities/
- [X] T015 [P] Implement ReviewTask + ClauseDecision + ReviewerSla entities in backend/src/ClauseLens.Domain/Entities/ReviewTask.cs
- [X] T016 [P] Implement AuditEntry (append-only) in backend/src/ClauseLens.Domain/Entities/AuditEntry.cs
- [X] T017 Configure EF Core DbContext with multi-tenant global query filter and row-level security in backend/src/ClauseLens.Infrastructure/Persistence/ClauseLensDbContext.cs
- [X] T018 [P] Create initial EF Core migration creating all tables in backend/src/ClauseLens.Infrastructure/Migrations/
- [X] T019 Implement self-service tenant signup command + handler (creates Tenant + first Admin in PendingInvite/email-sent state) in backend/src/ClauseLens.Application/Identity/Commands/SignupTenant.cs
- [X] T020 [P] Implement email verification command + handler in backend/src/ClauseLens.Application/Identity/Commands/VerifyEmail.cs
- [X] T021 [P] Implement user-invite command (Admin invites a user) in backend/src/ClauseLens.Application/Identity/Commands/InviteUser.cs
- [X] T022 Implement password hashing + JWT issuance in backend/src/ClauseLens.Infrastructure/Auth/JwtTokenService.cs
- [X] T023 [P] Implement OIDC-style auth middleware (email/password) in backend/src/ClauseLens.Api/Middleware/AuthMiddleware.cs
- [X] T024 Implement MediatR pipeline with FluentValidation, logging, and tenant-scope behavior in backend/src/ClauseLens.Application/Behaviors/
- [X] T025 [P] Configure Serilog with daily-rotated JSON files at logs/{yyyy-MM-dd}.json in backend/src/ClauseLens.Api/Program.cs
- [X] T026 [P] Configure OpenTelemetry tracing (gateway → services → AI service) in backend/src/ClauseLens.Infrastructure/Telemetry/
- [X] T027 [P] Configure MassTransit with Azure Service Bus transport in backend/src/ClauseLens.Infrastructure/Messaging/
- [X] T028 [P] Implement transactional outbox publisher in backend/src/ClauseLens.Infrastructure/Messaging/OutboxPublisher.cs
- [X] T029 [P] Configure Redis connection multiplexer and cache abstraction in backend/src/ClauseLens.Infrastructure/Caching/
- [X] T030 [P] Configure Azure AI Search index client in backend/src/ClauseLens.Infrastructure/Search/AzureAiSearchClient.cs
- [X] T031 [P] Implement semantic-kernel adapter (port) in backend/src/ClauseLens.Application/Abstractions/IAiOrchestrator.cs
- [X] T032 [P] Implement langchain/llamaindex adapter (HTTP) in ai-service/src/orchestration/ai_orchestrator.py
- [X] T033 Implement AI service FastAPI skeleton with /analyze-clause and /generate-redline endpoints in ai-service/src/api/main.py
- [X] T034 [P] Implement Pydantic schemas (ClauseInput, RiskFlagOutput, RedlineOutput) in ai-service/src/models/schemas.py
- [X] T035 [P] Implement prompt-injection defense + PII redaction middleware in ai-service/src/api/guardrails.py
- [X] T036 Implement global Angular app shell, auth interceptor, role-based guard in frontend/src/app/core/
- [X] T037 [P] Implement login, signup, and email-verify pages in frontend/src/app/features/auth/
- [X] T038 [P] Implement shared UI components (button, table, modal, toast) in frontend/src/app/shared/
- [X] T039 Implement dev secrets scaffolding (appsettings.Development.json template) in backend/src/ClauseLens.Api/appsettings.json
- [X] T040 [P] Write ArchUnitNET layering tests (Domain has no infra refs; Application no infra refs) in backend/tests/ClauseLens.Architecture.Tests/

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Upload Contract and Segment Clauses (Priority: P1) 🎯 MVP

**Goal**: Contract Owner can upload a PDF/DOCX, the system segments it into clauses, and the result is queryable.

**Independent Test**: A logged-in Contract Owner POSTs a valid PDF to `/api/v1/contracts` and receives a 201 with a contract ID; GET on `/api/v1/contracts/{id}` returns the segmented clause list within 2 minutes.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T041 [P] [US1] Contract test for POST /contracts (multipart upload) in backend/tests/ClauseLens.Integration.Tests/Contracts/UploadContractTests.cs
- [X] T042 [P] [US1] Contract test for GET /contracts/{id} returning clauses in backend/tests/ClauseLens.Integration.Tests/Contracts/GetContractTests.cs
- [X] T043 [P] [US1] Contract test rejecting unsupported file format with 400 in backend/tests/ClauseLens.Integration.Tests/Contracts/RejectInvalidFormatTests.cs
- [X] T044 [P] [US1] Contract test rejecting >25MB / >50 page docs in backend/tests/ClauseLens.Integration.Tests/Contracts/RejectOversizeTests.cs
- [X] T044a [P] [US1] Contract test for password-protected PDF returning 400 in backend/tests/ClauseLens.Integration.Tests/Contracts/PasswordProtectedTests.cs
- [X] T044b [P] [US1] Contract test for contract with embedded image returning clause with NeedsDiscussion state in backend/tests/ClauseLens.Integration.Tests/Contracts/NonTextualContentTests.cs
- [X] T045 [P] [US1] Playwright E2E test for upload happy-path in frontend/tests/e2e/upload-contract.spec.ts

### Implementation for User Story 1

- [X] T046 [P] [US1] Implement UploadContractCommand + handler in backend/src/ClauseLens.Application/Contracts/Commands/UploadContract.cs
- [X] T047 [P] [US1] Implement GetContractQuery + handler in backend/src/ClauseLens.Application/Contracts/Queries/GetContract.cs
- [X] T048 [US1] Implement document parser (PDF + DOCX) port in backend/src/ClauseLens.Application/Abstractions/IDocumentParser.cs
- [X] T049 [P] [US1] Implement PdfDocumentParser (PdfPig) in backend/src/ClauseLens.Infrastructure/Documents/PdfDocumentParser.cs
- [X] T050 [P] [US1] Implement DocxDocumentParser (Open XML) in backend/src/ClauseLens.Infrastructure/Documents/DocxDocumentParser.cs
- [X] T051 [US1] Implement clause segmentation heuristic (heading detection + numbering patterns) in backend/src/ClauseLens.Infrastructure/Documents/ClauseSegmenter.cs
- [X] T051a [P] [US1] Implement table text-extraction in ClauseSegmenter and image detection emitting a non-textual-content marker in backend/src/ClauseLens.Infrastructure/Documents/TableTextExtractor.cs
- [X] T051b [P] [US1] Implement password-detection pre-check in PdfDocumentParser and DocxDocumentParser raising PasswordProtectedDocumentException mapped to HTTP 400 in backend/src/ClauseLens.Infrastructure/Documents/
- [X] T052 [US1] Implement blob storage port + adapter (LocalFileSystem for dev, Azure Blob in prod) in backend/src/ClauseLens.Infrastructure/Storage/BlobStorage.cs
- [X] T053 [US1] Implement ContractsController endpoints (POST /contracts, GET /contracts/{id}) in backend/src/ClauseLens.Api/Controllers/ContractsController.cs
- [X] T054 [US1] Add validation: file format whitelist, 25MB cap, 50-page cap in backend/src/ClauseLens.Application/Contracts/Validators/UploadContractValidator.cs
- [X] T055 [US1] Implement Angular upload page with drag-drop + progress in frontend/src/app/features/contracts/upload/
- [X] T056 [US1] Implement Angular contract detail page showing clause list in frontend/src/app/features/contracts/detail/
- [X] T057 [US1] Add structured logging + correlation ID for upload operations in backend/src/ClauseLens.Api/Middleware/

**Checkpoint**: User Story 1 fully functional and independently testable. End-to-end demo: upload a PDF, see clauses listed.

---

## Phase 4: User Story 2 - Flag Risks Against Playbook (Priority: P1)

**Goal**: After segmentation, each clause is compared against the firm's playbook and risks are flagged with confidence scores.

**Independent Test**: Upload a contract; once analyzed, GET `/api/v1/contracts/{id}/risks` returns flags with severity, confidence, and the matched playbook rule. Low-confidence clauses auto-route to "Needs Discussion".

### Tests for User Story 2

- [X] T058 [P] [US2] Contract test for GET /contracts/{id}/risks returning flags in backend/tests/ClauseLens.Integration.Tests/Risks/GetRisksTests.cs
- [X] T059 [P] [US2] Contract test for low-confidence auto-routing to NeedsDiscussion in backend/tests/ClauseLens.Integration.Tests/Risks/LowConfidenceRoutingTests.cs
- [X] T060 [P] [US2] Contract test for compliant clause reference in backend/tests/ClauseLens.Integration.Tests/Risks/CompliantReferenceTests.cs
- [X] T061 [P] [US2] Contract test for unreviewed clause when no rule matches in backend/tests/ClauseLens.Integration.Tests/Risks/UnreviewedWhenNoRuleTests.cs
- [X] T062 [P] [US2] AI service pytest: clause classification returns confidence score in ai-service/tests/test_clause_analysis.py
- [X] T062a [P] [US2] Contract test: 3 matching rules produce 1 flag with severity = max(severities) and matchedRuleIds of length 3 in backend/tests/ClauseLens.Integration.Tests/Risks/RuleAggregationTests.cs
- [X] T063 [P] [US2] Eval gate test enforcing min confidence calibration in ai-service/tests/test_eval_gates.py

### Implementation for User Story 2

- [X] T064 [P] [US2] Implement Playbook template library seeder (NDA, MSA, DPA templates) in backend/src/ClauseLens.Infrastructure/Seeders/PlaybookTemplateSeeder.cs
- [X] T065 [P] [US2] Implement ImportPlaybookTemplatesCommand in backend/src/ClauseLens.Application/Playbooks/Commands/ImportPlaybookTemplates.cs
- [X] T065a [P] [US2] Implement PublishPlaybookRuleCommand requiring Admin approval before a draft rule becomes active in backend/src/ClauseLens.Application/Playbooks/Commands/PublishPlaybookRule.cs
- [X] T065b [P] [US2] Contract test: publishing a draft rule without Admin role returns 403; publishing with Admin role transitions status to Active in backend/tests/ClauseLens.Integration.Tests/Playbooks/PublishApprovalTests.cs
- [X] T066 [US2] Implement PlaybooksController (list, import templates) in backend/src/ClauseLens.Api/Controllers/PlaybooksController.cs
- [X] T067 [P] [US2] Implement RAG retriever (Azure AI Search hybrid vector+keyword) in backend/src/ClauseLens.Infrastructure/Search/PlaybookRetriever.cs
- [X] T068 [P] [US2] Implement AnalyzeClauseSkill in ai-service/src/analysis/clause_analyzer.py
- [X] T069 [P] [US2] Implement confidence scoring wrapper (returns High/Medium/Low) in ai-service/src/analysis/confidence.py
- [X] T070 [US2] Implement AnalyzeContractCommand (orchestrates per-clause calls) in backend/src/ClauseLens.Application/Analysis/Commands/AnalyzeContract.cs
- [X] T070a [P] [US2] Implement rule-aggregation logic returning highest severity, all matching rule IDs, and a single redline input (per FR-009a) in backend/src/ClauseLens.Application/Analysis/RuleAggregator.cs
- [X] T071 [US2] Implement low-confidence routing: any clause with all-Low flags → state = NeedsDiscussion in backend/src/ClauseLens.Domain/Entities/Clause.cs
- [X] T072 [P] [US2] Implement GetRisksQuery in backend/src/ClauseLens.Application/Analysis/Queries/GetRisks.cs
- [X] T073 [US2] Implement RisksController (GET /contracts/{id}/risks) in backend/src/ClauseLens.Api/Controllers/RisksController.cs
- [X] T074 [P] [US2] Implement Angular playbook management page in frontend/src/app/features/playbooks/
- [X] T075 [US2] Implement Angular risk review view (flags grouped by clause, confidence badge) in frontend/src/app/features/risks/

**Checkpoint**: User Story 2 fully functional. Demo: import NDA template → upload contract → see risk flags with confidence badges.

---

## Phase 5: User Story 3 - Get Suggested Redlines with Rationale (Priority: P1)

**Goal**: For each flagged clause, the system provides a redline suggestion with rationale and citations.

**Independent Test**: GET `/api/v1/contracts/{id}/redlines` returns a redline per flagged clause, each with `suggestedText`, `rationale`, `confidence`, and a citation to the triggering rule.

### Tests for User Story 3

- [X] T076 [P] [US3] Contract test for GET /contracts/{id}/redlines in backend/tests/ClauseLens.Integration.Tests/Redlines/GetRedlinesTests.cs
- [X] T077 [P] [US3] Contract test verifying rationale includes rule citation in backend/tests/ClauseLens.Integration.Tests/Redlines/RationaleCitationTests.cs
- [X] T078 [P] [US3] Contract test verifying single redline when multiple rules apply in backend/tests/ClauseLens.Integration.Tests/Redlines/MultiRuleSingleRedlineTests.cs
- [X] T079 [P] [US3] AI service pytest: redline generator returns text + rationale + citation in ai-service/tests/test_redline_generator.py

### Implementation for User Story 3

- [X] T080 [P] [US3] Implement GenerateRedlineSkill in ai-service/src/analysis/redline_generator.py
- [X] T081 [P] [US3] Implement citation validator (ensures every claim cites a rule ID) in ai-service/src/analysis/citation_validator.py
- [X] T082 [US3] Implement GenerateRedlinesCommand (one call per flagged clause, parallel via MassTransit) in backend/src/ClauseLens.Application/Analysis/Commands/GenerateRedlines.cs
- [X] T083 [P] [US3] Implement GetRedlinesQuery in backend/src/ClauseLens.Application/Analysis/Queries/GetRedlines.cs
- [X] T084 [US3] Implement RedlinesController (GET /contracts/{id}/redlines) in backend/src/ClauseLens.Api/Controllers/RedlinesController.cs
- [X] T085 [P] [US3] Implement Angular redline view with accept/reject UI in frontend/src/app/features/redlines/

**Checkpoint**: User Story 3 fully functional. Demo: view redlines alongside flags, see rationale + citations.

---

## Phase 6: User Story 4 - Compare Deviations from Standards (Priority: P2)

**Goal**: Side-by-side diff view of contract clause vs. playbook standard language.

**Independent Test**: Open a flagged clause's comparison view; system displays the contract text and the standard language with differences highlighted. A compliant clause shows a "matches standard" confirmation.

### Tests for User Story 4

- [X] T086 [P] [US4] Contract test for GET /contracts/{id}/clauses/{clauseId}/comparison in backend/tests/ClauseLens.Integration.Tests/Compare/GetComparisonTests.cs
- [X] T087 [P] [US4] Contract test confirming compliant clause shows match in backend/tests/ClauseLens.Integration.Tests/Compare/CompliantMatchTests.cs

### Implementation for User Story 4

- [X] T088 [P] [US4] Implement text-diff utility (token-level diff) in backend/src/ClauseLens.Infrastructure/Documents/TextDiffer.cs
- [X] T089 [US4] Implement GetClauseComparisonQuery in backend/src/ClauseLens.Application/Analysis/Queries/GetClauseComparison.cs
- [X] T090 [P] [US4] Implement comparison endpoint in backend/src/ClauseLens.Api/Controllers/ComparisonsController.cs
- [X] T091 [P] [US4] Implement Angular side-by-side diff component (uses jsdiff or similar) in frontend/src/app/features/compare/
- [X] T092 [US4] Wire comparison view into the clause detail page in frontend/src/app/features/contracts/

**Checkpoint**: User Story 4 fully functional and independently testable.

---

## Phase 7: User Story 5 - Extract Obligations (Priority: P2)

**Goal**: System identifies obligations in the contract (duty, party, deadline, trigger).

**Independent Test**: GET `/api/v1/contracts/{id}/obligations` returns a list of obligations; each with description, responsible party, and optional due date or trigger condition.

### Tests for User Story 5

- [X] T093 [P] [US5] Contract test for GET /contracts/{id}/obligations in backend/tests/ClauseLens.Integration.Tests/Obligations/GetObligationsTests.cs
- [X] T094 [P] [US5] Contract test verifying obligation fields (party, deadline, trigger) in backend/tests/ClauseLens.Integration.Tests/Obligations/ObligationFieldsTests.cs
- [X] T095 [P] [US5] AI service pytest: obligation extractor returns structured output in ai-service/tests/test_obligation_extractor.py

### Implementation for User Story 5

- [X] T096 [P] [US5] Implement ExtractObligationsSkill in ai-service/src/analysis/obligation_extractor.py
- [X] T097 [P] [US5] Implement structured-output schema validation (Pydantic) in ai-service/src/models/obligation.py
- [X] T098 [US5] Implement ExtractObligationsCommand (kicks off post-analysis) in backend/src/ClauseLens.Application/Analysis/Commands/ExtractObligations.cs
- [X] T099 [P] [US5] Implement GetObligationsQuery in backend/src/ClauseLens.Application/Analysis/Queries/GetObligations.cs
- [X] T100 [US5] Implement ObligationsController (GET /contracts/{id}/obligations) in backend/src/ClauseLens.Api/Controllers/ObligationsController.cs
- [X] T101 [P] [US5] Implement Angular obligation summary view in frontend/src/app/features/obligations/

**Checkpoint**: User Story 5 fully functional and independently testable.

---

## Phase 8: User Story 6 - Reviewer Workflow with SLA (Priority: P3)

**Goal**: Contract Owner assigns primary + optional secondary reviewers; reviewers decide per clause; 7-business-day SLA with day-3 nudge and day-7 reassign.

**Independent Test**: Contract Owner assigns a contract to a primary reviewer (and optionally 1–2 secondaries). Primary makes per-clause decisions. After 3 business days the Owner is nudged; after 7 business days the Owner can reassign with a reason captured in the audit log.

### Tests for User Story 6

- [X] T102 [P] [US6] Contract test for POST /contracts/{id}/review with primary + secondaries in backend/tests/ClauseLens.Integration.Tests/Review/AssignReviewTests.cs
- [X] T103 [P] [US6] Contract test for PUT /contracts/{id}/clauses/{clauseId}/decision in backend/tests/ClauseLens.Integration.Tests/Review/DecideClauseTests.cs
- [X] T104 [P] [US6] Contract test for POST /contracts/{id}/review/reassign with reason in backend/tests/ClauseLens.Integration.Tests/Review/ReassignReviewTests.cs
- [X] T105 [P] [US6] Domain test: SlaDeadline computed from assignedAt + 7 business days in backend/tests/ClauseLens.Domain.Tests/Review/SlaDeadlineTests.cs
- [X] T106 [P] [US6] Domain test: secondary reviewers cannot submit decisions in backend/tests/ClauseLens.Domain.Tests/Review/SecondaryCannotDecideTests.cs
- [X] T107 [P] [US6] Integration test: day-3 nudge fires; day-7 reassign allowed in backend/tests/ClauseLens.Integration.Tests/Review/SlaLifecycleTests.cs

### Implementation for User Story 6

- [X] T108 [US6] Implement AssignReviewCommand (validates: primary required; 0–2 secondaries; computes SlaDeadline) in backend/src/ClauseLens.Application/Review/Commands/AssignReview.cs
- [X] T109 [P] [US6] Implement DecideClauseCommand (only primary can decide; secondaries can comment) in backend/src/ClauseLens.Application/Review/Commands/DecideClause.cs
- [X] T110 [P] [US6] Implement ReassignReviewCommand (requires reason) in backend/src/ClauseLens.Application/Review/Commands/ReassignReview.cs
- [X] T111 [P] [US6] Implement SubmitReviewCommand (all clauses decided → contract → Reviewed) in backend/src/ClauseLens.Application/Review/Commands/SubmitReview.cs
- [X] T112 [US6] Implement SlaScheduler background service (fires nudge @ day-3, enables reassign @ day-7) in backend/src/ClauseLens.Infrastructure/Scheduling/SlaScheduler.cs
- [X] T113 [P] [US6] Implement rework loop: rejected clause → new redline → re-review; transitions Contract to RevisionsRequested → ReadyForReview (per FR/state machine) in backend/src/ClauseLens.Application/Review/Workflows/ReviewReworkLoop.cs
- [X] T114 [P] [US6] Implement ReviewController (assign, decide, submit, reassign) in backend/src/ClauseLens.Api/Controllers/ReviewController.cs
- [X] T115 [P] [US6] Implement Angular review assignment page in frontend/src/app/features/review/assign/
- [X] T116 [US6] Implement Angular reviewer decision UI (per-clause approve/reject/needs-discussion) in frontend/src/app/features/review/decide/

**Checkpoint**: User Story 6 fully functional. Demo: assign reviewer, walk through decisions, observe SLA timer.

---

## Phase 9: User Story 7 - Audit Trail (Priority: P3)

**Goal**: Every state-changing action is captured in an append-only audit log, filterable by contract and action type.

**Independent Test**: Perform any action (upload, analyze, decide, redline accept); GET `/api/v1/audit-log?contractId={id}` returns a chronological entry with actor, action, before/after state. Filtering by action type narrows results.

### Tests for User Story 7

- [X] T117 [P] [US7] Contract test for GET /audit-log filtered by contractId in backend/tests/ClauseLens.Integration.Tests/Audit/GetAuditByContractTests.cs
- [X] T118 [P] [US7] Contract test for GET /audit-log filtered by actionType in backend/tests/ClauseLens.Integration.Tests/Audit/GetAuditByActionTests.cs
- [X] T119 [P] [US7] Domain test: AuditEntry cannot be updated or deleted in backend/tests/ClauseLens.Domain.Tests/Audit/AppendOnlyTests.cs
- [X] T120 [P] [US7] Tenant isolation test: Tenant A cannot read Tenant B audit entries in backend/tests/ClauseLens.Integration.Tests/Audit/TenantIsolationTests.cs

### Implementation for User Story 7

- [X] T121 [US7] Implement AuditEntry domain event handler (every aggregate publishes to outbox) in backend/src/ClauseLens.Application/Behaviors/AuditBehavior.cs
- [X] T122 [P] [US7] Implement tamper-evident audit writer (hash-chained rows) in backend/src/ClauseLens.Infrastructure/Audit/TamperEvidentAuditWriter.cs
- [X] T123 [P] [US7] Implement GetAuditLogQuery (filterable by contractId, actionType, actorId, date range) in backend/src/ClauseLens.Application/Audit/Queries/GetAuditLog.cs
- [X] T124 [US7] Implement AuditController (GET /audit-log) in backend/src/ClauseLens.Api/Controllers/AuditController.cs
- [X] T125 [P] [US7] Implement Angular audit log viewer with filter UI in frontend/src/app/features/audit/

**Checkpoint**: User Story 7 fully functional. Demo: perform 5 actions, view the audit log with all 5 entries.

---

## Phase 10: Tenant Lifecycle & Compliance (Cross-Cutting, supports US7 and the constitution)

**Purpose**: Self-service tenant bootstrap, retention enforcement, offboarding, and GDPR right-to-erasure — driven by FR-007c and FR-013, called out separately because they are not user-story-fronted.

- [X] T126 Implement Tenant signup endpoint POST /tenants/signup in backend/src/ClauseLens.Api/Controllers/TenantsController.cs
- [X] T127 [P] Implement email-verification endpoint POST /auth/verify-email in backend/src/ClauseLens.Api/Controllers/AuthController.cs
- [X] T128 [P] Implement invite-user endpoint POST /users/invite in backend/src/ClauseLens.Api/Controllers/UsersController.cs
- [X] T129 Implement tenant offboarding endpoint POST /tenants/{id}/offboard (schedules soft delete + 30-day hard delete) in backend/src/ClauseLens.Api/Controllers/TenantsController.cs
- [X] T130 [P] Implement GDPR right-to-erasure endpoint POST /gdpr/erasure in backend/src/ClauseLens.Api/Controllers/GdprController.cs
- [X] T131 Implement OffboardingScheduler background service (transitions SoftDeleted → HardDeleted at scheduled time) in backend/src/ClauseLens.Infrastructure/Scheduling/OffboardingScheduler.cs
- [X] T132 [P] Implement ErasureProcessor honoring 30-day SLA in backend/src/ClauseLens.Infrastructure/Compliance/ErasureProcessor.cs
- [X] T133 Implement per-tenant retention policy (configurable years for contracts and audit log) in backend/src/ClauseLens.Infrastructure/Compliance/RetentionPolicy.cs
- [X] T134 [P] Write tenant-isolation test suite (cross-tenant data access returns 404 / 403) in backend/tests/ClauseLens.Integration.Tests/TenantIsolation/

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T135 [P] Documentation updates in docs/ (architecture diagram, onboarding guide, ops runbook)
- [X] T136 [P] Add CI eval gate step (AI eval thresholds run on every PR; AI observability metrics endpoint reachable) in .github/workflows/ci.yml
- [X] T145a [P] Emit OpenTelemetry metrics from ai-service for grounding score, retrieval hit rate, latency, token usage, cost, and drift in ai-service/src/telemetry/metrics.py
- [X] T137 [P] Add SAST/SCA/secret-scan steps to CI in .github/workflows/ci.yml
- [X] T138 [P] Add performance budget check (API p95 < 200ms reads, < 400ms writes) in .github/workflows/ci.yml
- [X] T138a [P] Add upload-timing performance test: 50-page PDF must segment in < 2 min (SC-001) in backend/tests/ClauseLens.Performance.Tests/UploadTimingTests.cs
- [X] T139 Terraform module for Azure landing zone (AKS, SQL, AI Search, Service Bus, Key Vault) in infra/terraform/
- [X] T140 [P] Helm chart for backend service in infra/k8s/charts/backend/
- [X] T141 [P] Helm chart for AI service in infra/k8s/charts/ai-service/
- [X] T142 [P] Dockerfile for backend in infra/docker/backend.Dockerfile (multi-stage, non-root, signed)
- [X] T143 [P] Dockerfile for ai-service in infra/docker/ai-service.Dockerfile
- [X] T144 [P] Cost dashboard (FinOps) for token usage, AI service spend in infra/terraform/cost/
- [X] T145 Run quickstart.md validation end-to-end and capture sign-off in specs/001-contract-review-system/validation-report.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Tenant Lifecycle (Phase 10)**: Depends on Foundational + US7 (audit) — can run in parallel with US1-US6 since it depends on no specific story
- **Polish (Phase 11)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Depends on US1 (Clause entity exists) but should be independently testable via seeded data
- **User Story 3 (P1)**: Can start after Foundational (Phase 2) - Depends on US2 (RiskFlag exists) but is independently testable
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - Depends on US1/US2 data; independently testable
- **User Story 5 (P2)**: Can start after Foundational (Phase 2) - Depends on US1 (Clause exists); independently testable
- **User Story 6 (P3)**: Can start after Foundational (Phase 2) - Depends on US1 (Contract state machine) and US3 (redlines for rework); independently testable
- **User Story 7 (P3)**: Can start after Foundational (Phase 2) - Independently testable once a few events exist

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Domain entities before application commands/queries
- Application handlers before API controllers
- API controllers before Angular UI
- Core implementation before integration

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Entities within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members
- Frontend and AI service can be developed in parallel with backend

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Contract test for POST /contracts (multipart upload) in backend/tests/ClauseLens.Integration.Tests/Contracts/UploadContractTests.cs"
Task: "Contract test for GET /contracts/{id} returning clauses in backend/tests/ClauseLens.Integration.Tests/Contracts/GetContractTests.cs"
Task: "Contract test rejecting unsupported file format with 400 in backend/tests/ClauseLens.Integration.Tests/Contracts/RejectInvalidFormatTests.cs"
Task: "Contract test rejecting >25MB / >50 page docs in backend/tests/ClauseLens.Integration.Tests/Contracts/RejectOversizeTests.cs"
Task: "Playwright E2E test for upload happy-path in frontend/tests/e2e/upload-contract.spec.ts"

# Launch all entities/parsers for User Story 1 together:
Task: "Implement PdfDocumentParser (PdfPig) in backend/src/ClauseLens.Infrastructure/Documents/PdfDocumentParser.cs"
Task: "Implement DocxDocumentParser (Open XML) in backend/src/ClauseLens.Infrastructure/Documents/DocxDocumentParser.cs"
Task: "Implement Angular upload page with drag-drop + progress in frontend/src/app/features/contracts/upload/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo
6. Add User Story 5 → Test independently → Deploy/Demo
7. Add User Story 6 → Test independently → Deploy/Demo
8. Add User Story 7 → Test independently → Deploy/Demo
9. Add Tenant Lifecycle → Test independently → Deploy/Demo
10. Polish: IaC, CI gates, cost dashboards, documentation

### Parallel Team Strategy

With multiple developers (recommended split):

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Upload + Segment) + User Story 4 (Compare)
   - Developer B: User Story 2 (Risk Flagging) + User Story 3 (Redlines)
   - Developer C: User Story 5 (Obligations) + User Story 6 (Reviewer Workflow)
   - Developer D: User Story 7 (Audit Trail) + Tenant Lifecycle (Phase 10)
   - Frontend dev: builds pages in parallel with the above, per user story
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Phase 10 (Tenant Lifecycle) supports FR-007c and FR-013 and is cross-cutting; it is intentionally separated from US6/US7 so it can ship alongside the audit trail or be deferred without blocking the core review workflow

## Phase 12: Convergence (post-implementation)

**Purpose**: Close gaps between the implemented code and the spec/plan/tasks
after the first implementation pass. Each item is traceable to its source FR,
SC, US acceptance scenario, plan decision, or Constitution principle.

- [X] T146 Add DbSet<RiskFlag> and DbSet<Redline> to ClauseLensDbContext with EF Core mapping per FR-003, FR-004 (missing)
- [X] T147 Create IRiskFlagRepository and IRedlineRepository interfaces and EF implementations per FR-003, FR-004 (missing)
- [X] T148 Generate initial EF Core migration (InitialCreate) with all entities per plan: persistence decision (missing)
- [X] T149 Add GenerateRedlinesCommand (or extend AnalyzeContractHandler) to call IAiOrchestrator.GenerateRedlineAsync and persist Redline entities per FR-004, US3 acceptance 1 (missing)
- [X] T150 Add ExtractObligationsCommand invoked post-analysis that calls IAiOrchestrator.ExtractObligationsAsync and persists Obligation entities per FR-006, US5 (missing)
- [X] T151 Implement GetRisksQuery and GetRedlinesQuery against the new repositories (replace Array.Empty placeholders) per FR-003, FR-004, US2/US3 (partial)
- [X] T152 Fix SlaScheduler nudge window check to compare now >= assignedAt + 3 business days AND now < slaDeadline per FR-007b (partial)
- [X] T153 Resolve actual owner email in SlaScheduler and pass to SendReviewerSlaNudgeAsync (replace "<owner>" literal) per FR-007b (partial)
- [X] T154 Add LoginCommand and POST /api/v1/auth/login endpoint to issue JWT after email verification per FR-007c, US1 (missing)
- [X] T155 Add Angular login page at frontend/src/app/features/auth/login.component.ts per FR-007c (missing)
- [X] T156 Add AddSecondaryCommentCommand and POST /api/v1/contracts/{id}/review/comments for non-blocking secondary reviewer comments per FR-007a (missing)
- [X] T157 Create ErasureRequest aggregate and persist it with SlaDeadline = now + 30 days per FR-013 (missing)
- [X] T158 Implement ErasureProcessor that finds ErasureRequest rows whose SLA has elapsed and performs deletion per FR-013 (missing)
- [X] T159 Implement OffboardingScheduler tick: find SoftDeleted tenants past 30 days, call HardDelete, publish domain event per FR-013 (partial)
- [X] T160 Fix AuditEventDispatcher.GetLatestHashAsync call in system context — never pass Guid.Empty, scope to per-tenant via ITenantContext per FR-008, SC-005 (partial)
- [X] T161 Register BuiltInTemplateSeeder in InfrastructureRegistration.AddClauseLensInfrastructure per FR-010 (missing)
- [X] T162 Create frontend/src/app/features/playbooks/ and frontend/src/app/features/risks/ components per US2, T074/T075 (missing)
- [X] T163 Create frontend/src/app/features/redlines/, compare/, obligations/, review/assign/, review/decide/, audit/ components per US3-US7 (missing)
- [X] T164 Implement anti-corruption layer (IAntiCorruptionLayer) for AI provider DTO translation per Constitution Principle II (MUST) (missing)
- [X] T165 Remove hardcoded JWT secret from JwtTokenService C# fallback and appsettings.json; load exclusively from env/Key Vault per Constitution Security Gates (MUST) (partial)
- [X] T166 Wire HTTP client BaseAddress for HttpAiOrchestrator via AddHttpClient<TClient,TImplementation>(c => c.BaseAddress = ...) per plan: AI integration (partial)
- [X] T167 Add RateLimiter middleware to Program.cs with per-tenant and per-user token buckets per plan: non-functional requirements (missing)
- [X] T168 Add blob-storage volume mount in docker-compose.yml so uploaded contracts survive container restarts per US1 (partial)
- [X] T169 Fix HttpContextCurrentUser lifetime from Singleton to Scoped in InfrastructureRegistration per FR-007 (partial)
- [X] T170 Wire PlaybookTemplateSeeder as a singleton DI service in InfrastructureRegistration per FR-010 (missing)
- [X] T171 Implement RiskFlag and Redline DbSet mappings (HasKey, HasIndex on ClauseId) per FR-003, FR-004 (missing)
- [X] T172 Add IBlobStorage abstract factory that returns LocalFileSystem in Development and AzureBlob in Production per plan: storage decision (partial)
- [X] T173 Wire MassTransit outbox tables (OutboxState, OutboxMessage) into the InitialCreate migration per plan: event-driven + outbox (partial)
- [X] T174 Add explicit `using ClauseLens.Infrastructure;` and `using ClauseLens.Application.Abstractions;` to Program.cs to remove implicit-using fragility per US1 (partial)
- [X] T175 Implement OpenAPI 429 rate-limit response in api-v1.yaml and enforce in Program.cs per plan: non-functional (missing)

## Phase 13: Convergence (post-Phase-12)

**Purpose**: Close the second round of gaps surfaced after the first
convergence pass. Each item traces to its source FR, SC, US acceptance
scenario, plan decision, or Constitution principle.

- [X] T176 Persist generated Redline and Obligation entities in AnalyzeContractHandler (call _redlines.AddRangeAsync and _obligations.AddRangeAsync) per FR-004, FR-006 (partial)
- [X] T177 Implement ReviewReworkLoop workflow: RequestRevisionsCommand + ResubmitRevisedContractCommand wiring Contract transitions InReview → RevisionsRequested → ReadyForReview per US6/AC3 (missing)
- [X] T178 Add ReviewerSlaNudgedDomainEvent and raise it inside ReviewTask.RecordSlaNudge so the audit pipeline captures SLA nudges per FR-008, SC-005 (missing)
- [X] T179 Extend GetRisksQuery to join RiskFlag → PlaybookRule and return rule text fields (RuleClauseType, RuleCondition, RuleStandardLanguage) per US2/AC1 (missing)
- [X] T180 Wire IPublishEndpoint into AuditEventDispatcher (or a new DomainEventDispatcher) to enqueue every captured domain event into the MassTransit outbox per plan: event-driven + outbox (missing)
- [X] T181 Add Angular routes for /contracts/:id/review/assign and /contracts/:id/review/decide; link from ContractDetailComponent per US6 (partial)
- [X] T182 Replace IsNullOrEmpty Service Bus check in MassTransitRegistration with an explicit MassTransitOptions.Transport enum and refuse to start in Production without the chosen transport configured per plan: messaging (contradicts)
- [X] T183 Render matched PlaybookRule fields in the Angular RisksComponent once T179 lands per US2/AC1 (partial)
- [X] T184 Replace placeholder [rule:{id}] in AnalyzeContractHandler.GenerateRedline call with a real PlaybookRule.StandardLanguage lookup per FR-004, US3/AC2 (partial)
- [X] T185 Add Microsoft.Extensions.Http.Resilience retry policy to HttpAiOrchestrator (3 attempts, exponential backoff for 5xx/429) per plan: non-functional reliability (partial)
- [X] T186 Add per-tenant-upload and per-tenant-read rate-limit policies (separate limits for write vs read paths); document thresholds in OpenAPI per plan: performance (partial)
- [X] T187 Backfill 429 TooManyRequests and 401 Unauthorized response references on every controller-documented endpoint in api-v1.yaml per plan: contract-first (partial)

## Phase 14: Convergence (post-Phase-13)

**Purpose**: Final convergence pass. Constitution Security Gates
(OWASP API Top 10) and SC-005 (audit no data loss) drive the highest
priorities. Each task traces to its source FR, SC, plan decision, or
Constitution principle.

- [ ] T188 Make LoginCommand tenant-aware (require tenant id from the verification token or signup email; remove IgnoreQueryFilters cross-tenant lookup) per Constitution Security Gates + OWASP A07 (contradicts)
- [ ] T189 Switch LoginComponent from localStorage to httpOnly secure cookie (server sets Set-Cookie on /auth/login; Angular uses withCredentials: true) per Constitution Security Gates + OWASP A03 (contradicts)
- [ ] T190 Use MassTransit outbox for domain event publishing (write to OutboxMessage in the same transaction as the domain change; outbox dispatcher publishes asynchronously) per plan: event-driven + outbox, SC-005 (partial)
- [ ] T191 Add User.TokenVersion; embed in JWT; reject tokens whose tokenVersion is stale (instant revocation on Disable) per FR-007c, US6, Constitution IV (partial)
- [ ] T192 Implement EraseTenantDataAsync: anonymize PII, delete contracts, retain audit, per FR-013, SC-005 (partial)
- [ ] T189a Apply [EnableRateLimiting("per-user-login")] on AuthController.Login and AuthController.VerifyEmail per T167 (partial)
- [ ] T193 Register IConnectionMultiplexer and IDistributedCache from StackExchange.Redis; expose an ICache abstraction used by the read queries per plan: caching (partial)
- [ ] T194 Replace RedlineResult.citations rule-id placeholder with the real RuleId Guid; tighten citation_validator to require a parseable Guid per FR-004 (partial)
- [ ] T195 Document dev-secret policy in AGENTS.md; rely on the T137 gitleaks CI step to block committed secrets per Constitution Security Gates (partial)
- [ ] T196 Wrap HttpAiOrchestrator calls in try/catch with structured failure (request id + truncated response shape) per plan: observability (partial)
- [ ] T197 Add a smoke upload test (1-byte PDF) and a docker-compose volume check per US1 (partial)
- [ ] T198 Add riskFlagIds denormalized array to the OpenAPI Contract schema per plan: contract-first (partial)
