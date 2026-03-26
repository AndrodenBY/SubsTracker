using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubsTracker.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameToIdentityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_UserGroups_UserGroupId",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "Auth0Id",
                table: "Users",
                newName: "IdentityId");
            
            migrationBuilder.AlterColumn<string>(
                name: "IdentityId",
                table: "Users",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false);

            migrationBuilder.RenameColumn(
                name: "UserGroupId",
                table: "Subscriptions",
                newName: "GroupEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_UserGroupId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_GroupEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserGroups",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subscriptions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_UserGroups_GroupEntityId",
                table: "Subscriptions",
                column: "GroupEntityId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_UserGroups_GroupEntityId",
                table: "Subscriptions");
            
            migrationBuilder.RenameColumn(
                name: "IdentityId",
                table: "Users",
                newName: "Auth0Id");

            migrationBuilder.AlterColumn<string>(
                name: "Auth0Id",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.RenameColumn(
                name: "GroupEntityId",
                table: "Subscriptions",
                newName: "UserGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_GroupEntityId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_UserGroupId");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserGroups",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subscriptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_UserGroups_UserGroupId",
                table: "Subscriptions",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }
    }
}
