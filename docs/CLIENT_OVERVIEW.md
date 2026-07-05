# ClauseLens — AI-Powered Contract Review

**For Legal Teams, Operations Leaders, and Business Stakeholders**

---

## 1. What is ClauseLens?

ClauseLens is a cloud-based, AI-powered contract review platform that reads your contracts, identifies risky clauses, compares them against your firm's playbook, and proposes rewrites with citations — in minutes instead of days.

It is purpose-built for small and mid-sized legal and operations teams who handle high volumes of NDAs, MSAs, DPAs, vendor agreements, and similar contracts but cannot afford the cost or turnaround of manual first-pass review by a senior attorney.

> **One-line description:** _ClauseLens is your firm's first-line contract reviewer — always on, always consistent, always citation-backed._

---

## 2. The Problem We Solve

### The Current State (Without ClauseLens)

| Pain Point | What It Costs You |
|---|---|
| Manual review of every contract by a senior lawyer | $300–$800 per contract, 2–5 business days per cycle |
| Inconsistent interpretation of "what's risky" across reviewers | Rework, missed risks, post-signature disputes |
| "Redline round-trips" between legal, sales, procurement | Deals stall; revenue is delayed |
| No visibility into _which_ clauses historically caused problems | The same mistakes get made on the next contract |
| Compliance teams (GDPR, SOC 2) cannot prove review happened | Audit findings, regulatory risk |
| Knowledge is in senior partners' heads — junior staff can't scale the practice | Hiring bottlenecks, partner burnout |

### What ClauseLens Changes

- **Minutes instead of days.** A 20-page contract is segmented, scored, and redlined within two minutes of upload.
- **Consistent risk posture.** Every contract is checked against the same playbook rules.
- **Defensible decisions.** Every flag and suggested redline is backed by a citation to a specific playbook rule.
- **Audit-ready.** Every action — upload, analysis, decision, redline — is recorded in a tamper-evident log.
- **Junior-staff leverage.** Associates and ops staff can run the first pass; partners only need to review the exceptions.

---

## 3. Who Is It For?

ClauseLens is designed for:

- **In-house legal teams** at SaaS companies, financial services firms, healthcare organizations, and any business that signs 25+ contracts per month.
- **Operations & procurement teams** who manage vendor contracts and need a consistent risk bar.
- **Outside counsel** handling high-volume, low-complexity contract review for multiple clients.
- **Compliance teams** that need an auditable review record for SOC 2, GDPR, and internal policy enforcement.

It is **not** designed to replace senior legal judgment on novel, high-value, or litigation-prone contracts. It is the first-pass reviewer that surfaces what a human should look at — and what they can confidently approve without further review.

---

## 4. How It Works — End-to-End Workflow

### 4.1 The Seven-Step Review Cycle

```
┌────────────────────────────────────────────────────────────────────────────┐
│  1. UPLOAD           2. SEGMENT         3. ANALYZE         4. FLAG         │
│  ┌──────────┐         ┌──────────┐        ┌──────────┐        ┌──────────┐   │
│  │  PDF or  │  ───▶   │  Split   │  ───▶  │  Match   │  ───▶  │  Apply   │   │
│  │  DOCX    │         │  into    │        │  each    │        │  per-rule│   │
│  │  file    │         │  clauses │        │  clause  │        │  severity│   │
│  └──────────┘         └──────────┘        │  to your │        │  + AI    │   │
│                                           │  playbook│        │  confidence│ │
│                                           └──────────┘        └──────────┘   │
│                                                                        │   │
│  7. AUDIT  ◀────  6. SUBMIT  ◀────  5. DECIDE  ◀────── ─ ─ ─ ─ ─ ─ ─   │
│  ┌──────────┐     ┌──────────┐     ┌──────────┐                            │
│  │ Tamper-  │     │  Owner   │     │  Primary │                            │
│  │ evident  │     │  closes  │     │  Reviewer│                            │
│  │ log of   │     │  the     │     │  approves│                            │
│  │ every    │     │  contract│     │  / rejects│                           │
│  │ action   │     │          │     │  per     │                            │
│  └──────────┘     └──────────┘     │  clause  │                            │
│                                     └──────────┘                            │
└────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Detailed Walkthrough

**Step 1 — Upload (≤ 2 minutes for a 50-page contract)**
The Contract Owner uploads a PDF or DOCX from the web app. The system validates the file (correct format, ≤ 25 MB, ≤ 50 pages) and rejects password-protected or encrypted files with a clear error.

**Step 2 — Segment**
ClauseLens splits the document into individual clauses (numbered sections, articles, or paragraphs), preserving the original text and position so the reviewer can navigate back to the source.

**Step 3 — Analyze (per clause)**
For each clause, the AI compares the language to your firm's published playbook rules. The system uses both:
- **Lexical + vector search** to find rules that may apply, even when the exact wording differs.
- **A large language model** to reason about whether the clause deviates from the rule.

**Step 4 — Flag**
Every clause that deviates from a rule gets a **RiskFlag** with:
- **Severity** (Low / Medium / High / Critical)
- **Confidence score** (High / Medium / Low)
- **A citation to the specific rule** it violated
- **A rationale** explaining the deviation in plain English

If multiple rules apply to the same clause, the system aggregates them into a single flag (with the highest severity surfaced) — avoiding alert fatigue.

**Step 5 — Decide**
The primary Reviewer works through the clauses:
- **Approve** — clause is fine as written
- **Reject with revisions** — clause needs to be changed; the AI suggests a redline (see below)
- **Needs Discussion** — the clause is ambiguous or unusual; flag for partner review

Secondary reviewers (up to two per contract) can post comments but cannot block or override the primary reviewer.

**Step 6 — Submit**
When all clauses are decided, the primary Reviewer submits. The Contract Owner sees a final summary and the contract status moves to **Reviewed**.

If a reviewer rejects with revisions, the Owner supplies updated redlines and the contract returns to **Ready for Review** for a re-review pass — this is the **rework loop**.

**Step 7 — Audit**
Every action — upload, AI analysis, reviewer decision, redline accept/reject, comment, reassignment — is recorded in a tamper-evident audit log keyed by tenant, with timestamps, actors, and before/after state snapshots. The log is filterable by contract, action type, actor, and date range.

### 4.3 The Re-Work Loop (for rejected clauses)

```
Reviewer rejects clause "X" with comment "Cap liability to 12 months fees"
                       │
                       ▼
Contract status → RevisionsRequested
                       │
                       ▼
Owner supplies updated redline (or accepts the AI's suggested one)
                       │
                       ▼
Clause returns to Reviewer for re-approval
                       │
                       ▼
On full re-approval, contract → Reviewed
```

### 4.4 The Reviewer SLA (preventing bottlenecks)

Real legal review legitimately takes days. But silent stalls are unacceptable.

| Day | What Happens |
|---|---|
| Day 0 | Contract assigned to a primary Reviewer. SLA window opens. |
| Day 3 (business) | The Contract Owner receives an automatic nudge: "Your reviewer hasn't moved on this yet." |
| Day 7 (business) | The Owner may reassign the contract to a new primary Reviewer. A reason must be provided and is recorded in the audit log. |

---

## 5. Key Capabilities

### 5.1 Playbook Management

A **playbook** is your firm's codified risk policy — a collection of rules like:
- _"Liability clauses must cap exposure at 12 months of fees paid."_
- _"Indemnification must exclude consequential damages."_
- _"Sub-processors must be disclosed in Annex II."_

ClauseLens ships with **industry-standard templates** (NDA, MSA, DPA) that you can import as a starting point. Every rule change goes through a **publish workflow** — only Admins can publish a rule change, and every publication is logged.

### 5.2 Redline Generation

For every flagged clause, the AI produces a **suggested redline** — a specific rewrite that aligns the clause to your playbook. The redline includes:
- The proposed text
- A rationale explaining the change
- A citation to the rule that motivated the change

The reviewer can accept, reject, or edit the suggestion.

### 5.3 Confidence Calibration (Built-In Uncertainty)

Not every AI judgment is equally reliable. ClauseLens tags every flag with a confidence level:
- **High** — the AI is sure; the clause clearly violates the rule
- **Medium** — likely violation; reviewer should confirm
- **Low** — borderline; the system automatically routes to "Needs Discussion" rather than overstating the risk

This prevents false positives and keeps the reviewer focused on the high-signal items.

### 5.4 Side-by-Side Comparison

For any flagged clause, ClauseLens shows the contract text next to your firm's standard language, with differences highlighted. This builds trust by making the AI's reasoning transparent.

### 5.5 Obligation Extraction

ClauseLens automatically identifies and extracts obligations from the contract — payment terms, reporting requirements, confidentiality duties, deadlines — and shows them in a structured summary.

### 5.6 Multi-Tenant Isolation

Each customer (tenant) sees only their own contracts, playbooks, and users. Tenant data is separated at the database level via row-level security; a Tenant A user can never read Tenant B data, even through a crafted API call.

### 5.7 Compliance & Data Lifecycle

- **GDPR-aligned**: configurable data retention, right-to-erasure workflow, data export.
- **SOC 2-aligned**: append-only audit log, structured logging, distributed tracing.
- **Default retention**: 7 years for contracts and audit log (configurable per tenant); playbooks retained while the tenant is active.
- **Offboarding**: when a tenant offboards, their data is soft-deleted for 30 days (recovery window) then permanently deleted.
- **Right-to-erasure**: GDPR requests are honored within 30 days.

---

## 6. Roles & Permissions

| Role | Can Upload | Can Edit Playbook | Can Assign Reviewers | Can Decide Clauses | Can View Audit Log | Can Manage Users |
|---|---|---|---|---|---|---|
| **Admin** | ✓ | ✓ | — | — | ✓ | ✓ |
| **Contract Owner** | ✓ | — | ✓ | — | — | — |
| **Primary Reviewer** | — | — | — | ✓ | — | — |
| **Secondary Reviewer** | — | — | — | Comments only | — | — |

The first user in a new tenant is automatically the Admin. Admins then invite Contract Owners and Reviewers via email.

---

## 7. Deployment & Integration

### 7.1 Deployment Model

- **Cloud-hosted SaaS** (Azure). Multi-region capable.
- **Browser-based**: modern web app, no install required.
- **Mobile-responsive**: works on tablets and phones for on-the-go review.
- **No external system integration required for MVP**: documents are uploaded manually. Future roadmap includes DMS, DocuSign, and SSO integrations.

### 7.2 What You Don't Need to Do

- Install any software
- Manage any servers
- Set up any databases
- Buy any licenses for AI models — included in the subscription

### 7.3 What We Need From You

- A business email to create your tenant
- Your firm's playbook rules (or use the industry-standard templates)
- A modern web browser

---

## 8. Security & Privacy

ClauseLens is built for B2B legal work and meets the standards your security team expects:

- **Encryption in transit**: TLS 1.2+ for all client-server communication.
- **Encryption at rest**: Azure SQL TDE; blob storage AES-256.
- **Authentication**: email + password (with email verification) and JWT-based session tokens.
- **Authorization**: role-based access control enforced at the API and database layers.
- **Multi-tenant isolation**: row-level security in the database; tenant key on every query.
- **Append-only audit log**: tamper-evident with cryptographic hash chaining.
- **Prompt-injection defense and PII redaction** at the AI service layer.
- **Rate limiting** per tenant and per IP to prevent abuse.
- **Secrets management**: signing keys loaded from environment / Key Vault, never committed to source.
- **Container images signed** in CI; SAST/SCA/secret-scans block insecure code from reaching production.
- **CI gates enforce** build + tests, AI eval thresholds, SAST/SCA, tenant-isolation tests, and performance budgets on every pull request.

---

## 9. Performance & Scale

Designed for the SMB multi-tenant SaaS target (25–100 contracts per month per tenant, 5–10 concurrent users):

| Metric | Target |
|---|---|
| API reads (p95) | < 200 ms |
| API writes (p95) | < 400 ms |
| 50-page contract upload → segmented clauses | < 2 minutes |
| AI first token | < 1.5 s |
| Grounded answer (streamed) | < 6 s |
| Risk-flagging recall (deviation detection) | ≥ 90% |
| Redline acceptance rate | ≥ 80% |

The platform scales horizontally via Kubernetes, with cost-controlled AI model routing (cheap models first, premium only when needed).

---

## 10. Pricing Model (Indicative)

| Tier | Contracts / Month | Concurrent Users | Price (annual) |
|---|---|---|---|
| **Starter** | up to 50 | 3 | $12,000 / year |
| **Growth** | up to 250 | 10 | $36,000 / year |
| **Scale** | unlimited | unlimited | Custom |

_(Pricing is indicative; the final commercial structure will be confirmed in the Statement of Work.)_

All tiers include:
- Unlimited users (within the concurrency cap)
- Industry-standard playbook templates
- Standard support (business hours, 24-hour response)
- Quarterly product updates

---

## 11. Roadmap

| Quarter | Capability |
|---|---|
| **MVP (now)** | Upload + segment, risk flagging, redlines, review workflow, audit log, playbook templates, multi-tenant |
| **Q+1** | DocuSign integration, SSO (SAML/OIDC), advanced obligation calendar |
| **Q+2** | DMS integration (iManage, NetDocuments), playbook auto-tuning from past decisions, multi-language support |
| **Q+3** | Negotiation agent (suggests counter-positions on the counterparty's redlines), benchmark reports ("how does our review time compare to industry?") |

---

## 12. Frequently Asked Questions

**Q: Does ClauseLens replace our lawyers?**
A: No. ClauseLens is the first-pass reviewer. It surfaces what a human should look at and what they can confidently approve. Senior lawyers stay focused on the exceptions and the high-value work.

**Q: How accurate is the AI?**
A: We target ≥ 90% recall on deviation detection and ≥ 80% redline acceptance rate. Every flag carries an explicit confidence score, and low-confidence flags are routed to "Needs Discussion" rather than overstating risk.

**Q: Can I trust the suggested redlines?**
A: Every redline is required to cite the specific playbook rule that motivated the change. Citations are validated before display; the rationale cannot be presented without a backing rule.

**Q: What happens to my data if I cancel?**
A: Your data is soft-deleted for 30 days (recovery window) then permanently deleted. You can export all your data before cancellation.

**Q: Can I bring my own playbook?**
A: Yes. You can edit any rule, add your own rules, or import the industry-standard templates and customize.

**Q: Is my data used to train AI models?**
A: No. Your contracts are never used to train shared AI models.

**Q: How long does it take to onboard?**
A: A typical tenant is reviewing contracts within 30 minutes of signup. Plan for ~2 hours to import and customize your playbook.

**Q: What happens if the AI service is down?**
A: The system degrades gracefully — uploads still succeed (clauses are still segmented), and analysis resumes when the AI service recovers. Reviewers can still approve clauses that don't need AI input.

---

## 13. Glossary

- **Clause** — A segmented portion of a contract (e.g., "Section 5.2 — Limitation of Liability").
- **Playbook** — Your firm's codified contract-review policy.
- **Rule** — A specific clause-type + condition + standard-language pattern (e.g., "For 'liability' clauses, the language 'unlimited liability' is Critical-severity deviation from 'Liability shall be capped at 12 months of fees.'")
- **RiskFlag** — A record marking a clause as deviating from one or more rules.
- **Redline** — A suggested rewrite for a flagged clause.
- **Reviewer** — A user assigned to decide on flagged clauses.
- **SLA** — Service Level Agreement; here, the 7-business-day window for a reviewer to act.
- **Tenant** — An isolated customer organization in the multi-tenant platform.

---

## 14. Contact

For questions, demos, or onboarding: **sales@clauselens.example**

For support: **support@clauselens.example**

For security inquiries: **security@clauselens.example**

---

_Copyright © 2026 ClauseLens. All rights reserved. This document is provided for informational purposes and does not constitute a binding agreement. The capabilities, pricing, and roadmap described herein are subject to change._
