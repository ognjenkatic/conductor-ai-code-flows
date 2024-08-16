using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codeflows.Portal.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRunNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WorkflowId",
                table: "RefactorRuns",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "RefactorRuns",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "RefactorRuns");

            migrationBuilder.AlterColumn<string>(
                name: "WorkflowId",
                table: "RefactorRuns",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
