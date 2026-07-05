# Quickstart: AI-Powered Contract Review System

## Prerequisites

- .NET 10 SDK
- Python 3.12
- Node.js 22+
- Docker Desktop
- Azure CLI (for cloud deployment)
- Terraform (for IaC)

## Local Development

### Clone and Setup

```bash
git clone <repo-url>
cd ClauseLens
```

### Backend (.NET)

```bash
cd backend
dotnet restore
dotnet build
dotnet test
dotnet run --project src/ClauseLens.Api
```

API available at `http://localhost:5000` with Swagger at `/swagger`.

### Frontend (Angular)

```bash
cd frontend
npm install
ng serve
```

App available at `http://localhost:4200`.

### AI Service (Python)

```bash
cd ai-service
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
uvicorn src.api.main:app --reload
```

API available at `http://localhost:8000`.

### Full Stack with Docker

```bash
docker compose up --build
```

This starts all services plus SQL Server, Redis, and Azure AI Search emulator.

## Validation Scenarios

### Scenario 1: Upload and Segment a Contract

```bash
# Upload a PDF contract
curl -X POST http://localhost:5000/api/v1/contracts \
  -F "file=@test-contracts/sample-nda.pdf"

# Expected: 201 Created with contract ID and status "uploaded"
```

### Scenario 2: Risk Analysis

```bash
# Get risk flags (after AI analysis completes)
curl http://localhost:5000/api/v1/contracts/{contractId}/risks

# Expected: Array of RiskFlag objects with severity and rationale
```

### Scenario 3: Generate Redlines

```bash
# Get suggested redlines
curl http://localhost:5000/api/v1/contracts/{contractId}/redlines

# Expected: Array of Redline objects with suggested text and citations
```

### Scenario 4: Review Workflow

```bash
# Assign primary + optional secondary reviewers
curl -X POST http://localhost:5000/api/v1/contracts/{contractId}/review \
  -H "Content-Type: application/json" \
  -d '{
    "primaryReviewerId": "<reviewer-guid>",
    "secondaryReviewerIds": ["<advisor-guid>"]
  }'

# Expected: 200 OK, contract transitions to InReview; SLA deadline = now + 7 business days

# Approve a clause (primary reviewer)
curl -X PUT http://localhost:5000/api/v1/contracts/{contractId}/clauses/{clauseId}/decision \
  -H "Content-Type: application/json" \
  -d '{"decision": "approved", "comment": "Standard NDA language"}'

# Expected: 200 OK, clause decision recorded
```

### Scenario 5: Reviewer Reassignment After SLA

```bash
# Reassign the primary reviewer after day 7
curl -X POST http://localhost:5000/api/v1/contracts/{contractId}/review/reassign \
  -H "Content-Type: application/json" \
  -d '{
    "newPrimaryReviewerId": "<new-reviewer-guid>",
    "reason": "Original reviewer on PTO, no response after day 7"
  }'

# Expected: 200 OK, audit log contains reassignment entry with reason
```

### Scenario 6: Tenant Signup & First Admin

```bash
# Self-service signup creates the tenant and first Admin
curl -X POST http://localhost:5000/api/v1/tenants/signup \
  -H "Content-Type: application/json" \
  -d '{
    "tenantName": "Acme Legal",
    "adminEmail": "[email protected]",
    "password": "SecurePass!23"
  }'

# Expected: 201 Created, verification email sent

# Verify email
curl -X POST http://localhost:5000/api/v1/auth/verify-email \
  -H "Content-Type: application/json" \
  -d '{"token": "<email-token>"}'

# Expected: 200 OK, Admin can now log in
```

### Scenario 7: Audit Trail

```bash
# Get audit log
curl http://localhost:5000/api/v1/audit-log?contractId={contractId}

# Expected: Array of AuditEntry objects with chronological actions
```

### Scenario 8: Tenant Offboarding

```bash
# Schedule offboarding (Admin only)
curl -X POST http://localhost:5000/api/v1/tenants/{tenantId}/offboard

# Expected: 202 Accepted; status -> SoftDeleted, hard delete in 30 days
```

## Expected Outcomes

| Scenario | Success Criterion | Reference |
|----------|------------------|-----------|
| Upload + segment | Response within 2 min (50 pg doc) | SC-001 |
| Risk analysis | 90%+ recall on deviating clauses | SC-002 |
| Redlines | 80%+ acceptance rate | SC-003 |
| Full review cycle | < 30 min for 20 pg contract | SC-004 |
| Audit trail | All actions recorded with no loss | SC-005 |
| Tenant isolation | Cross-tenant access returns 404 | SC-006 |
| AI confidence | Every flag carries High/Medium/Low; Low auto-routes to Needs Discussion | FR-012 |
| Reviewer SLA | Day-3 nudge fires; day-7 reassign allowed with reason | FR-007b |
| Tenant signup | First Admin created and email-verified before first login | FR-007c |
| Offboarding | Soft delete scheduled, hard delete at +30 days | FR-013 |
