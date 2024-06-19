using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finite_State_Machine_Designer.Migrations
{
    /// <inheritdoc />
    public partial class FsmModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StateMachines",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(36)", fixedLength: true, maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    TransitionSearchRadius = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StateMachines_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(36)", fixedLength: true, maxLength: 36, nullable: false),
                    IsDrawable = table.Column<bool>(type: "bit", nullable: false),
                    Radius = table.Column<float>(type: "real", nullable: false),
                    IsFinalState = table.Column<bool>(type: "bit", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FiniteStateMachineId = table.Column<string>(type: "nchar(36)", nullable: false),
                    Coordinate_X = table.Column<double>(type: "float", nullable: false),
                    Coordinate_Y = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.Id);
                    table.ForeignKey(
                        name: "FK_States_StateMachines_FiniteStateMachineId",
                        column: x => x.FiniteStateMachineId,
                        principalTable: "StateMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(36)", fixedLength: true, maxLength: 36, nullable: false),
                    FromStateId = table.Column<string>(type: "nchar(36)", nullable: true),
                    ToStateId = table.Column<string>(type: "nchar(36)", nullable: true),
                    SelfAngle = table.Column<double>(type: "float", nullable: false),
                    ParallelAxis = table.Column<double>(type: "float", nullable: false),
                    PerpendicularAxis = table.Column<double>(type: "float", nullable: false),
                    MinPerpendicularDistance = table.Column<double>(type: "float", nullable: false),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    Radius = table.Column<double>(type: "float", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FiniteStateMachineId = table.Column<string>(type: "nchar(36)", nullable: false),
                    CenterArc_X = table.Column<double>(type: "float", nullable: false),
                    CenterArc_Y = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transitions_StateMachines_FiniteStateMachineId",
                        column: x => x.FiniteStateMachineId,
                        principalTable: "StateMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transitions_States_FromStateId",
                        column: x => x.FromStateId,
                        principalTable: "States",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transitions_States_ToStateId",
                        column: x => x.ToStateId,
                        principalTable: "States",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StateMachines_ApplicationUserId",
                table: "StateMachines",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_States_FiniteStateMachineId",
                table: "States",
                column: "FiniteStateMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Transitions_FiniteStateMachineId",
                table: "Transitions",
                column: "FiniteStateMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Transitions_FromStateId",
                table: "Transitions",
                column: "FromStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Transitions_ToStateId",
                table: "Transitions",
                column: "ToStateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transitions");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "StateMachines");
        }
    }
}
