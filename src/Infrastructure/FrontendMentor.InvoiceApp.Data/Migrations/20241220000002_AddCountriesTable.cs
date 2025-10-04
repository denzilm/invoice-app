using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrontendMentor.InvoiceApp.Data.Migrations;

/// <inheritdoc />
public partial class AddCountriesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Countries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Code = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Countries", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Countries_Code",
            table: "Countries",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Countries_Name",
            table: "Countries",
            column: "Name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Countries");
    }
}
