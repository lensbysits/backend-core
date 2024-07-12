using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lens.Services.Masterdata.EF.Migrations
{
    public partial class AddTagsOnMasterdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Masterdatas_MasterdataTypeId_Key",
                table: "Masterdatas");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Masterdatas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Masterdatas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "Masterdatas",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Masterdatas_MasterdataTypeId_Key",
                table: "Masterdatas",
                columns: new[] { "MasterdataTypeId", "Key" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Masterdatas_MasterdataTypeId_Key",
                table: "Masterdatas");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "Masterdatas");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Masterdatas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Masterdatas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Masterdatas_MasterdataTypeId_Key",
                table: "Masterdatas",
                columns: new[] { "MasterdataTypeId", "Key" },
                unique: true,
                filter: "[Key] IS NOT NULL");
        }
    }
}
