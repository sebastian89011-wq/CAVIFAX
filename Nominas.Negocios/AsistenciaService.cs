using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class AsistenicaService()
{

    private readonly NominasDbContext _db;
    public AsistenicaService(NominasDbContext db)
    {
        _db = db;
    }



}
    