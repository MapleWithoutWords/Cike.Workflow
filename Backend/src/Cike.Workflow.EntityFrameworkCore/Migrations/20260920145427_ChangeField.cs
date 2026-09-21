using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cike.Workflow.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class ChangeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "WorkflowInstances",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "SerializedOptions",
                table: "BookmarkQueueItems",
                type: "json",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Triggers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    WorkflowDefinitionId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkflowDefinitionVersionId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivityId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hash = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    SerializedPayload = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, comment: "CreatedAt"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "CreatedBy"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "UpdatedBy")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Triggers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_CreatedBy",
                table: "Triggers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_Hash",
                table: "Triggers",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_Name",
                table: "Triggers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TenantId",
                table: "Triggers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_UpdatedBy",
                table: "Triggers",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_WorkflowDefinitionId",
                table: "Triggers",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_WorkflowDefinitionId_Hash_ActivityId_TenantId",
                table: "Triggers",
                columns: new[] { "WorkflowDefinitionId", "Hash", "ActivityId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_WorkflowDefinitionVersionId",
                table: "Triggers",
                column: "WorkflowDefinitionVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Triggers");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "WorkflowInstances");

            migrationBuilder.AlterColumn<string>(
                name: "SerializedOptions",
                table: "BookmarkQueueItems",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
