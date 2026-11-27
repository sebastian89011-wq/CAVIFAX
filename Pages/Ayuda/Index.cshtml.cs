using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nominas.Web.Pages.Ayuda
{
    /// <summary>
    /// Manual de usuario. Es una página solo de lectura: todo el contenido está en Index.cshtml
    /// y las capturas en wwwroot/img/manual, así que no consulta la base de datos.
    /// </summary>
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
