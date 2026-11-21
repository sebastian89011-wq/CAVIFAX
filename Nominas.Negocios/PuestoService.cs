using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class PuestoService
{
    private readonly NominasDbContext _db;

    public PuestoService(NominasDbContext db)
    {
        _db = db;
    }

    public List<Puesto> Listar()
    {
        return _db.Puestos.OrderBy(p => p.Nombre).ToList();
    }

    public void Crear(Puesto puesto)
    {
        if (string.IsNullOrWhiteSpace(puesto.Nombre))
            throw new ValidacionException("El nombre del puesto es obligatorio.");

        if (_db.Puestos.Any(p => p.Nombre == puesto.Nombre))
            throw new ValidacionException("Ya existe un puesto con ese nombre.");

        _db.Puestos.Add(puesto);
        _db.SaveChanges();
    }
}