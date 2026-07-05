<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at /Users/rcrock1978/Documents/PROJECTS/Portfolio_014/ClauseLens/specs/001-contract-review-system/plan.md
<!-- SPECKIT END -->

# ClauseLens — Agent Guide

## Project state

Greenfield spec-driven development (SDD) project using **SpecKit v0.11.6** + **OpenCode**. No application code exists yet — the project is in the specification phase.

## SDD lifecycle

```
speckit.specify → [review gate] → speckit.plan → [review gate] → speckit.tasks → speckit.implement
```

Each gate is an approve/reject prompt. Rejecting aborts the workflow.

## Commands

All `speckit.*` commands use dot notation — invoke as `/speckit.specify`, `/speckit.plan`, etc. Command instructions are in `.opencode/commands/`.

| Command | What it does |
|---------|-------------|
| `speckit.specify` | Creates `specs/<NNN>-<name>/spec.md` |
| `speckit.plan` | Generates `plan.md`, `data-model.md`, `contracts/`, `research.md`, `quickstart.md`; updates AGENTS.md markers |
| `speckit.tasks` | Decomposes plan into `tasks.md` |
| `speckit.implement` | Executes tasks phase-by-phase from `tasks.md` |
| `speckit.clarify` | Clarify ambiguous specs |
| `speckit.analyze` | Analyze existing specs |
| `speckit.checklist` | Generate checklist artifacts |
| `speckit.constitution` | Fill/review `constitution.md` |
| `speckit.converge` | Converge on decisions |
| `speckit.taskstoissues` | Convert tasks to GitHub issues |
| `speckit.agent-context.update` | Refresh AGENTS.md managed section |

## Agent context mechanism

The `<!-- SPECKIT START -->` / `<!-- SPECKIT END -->` markers are managed by the `agent-context` extension (config: `.specify/extensions/agent-context/agent-context-config.yml`). It auto-updates after `speckit.specify` and `speckit.plan`. **Always read the referenced plan file** when starting work on a feature.

## Conventions

- Specs live in `specs/<NNN>-<short-name>/`
- `.specify/memory/constitution.md` is **unfilled** — complete via `speckit.constitution` before implementation
- All commands use dot notation (`speckit.*`)
- Project root is `ClauseLens/` (not the portfolio root)
- The `.opencode/.gitignore` ignores `node_modules/`, `package.json`, `package-lock.json` — these are managed by Speckit installation

## Setup prerequisites

- **Python 3** with `pyyaml` required for agent-context scripts (`.specify/extensions/agent-context/scripts/`)
- Run `.specify/scripts/bash/check-prerequisites.sh` to verify an environment

## Target architecture (from PRD)

**.NET 10 / C# 14 + Python 3.12 AI services.** Clean Architecture, DDD, CQRS, MediatR, Event-Driven, Outbox. Services: Document Intake, Clause Analysis, Playbook & Rules, Review Workflow, Audit, AI/Inference (Python). Persistence: SQL Server + Azure AI Search via EF Core, Redis. Messaging: Azure Service Bus / RabbitMQ (MassTransit). AI: Semantic Kernel .NET + LangChain/LlamaIndex Python, RAG, MCP, vector search. Deployment: Docker → AKS, Terraform IaC, GitHub Actions CI/CD.
