using Nominas.Entidades;

namespace Nominas.Web.Pages
{
    /// <summary>
    /// Color del renglón de una tabla según el estado del registro.
    /// Se usa así en las vistas:  &lt;tr class="@ColorEstado.Fila(e.Estatus)"&gt;
    ///
    /// Hay 5 colores, definidos en wwwroot/css/site.css (sección "Renglones por estado"),
    /// y son los mismos tonos que ya usan las etiquetas (badges) de estado:
    ///   fila-exito   verde      todo en orden (activo, pagada, entregado)
    ///   fila-aviso   ámbar      requiere atención (borrador, suspendida, baja temporal)
    ///   fila-peligro rojo       falta algo urgente (pago pendiente)
    ///   fila-marca   azul CAVI  en curso (nómina autorizada, préstamo activo)
    ///   fila-neutra  gris       ya terminó (terminada, baja definitiva, liquidado)
    /// </summary>
    public static class ColorEstado
    {
        public static string Fila(EstatusEmpleado estatus) => estatus switch
        {
            EstatusEmpleado.Activo => "fila-exito",
            EstatusEmpleado.BajaTemporal => "fila-aviso",
            _ => "fila-neutra"                       // baja definitiva
        };

        public static string Fila(EstatusObra estatus) => estatus switch
        {
            EstatusObra.Activa => "fila-exito",
            EstatusObra.Suspendida => "fila-aviso",
            _ => "fila-neutra"                       // terminada
        };

        public static string Fila(EstatusNomina estatus) => estatus switch
        {
            EstatusNomina.Borrador => "fila-aviso",
            EstatusNomina.Autorizada => "fila-marca",
            _ => "fila-exito"                        // pagada
        };

        public static string Fila(EstatusPago estatus) => estatus switch
        {
            EstatusPago.Pendiente => "fila-peligro",
            _ => "fila-exito"                        // entregado
        };

        public static string Fila(EstatusPrestamo estatus) => estatus switch
        {
            EstatusPrestamo.Activo => "fila-marca",
            _ => "fila-neutra"                       // liquidado
        };
    }
}
