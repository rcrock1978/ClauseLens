# ClauseLens — Client & Stakeholder Presentation

**Recommended deck structure (14 slides, ~25 minutes + Q&A).**
Each section is a self-contained slide with title, body copy, and speaker notes. The layout assumes a modern 16:9 template with a navy/sky-blue palette (matches the Angular UI).

> **Visual direction:** Lots of whitespace. One idea per slide. Use real screenshots of the running product where the slide says "[Screenshot]". Use the diagrams in §4 of `CLIENT_OVERVIEW.md` (the seven-step review cycle and the rework loop) for the workflow slides.

---

## Slide 1 — Title

**Title:**
# ClauseLens

**Subtitle:**
AI-Powered Contract Review for Legal & Operations Teams

**Footer:**
Presented by: ClauseLens  ·  [Date]  ·  Confidential

**Speaker notes (30 s):**
> Welcome. Today I'm going to show you ClauseLens — an AI-powered contract review platform we built specifically for in-house legal and operations teams who handle a high volume of contracts but don't have the headcount or the budget to scale first-pass review linearly with that volume. By the end of this 25 minutes you'll see what the product does, who it serves, how it works, what it costs, and the security posture behind it.

---

## Slide 2 — The Problem

**Title:** The Hidden Cost of Manual First-Pass Review

**Body (three big numbers, big typography):**
- **$300 – $800** — average cost of a single contract first-pass review
- **2 – 5 business days** — average turnaround per contract
- **Inconsistent** — risk posture varies by reviewer, by day, by who had coffee

**Sub-bullet (small text below):**
Every contract your business signs goes through this bottleneck. NDAs, MSAs, vendor agreements, DPAs — they all queue.

**Speaker notes (45 s):**
> Before I show you the product, let me make the problem concrete. If you handle, say, 80 contracts a month and you're paying $500 per first-pass review, that's $40,000 a month — half a million dollars a year — just on the first pass. And the turnaround means your sales deals stall, your procurement onboarding slips, and your legal team is buried. But the most expensive part isn't the dollar amount — it's the inconsistency. Two reviewers, two interpretations. A clause that was approved last month gets flagged this month. And when something goes wrong, there's no record of who decided what and why.

---

## Slide 3 — What ClauseLens Does

**Title:** What ClauseLens Does

**Body (one-sentence headline):**
> ClauseLens is your firm's first-line contract reviewer — always on, always consistent, always citation-backed.

**Three-icon row beneath:**
- 🔍 **Reads** your contracts in minutes
- 🚩 **Flags** risks against your playbook
- ✏️ **Suggests** redlines with citations

**Speaker notes (45 s):**
> ClauseLens reads a contract in minutes, flags every clause that deviates from your playbook, and proposes a redline for each flag — with a citation to the exact rule that motivated the change. Your lawyers review the exceptions, not the boilerplate. And every decision is recorded in a tamper-evident audit log so you can prove what was reviewed and why.

---

## Slide 4 — Who It's For

**Title:** Who It's For

**Three columns:**

| In-House Legal | Operations & Procurement | Outside Counsel |
|---|---|---|
| SaaS, financial services, healthcare | Vendor & supplier contracts | Multi-client practice |
| 25+ contracts/month | Consistent risk bar across vendors | High-volume, low-complexity |
| Compliance & SOC 2 obligations | Procurement velocity | Client self-service portal |

**Caption beneath:** _Not designed to replace senior legal judgment on novel or high-value contracts. Designed to scale the first pass._

**Speaker notes (30 s):**
> Three personas we built for. First, in-house legal at mid-sized companies — SaaS, financial services, healthcare — anyone signing 25+ contracts a month. Second, operations and procurement teams that manage vendor contracts and need a consistent risk bar. Third, outside counsel who handle high-volume work for multiple clients. The common thread: a high volume of contracts that need consistent, defensible first-pass review.

---

## Slide 5 — End-to-End Workflow (The Seven Steps)

**Title:** How It Works — The Review Cycle

**Body:** [Insert the seven-step review cycle diagram from CLIENT_OVERVIEW.md §4.1]

**Sub-bullet (single line under the diagram):**
_A 50-page contract: uploaded, segmented, scored, and redlined within 2 minutes._

**Speaker notes (60 s):**
> Here's the flow end-to-end. The Contract Owner uploads a PDF or Word document. The system segments it into clauses — typically 30 to 80 clauses for a standard MSA. For each clause, the AI scores it against your published playbook rules and emits a risk flag with severity, confidence, the rule it violated, and a rationale. Secondary reviewers comment. The primary reviewer approves, rejects with revisions, or flags for discussion. The Owner resubmits rejected clauses with updated redlines, the reviewer re-approves, the contract closes. Every action is logged. The whole first pass is two minutes; the full review cycle is a single meeting for most contracts.

---

## Slide 6 — The Re-Work Loop (for rejected clauses)

**Title:** The Re-Work Loop

**Body:** [Insert the rework-loop diagram from CLIENT_OVERVIEW.md §4.3]

**Speaker notes (30 s):**
> When a reviewer rejects a clause with revisions, the system moves the contract to "Revisions Requested." The Owner updates the redline — either accepting the AI's suggestion or writing their own — and the contract returns to Ready for Review. The reviewer re-approves. The whole loop is auditable and re-runs cleanly.

---

## Slide 7 — The Reviewer SLA (no more silent stalls)

**Title:** Reviewers Don't Get to Stall

**Body — a 7-day timeline graphic:**

| Day | Event |
|---|---|
| **Day 0** | Contract assigned. SLA window opens. |
| **Day 3** (business) | Owner is auto-nudged: "Your reviewer hasn't moved on this." |
| **Day 7** (business) | Owner may reassign. Reason required. Logged. |

**Speaker notes (30 s):**
> Real legal review takes days. But silent stalls are unacceptable. After three business days, the Contract Owner gets an automatic nudge. After seven, they can reassign with one click and a reason — captured in the audit log. Junior reviewers can take vacations; nothing falls through the cracks.

---

## Slide 8 — Confidence Calibration (Built-In Honesty)

**Title:** Built-In Honesty — Every Flag Carries a Confidence Score

**Body (three icon callouts):**
- **High** — AI is sure; the clause clearly violates the rule
- **Medium** — likely violation; reviewer should confirm
- **Low** — borderline; auto-routes to "Needs Discussion" instead of overstating risk

**Sub-line:** _Prevents alert fatigue. Keeps reviewers focused on the high-signal items._

**Speaker notes (30 s):**
> AI isn't always right. We built calibration in. Every flag carries a confidence score. Low-confidence flags don't show up as red flags — they go to a "Needs Discussion" lane. The reviewer spends their time on the cases the AI is sure about, and the borderline cases go to a human-in-the-loop queue. We've tuned this to be honest about uncertainty, not to game the metrics.

---

## Slide 9 — Playbook Management

**Title:** Your Playbook, Not Ours

**Body (icon row + 3 lines):**
- 📚 Ship with **NDA, MSA, DPA** industry-standard templates
- ✏️ Admins edit any rule; changes go through a publish workflow
- 🔍 Every rule change is logged

**Caption:** _The AI doesn't impose our opinion on your contracts — it enforces your opinion._

**Speaker notes (30 s):**
> Your playbook is the source of truth. We ship with industry-standard templates for the most common contract types so you can be reviewing contracts in 30 minutes. But every rule is yours to edit. Admins can update severity, change standard language, add new clause types. Rule publication is a deliberate action that goes through a publish workflow, and every change is logged. The AI doesn't impose our opinion — it enforces yours.

---

## Slide 10 — Compliance & Security

**Title:** Built for Legal Work

**Body — three columns:**

| Compliance | Security | Privacy |
|---|---|---|
| GDPR-aligned (configurable retention, right-to-erasure in 30 days) | TLS 1.2+ in transit | Multi-tenant isolation (row-level security) |
| SOC 2-aligned (audit log, structured logging, distributed tracing) | Encryption at rest (Azure SQL TDE, blob AES-256) | Tenant A can never read Tenant B data |
| Append-only audit log (tamper-evident hash chain) | Email + password auth with email verification | AI never trained on your contracts |

**Footer:** _OWASP API Top 10 audited. SAST + SCA + secret-scans block insecure code from reaching production. Signed container images._

**Speaker notes (45 s):**
> We treat legal work as the regulated, high-stakes domain it is. Multi-tenant isolation is enforced at the database level — even through a crafted API call, Tenant A cannot read Tenant B. Every action is logged in a tamper-evident audit log; entries are hash-chained, so any tampering is detectable. AI training is opt-in and off by default — your contracts never train shared models. And our CI pipeline enforces build, tests, AI eval thresholds, security scans, tenant-isolation tests, and performance budgets on every change.

---

## Slide 11 — Performance & Scale

**Title:** Built for Your Volume

**Body — a table of performance numbers:**

| Metric | Target |
|---|---|
| API reads (p95) | < 200 ms |
| API writes (p95) | < 400 ms |
| 50-page contract → segmented clauses | < 2 minutes |
| AI first token | < 1.5 s |
| Grounded answer (streamed) | < 6 s |
| Risk-flagging recall | ≥ 90% |
| Redline acceptance rate | ≥ 80% |

**Caption:** _Scales horizontally on Kubernetes. Cost-controlled AI routing (cheap model first, premium only when needed)._

**Speaker notes (20 s):**
> These are the targets, not aspirations — they're enforced by our CI pipeline. The system is built for the mid-market SaaS sweet spot: 25 to 100 contracts a month per tenant, 5 to 10 concurrent users, and burstable. Cost is controlled by routing cheap models first and using premium only when the cheap model has low confidence.

---

## Slide 12 — Pricing

**Title:** Pricing

**Body — a three-tier table:**

| | Starter | Growth | Scale |
|---|---|---|---|
| Contracts / month | up to 50 | up to 250 | unlimited |
| Concurrent users | 3 | 10 | unlimited |
| Annual | $12,000 | $36,000 | Custom |

**Includes (all tiers):**
- Unlimited users (within cap)
- Industry-standard playbook templates
- Standard support (business hours, 24h response)
- Quarterly product updates

**Speaker notes (20 s):**
> Three tiers, transparent pricing. Starter is for early adoption; Growth is the mid-market sweet spot; Scale is enterprise. All tiers include the full product, all integrations on the roadmap, and standard support. The exact commercial structure will be confirmed in the Statement of Work.

---

## Slide 13 — Roadmap

**Title:** What's Next

**Body — a quarterly timeline:**

| Quarter | Capability |
|---|---|
| **MVP** (now) | Upload + segment, risk flagging, redlines, review workflow, audit log, multi-tenant |
| **Q+1** | DocuSign integration, SSO, obligation calendar |
| **Q+2** | DMS integration, playbook auto-tuning, multi-language |
| **Q+3** | Negotiation agent (counter-redlines), benchmark reports |

**Speaker notes (20 s):**
> The MVP is what you're seeing today. Q+1 is integrations — DocuSign for signature workflow, SSO for enterprise identity, an obligation calendar so deadlines don't get missed. Q+2 brings DMS integration and self-tuning playbooks — the AI gets smarter from your review history. Q+3 is a negotiation agent that suggests counter-positions to the counterparty's redlines, and benchmark reports so you can see how your review time compares to industry peers.

---

## Slide 14 — Next Steps

**Title:** Next Steps

**Body — three large CTAs:**

1. **Book a 30-minute live demo** — we'll walk through your real playbook and a sample contract
2. **Pilot program** — 30 days, 50 contracts, your playbook, your team
3. **Security review** — we'll send the SOC 2 readiness package, the architecture diagram, and the penetration-test summary

**Contact block (bottom right):**
sales@clauselens.example  ·  [Calendly link]  ·  clauselens.example

**Speaker notes (30 s):**
> Three ways to move forward. The fastest is a 30-minute live demo — bring a real playbook and a sample contract and we'll show you what the AI catches. If you want to see it in your environment, we have a 30-day pilot program: 50 contracts, your playbook, your team. And for security and procurement teams, we have a SOC 2 readiness package ready to send. Thank you. Questions?

---

# 🎨 Design Notes for the Deck Builder

**Color palette (matches the Angular UI):**
- Primary navy: `#0f172a` (backgrounds, headings)
- Surface: `#1e293b` (cards)
- Foreground: `#f8fafc` (text on dark)
- Accent sky: `#38bdf8` (CTAs, highlights)
- Accent foreground: `#0c4a6e` (text on accent)
- Status: `#22c55e` (success), `#f59e0b` (warn), `#ef4444` (danger)

**Typography:**
- Headings: a clean sans-serif (Inter, IBM Plex Sans, or the host platform's default)
- Body: same family, regular weight
- Big numbers: bold, ≥ 96pt for slide-2 hero stats

**Iconography:** Use a single line-weight icon set (Lucide or Phosphor). Avoid mixing styles.

**Charts:** Use horizontal timelines for the SLA slide and the roadmap. Use a stacked or side-by-side comparison for the pricing slide. Avoid 3D charts.

**Animation discipline:** No builds, no fly-ins. Fade between slides. Reveal bullets one at a time on the workflow diagram only.

**Photography:** Avoid stock photos. Use real product screenshots on slides 3, 5, 6, 7, 8, 9. The product UI is the most persuasive evidence.

---

# 🎤 Speaker Notes Summary (timing budget)

| Slide | Topic | Time |
|---|---|---|
| 1 | Title / welcome | 0:30 |
| 2 | The problem | 0:45 |
| 3 | What it does | 0:45 |
| 4 | Who it's for | 0:30 |
| 5 | Workflow (seven steps) | 1:00 |
| 6 | Re-work loop | 0:30 |
| 7 | Reviewer SLA | 0:30 |
| 8 | Confidence calibration | 0:30 |
| 9 | Playbook management | 0:30 |
| 10 | Compliance & security | 0:45 |
| 11 | Performance | 0:20 |
| 12 | Pricing | 0:20 |
| 13 | Roadmap | 0:20 |
| 14 | Next steps | 0:30 |
| | **Subtotal** | **7:15** |
| | Q&A (target) | 12:00 |
| | **Total** | **~20 min** |

Adjust pacing to fit a 25-, 30-, or 45-minute slot by trimming Q&A or skipping one of slides 6 / 7 / 9 if your audience is short on time.

---

# ✅ Q&A Prep — Likely Questions

| Question | Answer |
|---|---|
| _"Does this replace our lawyers?"_ | No. It's the first pass. Lawyers stay focused on the exceptions and the high-value work. |
| _"What if the AI gets it wrong?"_ | Every flag carries a confidence score; low-confidence flags go to a "Needs Discussion" lane, not a red flag. Reviewers always have the final say. |
| _"How do you handle my playbook?"_ | Your playbook is yours. We ship templates, you customize. Rule changes are logged. |
| _"What about data security?"_ | Multi-tenant isolation at the database level, encryption in transit and at rest, append-only audit log with tamper-evidence, AI never trained on your data. |
| _"What does it cost?"_ | Starter $12k/year, Growth $36k/year, Scale custom. Detailed pricing in the deck. |
| _"How long to onboard?"_ | 30 minutes for first review; 2 hours to import and customize your playbook. |
| _"What's the SLA / availability?"_ | 99.9% target in Scale tier. Standard support responds within 24h business hours. |
| _"Can we try it on our own contracts before signing?"_ | Yes — 30-day pilot, 50 contracts, your playbook, your team. |
