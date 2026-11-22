using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class ObraService
{
    private readonly NominasDbContext _db;

    public ObraService(NominasDbContext db)
    {
        _db = db;
    }

    public List<Obra> Listar()
    {
        return _db.Obras.OrderBy(o => o.IdObra).ToList();
    }

    public Obra? ObtenerPorId(int id)
    {
        return _db.Obras.Find(id);
    }

    public void Crear(Obra obra)
    {
        // Toda obra nueva empieza activa y sin fecha de fin real
        obra.Estatus = EstatusObra.Activa;
        obra.FechaFinReal = null;

        this.ValidarDatos(obra, esNueva: true);
        _db.Obras.Add(obra);
        _db.SaveChanges();
    }

    public void Editar(Obra obra)
    {
        var existente = _db.Obras.Find(obra.IdObra);
        if (existente == null)
            throw new ValidacionException("La obra no existe.");

        this.ValidarDatos(obra, esNueva: false);

        existente.IdCliente = obra.IdCliente;
        existente.IdResponsable = obra.IdResponsable;
        existente.IdSupervisor = obra.IdSupervisor;
        existente.Nombre = obra.Nombre;
        existente.Ubicacion = obra.Ubicacion;
        existente.FechaInicio = obra.FechaInicio;
        existente.FechaFinProgramada = obra.FechaFinProgramada;
        existente.FechaFinReal = obra.FechaFinReal;
        existente.PresupuestoManoObra = obra.PresupuestoManoObra;
        existente.LimiteAlertaSaldo = obra.LimiteAlertaSaldo;
        existente.BonoTerminacionPorTrabajador = obra.BonoTerminacionPorTrabajador;
        existente.Estatus = obra.Estatus;
        _db.SaveChanges();
    }

    // ---------- Presupuesto (RN-12) ----------

    // Gastado = suma del costo bruto de esta obra en nóminas autorizadas o pagadas (el borrador no cuenta)
    public decimal Gastado(int idObra)
    {
        return _db.DetallesNomina
            .Where(d => d.IdObra == idObra
                     && _db.Nominas.Any(n => n.IdNomina == d.IdNomina && n.Estatus != EstatusNomina.Borrador))
            .Sum(d => d.CostoBruto);
    }

    public ResumenPresupuestoObra ObtenerResumen(Obra obra)
    {
        decimal gastado = this.Gastado(obra.IdObra);
        decimal saldo = obra.PresupuestoManoObra - gastado;

        return new ResumenPresupuestoObra
        {
            Presupuesto = obra.PresupuestoManoObra,
            Gastado = gastado,
            Saldo = saldo,
            Porcentaje = obra.PresupuestoManoObra > 0 ? (int)(gastado * 100 / obra.PresupuestoManoObra) : 0,
            Alerta = saldo <= obra.LimiteAlertaSaldo
        };
    }

    // Costo de la obra en cada nómina (incluye el borrador como vista previa), de la más reciente a la más antigua
    public List<CostoSemanaObra> CostosPorNomina(int idObra)
    {
        var detalles = _db.DetallesNomina.Where(d => d.IdObra == idObra).ToList();
        var idsNomina = detalles.Select(d => d.IdNomina).Distinct().ToList();
        var nominas = _db.Nominas.Where(n => idsNomina.Contains(n.IdNomina)).ToList();

        return detalles
            .GroupBy(d => d.IdNomina)
            .Select(g => new CostoSemanaObra
            {
                Nomina = nominas.First(n => n.IdNomina == g.Key),
                Trabajadores = g.Select(d => d.IdEmpleado).Distinct().Count(),
                Dias = g.Sum(d => d.DiasTrabajados),
                Total = g.Sum(d => d.CostoBruto)
            })
            .OrderByDescending(c => c.Nomina.FechaInicio)
            .ToList();
    }

    // ---------- Validaciones ----------

    // Reglas tomadas de la tabla obra (longitudes, FK y CHECK)
    private void ValidarDatos(Obra obra, bool esNueva)
    {
        if (string.IsNullOrWhiteSpace(obra.Nombre))
            throw new ValidacionException("El nombre de la obra es obligatorio.");
        if (obra.Nombre.Length > 100)
            throw new ValidacionException("El nombre no puede tener más de 100 caracteres.");

        if (string.IsNullOrWhiteSpace(obra.Ubicacion))
            throw new ValidacionException("La ubicación es obligatoria.");
        if (obra.Ubicacion.Length > 200)
            throw new ValidacionException("La ubicación no puede tener más de 200 caracteres.");

        if (!_db.Clientes.Any(c => c.IdCliente == obra.IdCliente))
            throw new ValidacionException("Selecciona un cliente válido.");

        // Al crear, el responsable y el supervisor deben estar activos.
        // Al editar solo se pide que existan, para no bloquear obras cuyo responsable ya se dio de baja.
        this.ValidarEmpleado(obra.IdResponsable, "responsable", esNueva);
        this.ValidarEmpleado(obra.IdSupervisor, "supervisor", esNueva);

        if (obra.FechaFinProgramada < obra.FechaInicio)
            throw new ValidacionException("La fecha de fin programada no puede ser anterior a la de inicio.");
        if (obra.FechaFinReal != null && obra.FechaFinReal < obra.FechaInicio)
            throw new ValidacionException("La fecha de fin real no puede ser anterior a la de inicio.");
        if (obra.Estatus == EstatusObra.Terminada && obra.FechaFinReal == null)
            throw new ValidacionException("Para marcar la obra como terminada, captura la fecha de fin real.");

        if (obra.PresupuestoManoObra <= 0)
            throw new ValidacionException("El presupuesto debe ser mayor a 0.");
        if (obra.LimiteAlertaSaldo < 0 || obra.LimiteAlertaSaldo > obra.PresupuestoManoObra)
            throw new ValidacionException("El límite de alerta debe estar entre 0 y el presupuesto.");
        if (obra.BonoTerminacionPorTrabajador < 0)
            throw new ValidacionException("El bono no puede ser negativo.");
    }

    private void ValidarEmpleado(int idEmpleado, string rol, bool debeEstarActivo)
    {
        var empleado = _db.Empleados.Find(idEmpleado);
        if (empleado == null)
            throw new ValidacionException($"Selecciona un {rol} válido.");
        if (debeEstarActivo && empleado.Estatus != EstatusEmpleado.Activo)
            throw new ValidacionException($"El {rol} debe ser un empleado activo.");
    }
}
