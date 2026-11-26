using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiloShop.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261126115429_StorefrontMerchandising : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "HomepagePlacement",
            table: "Reviews",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "HomepagePlacement",
            table: "Products",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "Icon",
            table: "Categories",
            type: "nvarchar(24)",
            maxLength: 24,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "HomepagePlacement",
            table: "Reviews");

        migrationBuilder.DropColumn(
            name: "HomepagePlacement",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Icon",
            table: "Categories");
    }
}
