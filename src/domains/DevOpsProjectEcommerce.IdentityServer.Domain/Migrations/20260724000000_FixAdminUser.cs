using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevOpsProjectEcommerce.IdentityServer.Domain.Migrations
{
    /// <inheritdoc />
    /// Updates the seeded admin user from the original project email (admin@nsse.com)
    /// to the correct project email (admin@ecommerce-devopsproject.com).
    /// Password: DevOpsNaNuvem1! (SHA256 via Encoding.UTF8.GetString, no NUL bytes)
    public partial class FixAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Password" },
                values: new object[]
                {
                    "admin@ecommerce-devopsproject.com",
                    "*\ufffd\u001d\u0017\u000dJ\ufffdi\ufffd\u00077\ufffdmO\u0002Bz\ufffdy\u0008\ufffd\ufffd\u0015\ufffd\ufffd\ufffd#!'\ufffd"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Password" },
                values: new object[]
                {
                    "admin@nsse.com",
                    "yJ\ufffdw\ufffdflVG6\ufffd@85F\ufffdQ\ufffdS\ufffd0\ufffdC\ufffds'P\ufffd\ufffd"
                });
        }
    }
}
