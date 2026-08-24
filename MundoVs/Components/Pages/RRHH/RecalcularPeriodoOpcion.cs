namespace MundoVs.Components.Pages.RRHH;

// Opción elegida por el operador en el diálogo previo a "Recalcular periodo". Es la intención
// del UI; cada página la traduce a los parámetros del processor (forzarDefaultEmpleado /
// modoCalculoForzado). DefaultEmpleado = "usa lo configurado en Empleados"; VsHorario y
// MarcajeReloj = fuerza ese método concreto sin importar el default.
// English: Option chosen by the operator in the pre-"Recalculate period" dialog. This is the
// UI intent; each page translates it into the processor params (forzarDefaultEmpleado /
// modoCalculoForzado). DefaultEmpleado = "use what's configured in Empleados"; VsHorario and
// MarcajeReloj = force that concrete method regardless of the default.
public enum RecalcularPeriodoOpcion
{
    DefaultEmpleado,
    VsHorario,
    MarcajeReloj
}