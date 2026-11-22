using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.NominaSemanal
{
    public class DetalleModel : PageModel
    {
        public Nomina Nomina { get; set; } = new();
        public List<DetalleNomina> Detalles { get; set; } = new();
        public List<PagoEmpleado> Pagos { get; set; } = new();

        // Pagos de semanas anteriores que todavía no se entregan (RN-13)
        public List<PagoEmpleado> PendientesAnteriores { get; set; } = new();

        [BindProperty]
        public decimal EfectivoRecibido { get; set; }

        public decimal TotalBruto => Pagos.Sum(p => p.TotalBruto);
        public decimal TotalDescuentos => Pagos.Sum(p => p.TotalDescuentos);
        public decimal TotalNeto => Pagos.Sum(p => p.TotalNeto);
        public decimal TotalPendientesAnteriores => PendientesAnteriores.Sum(p => p.TotalNeto);

        public IActionResult OnGet(int id)
        {
            var encontrada = DatosPrueba.Nominas.FirstOrDefault(n => n.IdNomina == id);
            if (encontrada == null)
            {
                return NotFound();
            }

            Nomina = encontrada;
            Detalles = DatosPrueba.DetallesNomina.Where(d => d.IdNomina == id).ToList();
            Pagos = DatosPrueba.PagosEmpleado.Where(p => p.IdNomina == id).ToList();
            PendientesAnteriores = DatosPrueba.PagosEmpleado
                .Where(p => p.IdNomina < id && p.Estatus == EstatusPago.Pendiente)
                .ToList();

            // Sugerencia: el efectivo necesario = lo de esta semana + lo pendiente de semanas anteriores
            EfectivoRecibido = Nomina.Estatus == EstatusNomina.Borrador
                ? TotalNeto + TotalPendientesAnteriores
                : Nomina.EfectivoRecibido;

            return Page();
        }

        // Botón "Autorizar nómina" (asp-page-handler="Autorizar")
        public IActionResult OnPostAutorizar(int id)
        {
            // Aquí el backend, en una transacción: fija tarifas y totales, guarda EfectivoRecibido,
            // cambia a Autorizada, crea los abonos de préstamos y deja los pagos en Pendiente.
            return RedirectToPage(new { id });
        }

        // Botón "Marcar entregado" de cada pago (asp-page-handler="Entregar")
        public IActionResult OnPostEntregar(int id, int idPago)
        {
            // Aquí el backend marcará el PagoEmpleado como Entregado, con la fecha de hoy
            // y la nómina de la semana en que se entregó (RN-13).
            return RedirectToPage(new { id });
        }

        public string SemanaDeNomina(int idNomina)
        {
            var nomina = DatosPrueba.Nominas.FirstOrDefault(n => n.IdNomina == idNomina);
            return nomina == null ? "—" : DatosPrueba.Semana(nomina);
        }
    }
}
