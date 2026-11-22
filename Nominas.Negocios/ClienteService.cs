using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class ClienteService
{
    private readonly NominasDbContext _db;

    public ClienteService(NominasDbContext db)
    {
        _db = db;
    }

    public List<Cliente> Listar()
    {
        return _db.Clientes.OrderBy(c => c.IdCliente).ToList();
    }

    public Cliente? ObtenerPorId(int id)
    {
        return _db.Clientes.Find(id);
    }

    public void Crear(Cliente cliente)
    {
        this.ValidarDatos(cliente);
        _db.Clientes.Add(cliente);
        _db.SaveChanges();
    }

    public void Editar(Cliente cliente)
    {
        var existente = _db.Clientes.Find(cliente.IdCliente);
        if (existente == null)
            throw new ValidacionException("El cliente no existe.");

        this.ValidarDatos(cliente);

        existente.Nombre = cliente.Nombre;
        existente.Domicilio = cliente.Domicilio;
        _db.SaveChanges();
    }

    // Longitudes de la tabla cliente: nombre VARCHAR(100), domicilio VARCHAR(200), ambos NOT NULL
    private void ValidarDatos(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            throw new ValidacionException("El nombre del cliente es obligatorio.");

        if (cliente.Nombre.Length > 100)
            throw new ValidacionException("El nombre no puede tener más de 100 caracteres.");

        if (string.IsNullOrWhiteSpace(cliente.Domicilio))
            throw new ValidacionException("El domicilio es obligatorio.");

        if (cliente.Domicilio.Length > 200)
            throw new ValidacionException("El domicilio no puede tener más de 200 caracteres.");
    }
}
