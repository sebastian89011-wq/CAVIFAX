using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;     // para EstatusEmpleado, EstatusObra, etc.
using Nominas.Web.Pages;     // para DatosPrueba

namespace AplicacionNomina.Pages
{
    public class IndexModel : PageModel
    {
        // Tarjetas de arriba
        public int EmpleadosActivos { get; set; }
        public int ObrasActivas { get; set; }
        public int PagosPendientes { get; set; }
        public decimal PagosPendientesTotal { get; set; }
        public int PrestamosActivos { get; set; }
        public decimal PrestamosTotal { get; set; }

        // Avisos
        public Nomina? NominaEnBorrador { get; set; }
        public int ObrasConSaldoBajo { get; set; }

        // Presupuesto de cada obra activa
        public List<ResumenObra> Obras { get; set; } = new();

        public void OnGet()
        {
            // Datos de prueba: el backend los cambiará por consultas a la base de datos
            EmpleadosActivos = DatosPrueba.Empleados
                .Count(e => e.Estatus == EstatusEmpleado.Activo);

            ObrasActivas = DatosPrueba.Obras.Count(o => o.Estatus == EstatusObra.Activa);

            // Pagos pendientes de nóminas ya autorizadas (los del borrador todavía no son pagos reales)
            var pendientes = DatosPrueba.PagosEmpleado
                .Where(p => p.Estatus == EstatusPago.Pendiente &&
                            DatosPrueba.Nominas.First(n => n.IdNomina == p.IdNomina).Estatus != EstatusNomina.Borrador)
                .ToList();
            PagosPendientes = pendientes.Count;
            PagosPendientesTotal = pendientes.Sum(p => p.TotalNeto);

            PrestamosActivos = DatosPrueba.Prestamos.Count(p => p.Estatus == EstatusPrestamo.Activo);
            PrestamosTotal = DatosPrueba.Prestamos
                .Where(p => p.Estatus == EstatusPrestamo.Activo)
                .Sum(p => p.SaldoPendiente);

            NominaEnBorrador = DatosPrueba.Nominas.FirstOrDefault(n => n.Estatus == EstatusNomina.Borrador);

            // Una tarjeta por obra activa con su saldo (RN-12)
            Obras = DatosPrueba.Obras
                .Where(o => o.Estatus == EstatusObra.Activa)
                .Select(o =>
                {
                    var gastado = DatosPrueba.GastadoPorObra.GetValueOrDefault(o.IdObra);
                    var saldo = o.PresupuestoManoObra - gastado;
                    return new ResumenObra
                    {
                        Obra = o,
                        Gastado = gastado,
                        Saldo = saldo,
                        Porcentaje = o.PresupuestoManoObra > 0 ? (int)(gastado * 100 / o.PresupuestoManoObra) : 0,
                        Alerta = saldo <= o.LimiteAlertaSaldo
                    };
                })
                .ToList();

            ObrasConSaldoBajo = Obras.Count(r => r.Alerta);
        }

        // Clase pequeña solo para esta pantalla: los números de una obra
        public class ResumenObra
        {
            public Obra Obra { get; set; } = new();
            public decimal Gastado { get; set; }
            public decimal Saldo { get; set; }
            public int Porcentaje { get; set; }
            public bool Alerta { get; set; }
        }
    }
}
