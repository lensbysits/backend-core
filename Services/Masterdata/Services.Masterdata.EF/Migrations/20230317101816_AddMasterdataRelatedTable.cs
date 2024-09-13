using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lens.Services.Masterdata.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterdataRelatedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MasterdataRelated",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    ParentMasterdataId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildMasterdataId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RecordState = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterdataRelated", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterdataRelated_Masterdatas_ChildMasterdataId",
                        column: x => x.ChildMasterdataId,
                        principalTable: "Masterdatas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MasterdataRelated_Masterdatas_ParentMasterdataId",
                        column: x => x.ParentMasterdataId,
                        principalTable: "Masterdatas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MasterdataRelated_ChildMasterdataId",
                table: "MasterdataRelated",
                column: "ChildMasterdataId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterdataRelated_ParentMasterdataId_ChildMasterdataId",
                table: "MasterdataRelated",
                columns: new[] { "ParentMasterdataId", "ChildMasterdataId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterdataRelated");
        }
    }
}
