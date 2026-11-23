using Microsoft.AspNetCore.Mvc.RazorPages;
using Nominas.Entidades;
using Nominas.Negocios;

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

        private List<Cliente> _clientes = new();
        private readonly EmpleadoService _empleadoService;
        private readonly ObraService _obraService;
        private readonly ClienteService _clienteService;
        private readonly PrestamoService _prestamoService;
        private readonly NominaService _nominaService;

        public IndexModel(EmpleadoService empleadoService, ObraService obraService, ClienteService clienteService,
                          PrestamoService prestamoService, NominaService nominaService)
        {
            _empleadoService = empleadoService;
            _obraService = obraService;
            _clienteService = clienteService;
            _prestamoService = prestamoService;
            _nominaService = nominaService;
        }

        public void OnGet()
        {
            EmpleadosActivos = _empleadoService.ListarActivos().Count;

            // Pagos pendientes de nóminas ya autorizadas (los del borrador todavía no son pagos reales)
            var pendientes = _nominaService.PagosPorEntregar();
            PagosPendientes = pendientes.Count;
            PagosPendientesTotal = pendientes.Sum(p => p.TotalNeto);

            var prestamosActivos = _prestamoService.Listar().Where(p => p.Estatus == EstatusPrestamo.Activo).ToList();
            PrestamosActivos = prestamosActivos.Count;
            PrestamosTotal = prestamosActivos.Sum(p => p.SaldoPendiente);

            NominaEnBorrador = _nominaService.Listar().FirstOrDefault(n => n.Estatus == EstatusNomina.Borrador);

            // Una tarjeta por obra activa con su saldo (RN-12); el cálculo lo hace ObraService
            _clientes = _clienteService.Listar();
            Obras = _obraService.Listar()
                .Where(o => o.Estatus == EstatusObra.Activa)
                .Select(o =>
                {
                    var resumen = _obraService.ObtenerResumen(o);
                    return new ResumenObra
                    {
                        Obra = o,
                        Gastado = resumen.Gastado,
                        Saldo = resumen.Saldo,
                        Porcentaje = resumen.Porcentaje,
                        Alerta = resumen.Alerta
                    };
                })
                .ToList();

            ObrasActivas = Obras.Count;
            ObrasConSaldoBajo = Obras.Count(r => r.Alerta);
        }

        public string SemanaBorrador => NominaEnBorrador == null ? "" : NominaService.Semana(NominaEnBorrador);

        public string NombreCliente(int idCliente)
        {
            return _clientes.FirstOrDefault(c => c.IdCliente == idCliente)?.Nombre ?? "—";
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
