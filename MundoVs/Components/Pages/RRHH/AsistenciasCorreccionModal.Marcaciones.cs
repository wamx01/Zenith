using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Components.Pages.RRHH;

public partial class AsistenciasCorreccionModal
{
    private string ObtenerHoraMarcacion(RrhhMarcacion marcacion)
        => ObtenerFechaHoraLocalMarcacion(marcacion).ToString("HH:mm");

    private string ObtenerAuditoriaMarcacionPresentacion(RrhhMarcacion marcacion)
        => ObtenerAuditoriaMarcacion(marcacion);

    private async Task CambiarClasificacionMarcacionDesdeUiAsync((Guid MarcacionId, TipoClasificacionMarcacionRrhh Clasificacion) cambio)
        => await CambiarClasificacionMarcacionAsync(cambio.MarcacionId, cambio.Clasificacion);

    private async Task GuardarPerdonManualAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeReprocesar)
        {
            error = "No tienes permisos para perdonar minutos del día.";
            return;
        }

        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var asistencia = await db.RrhhAsistencias.FirstOrDefaultAsync(a => a.Id == AsistenciaActual.Id);
            if (asistencia == null)
            {
                error = "No se encontró la asistencia actual.";
                return;
            }

            asistencia.MinutosPerdonadosManual = Math.Max(0, minutosPerdonManualCaptura);
            asistencia.ObservacionPerdonManual = asistencia.MinutosPerdonadosManual > 0
                ? $"Perdón manual aplicado por {usuarioActual}."
                : null;
            asistencia.UpdatedAt = DateTime.UtcNow;
            asistencia.UpdatedBy = usuarioActual;

            await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: perdón manual de minutos.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};minutosPerdonados={asistencia.MinutosPerdonadosManual}");
            await db.SaveChangesAsync();
            await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha, "Perdón manual aplicado al día.", recargarResolucion: true);
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private async Task CambiarClasificacionMarcacionAsync(Guid marcacionId, TipoClasificacionMarcacionRrhh clasificacion)
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var marcacion = await db.RrhhMarcaciones.FirstOrDefaultAsync(m => m.Id == marcacionId);
        if (marcacion == null)
        {
            return;
        }

        marcacion.ClasificacionOperativa = clasificacion;
        marcacion.TipoMarcacionRaw = clasificacion.ToString();
        marcacion.UpdatedAt = DateTime.UtcNow;
        marcacion.UpdatedBy = usuarioActual;
        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: cambio de clasificación de marcación.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};marcacion={marcacionId};clasificacion={clasificacion}");
        await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha);
    }

    private async Task AlternarAnulacionMarcacionAsync(Guid marcacionId)
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var marcacion = await db.RrhhMarcaciones.FirstOrDefaultAsync(m => m.Id == marcacionId);
        if (marcacion == null)
        {
            return;
        }

        marcacion.EsAnulada = !marcacion.EsAnulada;
        marcacion.Procesada = false;
        marcacion.UpdatedAt = DateTime.UtcNow;
        marcacion.UpdatedBy = usuarioActual;
        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: cambio de estado de marcación.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};marcacion={marcacionId};anulada={marcacion.EsAnulada}");
        await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha);
    }

    private void IniciarEdicionMarcacionManual(RrhhMarcacion marcacion)
    {
        marcacionManualEditandoId = marcacion.Id;
        marcacionManualEditandoHoraTexto = ObtenerFechaHoraLocalMarcacion(marcacion).ToString("HH:mm");
        marcacionManualEditandoObservacion = marcacion.ObservacionManual;
    }

    private void CancelarEdicionMarcacionManual()
    {
        marcacionManualEditandoId = null;
        marcacionManualEditandoHoraTexto = string.Empty;
        marcacionManualEditandoObservacion = null;
    }

    private void ReiniciarEdicionManual()
    {
        marcacionManualEditandoId = null;
        marcacionManualEditandoHoraTexto = string.Empty;
        marcacionManualEditandoObservacion = null;
    }

    private async Task GuardarMarcacionManualDiaAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!TimeSpan.TryParse(manualHoraTexto, out var hora))
        {
            error = "La hora debe tener formato HH:mm.";
            return;
        }

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == AsistenciaActual.EmpleadoId);
        if (empleado == null)
        {
            error = "No se encontró el empleado.";
            return;
        }

        var checador = await db.RrhhChecadores
            .Where(c => c.EmpresaId == _empresaId && c.IsActive)
            .OrderBy(c => c.Nombre)
            .FirstOrDefaultAsync();

        if (checador == null)
        {
            error = "No hay checador activo configurado para registrar la marcación manual.";
            return;
        }

        var fechaLocal = AsistenciaActual.Fecha.ToDateTime(TimeOnly.MinValue).Add(hora);
        var zonaHorariaAplicada = ObtenerZonaHorariaAplicada(checador.ZonaHoraria);
        var fechaUtc = ConvertirLocalChecadorAUtc(fechaLocal, zonaHorariaAplicada);
        db.RrhhMarcaciones.Add(new RrhhMarcacion
        {
            Id = Guid.NewGuid(),
            EmpresaId = _empresaId,
            ChecadorId = checador.Id,
            EmpleadoId = AsistenciaActual.EmpleadoId,
            CodigoChecador = empleado.CodigoChecador ?? empleado.NumeroEmpleado,
            FechaHoraMarcacionLocal = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified),
            FechaHoraMarcacionUtc = fechaUtc,
            ZonaHorariaAplicada = zonaHorariaAplicada,
            TipoMarcacionRaw = manualClasificacion.ToString(),
            Origen = "Manual",
            EventoIdExterno = $"MANUAL-{Guid.NewGuid():N}",
            HashUnico = $"manual:{_empresaId}:{AsistenciaActual.EmpleadoId}:{fechaUtc:O}",
            EsManual = true,
            ClasificacionOperativa = manualClasificacion,
            Procesada = false,
            ObservacionManual = string.IsNullOrWhiteSpace(manualObservacion) ? null : manualObservacion.Trim(),
            PayloadRaw = "Marcación manual creada desde Asistencias.",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuarioActual,
            IsActive = true
        });

        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: alta de marcación manual.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};hora={manualHoraTexto};clasificacion={manualClasificacion};obs={manualObservacion}");
        await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha, "Marcación manual agregada y día reprocesado.");

        manualHoraTexto = string.Empty;
        manualObservacion = null;
    }

    private async Task GuardarEdicionMarcacionManualAsync()
    {
        if (AsistenciaActual == null || !marcacionManualEditandoId.HasValue)
        {
            return;
        }

        error = null;
        ok = null;

        if (!TimeSpan.TryParse(marcacionManualEditandoHoraTexto, out var hora))
        {
            error = "La hora editada debe tener formato HH:mm.";
            return;
        }

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var marcacion = await db.RrhhMarcaciones
            .Include(m => m.Checador)
            .FirstOrDefaultAsync(m => m.Id == marcacionManualEditandoId.Value);
        if (marcacion == null || !marcacion.EsManual)
        {
            error = "Solo se pueden editar marcaciones manuales.";
            return;
        }

        var fechaLocal = AsistenciaActual.Fecha.ToDateTime(TimeOnly.MinValue).Add(hora);
        var zonaHorariaAplicada = string.IsNullOrWhiteSpace(marcacion.ZonaHorariaAplicada)
            ? ObtenerZonaHorariaAplicada(marcacion.Checador?.ZonaHoraria)
            : marcacion.ZonaHorariaAplicada;
        var fechaUtc = ConvertirLocalChecadorAUtc(fechaLocal, zonaHorariaAplicada);
        var hashNuevo = $"manual:{_empresaId}:{AsistenciaActual.EmpleadoId}:{fechaUtc:O}";
        var duplicada = await db.RrhhMarcaciones.AnyAsync(m => m.Id != marcacion.Id && m.HashUnico == hashNuevo);
        if (duplicada)
        {
            error = "Ya existe otra marcación manual con esa hora.";
            return;
        }

        marcacion.FechaHoraMarcacionLocal = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified);
        marcacion.FechaHoraMarcacionUtc = fechaUtc;
        marcacion.ZonaHorariaAplicada = zonaHorariaAplicada;
        marcacion.HashUnico = hashNuevo;
        marcacion.ObservacionManual = string.IsNullOrWhiteSpace(marcacionManualEditandoObservacion) ? null : marcacionManualEditandoObservacion.Trim();
        marcacion.Procesada = false;
        marcacion.UpdatedAt = DateTime.UtcNow;
        marcacion.UpdatedBy = usuarioActual;
        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: edición de marcación manual.", $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};marcacion={marcacion.Id};hora={marcacionManualEditandoHoraTexto};obs={marcacionManualEditandoObservacion}");
        await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha, "Marcación manual actualizada y día reprocesado.");

        CancelarEdicionMarcacionManual();
    }

    private void AlternarMarcacionesDia()
        => _mostrarMarcacionesDia = !_mostrarMarcacionesDia;

    private async Task GuardarTurnoDiaAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        var usuarioActual = await ObtenerUsuarioActualAsync();
        await using var db = await DbFactory.CreateDbContextAsync();
        var fecha = AsistenciaActual.Fecha;
        var vigencias = await db.RrhhEmpleadosTurno
            .Where(v => v.EmpresaId == _empresaId && v.EmpleadoId == AsistenciaActual.EmpleadoId && v.IsActive)
            .ToListAsync();

        foreach (var vigente in vigencias.Where(v => v.VigenteDesde <= fecha && (v.VigenteHasta == null || v.VigenteHasta >= fecha)))
        {
            if (vigente.VigenteDesde == fecha && vigente.VigenteHasta == fecha)
            {
                db.RrhhEmpleadosTurno.Remove(vigente);
                continue;
            }

            if (vigente.VigenteDesde == fecha)
            {
                vigente.VigenteDesde = fecha.AddDays(1);
            }
            else if (vigente.VigenteHasta == null || vigente.VigenteHasta >= fecha)
            {
                vigente.VigenteHasta = fecha.AddDays(-1);
            }
            vigente.UpdatedAt = DateTime.UtcNow;
            vigente.UpdatedBy = usuarioActual;
        }

        if (Guid.TryParse(turnoDiaSeleccionadoIdTexto, out var turnoId) && turnoId != Guid.Empty)
        {
            db.RrhhEmpleadosTurno.Add(new RrhhEmpleadoTurno
            {
                Id = Guid.NewGuid(),
                EmpresaId = _empresaId,
                EmpleadoId = AsistenciaActual.EmpleadoId,
                TurnoBaseId = turnoId,
                VigenteDesde = fecha,
                VigenteHasta = fecha,
                Observaciones = string.IsNullOrWhiteSpace(turnoDiaObservaciones) ? "Cambio de turno desde asistencia." : turnoDiaObservaciones.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuarioActual,
                IsActive = true
            });
        }

        await RegistrarBitacoraCorreccionAsync(db, "Se aplicó corrección de asistencia: cambio de turno por día.", $"empleado={AsistenciaActual.EmpleadoId};fecha={fecha:yyyy-MM-dd};turno={turnoDiaSeleccionadoIdTexto};obs={turnoDiaObservaciones}");
        await ReprocesarYRefrescarDiaAsync(db, fecha, "Turno del día actualizado y asistencia reprocesada.");
    }

    private void AlternarAccionesRapidasTurno()
    {
        var nuevoEstado = !_mostrarAccionesRapidasTurno;
        _mostrarAccionesRapidasTurno = nuevoEstado;
        if (nuevoEstado)
        {
            _mostrarAccionesRapidasTiempo = false;
            _mostrarAccionesRapidasPermiso = false;
            _mostrarAccionesRapidasModoExtra = false;
        }
    }

    private async Task ConfirmarSalidaTempranaCompensaDescansoAsync()
    {
        if (AsistenciaActual == null)
        {
            return;
        }

        error = null;
        ok = null;

        if (!PuedeReprocesar)
        {
            error = "No tienes permisos para confirmar esta regla operativa.";
            return;
        }

        if (TieneConfirmacionSalidaTempranaCompensaDescanso())
        {
            ok = "Esta regla ya fue confirmada previamente.";
            return;
        }

        try
        {
            await using var db = await DbFactory.CreateDbContextAsync();
            await RegistrarBitacoraCorreccionAsync(
                db,
                "Se aplicó corrección de asistencia: salida temprana sustituyó descanso no marcado.",
                $"empleado={AsistenciaActual.EmpleadoId};fecha={AsistenciaActual.Fecha:yyyy-MM-dd};salidaAnticipada={AsistenciaActual.MinutosSalidaAnticipada};descansoProgramado={AsistenciaActual.MinutosDescansoProgramado}");

            await db.SaveChangesAsync();
            await CargarMarcacionesDiaAsync();
            ok = "Se confirmó en bitácora que la salida temprana sustituyó el descanso no marcado.";
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private bool EsEscenarioSalidaTempranaCompensaDescanso()
        => asesorCorreccionActual?.Escenario == "SalidaTempranaCompensaDescanso";

    private bool TieneConfirmacionSalidaTempranaCompensaDescanso()
        => bitacoraCorreccionDia.Any(l => l.Mensaje.Contains("salida temprana sustituyó descanso no marcado", StringComparison.OrdinalIgnoreCase));

    private async Task GuardarDescansosNoDescontarAsync()
    {
        if (AsistenciaActual == null || !PuedeReprocesar)
        {
            return;
        }

        error = null;
        ok = null;

        try
        {
            var usuarioActual = await ObtenerUsuarioActualAsync();
            await using var db = await DbFactory.CreateDbContextAsync();
            var asistencia = await db.RrhhAsistencias.FirstOrDefaultAsync(a => a.Id == AsistenciaActual.Id);
            if (asistencia == null)
            {
                error = "No se encontró la asistencia actual.";
                return;
            }

            var valor = _descansosNoDescontarSeleccionados.Count == 0
                ? null
                : string.Join(",", _descansosNoDescontarSeleccionados.Order());

            asistencia.DescansosNoDescontar = valor;
            asistencia.UpdatedAt = DateTime.UtcNow;
            asistencia.UpdatedBy = usuarioActual;
            await db.SaveChangesAsync();

            await ReprocesarYRefrescarDiaAsync(db, AsistenciaActual.Fecha, _descansosNoDescontarSeleccionados.Count > 0
                ? $"Descansos no descontados actualizados: D{string.Join(", D", _descansosNoDescontarSeleccionados.Order())}."
                : "Se restauró el descuento automático de todos los descansos.", recargarResolucion: true);
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private void CargarDescansosNoDescontar(RrhhAsistencia asistencia)
    {
        _descansosNoDescontarSeleccionados.Clear();
        if (string.IsNullOrWhiteSpace(asistencia.DescansosNoDescontar))
        {
            return;
        }

        foreach (var segmento in asistencia.DescansosNoDescontar.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(segmento, out var numero))
            {
                _descansosNoDescontarSeleccionados.Add(numero);
            }
        }
    }

    private List<(int Numero, TimeSpan Inicio, TimeSpan Fin, int Minutos, bool EsPagado)> ObtenerDescansosConfiguradosDia()
    {
        var detalle = ObtenerDetalleTurnoSeleccionadoDia();
        if (detalle == null || !detalle.Labora)
        {
            return [];
        }

        return detalle.Descansos
            .Where(d => d.HoraInicio.HasValue && d.HoraFin.HasValue)
            .OrderBy(d => d.Orden)
            .Select(d => ((int)d.Orden, d.HoraInicio!.Value, d.HoraFin!.Value, (int)Math.Round((d.HoraFin!.Value - d.HoraInicio!.Value).TotalMinutes), d.EsPagado))
            .ToList();
    }

    private bool EsDescansoNoDescontar(int numeroDescanso)
        => _descansosNoDescontarSeleccionados.Contains(numeroDescanso);

    private void ToggleDescansoNoDescontar(int numeroDescanso)
    {
        if (_descansosNoDescontarSeleccionados.Contains(numeroDescanso))
        {
            _descansosNoDescontarSeleccionados.Remove(numeroDescanso);
        }
        else
        {
            _descansosNoDescontarSeleccionados.Add(numeroDescanso);
        }
    }
}