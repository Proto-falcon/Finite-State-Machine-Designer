using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finite_State_Machine_Designer.Migrations
{
    /// <inheritdoc />
    public partial class FiniteStateMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StateMachines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDrawable = table.Column<bool>(type: "bit", nullable: false),
                    Radius = table.Column<float>(type: "real", nullable: false),
                    IsFinalState = table.Column<bool>(type: "bit", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FiniteStateMachineId = table.Column<long>(type: "bigint", nullable: false),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transitions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SelfAngle = table.Column<double>(type: "float", nullable: false),
                    ParallelAxis = table.Column<double>(type: "float", nullable: false),
                    PerpendicularAxis = table.Column<double>(type: "float", nullable: false),
                    MinPerpendicularDistance = table.Column<double>(type: "float", nullable: false),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    Radius = table.Column<double>(type: "float", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FiniteStateMachineId = table.Column<long>(type: "bigint", nullable: false),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FiniteStateTransition",
                columns: table => new
                {
                    StatesId = table.Column<long>(type: "bigint", nullable: false),
                    TransitionsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiniteStateTransition", x => new { x.StatesId, x.TransitionsId });
                    table.ForeignKey(
                        name: "FK_FiniteStateTransition_States_StatesId",
                        column: x => x.StatesId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FiniteStateTransition_Transitions_TransitionsId",
                        column: x => x.TransitionsId,
                        principalTable: "Transitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiniteStateTransition_TransitionsId",
                table: "FiniteStateTransition",
                column: "TransitionsId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiniteStateTransition");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "Transitions");

            migrationBuilder.DropTable(
                name: "StateMachines");
        }
    }
}
