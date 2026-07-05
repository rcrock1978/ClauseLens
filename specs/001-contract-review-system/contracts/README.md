# Contracts: AI-Powered Contract Review System

This directory defines the interface contracts that enable parallel work
across the .NET backend, Angular frontend, and Python AI service.

## Contract Types

### REST API (OpenAPI)

The backend exposes versioned REST endpoints documented via OpenAPI/Swagger.
See `api-v1.yaml` for the full specification.

### Integration Events

Events published via MassTransit (Azure Service Bus / RabbitMQ). Schemas
are defined as .NET records shared across services.

### MCP Tools

The Python AI service exposes MCP tools for agent use:

- `segment_contract` — Segment a contract into clauses
- `match_playbook` — Match clauses against playbook rules
- `suggest_redline` — Generate a redline suggestion for a flagged clause
- `extract_obligations` — Extract obligations from a contract
