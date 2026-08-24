# 11. Prenómina en `RRHH`

> **Prenómina → Nómina (fusión 2026-08-22)**
>
> La etapa separada de **Prenómina** fue eliminada. La entidad `Prenomina` ya no existe en el sistema.
>
> El flujo de RRHH ahora tiene **2 páginas**:
>
> `Asistencia Semanal (neteo) -> Nómina`
>
> La página `/rrhh/prenominas` ahora **redirige a `/rrhh/nominas`**.
>
> La captura y revisión que antes vivía en prenómina ahora es una fase dentro de la propia Nómina:
> - **Cerrar periodo y calcular** — estampa `FechaCierreCaptura` y ejecuta el cálculo (solo sobre semana cerrada, `NominaPeriodoHelper.ObtenerPeriodo`).
> - **Reabrir captura** — limpia `FechaCierreCaptura` para volver a revisar antes del cierre definitivo.
>
> Toda la operatoria y el detalle del cálculo se documentan en:
>
> - [12. Nómina en RRHH](./12-rrhh-nomina.md)
> - [Mapa técnico de nómina](./rrhh-nomina-tecnica.md)
>
> Esta página se conserva únicamente como aviso de redirección para que los enlaces existentes no se rompan.

---

> Última revisión: 2026-08-22

## Ver también
- [08. Esquemas de pago](./08-rrhh-esquemas-de-pago.md)
- [09. Empleados](./09-rrhh-empleados.md)
- [10. Marcaciones y asistencias](./10-rrhh-marcaciones-y-asistencias.md)
- [12. Nómina](./12-rrhh-nomina.md) — antigua "Prenómina + Nómina" unificadas
- [13. Vales de destajo](./13-rrhh-vales-destajo.md)
- [15. Bonos y deducciones](./15-rrhh-bonos-y-deducciones.md)
- [16. Banco de horas](./16-rrhh-banco-de-horas.md)
- [17. Ausencias](./17-rrhh-ausencias.md)
- [Mapa técnico de nómina](./rrhh-nomina-tecnica.md)
- [Especificación técnica del módulo RRHH](../modulos/detalle/06-rrhh.md)