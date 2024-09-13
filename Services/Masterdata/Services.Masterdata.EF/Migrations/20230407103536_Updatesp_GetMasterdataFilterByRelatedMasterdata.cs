using Lens.Core.Data.EF;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lens.Services.Masterdata.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdatespGetMasterdataFilterByRelatedMasterdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RunFiles("RawSql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RevertFiles("RawSql");
        }
    }
}
