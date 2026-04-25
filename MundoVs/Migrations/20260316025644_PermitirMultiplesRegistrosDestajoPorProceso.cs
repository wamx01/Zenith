using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoVs.Migrations
{
    /// <inheritdoc />
    public partial class PermitirMultiplesRegistrosDestajoPorProceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesos_Pedido~",
                table: "RegistrosDestajoProceso");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesoId",
                table: "RegistrosDestajoProceso");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesoId",
                table: "RegistrosDestajoProceso",
                column: "PedidoSerigrafiaTallaProcesoId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesos_Pedido~",
                table: "RegistrosDestajoProceso",
                column: "PedidoSerigrafiaTallaProcesoId",
                principalTable: "PedidoSerigrafiaTallaProcesos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesos_Pedido~",
                table: "RegistrosDestajoProceso");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesoId",
                table: "RegistrosDestajoProceso");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesoId",
                table: "RegistrosDestajoProceso",
                column: "PedidoSerigrafiaTallaProcesoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosDestajoProceso_PedidoSerigrafiaTallaProcesos_Pedido~",
                table: "RegistrosDestajoProceso",
                column: "PedidoSerigrafiaTallaProcesoId",
                principalTable: "PedidoSerigrafiaTallaProcesos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
