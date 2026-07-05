using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClauseLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "clauselens");

            // ── Tenants
            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SoftDeleteScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OffboardedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetentionYearsContracts = table.Column<int>(type: "int", nullable: false),
                    RetentionYearsAudit = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table => table.PrimaryKey("PK_Tenants", x => x.Id));

            // ── Users
            migrationBuilder.CreateTable(
                name: "Users",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvitedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmailVerificationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey("FK_Users_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
                });

            // ── Playbooks
            migrationBuilder.CreateTable(
                name: "Playbooks",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsTemplate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table => table.PrimaryKey("PK_Playbooks", x => x.Id));

            // ── PlaybookRules
            migrationBuilder.CreateTable(
                name: "PlaybookRules",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlaybookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClauseType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    StandardLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Guideline = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookRules", x => x.Id);
                    table.ForeignKey("FK_PlaybookRules_Playbooks_PlaybookId", x => x.PlaybookId, "Playbooks", "Id", onDelete: ReferentialAction.Cascade);
                });

            // ── Contracts
            migrationBuilder.CreateTable(
                name: "Contracts",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BlobUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table => table.PrimaryKey("PK_Contracts", x => x.Id));

            // ── Clauses
            migrationBuilder.CreateTable(
                name: "Clauses",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Heading = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ContainsNonTextualContent = table.Column<bool>(type: "bit", nullable: false),
                    SystemNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clauses", x => x.Id);
                    table.ForeignKey("FK_Clauses_Contracts_ContractId", x => x.ContractId, "Contracts", "Id", onDelete: ReferentialAction.Cascade);
                });

            // ── RiskFlags
            migrationBuilder.CreateTable(
                name: "RiskFlags",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClauseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchedRuleIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Confidence = table.Column<int>(type: "int", nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFlags", x => x.Id);
                    table.ForeignKey("FK_RiskFlags_Clauses_ClauseId", x => x.ClauseId, "Clauses", "Id", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(name: "IX_RiskFlags_ClauseId", table: "RiskFlags", schema: "clauselens", column: "ClauseId");
            migrationBuilder.CreateIndex(name: "IX_RiskFlags_RuleId",  table: "RiskFlags", schema: "clauselens", column: "RuleId");

            // ── Redlines
            migrationBuilder.CreateTable(
                name: "Redlines",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskFlagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuggestedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Citations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Confidence = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redlines", x => x.Id);
                    table.ForeignKey("FK_Redlines_RiskFlags_RiskFlagId", x => x.RiskFlagId, "RiskFlags", "Id", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(name: "IX_Redlines_RiskFlagId", table: "Redlines", schema: "clauselens", column: "RiskFlagId");

            // ── Obligations
            migrationBuilder.CreateTable(
                name: "Obligations",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClauseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsibleParty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TriggerCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obligations", x => x.Id);
                    table.ForeignKey("FK_Obligations_Clauses_ClauseId", x => x.ClauseId, "Clauses", "Id", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(name: "IX_Obligations_ClauseId", table: "Obligations", schema: "clauselens", column: "ClauseId");

            // ── ReviewTasks
            migrationBuilder.CreateTable(
                name: "ReviewTasks",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecondaryReviewerIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlaDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlaNudgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReassignedFromId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReassignmentReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewTasks", x => x.Id);
                    table.ForeignKey("FK_ReviewTasks_Contracts_ContractId", x => x.ContractId, "Contracts", "Id", onDelete: ReferentialAction.Cascade);
                });

            // ── ClauseDecisions
            migrationBuilder.CreateTable(
                name: "ClauseDecisions",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClauseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClauseDecisions", x => x.Id);
                    table.ForeignKey("FK_ClauseDecisions_ReviewTasks_ReviewTaskId", x => x.ReviewTaskId, "ReviewTasks", "Id", onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(name: "IX_ClauseDecisions_ReviewTaskId_ClauseId", table: "ClauseDecisions", schema: "clauselens", columns: new[] { "ReviewTaskId", "ClauseId" }, unique: true);

            // ── AuditEntries
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BeforeStateJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterStateJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreviousHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table => table.PrimaryKey("PK_AuditEntries", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_AuditEntries_TenantId_CreatedAt", table: "AuditEntries", schema: "clauselens", columns: new[] { "TenantId", "CreatedAt" });

            // ── ErasureRequests
            migrationBuilder.CreateTable(
                name: "ErasureRequests",
                schema: "clauselens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SlaDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table => table.PrimaryKey("PK_ErasureRequests", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_ErasureRequests_TenantId_SlaDeadline", table: "ErasureRequests", schema: "clauselens", columns: new[] { "TenantId", "SlaDeadline" });

            // ── MassTransit Outbox (T173)
            migrationBuilder.CreateTable(
                name: "OutboxState",
                schema: "clauselens",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                },
                constraints: table => table.PrimaryKey("PK_OutboxState", x => x.OutboxId));

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                schema: "clauselens",
                columns: table => new
                {
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EnqueuedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ConversationId2 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceAddress2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DestinationAddress2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutboxConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                },
                constraints: table => table.PrimaryKey("PK_OutboxMessage", x => x.Sequence));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OutboxMessage", schema: "clauselens");
            migrationBuilder.DropTable(name: "OutboxState", schema: "clauselens");
            migrationBuilder.DropTable(name: "ErasureRequests", schema: "clauselens");
            migrationBuilder.DropTable(name: "AuditEntries", schema: "clauselens");
            migrationBuilder.DropTable(name: "ClauseDecisions", schema: "clauselens");
            migrationBuilder.DropTable(name: "ReviewTasks", schema: "clauselens");
            migrationBuilder.DropTable(name: "Obligations", schema: "clauselens");
            migrationBuilder.DropTable(name: "Redlines", schema: "clauselens");
            migrationBuilder.DropTable(name: "RiskFlags", schema: "clauselens");
            migrationBuilder.DropTable(name: "Clauses", schema: "clauselens");
            migrationBuilder.DropTable(name: "Contracts", schema: "clauselens");
            migrationBuilder.DropTable(name: "PlaybookRules", schema: "clauselens");
            migrationBuilder.DropTable(name: "Playbooks", schema: "clauselens");
            migrationBuilder.DropTable(name: "Users", schema: "clauselens");
            migrationBuilder.DropTable(name: "Tenants", schema: "clauselens");
        }
    }
}
