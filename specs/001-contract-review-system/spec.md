# Feature Specification: AI-Powered Contract Review System

**Feature Branch**: `001-contract-review-system`

**Created**: 2026-07-01

**Status**: Draft

**Input**: User description: "AI contract review that flags risky clauses against your playbook."

## Clarifications

### Session 2026-07-01

- Q: How do tenants initially populate their playbook rules? → A: Import from standard templates — system ships with industry-standard clause templates (NDA, MSAs, etc.) that tenants can import and customize.
- Q: What states does a contract pass through during review? → A: Multi-stage with per-clause sign-off — each clause independently approved/rejected; contract is "Reviewed" when all clauses have a decision. Includes rework loops per clause.
- Q: What is explicitly out-of-scope for MVP? → A: Exclude external integrations (DMS, DocuSign, SSO provider connections) and multi-language contract support. Manual file upload only for MVP.
- Q: What are the maximum document size and page count limits for MVP? → A: 50 pages / 25 MB max. Larger documents are rejected with clear guidance.
- Q: What user roles exist and what permissions does each have? → A: Three roles — Admin (user management, playbook editing, audit access), Contract Owner (upload, assign, final decisions), Reviewer (analyze and recommend on assigned contracts).
- Q: What is the expected workload scale for MVP? → A: 25-100 contracts per month per tenant, moderate concurrency (5-10 active users).
- Q: What are the independent clause-level states? → A: Six states — Unreviewed, Compliant, Flagged, Approved, Rejected, Needs Discussion.
- Q: How does the system behave when AI analysis times out or returns low confidence? → A: Best-effort per clause — failed clauses show "Analysis Unavailable" with reason; remaining clauses proceed normally.
- Q: What compliance and regulatory frameworks apply? → A: GDPR + SOC 2 — privacy data protection and security controls as the B2B SaaS baseline.
- Q: How are conflicting reviewer decisions resolved? → A: Contract Owner decides with re-route — disagreeing reviewers provide rationale, then owner makes final call.
- Q: What confidence threshold does the AI use before flagging a clause, and what happens at low confidence? → A: Every flag carries an explicit confidence score (High/Medium/Low); clauses with Low confidence are auto-routed to "Needs Discussion" instead of "Flagged" so reviewers see uncertainty rather than a false-positive risk.
- Q: What are the data retention and tenant offboarding rules under GDPR + SOC 2? → A: Configurable per-tenant retention (default 7 years for contracts and audit log; playbook retained indefinitely while tenant is active). Tenant offboarding = 30-day soft delete then hard delete. GDPR right-to-erasure requests honored within 30 days.
- Q: Can a contract be reviewed by a panel, or is there always a single reviewer? → A: Primary reviewer (the decider) plus 0–2 secondary reviewers (comments only). The primary owns the clause decision; secondaries cannot block or override.
- Q: What happens when an assigned Reviewer is unavailable or stalls? → A: 7 business-day SLA on the primary Reviewer. At 3 business days the Contract Owner is auto-nudged; at 7 business days the Owner may reassign the contract to a new primary Reviewer with a reason that is captured in the audit log.
- Q: How is a tenant created and how does its first Admin get provisioned? → A: Self-service signup — the first Admin registers with email + password, verifies the email before first login, then invites all subsequent users in their tenant.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upload Contract and Segment Clauses (Priority: P1)

A legal or ops user uploads a contract document (PDF, DOCX) and the system
automatically segments it into individual clauses for review.

**Why this priority**: This is the entry point for the entire workflow —
without contract upload and clause segmentation, no further analysis is possible.

**Independent Test**: A user can upload a valid contract document and receive
a segmented list of clauses, each with extracted text and position metadata.

**Acceptance Scenarios**:

1. **Given** a user is authenticated, **When** they upload a PDF or DOCX
   contract document, **Then** the system processes the document and returns
   a segmented list of clauses within 2 minutes.
2. **Given** a user uploads an unsupported file format, **When** the upload
   completes, **Then** the system rejects the file with a clear error message
   explaining supported formats.
3. **Given** a contract has been uploaded, **When** the user views the result,
   **Then** each clause displays its extracted text and sequential position
   within the document.

---

### User Story 2 - Flag Risks Against Playbook (Priority: P1)

The system compares each clause against the firm's playbook standards and flags
clauses that deviate from accepted terms.

**Why this priority**: Risk flagging is the core value proposition — it replaces
manual review against inconsistent standards with automated, grounded analysis.

**Independent Test**: A user can upload a contract and receive a risk assessment
showing which clauses are flagged, why, and which playbook rule was triggered.

**Acceptance Scenarios**:

1. **Given** a contract has been segmented into clauses, **When** the risk
   analysis completes, **Then** each flagged clause displays the specific
   playbook rule it violates and a severity level.
2. **Given** a clause matches an accepted playbook standard, **When** the
   analysis completes, **Then** that clause is marked as compliant with
   a reference to the matching playbook rule.
3. **Given** the playbook has no rule for a particular clause type, **When**
   the analysis completes, **Then** that clause is marked as unreviewed with
   a suggestion to add a playbook rule.

---

### User Story 3 - Get Suggested Redlines with Rationale (Priority: P1)

For each flagged clause, the system provides a suggested redline (revised
wording) with an explanation of why the change is recommended.

**Why this priority**: Redlines turn risk flags into actionable next steps,
dramatically reducing the time from review to negotiation.

**Independent Test**: A user can view suggested redlines for any flagged
clause, each with a rationale explaining the proposed change.

**Acceptance Scenarios**:

1. **Given** a clause has been flagged as risky, **When** the user requests
   a redline suggestion, **Then** the system provides revised wording with
   a citation to the playbook rule that motivates the change.
2. **Given** a user reviews a suggested redline, **When** they view the
   rationale, **Then** the rationale includes the specific risk mitigated
   and the playbook standard applied.
3. **Given** a clause has multiple applicable playbook rules, **When** the
   system generates redlines, **Then** it produces a single suggestion that
   satisfies all applicable rules.

---

### User Story 4 - Compare Deviations from Standards (Priority: P2)

Users can view a side-by-side comparison of their contract's clauses against
the firm's standard language, highlighting deviations.

**Why this priority**: Deviation comparison builds trust by showing exactly
what changed and where, making review outcomes transparent and defensible.

**Independent Test**: A user can select a clause and see a diff-style
comparison against the firm's standard language for that clause type.

**Acceptance Scenarios**:

1. **Given** a clause is flagged as deviating from the playbook, **When**
   the user opens the comparison view, **Then** the system displays the
   contract clause alongside the recommended standard language with
   differences highlighted.
2. **Given** a clause matches the playbook standard, **When** the user
   opens the comparison view, **Then** the system confirms the clause
   matches the standard with no deviations.

---

### User Story 5 - Extract Obligations (Priority: P2)

The system identifies and extracts obligations from the contract, such as
reporting requirements, payment terms, confidentiality duties, and deadlines.

**Why this priority**: Extracted obligations give legal and ops teams a
structured summary of what the company must do, reducing the risk of
missed commitments.

**Independent Test**: A user can view an obligation summary extracted from
their contract, listing each obligation, the responsible party, and the
deadline or trigger event.

**Acceptance Scenarios**:

1. **Given** a contract has been analyzed, **When** the user views the
   obligation summary, **Then** each obligation shows the duty description,
   the party responsible, and any deadline or condition.
2. **Given** an obligation has a specific due date, **When** the user views
   the summary, **Then** the due date is displayed and the obligation is
   added to any compliance calendar.

---

### User Story 6 - Reviewer Workflow (Priority: P3)

Users can assign review tasks to team members, track review progress, and
record approval or rejection decisions for each clause.

**Why this priority**: Workflow management coordinates multi-stakeholder
review, ensuring every clause is reviewed and decisions are documented.

**Independent Test**: A user can assign a contract for review, and the
assignee can review clauses, make decisions, and submit their review.

**Acceptance Scenarios**:

1. **Given** a contract has been analyzed (state: Ready for Review),
   **When** the contract owner assigns it to a reviewer, **Then** the
   contract transitions to In Review state and the reviewer receives
   access.
2. **Given** a reviewer is reviewing a contract, **When** they make a
   decision on an individual clause, **Then** that clause is marked
   as Approved, Rejected with comment, or Needs Discussion.
3. **Given** a clause was Rejected with revisions requested, **When**
   the user provides an updated redline, **Then** that clause returns
   to the reviewer for re-approval (rework loop).
4. **Given** a reviewer has made a decision on every clause, **When**
   they submit their review, **Then** the contract transitions to
   Reviewed state and the owner sees each clause's final status.

---

### User Story 7 - Audit Trail (Priority: P3)

Every action on a contract — upload, analysis, review decision, redline
acceptance — is recorded in an append-only audit log.

**Why this priority**: An audit trail is essential for compliance, dispute
resolution, and continuous improvement of the review process.

**Independent Test**: A user can view an audit log for any contract showing
who performed what action and when.

**Acceptance Scenarios**:

1. **Given** any action is performed on a contract, **When** the action
   completes, **Then** an audit entry is created with timestamp, actor,
   action type, and before/after state.
2. **Given** a user views the audit log, **When** they filter by action
   type or actor, **Then** the log displays only matching entries.

### Edge Cases

- Password-protected or encrypted documents are rejected at upload with
  HTTP 400 and a clear error message (per FR-001a).
- Documents with embedded tables, images, or mixed formatting are
  handled per FR-002a: table text is extracted, images are skipped, and
  affected clauses are routed to `Needs Discussion` with a system note.
- An empty playbook results in all clauses being marked `Unreviewed`
  with a suggestion to import standard templates (per US2 acceptance
  scenario 3). Conflicting rules for the same clause type are resolved
  by FR-009a (single flag with highest severity, aggregated citations).
- AI analysis operates per clause: if a clause times out or returns Low
  confidence, that clause is auto-routed to `Needs Discussion` (per
  FR-012) with a system note indicating the reason; unaffected clauses
  proceed normally.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST accept contract uploads in PDF and DOCX formats.
- **FR-001a**: System MUST reject password-protected or encrypted
  PDF/DOCX files with HTTP 400 and a clear error message
  ("Password-protected documents are not supported — please remove
  the password and re-upload").
- **FR-002**: System MUST automatically segment uploaded contracts into
  individual clauses with extracted text and position metadata.
- **FR-002a**: The clause segmenter MUST extract text from simple
  embedded tables as plain text (concatenated cell content, left-to-
  right, top-to-bottom), MUST skip images entirely, and MUST mark any
  clause containing non-textual content as `Needs Discussion` with a
  system note "Contains non-textual content — manual review required".
- **FR-003**: System MUST compare each clause against the firm's playbook
  rules and flag deviations with severity levels.
- **FR-004**: System MUST generate suggested redlines for flagged clauses
  with rationales citing the applicable playbook rules.
- **FR-005**: System MUST provide a side-by-side comparison view showing
  the contract clause versus the playbook standard language.
- **FR-006**: System MUST extract obligations from contracts, identifying
  the duty, responsible party, and any deadline or trigger condition.
- **FR-007**: System MUST support three user roles — Admin (user
  management, playbook editing, audit log access), Contract Owner (upload
  contracts, assign reviewers, make final decisions), and Reviewer
  (Primary: decides per-clause Approve/Reject/Needs Discussion;
  Secondary: comments-only, non-blocking — see FR-007a).
- **FR-007a**: System MUST allow a Contract Owner to assign exactly one
  primary Reviewer (the decider) and optionally up to two secondary
  Reviewers (comments-only, non-blocking) per contract.
- **FR-007b**: System MUST enforce a 7 business-day SLA on the primary
  Reviewer: auto-nudge the Contract Owner at 3 business days, and
  allow the Owner to reassign to a new primary Reviewer at 7 business
  days with a mandatory reason recorded in the audit log.
- **FR-007c**: System MUST support self-service tenant signup where the
  first Admin registers with email + password, MUST require email
  verification before first login, and MUST allow the Admin to invite
  subsequent users within the tenant.
- **FR-008**: System MUST maintain an append-only audit log recording all
  actions performed on contracts.
- **FR-009**: System MUST persist playbooks — collections of rules defining
  acceptable and unacceptable clause language for the firm.
- **FR-009a**: When multiple PlaybookRules match a single Clause, the
  system MUST emit a single RiskFlag with the highest severity among
  the matching rules, MUST aggregate all matching rule IDs in the
  flag's `matchedRuleIds` collection, and MUST produce a single Redline
  that satisfies all applicable rules. The system MUST log a warning
  when more than three rules match a single clause.
- **FR-010**: System MUST ship with a library of industry-standard clause
  templates that tenants can import as a starting point for their playbook.
- **FR-011**: System MUST enforce tenant-level data isolation so one
  customer's contracts and playbooks are invisible to others.
- **FR-012**: System MUST emit a per-flag AI confidence score
  (High/Medium/Low) for every risk flag and redline suggestion, and
  MUST auto-route clauses with Low confidence to the "Needs Discussion"
  state rather than "Flagged".
- **FR-013**: System MUST support configurable per-tenant retention
  (default 7 years for contracts and audit log; playbook retained
  indefinitely while tenant is active), MUST soft-delete tenant data
  on offboarding for 30 days before hard delete, and MUST honor
  GDPR right-to-erasure requests within 30 days.

### Key Entities

- **Contract**: The uploaded document and its metadata (status, owner,
  upload date, tenant). State machine: Uploaded → Analyzing →
  ReadyForReview → InReview → Reviewed, with a RevisionsRequested
  back-edge from InReview to ReadyForReview triggered by reviewer
  rejection (per US6 acceptance scenario 3).
- **Clause**: A segmented portion of the contract with extracted text,
  position index, analysis results, and status (Unreviewed, Compliant,
  Flagged, Approved, Rejected, Needs Discussion).
- **Playbook**: A collection of rules defining acceptable clause language
  for a tenant, organized by clause type.
- **PlaybookRule**: A specific standard (acceptable wording, risk
  conditions, severity) for a clause type within a playbook.
- **RiskFlag**: A record linking a clause to a playbook rule it violates,
  with severity level and analysis timestamp.
- **Redline**: A suggested revision for a flagged clause with rationale
  and citations.
- **Obligation**: An extracted duty from the contract with responsible
  party, description, and deadline.
- **ReviewTask**: An assignment of a contract to a reviewer with status
  tracking and decision records.
- **AuditEntry**: An append-only record of an action performed on a
  contract.
- **User**: A tenant member with a role (Admin, ContractOwner, Reviewer),
  email-verification state, and invite/activation lifecycle.
- **Tenant**: An isolated customer organization with retention settings,
  offboarding lifecycle, and configurable compliance parameters.
- **ClauseDecision**: A reviewer's per-clause verdict (Approved,
  RejectedWithComment, NeedsDiscussion) attached to a ReviewTask.
- **PlaybookTemplate**: An industry-standard clause template (NDA, MSA,
  DPA, etc.) importable by tenants to bootstrap a Playbook.
- **ReviewerSla**: Time-bound state for a ReviewTask: assignedAt,
  slaDeadline, slaNudgedAt, reassignmentReason.
- **ConfidenceLevel**: Enum (High, Medium, Low) attached to RiskFlag
  and Redline; drives Low → NeedsDiscussion routing.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can upload a contract (up to 50 pages / 25 MB) and
  receive segmented clauses within 2 minutes.
- **SC-002**: Risk flagging covers at least 90% of clauses that deviate
  from playbook rules (high recall).
- **SC-003**: At least 80% of suggested redlines are accepted or accepted
  with minor edits by users.
- **SC-004**: Users can complete a full review cycle (upload to final
  decision) in under 30 minutes for a standard 20-page contract.
- **SC-005**: Audit log preserves all state-changing actions with no
  data loss or tampering.
- **SC-006**: Tenant A cannot access any data belonging to Tenant B under
  any condition.

## Assumptions

- Users have stable internet connectivity and a modern web browser.
- Contracts may include simple tables, images, and standard formatting —
  handling is defined in FR-002a. Handwriting, complex nested tables, and
  rich graphical content are out of scope for MVP and will be flagged for
  manual review.
- Each tenant maintains their own playbook; initial population starts from
  industry-standard clause templates (NDA, MSA, etc.) that tenants import
  and customize.
- The system is cloud-hosted (no on-premise deployment for MVP).
- English-language contracts are the primary target for MVP; additional
  language support is deferred.
- System must comply with GDPR (data privacy, right to deletion
  honored within 30 days, data portability) and SOC 2 (security
  controls, audit, availability). Default retention is 7 years for
  contracts and audit log; playbooks retained while tenant is active.
- External system integrations (DMS, DocuSign, SSO provider connections)
  are out of scope for MVP. File upload is manual via the web interface.
- Users have basic digital literacy sufficient to upload files and navigate
  a web application.
- Expected workload is 25-100 contracts per month per tenant with 5-10
  concurrent active users during business hours.
