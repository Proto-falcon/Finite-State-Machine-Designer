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
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    TransitionSearchRadius = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StateMachines_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiniteStateTransitions",
                columns: table => new
                {
                    StatesId = table.Column<long>(type: "bigint", nullable: false),
                    TransitionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiniteStateTransitions", x => new { x.StatesId, x.TransitionId });
                    table.ForeignKey(
                        name: "FK_FiniteStateTransitions_States_StatesId",
                        column: x => x.StatesId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FiniteStateTransitions_Transitions_TransitionId",
                        column: x => x.TransitionId,
                        principalTable: "Transitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiniteStateTransitions_TransitionId",
                table: "FiniteStateTransitions",
                column: "TransitionId");

            migrationBuilder.CreateIndex(
                name: "IX_StateMachines_UserId",
                table: "StateMachines",
                column: "UserId");

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
                name: "FiniteStateTransitions");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "Transitions");

            migrationBuilder.DropTable(
                name: "StateMachines");
        }
    }
}
