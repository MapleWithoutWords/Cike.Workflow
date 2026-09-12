using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cike.Workflow.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class UniqueWorkflowDefinitionVersionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActivityInstanceExecutionRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", maxLength: 64, nullable: false),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivityNodeId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivityType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivityTypeVersion = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasBookmarks = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AggregateFaultCount = table.Column<int>(type: "int", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SchedulingActivityExecutionId = table.Column<long>(type: "bigint", nullable: false),
                    SchedulingActivityId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SchedulingWorkflowInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    CallStackDepth = table.Column<int>(type: "int", nullable: true),
                    SerializedActivityState = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerializedException = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerializedMetadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerializedOutputs = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerializedPayload = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerializedProperties = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "CreatedAt"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "CreatedBy"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "UpdatedBy")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityInstanceExecutionRecords", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookmarkQueueItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    CorrelationId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookmarkId = table.Column<long>(type: "bigint", nullable: false),
                    StimulusHash = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivityInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityTypeName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerializedOptions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "CreatedAt"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "CreatedBy"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "UpdatedBy")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookmarkQueueItems", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hash = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    CorrelationId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerializedMetadata = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerializedPayload = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "CreatedAt"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "CreatedBy"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "UpdatedBy")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    WorkspaceId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, comment: "名称")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentId = table.Column<long>(type: "bigint", nullable: false, comment: "父级目录ID"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "CreatedAt"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "CreatedBy"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "UpdatedBy"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    WorkspaceId = table.Column<long>(type: "bigint", nullable: false),
                    FolderId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    DefinitionId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, comment: "名称")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false, comment: "描述")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UsableAsActivity = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaterializerName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, comment: "描述")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalStringData = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsReadonly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystem = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsLatest = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPublished = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PublishedNote = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false, comment: "版本说明")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublishedBy = table.Column<Guid>(type: "char(36)", nullable: false, comment: "发布人ID", collation: "ascii_general_ci"),
                    PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), comment: "发布时间"),
                    SerializedOptions = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "CreatedAt"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "CreatedBy"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "UpdatedBy"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    WorkspaceId = table.Column<long>(type: "bigint", nullable: false),
                    DefinitionId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefinitionVersionId = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ParentWorkflowInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorrelationId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsExecuting = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncidentCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SerializedWorkflowState = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "CreatedAt"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "CreatedBy"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "UpdatedBy"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Workspace",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "主键"),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, comment: "名称")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false, comment: "描述")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "CreatedAt"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "CreatedBy"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "UpdatedBy"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspace", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_ActivityId",
                table: "ActivityInstanceExecutionRecords",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_ActivityName",
                table: "ActivityInstanceExecutionRecords",
                column: "ActivityName");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_ActivityType",
                table: "ActivityInstanceExecutionRecords",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_ActivityType_ActivityTypeVe~",
                table: "ActivityInstanceExecutionRecords",
                columns: new[] { "ActivityType", "ActivityTypeVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_ActivityTypeVersion",
                table: "ActivityInstanceExecutionRecords",
                column: "ActivityTypeVersion");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_CreatedBy",
                table: "ActivityInstanceExecutionRecords",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_FinishedAt",
                table: "ActivityInstanceExecutionRecords",
                column: "FinishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_HasBookmarks",
                table: "ActivityInstanceExecutionRecords",
                column: "HasBookmarks");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_SchedulingActivityId",
                table: "ActivityInstanceExecutionRecords",
                column: "SchedulingActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_SchedulingWorkflowInstanceId",
                table: "ActivityInstanceExecutionRecords",
                column: "SchedulingWorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_Status",
                table: "ActivityInstanceExecutionRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_TenantId",
                table: "ActivityInstanceExecutionRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_UpdatedBy",
                table: "ActivityInstanceExecutionRecords",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityInstanceExecutionRecords_WorkflowInstanceId",
                table: "ActivityInstanceExecutionRecords",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_ActivityInstanceId",
                table: "BookmarkQueueItems",
                column: "ActivityInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_ActivityTypeName",
                table: "BookmarkQueueItems",
                column: "ActivityTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_BookmarkId",
                table: "BookmarkQueueItems",
                column: "BookmarkId");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_CorrelationId",
                table: "BookmarkQueueItems",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_CreatedAt",
                table: "BookmarkQueueItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_CreatedBy",
                table: "BookmarkQueueItems",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_StimulusHash",
                table: "BookmarkQueueItems",
                column: "StimulusHash");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_UpdatedBy",
                table: "BookmarkQueueItems",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkQueueItems_WorkflowInstanceId",
                table: "BookmarkQueueItems",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_ActivityInstanceId",
                table: "Bookmarks",
                column: "ActivityInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_CreatedBy",
                table: "Bookmarks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_Hash",
                table: "Bookmarks",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_Name",
                table: "Bookmarks",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_Name_Hash",
                table: "Bookmarks",
                columns: new[] { "Name", "Hash" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_Name_Hash_WorkflowInstanceId",
                table: "Bookmarks",
                columns: new[] { "Name", "Hash", "WorkflowInstanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UpdatedBy",
                table: "Bookmarks",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_WorkflowInstanceId",
                table: "Bookmarks",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_CreatedBy",
                table: "Folders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_IsDeleted",
                table: "Folders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentId",
                table: "Folders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_TenantId",
                table: "Folders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_UpdatedBy",
                table: "Folders",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_CreatedBy",
                table: "WorkflowDefinitions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_DefinitionId_Version_IsDeleted",
                table: "WorkflowDefinitions",
                columns: new[] { "DefinitionId", "Version", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_FolderId",
                table: "WorkflowDefinitions",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_IsDeleted",
                table: "WorkflowDefinitions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_IsLatest",
                table: "WorkflowDefinitions",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_IsPublished",
                table: "WorkflowDefinitions",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_IsSystem",
                table: "WorkflowDefinitions",
                column: "IsSystem");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_Name",
                table: "WorkflowDefinitions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_TenantId",
                table: "WorkflowDefinitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_UpdatedBy",
                table: "WorkflowDefinitions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_UsableAsActivity",
                table: "WorkflowDefinitions",
                column: "UsableAsActivity");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_Version",
                table: "WorkflowDefinitions",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CorrelationId",
                table: "WorkflowInstances",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CreatedBy",
                table: "WorkflowInstances",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_FinishedAt",
                table: "WorkflowInstances",
                column: "FinishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_IsDeleted",
                table: "WorkflowInstances",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_IsExecuting",
                table: "WorkflowInstances",
                column: "IsExecuting");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_Name",
                table: "WorkflowInstances",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_Status",
                table: "WorkflowInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId",
                table: "WorkflowInstances",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_Status",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_Status_Version",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "Status", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_UpdatedBy",
                table: "WorkflowInstances",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_Code",
                table: "Workspace",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_Code_Name",
                table: "Workspace",
                columns: new[] { "Code", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_CreatedBy",
                table: "Workspace",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_IsDeleted",
                table: "Workspace",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_TenantId",
                table: "Workspace",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_UpdatedBy",
                table: "Workspace",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityInstanceExecutionRecords");

            migrationBuilder.DropTable(
                name: "BookmarkQueueItems");

            migrationBuilder.DropTable(
                name: "Bookmarks");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "Workspace");
        }
    }
}
