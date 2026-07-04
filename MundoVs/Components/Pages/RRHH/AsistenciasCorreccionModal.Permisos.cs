using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class AsistenciasCorreccionModal
{
    private int ObtenerMinutosPermisoSugeridos(RrhhAsistencia asistencia)
    {
        var permisoAplicadoActual = permisoDiaSeleccionado == null
            ? 0
            : (int)Math.Round(Math.Max(0m, permisoDiaSeleccionado.Horas) * 60m, MidpointRounding.AwayFromZero);
        return Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosPermisoSugeridos(asistencia, minutosCompensadosPermisoAprobados) - permisoAplicadoActual);
    }

    private int ObtenerMinutosPermisoCapturados()
        => (int)Math.Round(Math.Max(0m, horasPermisoDiaCaptura) * 60m, MidpointRounding.AwayFromZero);

    private int ObtenerMinutosFaltanteRemanenteActual()
    {
        if (AsistenciaActual == null)
        {
            return 0;
        }

        var faltanteBruto = ObtenerMinutosPermisoSugeridos(AsistenciaActual);
        return Math.Max(0, faltanteBruto - Math.Min(faltanteBruto, ObtenerMinutosPermisoCapturados()));
    }

    private bool TieneCompensacionPermisoAprobada()
        => minutosCompensadosPermisoAprobados > 0;

    private bool PuedeAprobarCompensacionPermiso()
        => PuedeAprobarTiempoExtra && (ObtenerMaximoMinutosCompensacionPermisoEditable() > 0 || minutosCompensadosPermisoAprobados > 0);

    private int ObtenerMinutosCompensacionPermisoSugeridosAprobacion()
    {
        if (AsistenciaActual == null)
        {
            return 0;
        }

        var faltanteNeto = Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteBanco(AsistenciaActual));
        return Math.Max(0, Math.Min(faltanteNeto, minutosRecuperablesPermisoAprobables));
    }

    private int ObtenerMaximoMinutosCompensacionPermisoEditable()
        => Math.Max(minutosCompensadosPermisoAprobados, ObtenerMinutosCompensacionPermisoSugeridosAprobacion());

    private int ObtenerMinutosCompensacionPermisoPendientes()
        => Math.Max(0, minutosRecuperablesPermisoAprobables - minutosCompensadosPermisoAprobados);

    private string ObtenerResumenCompensacionPermiso()
    {
        if (minutosRecuperablesPermisoAprobables <= 0 && minutosCompensadosPermisoAprobados <= 0)
        {
            return "No se detecta tiempo recuperable por aprobación para este día.";
        }

        var minutosSugeridos = ObtenerMinutosCompensacionPermisoSugeridosAprobacion();

        if (minutosCompensadosPermisoAprobados > 0)
        {
            var pendientes = ObtenerMinutosCompensacionPermisoPendientes();
            return pendientes > 0
                ? $"Compensación aprobada: {FormatearMinutos(minutosCompensadosPermisoAprobados)}. Ya reduce el faltante y el permiso sugerido del día. Quedan {FormatearMinutos(pendientes)} recuperables detectados por revisar."
                : $"Compensación aprobada: {FormatearMinutos(minutosCompensadosPermisoAprobados)}. Ya fue aplicada al faltante y al tiempo visible operativo del día.";
        }

        return minutosSugeridos < minutosRecuperablesPermisoAprobables
            ? $"Tiempo recuperable detectado: {FormatearMinutos(minutosRecuperablesPermisoAprobables)}. Para este día solo conviene aprobar hasta {FormatearMinutos(minutosSugeridos)} porque ese es el faltante neto a compensar."
            : $"Tiempo recuperable detectado: {FormatearMinutos(minutosRecuperablesPermisoAprobables)}. Requiere aprobación para ajustar faltante, permiso sugerido y tiempo visible del día.";
    }

    private string ObtenerGuiaCompensacionPermiso()
    {
        if (TieneCompensacionPermisoAprobada())
        {
            return "Ya existe una compensación aprobada. Ajústala solo si el tiempo recuperado validado cambió.";
        }

        return minutosRecuperablesPermisoAprobables > 0
            ? "Úsala solo si el empleado recuperó tiempo y quieres descontarlo del faltante o del permiso sugerido."
            : "No hay tiempo recuperable pendiente; primero revisa permiso, turno o marcaciones.";
    }

    private string ObtenerEstadoCompensacionSeccion()
    {
        if (TieneCompensacionPermisoAprobada())
        {
            return "Aprobada";
        }

        return minutosRecuperablesPermisoAprobables > 0
            ? "Por revisar"
            : "No aplica";
    }

    private string ObtenerClaseEstadoCompensacionSeccion()
    {
        if (TieneCompensacionPermisoAprobada())
        {
            return "text-bg-info";
        }

        return minutosRecuperablesPermisoAprobables > 0
            ? "text-bg-warning"
            : "text-bg-light";
    }

    private async Task AprobarCompensacionPermisoAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeAprobarTiempoExtra)
        {
            error = "No tienes permisos para aprobar compensaciones del día.";
            return;
        }

        if (!PuedeAprobarCompensacionPermiso())
        {
            error = "No hay minutos recuperables disponibles para aprobar en este día.";
            return;
        }

        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var fecha = AsistenciaActual.Fecha;
            var minutosAprobados = Math.Max(0, minutosCompensacionPermisoCaptura);
            var maximoEditable = ObtenerMaximoMinutosCompensacionPermisoEditable();

            if (minutosAprobados > maximoEditable)
            {
                error = $"Solo puedes aprobar hasta {FormatearMinutos(maximoEditable)} para este día.";
                return;
            }

            saldoBancoHorasSeleccionado = await TiempoExtraResolutionService.RemoverCompensacionPermisoBancoHorasAsync(db, _empresaId, AsistenciaActual.EmpleadoId, fecha);

            await RegistrarBitacoraCorreccionAsync(
                db,
                "Se aplicó corrección de asistencia: compensación aprobada de permiso.",
                $"empleado={AsistenciaActual.EmpleadoId};fecha={fecha:yyyy-MM-dd};minutosCompensados={minutosAprobados};permisoActual={permisoDiaSeleccionado?.Horas.ToString("0.##") ?? "0"};saldoBanco={saldoBancoHorasSeleccionado};usuario={usuarioActual}");

            await db.SaveChangesAsync();
            await CargarMarcacionesDiaAsync();
            await NotificarActualizacionAsync();
            ok = minutosAprobados > 0
                ? $"Compensación guardada por {FormatearMinutos(minutosAprobados)}. Solo ajusta el faltante y el permiso sugerido del día."
                : "Compensación retirada. Se restauraron el faltante y el permiso sugerido del día.";
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private async Task QuitarCompensacionPermisoAsync()
    {
        minutosCompensacionPermisoCaptura = 0;
        await AprobarCompensacionPermisoAsync();
    }

    private async Task AprobarCompensacionSugeridaResumenAsync()
    {
        minutosCompensacionPermisoCaptura = TieneCompensacionPermisoAprobada()
            ? minutosCompensadosPermisoAprobados
            : ObtenerMinutosCompensacionPermisoSugeridosAprobacion();

        await AprobarCompensacionPermisoAsync();
    }

    private string ObtenerResumenPermisoSugerido()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos para sugerir permiso.";
        }

        var faltanteBruto = ObtenerMinutosPermisoSugeridos(AsistenciaActual);
        var permisoCapturado = ObtenerMinutosPermisoCapturados();
        var permisoAplicado = Math.Min(faltanteBruto, permisoCapturado);
        var faltanteRemanente = Math.Max(0, faltanteBruto - permisoAplicado);
        var descansoNoPagado = ObtenerMinutosDescansoNoPagadoExcluidosDelPermiso(AsistenciaActual);
        if (faltanteBruto <= 0)
        {
            return descansoNoPagado > 0
                ? $"Del tiempo ausente, {FormatearMinutos(descansoNoPagado)} ya corresponden a descanso no pagado; hoy no se sugiere capturar permiso adicional."
                : "Hoy no se sugiere capturar permiso adicional.";
        }

        if (permisoCapturado > 0 && faltanteRemanente == 0)
        {
            return $"Con lo capturado ({FormatearMinutos(permisoCapturado)}), el permiso ya cubre el faltante neto del día.";
        }

        if (permisoCapturado > 0 && faltanteRemanente > 0)
        {
            return $"Con lo capturado ({FormatearMinutos(permisoCapturado)}), todavía quedarían {FormatearMinutos(faltanteRemanente)} por cubrir.";
        }

        if (descansoNoPagado <= 0)
        {
            return $"Permiso neto sugerido: {FormatearMinutos(faltanteBruto)}.";
        }

        return $"Permiso neto sugerido: {FormatearMinutos(faltanteBruto)}. Del tiempo ausente, {FormatearMinutos(descansoNoPagado)} corresponden a descanso no pagado y no deben capturarse como permiso.";
    }

    private string ObtenerResumenPermisoRapido()
    {
        if (AsistenciaActual == null)
        {
            return "Sin datos para permiso.";
        }

        var permisoAplicadoActual = permisoDiaSeleccionado == null
            ? 0
            : (int)Math.Round(Math.Max(0m, permisoDiaSeleccionado.Horas) * 60m, MidpointRounding.AwayFromZero);
        var sugerido = Math.Max(0, RrhhTiempoExtraPolicy.ObtenerMinutosPermisoSugeridos(AsistenciaActual, minutosCompensadosPermisoAprobados) - permisoAplicadoActual);
        var capturado = ObtenerMinutosPermisoCapturados();
        var remanente = ObtenerMinutosFaltanteRemanenteActual();

        if (permisoDiaSeleccionado != null)
        {
            return $"Permiso actual: {permisoDiaSeleccionado.Horas:0.##} h {(permisoDiaSeleccionado.ConGocePago ? "con goce" : "sin goce")}. Remanente actual: {FormatearMinutos(remanente)}.";
        }

        if (sugerido <= 0)
        {
            return "Hoy no se sugiere permiso adicional, salvo que quieras registrar una justificación operativa del día.";
        }

        return capturado > 0
            ? $"Sugerido: {FormatearMinutos(sugerido)}. Con tu captura actual vas en {FormatearMinutos(capturado)} y quedarían {FormatearMinutos(remanente)} por cubrir."
            : $"Sugerido neto: {FormatearMinutos(sugerido)}. Puedes ajustar horas y guardar aquí mismo.";
    }

    private string ObtenerResumenPermisoDiaCaptura()
    {
        if (permisoDiaSeleccionado == null)
        {
            return "Sin permiso parcial registrado. Si el empleado tuvo permiso ese día, regístralo aquí para que el reproceso no descuente doble los descansos no marcados.";
        }

        var partes = new List<string>
        {
            $"Permiso actual: {permisoDiaSeleccionado.Horas:0.##} h",
            permisoDiaSeleccionado.ConGocePago ? "con goce" : "sin goce"
        };

        if (permisoDiaSeleccionado.DescuentaBancoHoras)
        {
            partes.Add("descontado del banco de horas");
        }

        if (!string.IsNullOrWhiteSpace(permisoDiaSeleccionado.Motivo))
        {
            partes.Add(permisoDiaSeleccionado.Motivo.Trim());
        }

        return string.Join(" · ", partes);
    }

    private string ObtenerEstadoPermisoSeccion()
        => permisoDiaSeleccionado == null
            ? "Pendiente"
            : "Registrado";

    private string ObtenerClaseEstadoPermisoSeccion()
        => permisoDiaSeleccionado == null
            ? "text-bg-warning"
            : "text-bg-success";

    private string ObtenerGuiaRapidaPermiso()
        => permisoDiaSeleccionado == null
            ? "Paso rápido: usa el sugerido, confirma si es con goce y guarda para reprocesar el día."
            : "Ya existe un permiso. Ajusta horas, goce o motivo y guarda para volver a reprocesar.";

    private void AplicarPermisoDiaSugerido()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        horasPermisoDiaCaptura = decimal.Round(ObtenerMinutosPermisoSugeridos(AsistenciaActual) / 60m, 2, MidpointRounding.AwayFromZero);
    }

    private void PrepararPermisoSugeridoResumen()
        => AplicarPermisoDiaSugerido();

    private void AplicarCapturaPermisoSugerida()
    {
        horasPermisoDiaCaptura = AsistenciaActual == null
            ? 0
            : decimal.Round(ObtenerMinutosPermisoSugeridos(AsistenciaActual) / 60m, 2, MidpointRounding.AwayFromZero);
        permisoDiaConGoceCaptura = true;
        permisoDiaMotivo = null;
        permisoDiaObservaciones = null;
    }

    private void AplicarCapturaPermisoExistente()
    {
        if (permisoDiaSeleccionado == null)
        {
            return;
        }

        horasPermisoDiaCaptura = permisoDiaSeleccionado.Horas;
        permisoDiaConGoceCaptura = permisoDiaSeleccionado.ConGocePago;
        permisoDiaMotivo = permisoDiaSeleccionado.Motivo;
        permisoDiaObservaciones = permisoDiaSeleccionado.Observaciones;
    }

    private async Task GuardarPermisoDiaAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeReprocesar)
        {
            error = "No tienes permisos para registrar permisos del día.";
            return;
        }

        if (horasPermisoDiaCaptura <= 0)
        {
            error = "Captura las horas del permiso parcial.";
            return;
        }

        guardandoPermisoDia = true;
        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var fecha = AsistenciaActual.Fecha;
            var permiso = await db.RrhhAusencias.FirstOrDefaultAsync(a => a.EmpresaId == _empresaId
                && a.EmpleadoId == AsistenciaActual.EmpleadoId
                && a.Tipo == TipoAusenciaRrhh.Permiso
                && a.IsActive
                && a.FechaInicio <= fecha
                && a.FechaFin >= fecha);

            if (permiso == null)
            {
                permiso = new RrhhAusencia
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = _empresaId,
                    EmpleadoId = AsistenciaActual.EmpleadoId,
                    Tipo = TipoAusenciaRrhh.Permiso,
                    FechaInicio = fecha,
                    FechaFin = fecha,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = usuarioActual,
                    IsActive = true
                };
                db.RrhhAusencias.Add(permiso);
            }

            permiso.Estatus = EstatusAusenciaRrhh.Aplicada;
            permiso.Dias = 1;
            permiso.Horas = decimal.Round(horasPermisoDiaCaptura, 2, MidpointRounding.AwayFromZero);
            permiso.ConGocePago = permisoDiaConGoceCaptura;
            permiso.DescuentaBancoHoras = permisoDiaConGoceCaptura;
            permiso.Motivo = string.IsNullOrWhiteSpace(permisoDiaMotivo) ? "Permiso parcial desde asistencias." : permisoDiaMotivo.Trim();
            permiso.Observaciones = string.IsNullOrWhiteSpace(permisoDiaObservaciones) ? null : permisoDiaObservaciones.Trim();
            permiso.FechaAprobacion ??= DateTime.UtcNow;
            permiso.AprobadoPor = usuarioActual;
            permiso.UpdatedAt = DateTime.UtcNow;
            permiso.UpdatedBy = usuarioActual;
            permiso.IsActive = true;

            if (permiso.DescuentaBancoHoras)
            {
                saldoBancoHorasSeleccionado = await TiempoExtraResolutionService.AplicarPermisoConGoceBancoHorasAsync(db, new RrhhPermisoBancoHorasCommand
                {
                    EmpresaId = _empresaId,
                    EmpleadoId = AsistenciaActual.EmpleadoId,
                    AusenciaId = permiso.Id,
                    Fecha = fecha,
                    HorasPermiso = permiso.Horas,
                    Observaciones = $"Permiso parcial con goce desde asistencias. Motivo: {permiso.Motivo}",
                    UsuarioActual = usuarioActual
                });
            }
            else
            {
                saldoBancoHorasSeleccionado = await TiempoExtraResolutionService.RemoverPermisoBancoHorasAsync(db, _empresaId, AsistenciaActual.EmpleadoId, permiso.Id);
            }

            await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: permiso parcial del día.", $"empleado={AsistenciaActual.EmpleadoId};fecha={fecha:yyyy-MM-dd};horas={permiso.Horas:0.##};goce={permiso.ConGocePago};ausencia={permiso.Id};motivo={permiso.Motivo};saldoBanco={saldoBancoHorasSeleccionado}");
            await ReprocesarYRefrescarDiaAsync(
                db,
                fecha,
                permiso.DescuentaBancoHoras
                ? "Permiso parcial guardado, banco descontado y asistencia reprocesada."
                : "Permiso parcial guardado y asistencia reprocesada.");
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            guardandoPermisoDia = false;
        }
    }

    private async Task QuitarPermisoDiaAsync()
    {
        if (AsistenciaActual == null || permisoDiaSeleccionado == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeReprocesar)
        {
            error = "No tienes permisos para quitar permisos del día.";
            return;
        }

        guardandoPermisoDia = true;
        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var fecha = AsistenciaActual.Fecha;
            var permiso = await db.RrhhAusencias.FirstOrDefaultAsync(a => a.Id == permisoDiaSeleccionado.Id);
            if (permiso == null)
            {
                error = "No se encontró el permiso del día.";
                return;
            }

            permiso.IsActive = false;
            permiso.Estatus = EstatusAusenciaRrhh.Cancelada;
            permiso.UpdatedAt = DateTime.UtcNow;
            permiso.UpdatedBy = usuarioActual;

            saldoBancoHorasSeleccionado = await TiempoExtraResolutionService.RemoverPermisoBancoHorasAsync(db, _empresaId, AsistenciaActual.EmpleadoId, permiso.Id);

            await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: retiro de permiso parcial del día.", $"empleado={AsistenciaActual.EmpleadoId};fecha={fecha:yyyy-MM-dd};ausencia={permiso.Id};saldoBanco={saldoBancoHorasSeleccionado}");
            await ReprocesarYRefrescarDiaAsync(db, fecha, "Permiso parcial retirado, banco restaurado y asistencia reprocesada.");
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            guardandoPermisoDia = false;
        }
    }

    private void AlternarAccionesRapidasPermiso()
    {
        var nuevoEstado = !_mostrarAccionesRapidasPermiso;
        _mostrarAccionesRapidasPermiso = nuevoEstado;
        if (nuevoEstado)
        {
            _mostrarAccionesRapidasTiempo = false;
            _mostrarAccionesRapidasTurno = false;
            _mostrarAccionesRapidasModoExtra = false;
        }
    }

    private bool TieneAusenciaActual()
        => !string.IsNullOrWhiteSpace(resumenAusenciaActual);

    private string ObtenerResumenAusenciaActual()
        => resumenAusenciaActual ?? "—";

    private static string FormatearAusencia(RrhhAusencia ausencia)
        => ausencia.Tipo == TipoAusenciaRrhh.Vacaciones
            ? "Vacaciones"
            : $"Permiso {(ausencia.ConGocePago ? "con goce" : "sin goce")}{(ausencia.Horas > 0 ? $" ({ausencia.Horas:0.##} h)" : string.Empty)}";

    private string ObtenerResumenResolucion(RrhhAsistencia asistencia)
        => RrhhTiempoExtraPolicy.ObtenerResumenResolucionOperativa(asistencia, resumenAusenciaActual);
}