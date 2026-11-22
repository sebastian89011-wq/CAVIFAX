using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

namespace Nominas.Web.Pages.Puestos
{
    public class IndexModel : PageModel
    {
        public List<Puesto> Puestos { get; set; } = new();
        private readonly PuestoService _puestoService; 

        public IndexModel(PuestoService puestoService) {
            
            _puestoService = puestoService;

        }

        public void OnGet()
        {
            Puestos = _puestoService.Listar();
        }
          
    }
}
