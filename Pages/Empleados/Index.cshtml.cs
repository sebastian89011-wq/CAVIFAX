using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Empleados
{
    public class IndexModel : PageModel
    {
        public List<Empleado> Empleados { get; set; } = new();
        public List<Puesto> Puestos { get; set; } = new();
        private readonly EmpleadoService _empleadoService;
        private readonly PuestoService _puestoService;

        public IndexModel(EmpleadoService empleadoService, PuestoService puestoService)
        {
            _empleadoService = empleadoService;
            _puestoService = puestoService;
        }

        public void OnGet()
        {
            Puestos = _puestoService.Listar();
            Empleados = _empleadoService.Listar();
        }

        public string NombrePuesto(int idPuesto)
        {
            var puesto = Puestos.FirstOrDefault(p => p.IdPuesto == idPuesto);
            return puesto?.Nombre ?? "—";
        }
    }
}
