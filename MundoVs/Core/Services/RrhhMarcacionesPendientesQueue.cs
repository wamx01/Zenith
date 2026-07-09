using System.Threading.Channels;

namespace MundoVs.Core.Services;

/// <summary>
/// Identifica de forma única un par (empresa, checador) que requiere que el
/// procesador de marcaciones pendientes se ejecute.
/// </summary>
public readonly record struct RrhhColaPendientesItem(Guid EmpresaId, Guid ChecadorId);

/// <summary>
/// Cola FIFO en memoria que desacopla el endpoint HTTP de ingestión de
/// marcaciones del procesador pesado que clasifica las marcaciones y genera
/// las asistencias. Permite que el endpoint responda en milisegundos aunque
/// el procesador tarde minutos en vaciar el backlog acumulado.
/// </summary>
public sealed class RrhhMarcacionesPendientesQueue
{
    private readonly Channel<RrhhColaPendientesItem> _channel;

    public RrhhMarcacionesPendientesQueue()
    {
        // Unbounded: en operación normal la cola drena a 0-1 items; un tope
        // artificial podría bloquear al productor (el endpoint HTTP) en
        // escenarios de pico, mientras que el productor nunca debe esperar.
        // Si el servidor se reinicia con items en cola, se pierden: el agente
        // re-enviará las marcaciones y, con el re-marcado de duplicadas en
        // IngerirLoteAsync, las pendientes se reprocesan en el siguiente ciclo.
        _channel = Channel.CreateUnbounded<RrhhColaPendientesItem>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    /// <summary>
    /// Encola una solicitud de reproceso. No espera: el productor (endpoint
    /// HTTP) nunca debe bloquearse.
    /// </summary>
    public ValueTask EnqueueAsync(RrhhColaPendientesItem item, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(item, cancellationToken);

    /// <summary>
    /// Lee el siguiente item encolado. Solo debe ser invocado por un único
    /// consumidor (el worker dedicado).
    /// </summary>
    public ValueTask<RrhhColaPendientesItem> DequeueAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAsync(cancellationToken);

    /// <summary>
    /// Cantidad aproximada de items pendientes. Útil para diagnóstico.
    /// </summary>
    public int Count => _channel.Reader.Count;
}
