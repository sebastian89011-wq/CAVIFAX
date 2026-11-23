using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

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

        // Los totales vienen calculados del servicio
        public decimal TotalBruto { get; set; }
        public decimal TotalDescuentos { get; set; }
        public decimal TotalNeto { get; set; }
        public decimal TotalPendientesAnteriores { get; set; }

        private NominaDetalle _datos = new();
        private readonly NominaService _nominaService;

        public DetalleModel(NominaService nominaService)
        {
            _nominaService = nominaService;
        }

        public IActionResult OnGet(int id)
        {
            return CargarDatos(id) ? Page() : NotFound();
        }

        // Botón "Recalcular" (asp-page-handler="Recalcular"), solo en borrador
        public IActionResult OnPostRecalcular(int id)
        {
            return Ejecutar(id, () => _nominaService.Recalcular(id));
        }

        // Botón "Eliminar borrador" (asp-page-handler="Eliminar"), solo en borrador
        public IActionResult OnPostEliminar(int id)
        {
            try
            {
                _nominaService.EliminarBorrador(id);
                return RedirectToPage("Index");
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return CargarDatos(id) ? Page() : NotFound();
            }
        }

        // Botón "Autorizar nómina" (asp-page-handler="Autorizar")
        public IActionResult OnPostAutorizar(int id)
        {
            // TODO: usar el usuario de la sesión
            int? idUsuario = _nominaService.PrimerUsuarioActivo();
            if (idUsuario == null)
            {
                ModelState.AddModelError(string.Empty, "No hay ningún usuario activo para autorizar la nómina.");
                return CargarDatos(id) ? Page() : NotFound();
            }

            // TODO: Autorizar todavía no recibe EfectivoRecibido; el valor del formulario no se guarda.
            return Ejecutar(id, () => _nominaService.Autorizar(id, idUsuario.Value));
        }

        // Botón "Marcar entregado" de cada pago (asp-page-handler="Entregar").
        // Se entrega en la semana de esta nómina (RN-13).
        public IActionResult OnPostEntregar(int id, int idPago)
        {
            return Ejecutar(id, () => _nominaService.MarcarPagoEntregado(idPago, id));
        }

        // Llama al servicio; si hay error lo muestra en la misma página
        private IActionResult Ejecutar(int id, Action accion)
        {
            try
            {
                accion();
                return RedirectToPage(new { id });
            }
            catch (ValidacionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (NotImplementedException)
            {
                ModelState.AddModelError(string.Empty, "Esta acción todavía no está implementada en NominaService.");
            }
            return CargarDatos(id) ? Page() : NotFound();
        }

        private bool CargarDatos(int id)
        {
            var datos = _nominaService.ObtenerDetalle(id);
            if (datos == null)
            {
                return false;
            }

            _datos = datos;
            Nomina = datos.Nomina;
            Detalles = datos.Detalles;
            Pagos = datos.Pagos;
            PendientesAnteriores = datos.PendientesAnteriores;
            TotalBruto = datos.TotalBruto;
            TotalDescuentos = datos.TotalDescuentos;
            TotalNeto = datos.TotalNeto;
            TotalPendientesAnteriores = datos.TotalPendientesAnteriores;

            // Sugerencia: el efectivo necesario = lo de esta semana + lo pendiente de semanas anteriores
            EfectivoRecibido = Nomina.Estatus == EstatusNomina.Borrador
                ? datos.EfectivoSugerido
                : Nomina.EfectivoRecibido;
            return true;
        }

        public string Semana => NominaService.Semana(Nomina);

        public string NombreEmpleado(int idEmpleado)
        {
            return _datos.NombresEmpleado.GetValueOrDefault(idEmpleado, "—");
        }

        public string NombreObra(int idObra)
        {
            return _datos.NombresObra.GetValueOrDefault(idObra, "—");
        }

        public string SemanaDeNomina(int idNomina)
        {
            return _datos.Semanas.GetValueOrDefault(idNomina, "—");
        }
    }
}
