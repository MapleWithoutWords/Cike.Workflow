using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cike.Workflow.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class ChangeWorkflowDefinitionPublishByType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "PublishedBy",
                table: "WorkflowDefinitions",
                type: "bigint",
                nullable: false,
                comment: "发布人ID",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldComment: "发布人ID")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PublishedBy",
                table: "WorkflowDefinitions",
                type: "char(36)",
                nullable: false,
                comment: "发布人ID",
                collation: "ascii_general_ci",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "发布人ID");
        }
    }
}
