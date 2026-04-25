using System.Runtime.InteropServices;
using Zenith.Contracts.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;
using Zenith.Workers.Asistencia.Options;
using Microsoft.Extensions.Options;

namespace Zenith.Workers.Asistencia.Readers;

public sealed class ZkTecoMarcacionReader(
    IOptions<AsistenciaWorkerOptions> options,
    ILogger<ZkTecoMarcacionReader> logger) : IMarcacionReader
{
    public Task<IReadOnlyCollection<MarcacionRawDto>> LeerMarcacionesAsync(ChecadorConfigDto checador, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(checador.Ip))
        {
            logger.LogWarning("El checador {ChecadorNombre} no tiene IP configurada.", checador.Nombre);
            return Task.FromResult<IReadOnlyCollection<MarcacionRawDto>>([]);
        }

        object? sdkInstance = null;
        try
        {
            var progId = options.Value.ZkTecoProgId;
            var sdkType = Type.GetTypeFromProgID(progId, throwOnError: false);
            if (sdkType == null)
            {
                throw new InvalidOperationException($"No se encontró el ProgID '{progId}'. Instala el driver COM de ZKTeco en la máquina del worker.");
            }

            sdkInstance = Activator.CreateInstance(sdkType);
            if (sdkInstance == null)
            {
                throw new InvalidOperationException("No fue posible crear la instancia COM de ZKTeco.");
            }

            dynamic zk = sdkInstance;
            var conectado = zk.Connect_Net(checador.Ip, checador.Puerto);
            if (!conectado)
            {
                logger.LogWarning("No fue posible conectar al checador {ChecadorNombre} en {Ip}:{Puerto}.", checador.Nombre, checador.Ip, checador.Puerto);
                return Task.FromResult<IReadOnlyCollection<MarcacionRawDto>>([]);
            }

            try
            {
                var numeroMaquina = checador.NumeroMaquina <= 0 ? 1 : checador.NumeroMaquina;
                var leyo = zk.ReadGeneralLogData(numeroMaquina);
                if (!leyo)
                {
                    logger.LogInformation("El checador {ChecadorNombre} no devolvió registros al ejecutar ReadGeneralLogData.", checador.Nombre);
                    return Task.FromResult<IReadOnlyCollection<MarcacionRawDto>>([]);
                }

                var marcaciones = new List<MarcacionRawDto>();
                string enrollNumber = string.Empty;
                int verifyMode = 0;
                int inOutMode = 0;
                int year = 0;
                int month = 0;
                int day = 0;
                int hour = 0;
                int minute = 0;
                int second = 0;
                int workCode = 0;

                while (zk.SSR_GetGeneralLogData(numeroMaquina, out enrollNumber, out verifyMode, out inOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode))
                {
                    var fechaLocal = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
                    marcaciones.Add(new MarcacionRawDto
                    {
                        EmpresaId = checador.EmpresaId,
                        ChecadorId = checador.Id,
                        CodigoChecador = enrollNumber?.Trim() ?? string.Empty,
                        FechaHoraMarcacionUtc = fechaLocal.ToUniversalTime(),
                        TipoMarcacionRaw = inOutMode.ToString(),
                        Origen = "ZKTeco",
                        EventoIdExterno = $"{enrollNumber}-{year:D4}{month:D2}{day:D2}{hour:D2}{minute:D2}{second:D2}-{workCode}",
                        PayloadRaw = $"verify={verifyMode};inout={inOutMode};workcode={workCode}"
                    });
                }

                return Task.FromResult<IReadOnlyCollection<MarcacionRawDto>>(marcaciones);
            }
            finally
            {
                try
                {
                    zk.Disconnect();
                }
                catch
                {
                }
            }
        }
        catch (COMException ex)
        {
            logger.LogError(ex, "Error COM al leer el checador {ChecadorNombre}.", checador.Nombre);
            return Task.FromResult<IReadOnlyCollection<MarcacionRawDto>>([]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al leer marcaciones del checador {ChecadorNombre}.", checador.Nombre);
            return Task.FromResult<IReadOnlyCollection<MarcacionRawDto>>([]);
        }
        finally
        {
            if (sdkInstance != null && Marshal.IsComObject(sdkInstance))
            {
                Marshal.ReleaseComObject(sdkInstance);
            }
        }
    }
}