using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.Migrations
{
    public partial class AddCompanyRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "ID", "City", "Name", "PhoneNumbeer", "PostalCode", "State", "StreetAddress" },
                values: new object[] { 1, "tech City", "Tech Soluation", "121233213", "12121", "Il", "123 tech St" });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "ID", "City", "Name", "PhoneNumbeer", "PostalCode", "State", "StreetAddress" },
                values: new object[] { 2, "vid City", "Vivid Book", "9823323", "33231", "Il", "999 vid St" });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "ID", "City", "Name", "PhoneNumbeer", "PostalCode", "State", "StreetAddress" },
                values: new object[] { 3, "Lala Land", "Readers Club", "75634826", "556653", "Ny", "999 Main St" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "ID",
                keyValue: 3);
        }
    }
}
