using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;

namespace Nominas.Web.Pages.Obras
{
    public class DetalleModel : PageModel
    {
        public Obra Obra { get; set; } = new();
        public decimal Gastado { get; set; }
        public decimal Saldo { get; set; }
        public int Porcentaje { get; set; }
        public bool Alerta { get; set; }

        // Lo que costó esta obra en cada nómina (una fila por semana)
        public List<CostoSemana> CostosPorNomina { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            var encontrada = DatosPrueba.Obras.FirstOrDefault(o => o.IdObra == id);
            if (encontrada == null)
            {
                return NotFound();
            }

            Obra = encontrada;

            // Datos de prueba: el backend calculará el saldo con las nóminas autorizadas (RN-12)
            Gastado = DatosPrueba.GastadoPorObra.GetValueOrDefault(id);
            Saldo = Obra.PresupuestoManoObra - Gastado;
            Porcentaje = Obra.PresupuestoManoObra > 0 ? (int)(Gastado * 100 / Obra.PresupuestoManoObra) : 0;
            Alerta = Saldo <= Obra.LimiteAlertaSaldo;

            CostosPorNomina = DatosPrueba.DetallesNomina
                .Where(d => d.IdObra == id)
                .GroupBy(d => d.IdNomina)
                .Select(g => new CostoSemana
                {
                    Nomina = DatosPrueba.Nominas.First(n => n.IdNomina == g.Key),
                    Trabajadores = g.Select(d => d.IdEmpleado).Distinct().Count(),
                    Dias = g.Sum(d => d.DiasTrabajados),
                    Total = g.Sum(d => d.CostoBruto)
                })
                .OrderByDescending(c => c.Nomina.FechaInicio)
                .ToList();

            return Page();
        }

        // Clase pequeña solo para esta pantalla: una fila de la tabla de costos
        public class CostoSemana
        {
            public Nomina Nomina { get; set; } = new();
            public int Trabajadores { get; set; }
            public int Dias { get; set; }
            public decimal Total { get; set; }
        }
    }
}
