using Nominas.Datos;
using Nominas.Entidades;

namespace Nominas.Negocios;

public class AsistenciaService
{

    private readonly NominasDbContext _db;
    public AsistenciaService(NominasDbContext db)
    {
        _db = db;
    }



}
    