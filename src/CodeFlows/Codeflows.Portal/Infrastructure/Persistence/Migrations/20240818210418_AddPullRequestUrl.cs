using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codeflows.Portal.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPullRequestUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PullRequestUrl",
                table: "RefactorRuns",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PullRequestUrl",
                table: "RefactorRuns");
        }
    }
}
