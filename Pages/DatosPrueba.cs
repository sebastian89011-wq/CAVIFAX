using Nominas.Entidades;

namespace Nominas.Web.Pages;

// Datos temporales mientras el backend conecta la base de datos. Se borrará después.
public static class DatosPrueba
{
    public static List<Puesto> Puestos = new()
    {
        new Puesto { IdPuesto = 1, Nombre = "Ingeniero" },
        new Puesto { IdPuesto = 2, Nombre = "Albañil" },
        new Puesto { IdPuesto = 3, Nombre = "Ayudante general" }
    };

    public static List<Empleado> Empleados = new()
    {
        new Empleado { IdEmpleado = 1, IdPuesto = 2, Nombre = "Pedro", ApellidoPaterno = "Ramírez", ApellidoMaterno = "Soto",
                       Telefono = "6861234567", Domicilio = "Calle Durango 120, Mexicali",
                       Curp = "RASP900101HBCMTD05", Nss = "12345678901", ClaveIne = "RMSTPD90010102H100",
                       DocumentosCompletos = true, Especialidad = "Soldador",
                       TipoContratacion = TipoContratacion.Permanente, Estatus = EstatusEmpleado.Activo,
                       FechaIngreso = new DateOnly(2021, 3, 15), SueldoDiario = 450m, PagoHoraExtra = 70m },
        new Empleado { IdEmpleado = 2, IdPuesto = 3, Nombre = "Luis", ApellidoPaterno = "Gómez",
                       Telefono = "6869876543", Domicilio = "Av. Colón 45, Mexicali",
                       Curp = "GOLL950505HBCMSS02", Nss = "98765432109", ClaveIne = "GMLSLS95050502H200",
                       DocumentosCompletos = false,
                       TipoContratacion = TipoContratacion.Subcontratado, Estatus = EstatusEmpleado.BajaTemporal,
                       FechaIngreso = new DateOnly(2024, 8, 1), SueldoDiario = 320m, PagoHoraExtra = 50m },
        new Empleado { IdEmpleado = 3, IdPuesto = 2, Nombre = "Mario", ApellidoPaterno = "Ochoa", ApellidoMaterno = "Ruiz",
                       Telefono = "6865551122", Domicilio = "Calle Reforma 800, Mexicali",
                       Curp = "OORM880720HBCCZR09", Nss = "55566677788", ClaveIne = "OCRZMR88072002H300",
                       DocumentosCompletos = true, Especialidad = "Electricista",
                       TipoContratacion = TipoContratacion.Permanente, Estatus = EstatusEmpleado.Activo,
                       FechaIngreso = new DateOnly(2019, 1, 10), SueldoDiario = 480m, PagoHoraExtra = 75m }
    };
}