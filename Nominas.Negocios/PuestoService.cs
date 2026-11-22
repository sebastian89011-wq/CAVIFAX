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
        return _db.Puestos.OrderBy(p => p.IdPuesto).ToList();
    }

    public Puesto? ObtenerPorId(int id)
    {
        return _db.Puestos.Find(id);
    }

    private void ValidarDatos(Puesto puesto)
    {
        // las validaciones de nombre vacío, 40 y 150 caracteres
        if (string.IsNullOrWhiteSpace(puesto.Nombre))
            throw new ValidacionException("El nombre del puesto es obligatorio.");

        if (puesto.Nombre.Length > 40)
            throw new ValidacionException("El nombre no puede tener más de 40 caracteres.");

        if (puesto.Descripcion != null && puesto.Descripcion.Length > 150)
            throw new ValidacionException("La descripción no puede tener más de 150 caracteres");


        if (_db.Puestos.Any(p => p.Nombre == puesto.Nombre && p.IdPuesto != puesto.IdPuesto))
            throw new ValidacionException("Ya existe un puesto con ese nombre.");
    }

    public void Editar(Puesto puesto)
    {
        var existente = _db.Puestos.Find(puesto.IdPuesto);
        if (existente == null)
            throw new ValidacionException("El puesto no existe.");

        this.ValidarDatos(puesto);

        existente.Nombre = puesto.Nombre;
        existente.Descripcion = puesto.Descripcion;
        _db.SaveChanges();
    }

    public void Crear(Puesto puesto)
    {

        this.ValidarDatos(puesto);
        _db.Puestos.Add(puesto);
        _db.SaveChanges();
    }


}