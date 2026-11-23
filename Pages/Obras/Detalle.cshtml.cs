using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Obras
{
    public class DetalleModel : PageModel
    {
        public Obra Obra { get; set; } = new();
        public decimal Gastado { get; set; }
        public decimal Saldo { get; set; }
        public int Porcentaje { get; set; }
        public bool Alerta { get; set; }

        public string NombreCliente { get; set; } = string.Empty;
        public string NombreResponsable { get; set; } = string.Empty;
        public string NombreSupervisor { get; set; } = string.Empty;

        // Lo que costó esta obra en cada nómina (una fila por semana)
        public List<CostoSemanaObra> CostosPorNomina { get; set; } = new();

        private readonly ObraService _obraService;
        private readonly ClienteService _clienteService;
        private readonly EmpleadoService _empleadoService;

        public DetalleModel(ObraService obraService, ClienteService clienteService, EmpleadoService empleadoService)
        {
            _obraService = obraService;
            _clienteService = clienteService;
            _empleadoService = empleadoService;
        }

        public IActionResult OnGet(int id)
        {
            var encontrada = _obraService.ObtenerPorId(id);
            if (encontrada == null)
            {
                return NotFound();
            }

            Obra = encontrada;

            // El cálculo del presupuesto vive en Negocios (RN-12); la página solo lo muestra
            var resumen = _obraService.ObtenerResumen(Obra);
            Gastado = resumen.Gastado;
            Saldo = resumen.Saldo;
            Porcentaje = resumen.Porcentaje;
            Alerta = resumen.Alerta;

            NombreCliente = _clienteService.ObtenerPorId(Obra.IdCliente)?.Nombre ?? "—";
            NombreResponsable = NombreEmpleado(Obra.IdResponsable);
            NombreSupervisor = NombreEmpleado(Obra.IdSupervisor);

            CostosPorNomina = _obraService.CostosPorNomina(id);
            return Page();
        }

        public string Semana(Nomina n)
        {
            return $"{n.FechaInicio:dd/MM} al {n.FechaFin:dd/MM/yyyy}";
        }

        private string NombreEmpleado(int idEmpleado)
        {
            var e = _empleadoService.ObtenerPorId(idEmpleado);
            return e == null ? "—" : EmpleadoService.NombreCompleto(e);
        }
    }
}
