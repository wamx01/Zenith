using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

/// <summary>
/// Resolución de tiempo extra por periodo de nómina. La DETECCIÓN sigue siendo
/// diaria (RrhhAsistencia.MinutosExtra); la LIQUIDACIÓN se autoriza por periodo.
///
/// Fase 1: SIN netting. Faltante, retardo y extra se reportan independientes; el
/// operador solo reparte el extra detectado entre pago y banco. Los neteos
/// (extra absorbe faltante/retardo, restablece banco) llegan en fases posteriores.
/// </summary>
public sealed class RrhhResolucionPeriodoService : IRrhhResolucionPeriodoService
{
    private const string ReferenciaPeriodoExtraBancoPrefix = "Periodo";

    private readonly IRrhhTiempoExtraResolutionService _tiempoExtra;

    public RrhhResolucionPeriodoService(IRrhhTiempoExtraResolutionService tiempoExtra)
    {
        _tiempoExtra = tiempoExtra;
    }

    public async Task<RrhhResolucionTiempoExtraPeriodo> ObtenerOCrearPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        GarantizarAplicable(empleado);

        var calendario = ResolverPeriodo(empleado, fechaReferencia, corte);

        var existente = await db.RrhhResolucionesTiempoExtraPeriodo
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId
                && r.EmpleadoId == empleadoId
                && r.PeriodicidadPago == calendario.PeriodicidadPago
                && r.AnioPeriodo == calendario.AnioPeriodo
                && r.NumeroPeriodo == calendario.NumeroPeriodo, cancellationToken);

        if (existente is not null)
        {
            return existente;
        }

        var periodo = new RrhhResolucionTiempoExtraPeriodo
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            PeriodicidadPago = calendario.PeriodicidadPago,
            AnioPeriodo = calendario.AnioPeriodo,
            NumeroPeriodo = calendario.NumeroPeriodo,
            PeriodoKey = ConstruirPeriodoKey(calendario),
            PeriodoEtiqueta = calendario.Periodo,
            FechaInicio = DateOnly.FromDateTime(calendario.Inicio),
            FechaFin = DateOnly.FromDateTime(calendario.Fin),
            Estatus = RrhhResolucionPeriodoEstatus.Pendiente,
            CreatedAt = DateTime.UtcNow
        };
        db.RrhhResolucionesTiempoExtraPeriodo.Add(periodo);
        return periodo;
    }

    public async Task<RrhhResolucionPeriodoResumen> ObtenerResumenPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        var calendario = ResolverPeriodo(empleado, fechaReferencia, corte);
        var fechaInicio = DateOnly.FromDateTime(calendario.Inicio);
        var fechaFin = DateOnly.FromDateTime(calendario.Fin);

        var contexto = await _tiempoExtra.ObtenerContextoEmpleadoAsync(db, empresaId, empleadoId, cancellationToken);

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId
                && r.EmpleadoId == empleadoId
                && r.PeriodicidadPago == calendario.PeriodicidadPago
                && r.AnioPeriodo == calendario.AnioPeriodo
                && r.NumeroPeriodo == calendario.NumeroPeriodo, cancellationToken);

        var asistencias = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.EmpleadoId == empleadoId
                && a.Fecha >= fechaInicio && a.Fecha <= fechaFin)
            .OrderBy(a => a.Fecha)
            .ToListAsync(cancellationToken);

        var permisosPorDia = await ConstruirPermisosConGocePorDiaAsync(db, empresaId, empleadoId, fechaInicio, fechaFin, cancellationToken);

        var dias = asistencias
            .Select(a =>
            {
                var permisoDia = permisosPorDia.GetValueOrDefault(a.Fecha);
                var faltanteBruto = RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(a);
                var faltanteNeto = RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(a, permisoDia, Math.Max(0, a.MinutosCompensacionPermisoAprobados));
                return new RrhhResolucionPeriodoDia
                {
                    Fecha = a.Fecha,
                    MinutosExtra = Math.Max(0, a.MinutosExtra),
                    MinutosFaltante = faltanteBruto,
                    MinutosFaltanteNeto = faltanteNeto,
                    MinutosPermisoConGoce = permisoDia,
                    MinutosRetardo = RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, permisoDia),
                    MinutosTrabajadosNetos = RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(a)
                };
            })
            .ToList();

        var extraDetectado = dias.Sum(d => d.MinutosExtra);
        var faltanteNetoPeriodo = dias.Sum(d => d.MinutosFaltanteNeto);
        var retardoPeriodo = dias.Sum(d => d.MinutosRetardo);

        // Cadena de neteo (Fase 2 + Fase 3 + Fase 4):
        //   extraDetectado
        //     − faltanteNeto (permiso con goce ya descontado)   [Fase 2]
        //     − retardo del periodo                             [Fase 3]
        //     − banco consumido en el periodo                    [Fase 4]
        //   = extraAbsorbible (lo que se reparte pago/banco)
        // El sobrante de extra tras faltante tapa el retardo (Fase 3); el sobrante
        // tras retardo REPONE el banco consumido en el periodo (Fase 4) generando
        // un movimiento positivo al banco. Solo lo que sobra tras todo es pagable.
        var faltanteAbsorbido = Math.Min(extraDetectado, faltanteNetoPeriodo);
        var sobranteTrasFaltante = Math.Max(0, extraDetectado - faltanteNetoPeriodo);
        var retardoAbsorbido = Math.Min(sobranteTrasFaltante, retardoPeriodo);
        var sobranteTrasRetardo = Math.Max(0, sobranteTrasFaltante - retardoPeriodo);

        var bancoHorasHabilitado = contexto.Configuracion.BancoHorasHabilitado;
        var bancoConsumidoPeriodo = bancoHorasHabilitado
            ? await ObtenerMinutosBancoConsumidoPeriodoAsync(db, empresaId, empleadoId, fechaInicio, fechaFin, cancellationToken)
            : 0;
        var bancoRestaurado = Math.Min(sobranteTrasRetardo, bancoConsumidoPeriodo);
        var extraAbsorbible = Math.Max(0, sobranteTrasRetardo - bancoConsumidoPeriodo);

        return new RrhhResolucionPeriodoResumen
        {
            EsAplicable = empleado.TipoNomina != TipoNomina.Destajo,
            Periodo = periodo,
            PeriodicidadPago = calendario.PeriodicidadPago,
            AnioPeriodo = calendario.AnioPeriodo,
            NumeroPeriodo = calendario.NumeroPeriodo,
            PeriodoKey = ConstruirPeriodoKey(calendario),
            PeriodoEtiqueta = calendario.Periodo,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            MinutosExtraDetectado = extraDetectado,
            MinutosFaltanteDetectado = dias.Sum(d => d.MinutosFaltante),
            MinutosFaltanteNetoPeriodo = faltanteNetoPeriodo,
            MinutosPermisoConGocePeriodo = dias.Sum(d => d.MinutosPermisoConGoce),
            MinutosRetardoDetectado = dias.Sum(d => d.MinutosRetardo),
            MinutosTrabajadosNetosDetectado = dias.Sum(d => d.MinutosTrabajadosNetos),
            MinutosExtraAbsorbible = extraAbsorbible,
            MinutosFaltanteAbsorbidoExtra = faltanteAbsorbido,
            MinutosRetardoAbsorbidoExtra = retardoAbsorbido,
            MinutosBancoConsumidoPeriodo = bancoConsumidoPeriodo,
            MinutosBancoRestauradoExtra = bancoRestaurado,
            MinutosExtraDobles = periodo?.MinutosExtraDobles ?? 0,
            MinutosExtraTriples = periodo?.MinutosExtraTriples ?? 0,
            SaldoBancoHorasMinutos = contexto.SaldoBancoHorasMinutos,
            TopeBancoMinutos = contexto.Configuracion.TopeBancoMinutos,
            FactorTiempoExtra = contexto.Configuracion.FactorTiempoExtra,
            BancoHorasHabilitado = contexto.Configuracion.BancoHorasHabilitado,
            FactorAcumulacionBancoHoras = contexto.Configuracion.FactorAcumulacionBancoHoras,
            Dias = dias
        };
    }

    public async Task<RrhhResolucionPeriodoResult> AplicarResolucionPeriodoAsync(
        CrmDbContext db, RrhhResolucionPeriodoCommand command, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, command.EmpresaId, command.EmpleadoId, cancellationToken);
        GarantizarAplicable(empleado);

        var calendario = ResolverPeriodo(empleado, command.FechaReferencia, corte);
        var fechaInicio = DateOnly.FromDateTime(calendario.Inicio);
        var fechaFin = DateOnly.FromDateTime(calendario.Fin);

        var periodo = await ObtenerOCrearPeriodoAsync(db, command.EmpresaId, command.EmpleadoId, command.FechaReferencia, cancellationToken);
        var contexto = await _tiempoExtra.ObtenerContextoEmpleadoAsync(db, command.EmpresaId, command.EmpleadoId, cancellationToken);

        // Detección del periodo recalculada en vivo (snapshot autoritativo al autorizar).
        var asistencias = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == command.EmpresaId && a.EmpleadoId == command.EmpleadoId
                && a.Fecha >= fechaInicio && a.Fecha <= fechaFin)
            .ToListAsync(cancellationToken);

        var permisosPorDia = await ConstruirPermisosConGocePorDiaAsync(db, command.EmpresaId, command.EmpleadoId, fechaInicio, fechaFin, cancellationToken);

        var extraDetectado = asistencias.Sum(a => Math.Max(0, a.MinutosExtra));
        var faltanteDetectado = asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto(a));
        var faltanteNetoPeriodo = asistencias.Sum(a =>
            RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(a, permisosPorDia.GetValueOrDefault(a.Fecha), Math.Max(0, a.MinutosCompensacionPermisoAprobados)));
        var retardoDetectado = asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, permisosPorDia.GetValueOrDefault(a.Fecha)));
        var netoDetectado = asistencias.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo(a));

        // Cadena de neteo (Fase 2 + Fase 3 + Fase 4):
        //   extraDetectado − faltanteNeto − retardo − bancoConsumido = extraAbsorbible
        // El extra tapa el faltante neto (F2), luego el retardo (F3); el sobrante
        // REPONE el banco consumido en el periodo (F4) con un movimiento positivo
        // al banco (restauracion-banco). Solo lo que sobra tras todo es pagable.
        var faltanteAbsorbido = Math.Min(extraDetectado, faltanteNetoPeriodo);
        var sobranteTrasFaltante = Math.Max(0, extraDetectado - faltanteNetoPeriodo);
        var retardoAbsorbido = Math.Min(sobranteTrasFaltante, retardoDetectado);
        var sobranteTrasRetardo = Math.Max(0, sobranteTrasFaltante - retardoDetectado);

        var bancoHorasHabilitado = contexto.Configuracion.BancoHorasHabilitado;
        var bancoConsumidoPeriodo = bancoHorasHabilitado
            ? await ObtenerMinutosBancoConsumidoPeriodoAsync(db, command.EmpresaId, command.EmpleadoId, fechaInicio, fechaFin, cancellationToken)
            : 0;
        var bancoRestaurado = Math.Min(sobranteTrasRetardo, bancoConsumidoPeriodo);
        var extraAbsorbible = Math.Max(0, sobranteTrasRetardo - bancoConsumidoPeriodo);

        // F9 — DESCARTAR el extra: el operador acepta la detección (el periodo queda
        // resuelto y desbloquea el gate de prenómina) pero NO autoriza compensación
        // ni pago. La compensación NO es automática: requiere autorización explícita
        // (cualquier otro modo); sin ella, el faltante/retardo del periodo se
        // descuenta COMPLETO. Se anula el neteo (absorbidos=0) → sourcing lee
        // absorbidos=0 y descuenta el faltante/retardo en su totalidad.
        if (command.DescartarExtra)
        {
            if (command.MinutosBasePago > 0 || command.MinutosBaseBanco > 0 || command.Lineas.Count > 0)
            {
                throw new InvalidOperationException(
                    "Descartar el tiempo extra es incompatible con pagar o bancar minutos: no envíes líneas ni bases de pago/banco.");
            }
            faltanteAbsorbido = 0;
            retardoAbsorbido = 0;
            bancoRestaurado = 0;
            extraAbsorbible = 0;
        }

        int pagoBase;
        int bancoBase;
        int pago;            // minutos factorados a pago (bitácora)
        int banco;           // minutos factorados a banco (movimiento del ledger)
        int minutosDobles;
        int minutosTriples;
        int minutosSimples;
        decimal horasExtraFactoradas;
        decimal? factorTiempoExtraAplicado;
        decimal? factorAcumulacionBancoAplicado;

        if (command.Lineas.Count > 0)
        {
            // Fase 8 — autorización por líneas: cada segmento lleva su factor y destino.
            pagoBase = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago).Sum(l => Math.Max(0, l.Minutos));
            bancoBase = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Banco).Sum(l => Math.Max(0, l.Minutos));

            if (pagoBase + bancoBase > extraAbsorbible)
            {
                throw new InvalidOperationException(
                    $"La suma de las líneas ({pagoBase} pago + {bancoBase} banco = {pagoBase + bancoBase}) no puede exceder el tiempo extra absorbible del periodo ({extraAbsorbible} min). "
                    + $"Extra {extraDetectado} − faltante neto {faltanteNetoPeriodo} − retardo {retardoDetectado} − banco consumido {bancoConsumidoPeriodo} = {extraAbsorbible} min pagables.");
            }

            if (!bancoHorasHabilitado && bancoBase > 0)
            {
                throw new InvalidOperationException("El banco de horas no está habilitado para esta empresa.");
            }

            // Pago factorado (bitácora) = Σ pago.Minutos × Factor.
            pago = (int)Math.Round(command.Lineas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago)
                .Sum(l => Math.Max(0, l.Minutos) * Math.Max(0m, l.Factor)), MidpointRounding.AwayFromZero);
            // Banco factorado (movimiento del ledger) = Σ banco.Minutos × Factor (acumulación por línea, simétrica al pago).
            banco = bancoHorasHabilitado
                ? (int)Math.Round(command.Lineas
                    .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Banco)
                    .Sum(l => Math.Max(0, l.Minutos) * Math.Max(0m, l.Factor)), MidpointRounding.AwayFromZero)
                : 0;

            // Dobles/triples/simples derivados del factor de cada línea de pago (no del techo legal).
            minutosDobles = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor == 2m).Sum(l => Math.Max(0, l.Minutos));
            minutosTriples = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor == 3m).Sum(l => Math.Max(0, l.Minutos));
            minutosSimples = command.Lineas.Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor != 2m && l.Factor != 3m).Sum(l => Math.Max(0, l.Minutos));

            horasExtraFactoradas = command.Lineas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago)
                .Sum(l => Math.Max(0, l.Minutos) / 60m * Math.Max(0m, l.Factor));

            // Con líneas, el factor único ya no aplica: el calculador usa HorasExtraFactoradas.
            factorTiempoExtraAplicado = null;
            factorAcumulacionBancoAplicado = null;
        }
        else
        {
            // Path legado (bucket único + split legal dobles/triples).
            var factorTiempoExtra = command.FactorTiempoExtraOverride is > 0m
                ? command.FactorTiempoExtraOverride!.Value
                : contexto.Configuracion.FactorTiempoExtra;
            var factorAcumulacionBanco = command.FactorTiempoExtraOverride is > 0m
                ? command.FactorTiempoExtraOverride!.Value
                : contexto.Configuracion.FactorAcumulacionBancoHoras;

            pagoBase = Math.Max(0, command.MinutosBasePago);
            bancoBase = Math.Max(0, command.MinutosBaseBanco);

            // El cap es el extra ABSORBIBLE (tras tapar faltante, retardo y restaurar banco).
            if (pagoBase + bancoBase > extraAbsorbible)
            {
                throw new InvalidOperationException(
                    $"La suma base de pago ({pagoBase}) y banco ({bancoBase}) no puede exceder el tiempo extra absorbible del periodo ({extraAbsorbible} min). "
                    + $"Extra {extraDetectado} − faltante neto {faltanteNetoPeriodo} − retardo {retardoDetectado} − banco consumido {bancoConsumidoPeriodo} = {extraAbsorbible} min pagables.");
            }

            if (!bancoHorasHabilitado && bancoBase > 0)
            {
                throw new InvalidOperationException("El banco de horas no está habilitado para esta empresa.");
            }

            pago = (int)Math.Round(pagoBase * Math.Max(1m, factorTiempoExtra), MidpointRounding.AwayFromZero);
            banco = bancoHorasHabilitado
                ? (int)Math.Round(bancoBase * factorAcumulacionBanco, MidpointRounding.AwayFromZero)
                : 0;

            // Fase 5 — split legal del PAGO: los primeros minutos hasta el techo
            // configurable (HorasExtraDoblesPorSemana) se pagan como dobles; el
            // excedente como triples. Solo aplica al PAGO; el banco no se reparte.
            var configuracionNomina = await NominaConfiguracionLoader.LoadAsync(db, command.EmpresaId);
            var minutosDoblesTope = Math.Max(0, configuracionNomina.HorasExtraDoblesPorSemana) * 60;
            minutosDobles = Math.Min(pagoBase, minutosDoblesTope);
            minutosTriples = Math.Max(0, pagoBase - minutosDobles);
            minutosSimples = 0;
            horasExtraFactoradas = 0m;
            factorTiempoExtraAplicado = factorTiempoExtra;
            factorAcumulacionBancoAplicado = factorAcumulacionBanco;
        }

        // Movimientos previos del periodo (extra-banco + restauracion-banco): se
        // reemplazan al re-autorizar (idempotencia del ledger).
        var referenciaExtraBanco = ConstruirReferenciaPeriodo(command.EmpleadoId, periodo.PeriodoKey, "extra-banco");
        var referenciaRestauracionBanco = ConstruirReferenciaPeriodo(command.EmpleadoId, periodo.PeriodoKey, "restauracion-banco");
        var movimientosPrevios = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == command.EmpresaId
                && m.EmpleadoId == command.EmpleadoId
                && m.IsActive
                && (m.ReferenciaTipo == referenciaExtraBanco || m.ReferenciaTipo == referenciaRestauracionBanco))
            .ToListAsync(cancellationToken);

        // minutosBancoPrevios = neto de los movimientos previos (ambos positivos:
        // extra-banco genera banco, restauracion-banco repone consumido).
        var minutosBancoPrevios = (int)Math.Round(movimientosPrevios.Sum(m => m.Horas) * 60m, MidpointRounding.AwayFromZero);
        var saldoBancoDisponible = Math.Max(0, contexto.SaldoBancoHorasMinutos - minutosBancoPrevios);

        // Tope de banco (acumulado). La RESTAURACION está exenta del tope (repone
        // tiempo consumido, no es acumulación nueva — mismo principio que el path
        // diario legado eximía cubiertoBanco). Solo el banco del operador se topa.
        var saldoTrasRestauracion = saldoBancoDisponible + bancoRestaurado;
        var saldoFinalBanco = saldoTrasRestauracion + banco;
        var topeBancoMinutos = contexto.Configuracion.TopeBancoMinutos;
        if (banco > Math.Max(0, topeBancoMinutos - saldoTrasRestauracion))
        {
            var maximoAcumulable = Math.Max(0, topeBancoMinutos - saldoTrasRestauracion);
            throw new InvalidOperationException(
                $"La resolución excede el tope de banco de horas ({topeBancoMinutos} min). Máximo acumulable con esta decisión: {maximoAcumulable} min.");
        }

        if (movimientosPrevios.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientosPrevios);
        }

        // Fase 8 — idempotencia de líneas: reemplazar las previas del periodo antes
        // de insertar las nuevas (solo cuando se autoriza por líneas).
        var lineasPrevias = await db.RrhhResolucionesTiempoExtraLinea
            .Where(l => l.ResolucionPeriodoId == periodo.Id)
            .ToListAsync(cancellationToken);
        if (lineasPrevias.Count > 0)
        {
            db.RrhhResolucionesTiempoExtraLinea.RemoveRange(lineasPrevias);
        }

        if (command.Lineas.Count > 0)
        {
            var orden = 0;
            foreach (var linea in command.Lineas)
            {
                db.RrhhResolucionesTiempoExtraLinea.Add(new RrhhResolucionTiempoExtraLinea
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = command.EmpresaId,
                    EmpleadoId = command.EmpleadoId,
                    ResolucionPeriodoId = periodo.Id,
                    Orden = orden++,
                    Destino = linea.Destino,
                    Minutos = Math.Max(0, linea.Minutos),
                    Factor = Math.Max(0m, linea.Factor),
                    Observaciones = linea.Observaciones,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = command.UsuarioActual,
                    IsActive = true
                });
            }
        }

        // Snapshot de la detección en la entidad (autoritativo al momento de autorizar).
        periodo.MinutosExtraDetectado = extraDetectado;
        periodo.MinutosFaltanteDetectado = faltanteDetectado;
        periodo.MinutosFaltanteNetoDetectado = faltanteNetoPeriodo;
        periodo.MinutosFaltanteAbsorbidoExtra = faltanteAbsorbido;
        periodo.MinutosRetardoAbsorbidoExtra = retardoAbsorbido;
        periodo.MinutosBancoConsumidoDetectado = bancoConsumidoPeriodo;
        periodo.MinutosBancoRestauradoExtra = bancoRestaurado;
        periodo.MinutosRetardoDetectado = retardoDetectado;
        periodo.MinutosTrabajadosNetosDetectado = netoDetectado;
        periodo.MinutosExtraPago = pagoBase;
        periodo.MinutosExtraBanco = bancoBase;
        periodo.MinutosExtraDobles = minutosDobles;
        periodo.MinutosExtraTriples = minutosTriples;
        periodo.MinutosExtraSimples = minutosSimples;
        periodo.HorasExtraFactoradas = horasExtraFactoradas;
        periodo.FactorTiempoExtraAplicado = factorTiempoExtraAplicado;
        periodo.FactorAcumulacionBancoHorasAplicado = factorAcumulacionBancoAplicado;
        periodo.Resolucion = command.Resolucion;
        periodo.Estatus = RrhhResolucionPeriodoEstatus.Autorizada;
        periodo.ExtraDescartado = command.DescartarExtra;
        periodo.AutorizadoPor = command.UsuarioActual;
        periodo.FechaAutorizacion = DateTime.UtcNow;
        periodo.Observaciones = command.Observaciones;
        periodo.UpdatedAt = DateTime.UtcNow;
        periodo.UpdatedBy = command.UsuarioActual;

        if (banco > 0)
        {
            db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
            {
                Id = Guid.NewGuid(),
                EmpresaId = command.EmpresaId,
                EmpleadoId = command.EmpleadoId,
                Fecha = fechaFin,
                TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
                Horas = banco / 60m,
                ReferenciaTipo = referenciaExtraBanco,
                Observaciones = string.IsNullOrWhiteSpace(command.Observaciones)
                    ? $"Generado desde resolución de periodo {periodo.PeriodoEtiqueta}."
                    : command.Observaciones.Trim(),
                EsAutomatico = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.UsuarioActual,
                IsActive = true
            });
        }

        // Fase 4 — restauración del banco consumido en el periodo: movimiento
        // POSITIVO que repone el saldo consumido (mismo TipoMovimiento que la
        // generación por extra; se distingue por ReferenciaTipo).
        if (bancoRestaurado > 0)
        {
            db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
            {
                Id = Guid.NewGuid(),
                EmpresaId = command.EmpresaId,
                EmpleadoId = command.EmpleadoId,
                Fecha = fechaFin,
                TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
                Horas = bancoRestaurado / 60m,
                ReferenciaTipo = referenciaRestauracionBanco,
                Observaciones = $"Restauración de banco consumido en el periodo {periodo.PeriodoEtiqueta}.",
                EsAutomatico = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.UsuarioActual,
                IsActive = true
            });
        }

        return new RrhhResolucionPeriodoResult
        {
            Periodo = periodo,
            SaldoBancoActualMinutos = saldoFinalBanco,
            TopeBancoMinutos = topeBancoMinutos,
            FactorTiempoExtra = factorTiempoExtraAplicado ?? contexto.Configuracion.FactorTiempoExtra,
            BancoHorasHabilitado = bancoHorasHabilitado,
            FactorAcumulacionBancoHoras = factorAcumulacionBancoAplicado ?? contexto.Configuracion.FactorAcumulacionBancoHoras,
            MinutosBasePagoAplicados = pagoBase,
            MinutosBaseBancoAplicados = bancoBase,
            MinutosPagoAplicados = pago,
            MinutosBancoAplicados = banco,
            BitacoraDetalle = $"empleado={command.EmpleadoId};periodo={periodo.PeriodoKey};resolucion={command.Resolucion};extraDetectado={extraDetectado};faltanteNeto={faltanteNetoPeriodo};retardo={retardoDetectado};bancoConsumido={bancoConsumidoPeriodo};bancoRestaurado={bancoRestaurado};pagoBase={pagoBase};pagoFactorado={pago};bancoBase={bancoBase};bancoFactorado={banco};obs={command.Observaciones}"
        };
    }

    public async Task ReabrirPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, string usuarioActual, CancellationToken cancellationToken = default)
    {
        var (empleado, corte) = await CargarEmpleadoYCorteAsync(db, empresaId, empleadoId, cancellationToken);
        if (empleado.TipoNomina == TipoNomina.Destajo)
        {
            return;
        }

        var calendario = ResolverPeriodo(empleado, fechaReferencia, corte);
        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId
                && r.EmpleadoId == empleadoId
                && r.PeriodicidadPago == calendario.PeriodicidadPago
                && r.AnioPeriodo == calendario.AnioPeriodo
                && r.NumeroPeriodo == calendario.NumeroPeriodo, cancellationToken);

        if (periodo is null || periodo.Estatus != RrhhResolucionPeriodoEstatus.Autorizada)
        {
            return;
        }

        var referenciaExtraBanco = ConstruirReferenciaPeriodo(empleadoId, periodo.PeriodoKey, "extra-banco");
        var referenciaRestauracionBanco = ConstruirReferenciaPeriodo(empleadoId, periodo.PeriodoKey, "restauracion-banco");
        var movimientosPrevios = await db.RrhhBancoHorasMovimientos
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == empleadoId
                && m.IsActive
                && (m.ReferenciaTipo == referenciaExtraBanco || m.ReferenciaTipo == referenciaRestauracionBanco))
            .ToListAsync(cancellationToken);

        if (movimientosPrevios.Count > 0)
        {
            db.RrhhBancoHorasMovimientos.RemoveRange(movimientosPrevios);
        }

        // Fase 8 — al reabrir, también descartar las líneas de la resolución previa.
        var lineasPrevias = await db.RrhhResolucionesTiempoExtraLinea
            .Where(l => l.ResolucionPeriodoId == periodo.Id)
            .ToListAsync(cancellationToken);
        if (lineasPrevias.Count > 0)
        {
            db.RrhhResolucionesTiempoExtraLinea.RemoveRange(lineasPrevias);
        }

        periodo.Estatus = RrhhResolucionPeriodoEstatus.Reabierta;
        periodo.ExtraDescartado = false;
        periodo.MinutosExtraPago = 0;
        periodo.MinutosExtraBanco = 0;
        periodo.MinutosExtraDobles = 0;
        periodo.MinutosExtraTriples = 0;
        periodo.MinutosExtraSimples = 0;
        periodo.HorasExtraFactoradas = 0m;
        periodo.FactorTiempoExtraAplicado = null;
        periodo.FactorAcumulacionBancoHorasAplicado = null;
        periodo.FechaAutorizacion = null;
        periodo.Observaciones = $"Reabierto por corrección de marcación ({usuarioActual}).";
        periodo.UpdatedAt = DateTime.UtcNow;
        periodo.UpdatedBy = usuarioActual;
    }

    public async Task<RrhhResolucionPeriodoBackfillResult> BackfillDesdeAutorizacionDiariaAsync(
        CrmDbContext db, Guid? empresaId = null, string usuarioActual = "backfill", CancellationToken cancellationToken = default)
    {
        // Solo asistencias con autorización diaria heredada (pago o banco > 0).
        var asistencias = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => (empresaId == null || a.EmpresaId == empresaId.Value)
                && (a.MinutosExtraAutorizadosPago > 0 || a.MinutosExtraAutorizadosBanco > 0))
            .ToListAsync(cancellationToken);

        if (asistencias.Count == 0)
        {
            return new RrhhResolucionPeriodoBackfillResult();
        }

        var empleadoIds = asistencias.Select(a => a.EmpleadoId).Distinct().ToList();
        var empresaIds = asistencias.Select(a => a.EmpresaId).Distinct().ToList();

        var empleados = await db.Empleados
            .AsNoTracking()
            .Where(e => empleadoIds.Contains(e.Id))
            .Select(e => new { e.Id, e.EmpresaId, e.PeriodicidadPago, e.TipoNomina })
            .ToListAsync(cancellationToken);
        var empleadosPorId = empleados.ToDictionary(e => e.Id);

        var cortes = await db.NominaCortesRrhh
            .AsNoTracking()
            .Where(c => empresaIds.Contains(c.EmpresaId))
            .ToListAsync(cancellationToken);
        var cortesPorEmpresaPeriodicidad = cortes
            .GroupBy(c => (c.EmpresaId, c.PeriodicidadPago))
            .ToDictionary(g => g.Key, g => g.First());

        // Entidades de periodo ya existentes (para idempotencia: no sobreescribir).
        var existentes = await db.RrhhResolucionesTiempoExtraPeriodo
            .Where(r => (empresaId == null || r.EmpresaId == empresaId.Value)
                && empleadoIds.Contains(r.EmpleadoId))
            .Select(r => new { r.EmpresaId, r.EmpleadoId, r.PeriodicidadPago, r.AnioPeriodo, r.NumeroPeriodo })
            .ToListAsync(cancellationToken);
        var existentesClave = existentes
            .Select(r => (r.EmpresaId, r.EmpleadoId, r.PeriodicidadPago, r.AnioPeriodo, r.NumeroPeriodo))
            .ToHashSet();

        var periodosCreados = 0;
        var periodosOmitidos = 0;
        var empleadosProcesados = new HashSet<Guid>();

        foreach (var grupoEmpleado in asistencias.GroupBy(a => a.EmpleadoId))
        {
            if (!empleadosPorId.TryGetValue(grupoEmpleado.Key, out var empleado))
            {
                continue;
            }
            if (empleado.TipoNomina == TipoNomina.Destajo)
            {
                continue;
            }

            cortesPorEmpresaPeriodicidad.TryGetValue((empleado.EmpresaId, empleado.PeriodicidadPago), out var corte);

            // Agrupa por periodo resuelto (clave: periodicidad + año + número).
            foreach (var grupoPeriodo in grupoEmpleado.GroupBy(a =>
                {
                    var cal = NominaPeriodoHelper.ObtenerPeriodo(empleado.PeriodicidadPago, a.Fecha.ToDateTime(TimeOnly.MinValue), corte);
                    return (cal.PeriodicidadPago, cal.AnioPeriodo, cal.NumeroPeriodo);
                }))
            {
                var (periodicidad, anio, numero) = grupoPeriodo.Key;
                if (existentesClave.Contains((empleado.EmpresaId, empleado.Id, periodicidad, anio, numero)))
                {
                    periodosOmitidos++;
                    continue;
                }

                var calendario = NominaPeriodoHelper.ObtenerPeriodo(
                    empleado.PeriodicidadPago,
                    grupoPeriodo.First().Fecha.ToDateTime(TimeOnly.MinValue),
                    corte);

                var lista = grupoPeriodo.ToList();
                var permisosPorDiaPeriodo = await ConstruirPermisosConGocePorDiaAsync(
                    db, empleado.EmpresaId, empleado.Id,
                    DateOnly.FromDateTime(calendario.Inicio), DateOnly.FromDateTime(calendario.Fin),
                    cancellationToken);
                var pagoBase = lista.Sum(a => Math.Max(0, a.MinutosExtraAutorizadosPago));
                var bancoBase = lista.Sum(a => Math.Max(0, a.MinutosExtraAutorizadosBanco));
                var extraDetectado = lista.Sum(a => Math.Max(0, a.MinutosExtra));
                var faltanteDetectado = lista.Sum(RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteNeto);
                var retardoDetectado = lista.Sum(a => RrhhTiempoExtraPolicy.ObtenerMinutosRetardoEfectivos(a, permisosPorDiaPeriodo.GetValueOrDefault(a.Fecha)));
                var netoDetectado = lista.Sum(RrhhTiempoExtraPolicy.ObtenerMinutosNetoEfectivo);

                db.RrhhResolucionesTiempoExtraPeriodo.Add(new RrhhResolucionTiempoExtraPeriodo
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empleado.EmpresaId,
                    EmpleadoId = empleado.Id,
                    PeriodicidadPago = periodicidad,
                    AnioPeriodo = anio,
                    NumeroPeriodo = numero,
                    PeriodoKey = $"{periodicidad}-{anio}-{numero:00}",
                    PeriodoEtiqueta = calendario.Periodo,
                    FechaInicio = DateOnly.FromDateTime(calendario.Inicio),
                    FechaFin = DateOnly.FromDateTime(calendario.Fin),
                    MinutosExtraDetectado = extraDetectado,
                    MinutosFaltanteDetectado = faltanteDetectado,
                    MinutosRetardoDetectado = retardoDetectado,
                    MinutosTrabajadosNetosDetectado = netoDetectado,
                    MinutosExtraPago = pagoBase,
                    MinutosExtraBanco = bancoBase,
                    Estatus = RrhhResolucionPeriodoEstatus.Autorizada,
                    AutorizadoPor = usuarioActual,
                    FechaAutorizacion = DateTime.UtcNow,
                    Observaciones = "Migración one-shot desde autorización diaria histórica.",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = usuarioActual
                });

                periodosCreados++;
                empleadosProcesados.Add(empleado.Id);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new RrhhResolucionPeriodoBackfillResult
        {
            EmpleadosProcesados = empleadosProcesados.Count,
            PeriodosCreados = periodosCreados,
            PeriodosOmitidos = periodosOmitidos
        };
    }

    public async Task<RrhhResolucionPeriodoBackfillLineasResult> SembrarLineasEnResolucionesAutorizadasAsync(
        CrmDbContext db, Guid? empresaId = null, string usuarioActual = "backfill", CancellationToken cancellationToken = default)
    {
        // Fase 9 — resoluciones Autorizada PRE-Fase 8 (sin líneas). Se reconstruyen las
        // líneas a partir de los escalares persistidos para que el nuevo UI las muestre y
        // para que la resolución pase al path por líneas (factoradas). El monto se preserva:
        // ver SembrarLineasDesdeEscalares para la demostración de equivalencia.
        var periodos = await db.RrhhResolucionesTiempoExtraPeriodo
            .Where(r => (empresaId == null || r.EmpresaId == empresaId.Value)
                && r.Estatus == RrhhResolucionPeriodoEstatus.Autorizada
                && r.IsActive)
            .ToListAsync(cancellationToken);

        if (periodos.Count == 0)
        {
            return new RrhhResolucionPeriodoBackfillLineasResult();
        }

        var periodosIds = periodos.Select(p => p.Id).ToList();
        var periodosConLineas = await db.RrhhResolucionesTiempoExtraLinea
            .Where(l => periodosIds.Contains(l.ResolucionPeriodoId))
            .Select(l => l.ResolucionPeriodoId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var conLineasSet = periodosConLineas.ToHashSet();

        // Cache de configuración de nómina por empresa (factores + tope dobles).
        var empresaIds = periodos.Select(p => p.EmpresaId).Distinct().ToList();
        var configuraciones = new Dictionary<Guid, NominaConfiguracion>();
        foreach (var emp in empresaIds)
        {
            configuraciones[emp] = await NominaConfiguracionLoader.LoadAsync(db, emp);
        }

        var procesados = 0;
        var omitidos = 0;
        var lineasCreadas = 0;

        foreach (var periodo in periodos)
        {
            if (conLineasSet.Contains(periodo.Id))
            {
                omitidos++;
                continue;
            }

            var config = configuraciones[periodo.EmpresaId];
            var lineasSembradas = SembrarLineasDesdeEscalares(periodo, config, usuarioActual);
            if (lineasSembradas.Count == 0)
            {
                omitidos++; // sin pago ni banco que sembrar
                continue;
            }

            foreach (var linea in lineasSembradas)
            {
                db.RrhhResolucionesTiempoExtraLinea.Add(linea);
            }
            lineasCreadas += lineasSembradas.Count;

            // Recalcular escalares derivados desde las líneas sembradas y conmutar al
            // path por líneas (FactorTiempoExtraAplicado=null → el sourcing usa factoradas).
            periodo.MinutosExtraDobles = lineasSembradas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor == 2m)
                .Sum(l => l.Minutos);
            periodo.MinutosExtraTriples = lineasSembradas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor == 3m)
                .Sum(l => l.Minutos);
            periodo.MinutosExtraSimples = lineasSembradas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago && l.Factor != 2m && l.Factor != 3m)
                .Sum(l => l.Minutos);
            periodo.HorasExtraFactoradas = lineasSembradas
                .Where(l => l.Destino == RrhhDestinoTiempoExtraLinea.Pago)
                .Sum(l => l.Minutos / 60m * l.Factor);
            periodo.FactorTiempoExtraAplicado = null;
            periodo.FactorAcumulacionBancoHorasAplicado = null;
            periodo.UpdatedAt = DateTime.UtcNow;
            periodo.UpdatedBy = usuarioActual;

            procesados++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new RrhhResolucionPeriodoBackfillLineasResult
        {
            PeriodosProcesados = procesados,
            PeriodosOmitidos = omitidos,
            LineasCreadas = lineasCreadas
        };
    }

    /// <summary>
    /// Reconstruye las líneas de una resolución Autorizada pre-Fase 8 a partir de los
    /// escalares persistidos. Reproduce el monto del path legado:
    /// <list type="bullet">
    /// <item><b>Override</b> (FactorTiempoExtraAplicado con valor): el legado aplicaba ese
    /// factor a dobles Y triples → dos líneas @ ese factor. monto = (dobles+triples)/60 × F × sh
    /// = factoradas × sh. ✓</item>
    /// <item><b>Config</b> (factor null, backfill desde daily): el legado caía a
    /// FactorHoraExtra/FactorHoraExtraTriple → dos líneas con esos factores. monto =
    /// dobles/60×F2×sh + triples/60×F3×sh = factoradas × sh. ✓</item>
    /// </list>
    /// El split dobles/triples usa los escalares persistidos si los hay (Fase 5); si no
    /// (backfill desde daily), se deriva por el techo HorasExtraDoblesPorSemana.
    /// El banco usa una línea @ (FactorAcumulacionBancoHorasAplicado ?? config).
    /// </summary>
    private static List<RrhhResolucionTiempoExtraLinea> SembrarLineasDesdeEscalares(
        RrhhResolucionTiempoExtraPeriodo periodo, NominaConfiguracion config, string usuarioActual)
    {
        var lineas = new List<RrhhResolucionTiempoExtraLinea>();
        var orden = 0;

        var pagoBase = Math.Max(0, periodo.MinutosExtraPago);
        var bancoBase = Math.Max(0, periodo.MinutosExtraBanco);

        if (pagoBase > 0)
        {
            // Split dobles/triples: persistido (Fase 5) o derivado por tope (backfill desde daily).
            int doblesMin;
            int triplesMin;
            if (periodo.MinutosExtraDobles > 0 || periodo.MinutosExtraTriples > 0)
            {
                doblesMin = Math.Min(pagoBase, Math.Max(0, periodo.MinutosExtraDobles));
                triplesMin = Math.Max(0, pagoBase - doblesMin);
            }
            else
            {
                var topeMin = Math.Max(0, config.HorasExtraDoblesPorSemana) * 60;
                doblesMin = Math.Min(pagoBase, topeMin);
                triplesMin = Math.Max(0, pagoBase - doblesMin);
            }

            // Factores: override (mismo factor a dobles y triples) o config (F2/F3 distintos).
            decimal factorDobles;
            decimal factorTriples;
            if (periodo.FactorTiempoExtraAplicado.HasValue && periodo.FactorTiempoExtraAplicado.Value > 0m)
            {
                factorDobles = periodo.FactorTiempoExtraAplicado.Value;
                factorTriples = periodo.FactorTiempoExtraAplicado.Value;
            }
            else
            {
                factorDobles = Math.Max(0m, config.FactorHoraExtra);
                factorTriples = Math.Max(0m, config.FactorHoraExtraTriple);
            }

            if (doblesMin > 0)
            {
                lineas.Add(NuevaLineaSembrada(periodo, orden++, RrhhDestinoTiempoExtraLinea.Pago, doblesMin, factorDobles, usuarioActual));
            }
            if (triplesMin > 0)
            {
                lineas.Add(NuevaLineaSembrada(periodo, orden++, RrhhDestinoTiempoExtraLinea.Pago, triplesMin, factorTriples, usuarioActual));
            }
        }

        if (bancoBase > 0)
        {
            var factorBanco = periodo.FactorAcumulacionBancoHorasAplicado.HasValue
                && periodo.FactorAcumulacionBancoHorasAplicado.Value > 0m
                    ? periodo.FactorAcumulacionBancoHorasAplicado.Value
                    : Math.Max(0m, config.BancoHorasFactorAcumulacion);
            lineas.Add(NuevaLineaSembrada(periodo, orden++, RrhhDestinoTiempoExtraLinea.Banco, bancoBase, factorBanco, usuarioActual));
        }

        return lineas;
    }

    private static RrhhResolucionTiempoExtraLinea NuevaLineaSembrada(
        RrhhResolucionTiempoExtraPeriodo periodo, int orden, RrhhDestinoTiempoExtraLinea destino,
        int minutos, decimal factor, string usuarioActual)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = periodo.EmpresaId,
            EmpleadoId = periodo.EmpleadoId,
            ResolucionPeriodoId = periodo.Id,
            Orden = orden,
            Destino = destino,
            Minutos = minutos,
            Factor = factor,
            Observaciones = "Línea sembrada por backfill Fase 9 desde escalares pre-Fase 8.",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = usuarioActual,
            IsActive = true
        };

    private async Task<(Empleado Empleado, NominaCorteRrhh? Corte)> CargarEmpleadoYCorteAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, CancellationToken cancellationToken)
    {
        var empleado = await db.Empleados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empleadoId && e.EmpresaId == empresaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontró el empleado.");

        var corte = await db.NominaCortesRrhh
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.PeriodicidadPago == empleado.PeriodicidadPago, cancellationToken);

        return (empleado, corte);
    }

    /// <summary>
    /// Minutos de permiso CON GOCE prorrateados por día del periodo (misma regla
    /// que usa la vista semanal). El faltante cubierto por permiso NO debe ser
    /// tapado por el extra (Fase 2): el "faltante neto" es el bruto menos esto.
    /// </summary>
    private static async Task<Dictionary<DateOnly, int>> ConstruirPermisosConGocePorDiaAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaInicio, DateOnly fechaFin, CancellationToken cancellationToken)
    {
        var permisos = await db.RrhhAusencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId
                && a.EmpleadoId == empleadoId
                && a.IsActive
                && a.ConGocePago
                && a.Horas > 0
                && (a.Estatus == EstatusAusenciaRrhh.Aprobada || a.Estatus == EstatusAusenciaRrhh.Aplicada)
                && a.FechaInicio <= fechaFin
                && a.FechaFin >= fechaInicio)
            .ToListAsync(cancellationToken);

        var resultado = new Dictionary<DateOnly, int>();
        foreach (var permiso in permisos)
        {
            var minutosPorDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permiso);
            var inicio = permiso.FechaInicio < fechaInicio ? fechaInicio : permiso.FechaInicio;
            var fin = permiso.FechaFin > fechaFin ? fechaFin : permiso.FechaFin;
            for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
            {
                resultado[fecha] = resultado.GetValueOrDefault(fecha) + minutosPorDia;
            }
        }
        return resultado;
    }

    /// <summary>
    /// Minutos de banco CONSUMIDOS en el periodo (Fase 4): suma de movimientos
    /// <see cref="TipoMovimientoBancoHorasRrhh.Consumo"/> con Fecha dentro del
    /// periodo. Se EXCLUYE la cobertura-banco (<c>Asistencia:{id}:cobertura-banco</c>),
    /// porque ese consumo ya está representado como faltante neto en Fase 2
    /// (no hay RrhhAusencias que lo descuente) — incluirlo sería doble conteo.
    /// El Consumo se guarda con Horas NEGATIVAS, de ahí el signo invertido.
    /// </summary>
    private static async Task<int> ObtenerMinutosBancoConsumidoPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaInicio, DateOnly fechaFin, CancellationToken cancellationToken)
    {
        var consumos = await db.RrhhBancoHorasMovimientos
            .AsNoTracking()
            .Where(m => m.EmpresaId == empresaId
                && m.EmpleadoId == empleadoId
                && m.IsActive
                && m.TipoMovimiento == TipoMovimientoBancoHorasRrhh.Consumo
                && m.Fecha >= fechaInicio
                && m.Fecha <= fechaFin)
            .Select(m => new { m.Horas, m.ReferenciaTipo })
            .ToListAsync(cancellationToken);

        var consumidoHoras = consumos
            .Where(c => !EsCoberturaBanco(c.ReferenciaTipo))
            .Sum(c => -c.Horas); // Consumo se guarda negativo → invertir para obtener minutos consumidos

        return (int)Math.Round(consumidoHoras * 60m, MidpointRounding.AwayFromZero);
    }

    private static bool EsCoberturaBanco(string? referenciaTipo)
        => !string.IsNullOrEmpty(referenciaTipo) && referenciaTipo.Contains("cobertura-banco", StringComparison.Ordinal);

    private static NominaPeriodoCalendario ResolverPeriodo(Empleado empleado, DateOnly fechaReferencia, NominaCorteRrhh? corte)
        => NominaPeriodoHelper.ObtenerPeriodo(
            empleado.PeriodicidadPago,
            fechaReferencia.ToDateTime(TimeOnly.MinValue),
            corte);

    private static void GarantizarAplicable(Empleado empleado)
    {
        if (empleado.TipoNomina == TipoNomina.Destajo)
        {
            throw new InvalidOperationException(
                "Los empleados de destajo no participan en la resolución de tiempo extra por periodo.");
        }
    }

    private static string ConstruirPeriodoKey(NominaPeriodoCalendario calendario)
        => $"{calendario.PeriodicidadPago}-{calendario.AnioPeriodo}-{calendario.NumeroPeriodo:00}";

    private static string ConstruirReferenciaPeriodo(Guid empleadoId, string periodoKey, string sufijo)
        => $"{ReferenciaPeriodoExtraBancoPrefix}:{empleadoId:N}:{periodoKey}:{sufijo}";

    public async Task<IReadOnlyList<Guid>> ObtenerEmpleadosConExtraSinAutorizarAsync(
        CrmDbContext db, Guid empresaId, PeriodicidadPago periodicidad,
        DateTime inicio, DateTime fin, CancellationToken cancellationToken = default)
    {
        var fechaInicio = DateOnly.FromDateTime(inicio);
        var fechaFin = DateOnly.FromDateTime(fin);

        // Empleados con extra detectado en el rango (MinutosExtra >= 0, así que cualquier
        // fila > 0 implica Sum > 0). Distinct evita duplicar por día.
        var empleadosConExtra = await db.RrhhAsistencias
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId
                && a.Fecha >= fechaInicio && a.Fecha <= fechaFin
                && a.MinutosExtra > 0)
            .Select(a => a.EmpleadoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (empleadosConExtra.Count == 0)
            return Array.Empty<Guid>();

        // Acota a activos de la periodicidad (prenómina/nómina son por periodicidad).
        var activosPeriodicidad = await db.Empleados
            .AsNoTracking()
            .Where(e => e.EmpresaId == empresaId && e.IsActive && e.PeriodicidadPago == periodicidad
                && empleadosConExtra.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        if (activosPeriodicidad.Count == 0)
            return Array.Empty<Guid>();

        // Resoluciones Autorizadas del periodo (lookup por fechas, igual que el snapshot).
        var autorizados = await db.RrhhResolucionesTiempoExtraPeriodo
            .AsNoTracking()
            .Where(r => r.EmpresaId == empresaId
                && r.FechaInicio == fechaInicio && r.FechaFin == fechaFin
                && r.Estatus == RrhhResolucionPeriodoEstatus.Autorizada
                && r.IsActive
                && activosPeriodicidad.Contains(r.EmpleadoId))
            .Select(r => r.EmpleadoId)
            .ToListAsync(cancellationToken);

        var autorizadosSet = autorizados.ToHashSet();
        return activosPeriodicidad.Where(id => !autorizadosSet.Contains(id)).ToList();
    }
}