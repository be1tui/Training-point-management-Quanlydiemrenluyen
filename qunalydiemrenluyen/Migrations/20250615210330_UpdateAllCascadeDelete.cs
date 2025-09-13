using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QUANLYDIEMRENLUYEN.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAllCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Class_ClassId",
                table: "Account");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_Faculty_FacultyId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLog_Account_AccountId",
                table: "SystemLog");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "MeetingNotification",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingNotification_AccountId",
                table: "MeetingNotification",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Class_ClassId",
                table: "Account",
                column: "ClassId",
                principalTable: "Class",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Faculty_FacultyId",
                table: "Class",
                column: "FacultyId",
                principalTable: "Faculty",
                principalColumn: "FacultyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingNotification_Account_AccountId",
                table: "MeetingNotification",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLog_Account_AccountId",
                table: "SystemLog",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Class_ClassId",
                table: "Account");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_Faculty_FacultyId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingNotification_Account_AccountId",
                table: "MeetingNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLog_Account_AccountId",
                table: "SystemLog");

            migrationBuilder.DropIndex(
                name: "IX_MeetingNotification_AccountId",
                table: "MeetingNotification");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "MeetingNotification");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Class_ClassId",
                table: "Account",
                column: "ClassId",
                principalTable: "Class",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Faculty_FacultyId",
                table: "Class",
                column: "FacultyId",
                principalTable: "Faculty",
                principalColumn: "FacultyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLog_Account_AccountId",
                table: "SystemLog",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
