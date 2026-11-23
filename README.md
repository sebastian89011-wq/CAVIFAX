# CAVIFAX

Sistema de control de nóminas, asistencias y presupuestos para una constructora.
Proyecto final de la materia Base de Datos (UABC, Facultad de Ingeniería, Arquitectura y Diseño).

La aplicación registra empleados, obras, asistencias diarias y préstamos; calcula la nómina semanal
(costo bruto por obra y pago neto por trabajador), aplica los abonos de los préstamos, controla la
entrega del efectivo y avisa cuando el presupuesto de mano de obra de una obra está por agotarse.

## Tecnologías

| Componente | Versión |
|---|---|
| ASP.NET Core Razor Pages | .NET 10 |
| Entity Framework Core + proveedor `MySql.EntityFrameworkCore` | 10.0.12 |
| MySQL Community Server | 8.x (probado con 8.4) |
| Bootstrap 5 y jQuery Validation | incluidos en `wwwroot/lib` |

## Estructura

```
AplicacionNomina.slnx      Solución de Visual Studio
Nominas.Entidades/         Clases de las 11 tablas y los enums de estatus
Nominas.Datos/             NominasDbContext: mapeo a MySQL (snake_case, enums como texto)
Nominas.Negocios/          Servicios con las reglas de negocio (RN-01 a RN-16) y CalculadoraNomina
Pages/                     Razor Pages (interfaz): cada .cshtml.cs llama a un servicio
Program.cs                 Registro del DbContext y de los servicios
database/                  Scripts SQL de la base de datos
```

Las páginas nunca acceden a la base directamente: llaman a un servicio de `Nominas.Negocios`,
que valida las reglas y usa `NominasDbContext`. Si una regla no se cumple, el servicio lanza
`ValidacionException` y la página muestra el mensaje.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (o Visual Studio 2026 con la carga de trabajo de ASP.NET).
- MySQL Server 8 y un cliente para ejecutar scripts (MySQL Workbench o la línea de comandos `mysql`).

## Instalación

### 1. Crear la base de datos

Ejecutar los scripts de `database/` en este orden:

| Script | Contenido |
|---|---|
| `01_esquema.sql` | Crea `constructora_nominas` con sus 11 tablas, PK, FK (`ON DELETE RESTRICT`), UNIQUE y CHECK. **Borra la base si ya existe.** |
| `02_datos.sql` | Datos de prueba ficticios: puestos, empleados, clientes, obras, asistencias, préstamos y dos nóminas. |
| `03_consultas_vistas.sql` | Operaciones DML, las 10 consultas documentadas, 2 vistas y transacciones con COMMIT y ROLLBACK. |
| `04_rutinas_pruebas.sql` | 2 triggers, 1 función, 2 procedimientos almacenados y sus casos de prueba. |

Desde la línea de comandos:

```bash
mysql -u root -p < database/01_esquema.sql
mysql -u root -p constructora_nominas < database/02_datos.sql
mysql -u root -p constructora_nominas < database/03_consultas_vistas.sql
mysql -u root -p constructora_nominas < database/04_rutinas_pruebas.sql
```

### 2. Configurar la cadena de conexión

`appsettings.json` solo tiene una plantilla (`password=CONFIGURAR_EN_USER_SECRETS`). Cada persona
guarda su propia cadena en **User Secrets**, que queda fuera del repositorio. Desde la carpeta raíz:

```bash
dotnet user-secrets set "ConnectionStrings:Nominas" "server=localhost;port=3306;database=constructora_nominas;user=root;password=TU_CONTRASEÑA"
```

En Visual Studio también se puede hacer con clic derecho en el proyecto **Nominas.Web** →
*Administrar secretos de usuario*.

### 3. Ejecutar

```bash
dotnet run --project Nominas.Web.csproj
```

Abrir `http://localhost:5249` (o el puerto que indique la consola). En Visual Studio basta con
seleccionar el perfil `http` o `https` y presionar F5.

## Módulos

| Módulo | Qué hace |
|---|---|
| Inicio | Tablero con empleados activos, pagos por entregar, préstamos activos, nómina en borrador y saldo de cada obra. |
| Puestos y Clientes | Alta, consulta, edición y eliminación (solo si no tienen registros asociados). |
| Empleados | Alta, consulta, edición, detalle y búsqueda por nombre, CURP o NSS y por estatus. La baja es un cambio de estatus. |
| Obras | Alta, edición, búsqueda por obra, ubicación o cliente, y detalle con presupuesto, gastado, saldo, alerta y costo por semana. |
| Asistencias | Cuadrícula semanal por empleado y día: obra trabajada o falta, y horas extra. |
| Préstamos | Préstamos y anticipos con su historial de abonos; se cancelan solo si no tienen abonos. |
| Nómina | Generar el borrador de la semana, recalcularlo, autorizarlo y marcar cada pago como entregado. |

## Flujo de una nómina

1. Capturar las asistencias de la semana (lunes a sábado).
2. **Generar borrador:** agrupa las asistencias por empleado y obra, congela las tarifas, aplica el bono
   por terminación y calcula el pago neto con el abono del préstamo. Si el bruto no alcanza para el
   abono, este se difiere a la semana siguiente.
3. **Autorizar:** registra el efectivo recibido, crea los abonos, baja el saldo de los préstamos y liquida
   los que llegan a cero. Una nómina autorizada ya no se modifica.
4. **Marcar entregado:** cada pago se entrega en la semana en que se paga (puede ser una posterior).
   Cuando ya no quedan pagos pendientes, la nómina pasa a *Pagada*.

## Equipo

| Integrante | Rol |
|---|---|
| Sebastián Calderón Ávalos | Backend / arquitectura |
| Leonel Cajeme García | Frontend / interfaz web |
| Oscar Crespo Sánchez | Base de datos / SQL |
