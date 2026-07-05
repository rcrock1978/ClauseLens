# Data Model: AI-Powered Contract Review System

## Entities

### Contract

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| TenantId | Guid | Tenant isolation key (RLS) |
| OwnerId | Guid | User who uploaded the contract |
| FileName | string | Original uploaded filename |
| FileSize | long | Size in bytes |
| FileFormat | string | pdf or docx |
| Status | ContractStatus | Uploaded → Analyzing → ReadyForReview → InReview → Reviewed |
| CreatedAt | DateTime | Upload timestamp |
| UpdatedAt | DateTime | Last modification timestamp |

**ContractStatus** enum: `Uploaded`, `Analyzing`, `ReadyForReview`, `InReview`, `Reviewed`

### Clause

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| ContractId | Guid | FK → Contract |
| Index | int | Sequential position in document |
| Heading | string | Clause heading (if detectable) |
| Text | string | Extracted clause text |
| CreatedAt | DateTime | |

### Playbook

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| TenantId | Guid | Tenant isolation key |
| Name | string | Playbook name |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

### PlaybookRule

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| PlaybookId | Guid | FK → Playbook |
| ClauseType | string | Type of clause this rule applies to (e.g., "indemnification", "confidentiality") |
| Condition | string | Rule condition (e.g., "unlimited liability", "perpetual term") |
| Severity | RiskSeverity | low, medium, high, critical |
| StandardLanguage | string | Accepted standard wording |
| Guideline | string | Explanation of why this rule exists |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

**RiskSeverity** enum: `Low`, `Medium`, `High`, `Critical`

### RiskFlag

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| ClauseId | Guid | FK → Clause |
| RuleId | Guid | FK → PlaybookRule — primary matching rule |
| MatchedRuleIds | List\<Guid\> | All matching PlaybookRule IDs (per FR-009a) |
| Severity | RiskSeverity | Highest severity among matching rules (per FR-009a) |
| Confidence | ConfidenceLevel | High, Medium, Low — drives routing |
| Rationale | string | LLM-generated explanation |
| CreatedAt | DateTime | |
| ResolvedAt | DateTime? | When resolved (if applicable) |

**ConfidenceLevel** enum: `High`, `Medium`, `Low` — a RiskFlag with
`Low` confidence causes the owning Clause to be auto-routed to
`NeedsDiscussion` state (per FR-012). When more than 3 rules match
a single clause, a warning is logged (per FR-009a).

### Redline

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| RiskFlagId | Guid | FK → RiskFlag |
| SuggestedText | string | Revised wording |
| Rationale | string | Why this change is recommended, with playbook citations |
| Confidence | ConfidenceLevel | High, Medium, Low — propagated from RiskFlag |
| Status | RedlineStatus | Pending, Accepted, Rejected |
| CreatedAt | DateTime | |
| ResolvedAt | DateTime? | |

**RedlineStatus** enum: `Pending`, `Accepted`, `Rejected`

### Obligation

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| ClauseId | Guid | FK → Clause |
| Description | string | What must be done |
| ResponsibleParty | string | Which party bears the obligation |
| DueDate | DateTime? | If a specific deadline exists |
| TriggerCondition | string | Event that triggers the obligation |
| CreatedAt | DateTime | |

### ReviewTask

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| ContractId | Guid | FK → Contract |
| PrimaryReviewerId | Guid | FK → User — the decider (per FR-007a) |
| SecondaryReviewerIds | List\<Guid\> | FK → User — 0–2 comment-only reviewers |
| AssignedById | Guid | FK → User (Contract Owner) |
| AssignedAt | DateTime | When the current reviewer set was assigned |
| SlaDeadline | DateTime | AssignedAt + 7 business days |
| SlaNudgedAt | DateTime? | When the day-3 nudge fired |
| Status | ReviewTaskStatus | InProgress, Submitted, Reassigned |
| ReassignedFromId | Guid? | Previous primary Reviewer when reassigned |
| ReassignmentReason | string | Required when Status = Reassigned |
| CreatedAt | DateTime | |
| SubmittedAt | DateTime? | |

**ReviewTaskStatus** enum: `InProgress`, `Submitted`, `Reassigned`

### ClauseDecision

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| ReviewTaskId | Guid | FK → ReviewTask |
| ClauseId | Guid | FK → Clause |
| Decision | ClauseDecisionType | Approved, RejectedWithComment, NeedsDiscussion |
| Comment | string | Reviewer comment |
| CreatedAt | DateTime | |

**ClauseDecisionType** enum: `Approved`, `RejectedWithComment`, `NeedsDiscussion`

### AuditEntry

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| TenantId | Guid | Tenant isolation key |
| ContractId | Guid? | FK → Contract (optional, for contract-scoped events) |
| ActorId | Guid | User who performed the action |
| ActionType | string | e.g., "contract.uploaded", "riskflag.created", "redline.accepted" |
| BeforeState | string | JSON snapshot of state before the change |
| AfterState | string | JSON snapshot of state after the change |
| CreatedAt | DateTime | |

### Tenant

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| Name | string | Tenant name |
| Status | TenantStatus | Active, SoftDeleted, HardDeleted |
| SoftDeleteScheduledAt | DateTime? | When hard delete will occur (30 days after offboarding) |
| RetentionYearsContracts | int | Default 7 — per-tenant retention for contracts/audit |
| RetentionYearsAudit | int | Default 7 — per-tenant retention for audit log |
| CreatedAt | DateTime | |
| OffboardedAt | DateTime? | When the tenant initiated offboarding |

**TenantStatus** enum: `Active`, `SoftDeleted`, `HardDeleted`

### User

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Primary key |
| TenantId | Guid | Tenant isolation key |
| Email | string | Unique within tenant |
| EmailVerifiedAt | DateTime? | Required before first login (per FR-007c) |
| Role | UserRole | Admin, ContractOwner, Reviewer |
| Status | UserStatus | PendingInvite, Active, Disabled |
| CreatedAt | DateTime | |
| InvitedById | Guid? | Null only for the tenant's first Admin (self-signup) |

**UserRole** enum: `Admin`, `ContractOwner`, `Reviewer`
**UserStatus** enum: `PendingInvite`, `Active`, `Disabled`

## Relationships

```
Tenant (1) ──→ (N) User
Tenant (1) ──→ (N) Contract
Tenant (1) ──→ (N) Playbook
Tenant (1) ──→ (N) AuditEntry

Contract (1) ──→ (N) Clause
Contract (1) ──→ (N) ReviewTask
Contract (1) ──→ (N) AuditEntry

Clause (1) ──→ (N) RiskFlag
Clause (1) ──→ (N) Obligation
Clause (1) ──→ (N) ClauseDecision

RiskFlag (1) ──→ (0..1) Redline

Playbook (1) ──→ (N) PlaybookRule
PlaybookRule (1) ──→ (N) RiskFlag

ReviewTask (1) ──→ (N) ClauseDecision
ReviewTask (1) ──→ (1) Contract
User (1) ──→ (0..N) ReviewTask (as PrimaryReviewerId)
User (1) ──→ (0..N) User (as InvitedById)
Tenant (1) ──→ (0..N) User (as InvitedById, for the first Admin)
```

## State Machines

### Contract Status

```
Uploaded → Analyzing → ReadyForReview → InReview → Reviewed
                                    ↑         │
                                    └─ RevisionsRequested ──┘
```

The `RevisionsRequested` state is triggered by reviewer rejection
on one or more clauses (per US6 acceptance scenario 3) and returns
the contract to `ReadyForReview` once the Owner supplies updated
redlines for the rejected clauses.

### Clause Decision (per clause within a review task)

```
Pending → Approved
        → RejectedWithComment (→ rework loop: new redline → re-review)
        → NeedsDiscussion
```

## Validation Rules

- Contract file must be PDF or DOCX, ≤ 25 MB
- PlaybookRule.ClauseType must be a non-empty string ≤ 100 chars
- RiskFlag.Severity defaults to the matched PlaybookRule.Severity
- RiskFlag.Confidence MUST be High, Medium, or Low (per FR-012)
- A Clause whose only RiskFlag has Low confidence is auto-routed to
  NeedsDiscussion state
- Redline.SuggestedText must be non-empty
- ReviewTask.PrimaryReviewerId is required; SecondaryReviewerIds
  has 0–2 entries (per FR-007a)
- ReviewTask.ReassignmentReason is required when Status = Reassigned
  (per FR-007b)
- User.EmailVerifiedAt MUST be non-null before User.Status = Active
  (per FR-007c)
- The first User in a Tenant MUST have InvitedById = null (self-signup)
- AuditEntry.BeforeState and AfterState are JSON; one may be null for creation/deletion
- TenantId is required on all tenant-scoped entities (non-nullable)
- Tenant offboarding: Status → SoftDeleted, SoftDeleteScheduledAt = now+30d;
  a background job hard-deletes when the scheduled time arrives
  (per FR-013)
- GDPR right-to-erasure within 30 days applies per User, Contract, and
  AuditEntry subject (per FR-013)
