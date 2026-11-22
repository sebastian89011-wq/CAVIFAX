# CONTEXTO.md — Sistema de Nóminas para una Constructora

> Archivo de traspaso entre agentes. Contiene solo el estado del proyecto: lo hecho, lo decidido y lo pendiente.
> **Última actualización:** 2026-11-20

## 1. Proyecto y forma de trabajo

- Proyecto universitario (UABC, materia Base de Datos). Sistema de nóminas semanales para una constructora en C# con ASP.NET Core Razor Pages y MySQL.
- Fuentes: PDF «Análisis Problema» (requisitos oficiales, entrega **22-nov-2026**) y PDF «Propuesta de solución» (documento de trabajo, no aprobado).
- Equipo de 3: **alumno = Backend**, otro integrante = Frontend, otro = Base de datos (diseño y scripts SQL). No duplicar responsabilidades.
- Perfil del alumno: Java intermedio, C# limitado, HTML nulo, sin experiencia en Razor ni EF Core. **No conoce Maven, JDBC, JPA ni Hibernate; nunca ha conectado un programa a una base de datos.** El acceso a datos se explica desde cero. Las analogías con Java solo sirven para el lenguaje (clases, tipos, colecciones). Sabe SQL por la materia de BD.
- Modo tutor: paso a paso, comparar con Java, indicar proyecto y archivo, que el alumno escriba y decida, respuestas cortas, no cambiar requisitos sin avisar.

## ⚠️ ENTREGA DEL SISTEMA: 23-nov-2026 (decidido el 22-nov a la 1:30 a.m.)
Plan de emergencia. Claude Code genera lo repetitivo **siguiendo el patrón de PuestoService y de las páginas de Puestos**; el alumno revisa y se prepara para la defensa.
- **Fase 1:** CRUDs de Clientes, Empleados, Obras y Préstamos (servicios + conectar las páginas del frontend, sin DatosPrueba).
- **Fase 2:** Asistencias (servicio con RN-06, 07 y 08 + cuadrícula semanal).
- **Fase 3:** Nómina (GenerarBorrador, Recalcular, Autorizar en transacción, MarcarPagoEntregado, pendientes y efectivo sugerido) + resumen de presupuesto de Obra en Negocios.
- **Fase 4 (opcional si sobra tiempo):** inicio de sesión del responsable.
- ✅ **BD real: `constructora_nominas`** (de Oscar, el compañero de BD; MySQL local). Verificada contra el DbContext: coinciden las 11 tablas, las 85 columnas, los ENUM, las PK AUTO_INCREMENT y 9 de 10 UNIQUE. Ramas del repositorio: main, backend, frontend y dba.
- Decisiones del 22-nov: (1) User Secrets → `database=constructora_nominas`; (2) en la BD, el UNIQUE de nomina es `(fecha_inicio, fecha_fin)`: el DbContext se ajusta a la BD y Negocios garantiza una nómina por semana (GenerarBorrador calcula las fechas de lunes a sábado y revisa que no exista); (3) se crea el enum `Especialidad {Soldador, Electricista}` con conversión a texto, igual que los otros enums; (4) CONTEXTO.md debe copiarse a la raíz del repositorio. Después: `DROP DATABASE nominas_prueba`.
- Para la fase 3, la pregunta 9 se resuelve por defecto: la nómina pasa a "pagada" cuando todos sus PagoEmpleado están entregados.

## 2. Arquitectura (configurada ✅)

| Proyecto | Plantilla | Responsabilidad |
|---|---|---|
| Nominas.Entidades | Biblioteca de clases | Entidades (POCOs) |
| Nominas.Datos | Biblioteca de clases | DbContext y acceso a MySQL |
| Nominas.Negocios | Biblioteca de clases | Validaciones y cálculos |
| Nominas.Web | Razor Pages | Presentación |

Framework: **net10.0** (ImplicitUsings y Nullable activados). Referencias: Datos → Entidades; Negocios → Datos y Entidades; Web → Negocios. Las referencias son transitivas, así que Web registrará el DbContext en `Program.cs`. Sin NuGet todavía.

## 3. Avance

1. ✅ Referencias entre proyectos.
2. ✅ Entidades en C# (11 clases y 7 enums en `Nominas.Entidades`). **Falta confirmar que compila** después de renombrar `Obra.IdIngeniero` → `IdResponsable`.
3. ✅ Se adelantó el paso 5 (EF Core + MySQL): DbContext completo y conexión probada.
4. HTML esencial.
5. Razor Pages (PageModel, .cshtml / .cshtml.cs, OnGet, OnPost, model binding).
6. EF Core + MySQL. **En curso.** Se recomendó EF Core frente a ADO.NET y Dapper. Proveedor: **Oracle `MySql.EntityFrameworkCore` 10.x** (EF Core 10). Lo eligió Claude por defecto porque el alumno no respondió; cambiar a Pomelo es fácil. **Database First confirmado:** el compañero de BD crea las tablas con sus scripts. EF Core no crea tablas (sin migraciones) y no se usa scaffold; las clases de Entidades se mapean a mano con Fluent API en el DbContext (Nominas.Datos), con nombres snake_case. ✅ Paquete `MySql.EntityFrameworkCore` 10.0.12 instalado en Nominas.Datos. ✅ `Nominas.Datos/NominasDbContext.cs` con constructor y los 11 `DbSet` escritos por el alumno (se sugirieron los plurales `PagosEmpleado`, `AbonosPrestamo` y `DetallesNomina`). ✅ `OnModelCreating` con `HasKey` para las 11 entidades (excepciones: `DetalleNomina.IdDetalle` y `AbonoPrestamo.IdAbono`). En curso: convención snake_case (un ciclo sobre `modelBuilder.Model.GetEntityTypes()` con `SetTableName` y `SetColumnName` + el método `ASnakeCase`, que escribe el alumno con `StringBuilder`). Tablas en minúsculas. ✅ `ASnakeCase` escrito por el alumno y correcto. Siguiente: **enums como texto**. Problema detectado: MySQL guarda `'baja_temporal'` y C# tiene `BajaTemporal`, así que hace falta un convertidor que traduzca en ambos sentidos. Solo `EstatusEmpleado` tiene valores de dos palabras; los demás son de una palabra, pero aun así difieren en mayúsculas. ✅ Convertidores `HasConversion` para los 7 enums (snake_case ↔ `Enum.Parse` sin distinguir mayúsculas). ✅ 10 restricciones únicas con `HasIndex().IsUnique()`, solo como documentación (Database First). ✅ Se explicó la cadena de conexión (`ConnectionStrings:Nominas` en `appsettings.json` de Web) y `AddDbContext` + `UseMySQL` en `Program.cs`; falta que el alumno confirme que compila. ✅ **Conexión verificada (21-nov)** con una BD temporal `nominas_prueba` (solo la tabla `puesto`, MySQL 8.4 local, servicio MySQL84): `/prueba-bd` devolvió los 2 puestos. Funcionan la cadena de conexión, el proveedor de Oracle, snake_case y la PK. **Pendientes de limpieza:** borrar el endpoint `/prueba-bd` de Program.cs; cuando el compañero tenga la BD real, cambiar el nombre en la cadena y hacer `DROP DATABASE nominas_prueba`.
✅ **Seguridad resuelta (21-nov):** se cambió la contraseña de root; la cadena real está en **User Secrets** del proyecto Web; `appsettings.json` solo tiene una plantilla con `password=CONFIGURAR_EN_USER_SECRETS`. Cada integrante configura su propio `secrets.json`.
Nota de estructura: `appsettings.json` y `Program.cs` están en la **raíz del repositorio** (el proyecto Web está en la raíz), no en una carpeta `Nominas.Web/`. Las relaciones FK y las propiedades de navegación se dejan para cuando haga falta. Después: convención snake_case, PK explícitas, enums como texto, restricciones únicas y cadena de conexión en Web.
7. ⏳ **CRUD de Puestos como práctica** (primero) y después el de trabajadores. ✅ `Nominas.Negocios/ValidacionException.cs` y `PuestoService.cs`: `Listar` (ordenado por **IdPuesto**, decisión del alumno) y `Crear` (nombre obligatorio y ≤ 40, descripción ≤ 150 si viene, nombre no repetido con `Any`). ✅ `AddScoped<PuestoService>()` en Program.cs (se explicaron Singleton, Scoped y Transient). ✅ HTML esencial explicado. ✅ **`Pages/Puestos/Index` conectado a `PuestoService.Listar()`** (inyección por constructor en el PageModel; la vista no cambió). ✅ `Pages/Puestos/Crear` conectado: `[BindProperty]`, `OnPost` con `try/catch (ValidacionException)` → `ModelState.AddModelError` + `Page()`, y `asp-validation-summary="All"` en la vista. ✅ **Editar** conectado: `PuestoService.ObtenerPorId` (`Find`), `Editar` (Find del existente + copiar campos + `SaveChanges`) y el método privado `ValidarDatos`, compartido por Crear y Editar. La validación de nombre repetido usa `&& p.IdPuesto != puesto.IdPuesto`, que sirve para los dos casos porque en Crear el id es 0. `EditarModel.OnGet(int id)` devuelve `NotFound()` si no existe; la ruta es `@page "{id:int}"` con un input oculto `IdPuesto`. **CRUD de Puestos terminado** (sin Eliminar: no hay página). Pendiente menor: mejorar los textos de los mensajes de longitud. Siguiente recomendado: Clientes → Empleados. **El alumno empezó por su cuenta con Asistencias (22-nov).** Para probarla necesita las tablas empleado, obra, cliente y asistencia (la BD de prueba solo tiene `puesto`). Reglas que debe cubrir `AsistenciaService`: RN-06 (empleado activo, obra activa, fecha dentro del periodo de la obra), RN-07 (una por empleado y día: insertar o actualizar), RN-08 (horas extra ≥ 0 y solo si asistió) e IdObra NULL si y solo si no asistió. Se propone una regla adicional: no modificar asistencias de una semana cuya nómina ya está autorizada. El alumno administra las ramas de git por su cuenta.
8. Demás módulos.
9. Lógica de nóminas, préstamos y presupuestos. Ya está casi desbloqueada; solo falta la pregunta 9 de la sección 6.
10. Integración con frontend y BD.

## 4. Modelo vigente (11 entidades, ADOPTADO)

Es el modelo del PDF original más las soluciones del PDF «Propuesta de solución», **adoptadas por el equipo el 20-nov-2026**.
Notación: nombres en C# (PascalCase); en MySQL son snake_case. `?` = opcional / NULL.

| Entidad | Propiedades |
|---|---|
| **Puesto** | IdPuesto, Nombre (único), Descripcion? |
| **Empleado** | IdEmpleado, IdPuesto→Puesto, Nombre, ApellidoPaterno, ApellidoMaterno?, Telefono, Domicilio, Curp (único), Nss (único), ClaveIne, DocumentosCompletos, Especialidad?, TipoContratacion, FechaIngreso, SueldoDiario, PagoHoraExtra, Estatus |
| **Cliente** | IdCliente, Nombre, Domicilio |
| **Obra** | IdObra, IdCliente→Cliente, **IdResponsable**→Empleado, IdSupervisor→Empleado, Nombre, Ubicacion, FechaInicio, FechaFinProgramada, FechaFinReal?, PresupuestoManoObra, **LimiteAlertaSaldo**, **BonoTerminacionPorTrabajador**, Estatus |
| **Asistencia** | IdAsistencia, IdEmpleado→Empleado, **IdObra?**→Obra (NULL solo si no asistió), Fecha, Asistio, HorasExtra. Único (empleado, fecha) |
| **Usuario** | IdUsuario, IdEmpleado→Empleado (único), NombreUsuario (único), ContrasenaHash, Activo |
| **Nomina** | IdNomina, IdUsuarioAutoriza?→Usuario, FechaInicio, FechaFin, Estatus, FechaAutorizacion?, EfectivoRecibido |
| **DetalleNomina** (costo **bruto** por empleado y obra) | IdDetalle, IdNomina, IdEmpleado, IdObra, DiasTrabajados (`byte`), HorasExtra, **SueldoDiarioAplicado**, **PagoHoraExtraAplicado**, Bono, **CostoBruto** |
| **PagoEmpleado** 🆕 (pago **neto** por empleado y nómina) | IdPagoEmpleado, IdNomina, IdEmpleado, TotalBruto, TotalDescuentos, TotalNeto, Estatus, FechaPago?, IdNominaPago?→Nomina (semana en que se entregó). Único (nómina, empleado) |
| **Prestamo** | IdPrestamo, IdEmpleado, Tipo, Monto, Fecha, AbonoSemanal, SaldoPendiente, Estatus |
| **AbonoPrestamo** 🆕 | IdAbono, IdPrestamo, IdNomina, MontoAbonado, Fecha. Único (préstamo, nómina) |

**Enums:** TipoContratacion {Permanente, Subcontratado}, EstatusEmpleado {Activo, BajaTemporal, BajaDefinitiva}, EstatusObra {Activa, Suspendida, Terminada}, EstatusNomina {Borrador, Autorizada, Pagada}, EstatusPago {Pendiente, Entregado}, TipoPrestamo {Anticipo, Prestamo}, EstatusPrestamo {Activo, Liquidado}.

**Se quitó de DETALLE_NOMINA:** descuento_faltas, descuento_prestamo, estatus_pago, fecha_pago. `total_pagar` pasó a llamarse `CostoBruto`.

**Convenciones C#:** `public`, namespace de una línea, un tipo por archivo, dinero en `decimal`, DATE → `DateOnly`, DATETIME → `DateTime`, FK como `int`/`int?`, todavía sin propiedades de navegación ni Data Annotations (se configuran en EF).

## 5. Reglas de negocio vigentes

Son las RN-01 a RN-16 del PDF original, con estos ajustes:
- **RN-04 y RN-15:** "ingeniero" se interpreta como **responsable de la obra o administrador** (podría ser un arquitecto). Por ahora hay una sola cuenta: la del responsable, que solo ve las obras donde `Obra.IdResponsable` = su `Usuario.IdEmpleado`.
- **RN-10:** costo bruto por obra = días × sueldo aplicado + horas extra × tarifa aplicada + bono. Las faltas no se descuentan aparte. Neto del trabajador (PagoEmpleado) = Σ brutos − abono del préstamo (una vez por nómina) ≥ 0.
- **RN-12:** saldo de obra = presupuesto − Σ CostoBruto de nóminas **autorizadas**. El préstamo no reduce el costo de la obra. Hay alerta si el saldo ≤ LimiteAlertaSaldo.
- **RN-13:** un pago pendiente se actualiza en su mismo PagoEmpleado al entregarse; no se crea un detalle nuevo.
- **Autorización de nómina:** fija tarifas y totales, cambia el estado, crea los abonos y baja SaldoPendiente (liquida si llega a 0), y deja los PagoEmpleado en pendiente. Debe hacerse en una **transacción**.

### 4.1 Diagrama ER del compañero de BD (recibido el 20-nov)
Coincide con el modelo vigente: mismas tablas y columnas en snake_case. Diferencias y pendientes:
- `empleado.especialidad` es `ENUM('soldador','electricista') NULL` → enum `Especialidad?` en C# (decidido el 22-nov).
- El ER no muestra las restricciones **únicas compuestas**: asistencia(id_empleado, fecha), **detalle_nomina(id_nomina, id_empleado, id_obra)**, pago_empleado(id_nomina, id_empleado), abono_prestamo(id_prestamo, id_nomina), ni el único de nomina(fecha_inicio). Pedir que vayan en el script.
- `dias_trabajados` es TINYINT sin especificar UNSIGNED; en C# es `byte`.
- Confirmar si los nombres de las tablas en el script van en minúsculas: en un servidor MySQL en Linux importan las mayúsculas.
- Mapeo en EF: se mantiene PascalCase en C# y una **convención global a snake_case** en el DbContext. Las PK (`IdPuesto`, etc.) se configuran explícitamente, porque EF solo reconoce por convención `Id` o `PuestoId`.

### 4.2 Estado del frontend (rama `frontend`, commit 28e0156, 21-nov)
- Razor Pages + Bootstrap 5 + jQuery Validation. Menú en `_Layout.cshtml` (marca CAVIFAX) con Inicio, Empleados, Obras, Asistencias, Nómina, Préstamos, Clientes y Puestos.
- Páginas: Puestos (Index, Crear, Editar), Clientes (Index, Crear, Editar), Empleados (Index, Crear, Editar, Detalle), Obras (Index, Crear, Editar, Detalle), Asistencias (Index: cuadrícula semanal), Nómina (Index, Detalle: generar, autorizar, marcar entregado), Préstamos (Index, Crear, Detalle).
- **Datos falsos** en la clase estática `DatosPrueba.cs`. Puestos y Clientes arman su propia lista en OnGet. **Todos los OnPost están vacíos** (solo redirigen).
- ⚠️ Obras/Detalle y Nómina/Detalle **calculan en la página** el saldo, el % usado, la alerta, el bruto y el neto. Al conectar, esos cálculos deben pasar a Negocios y la página solo debe mostrarlos.
- Ya usa `IdResponsable`. Inicio sigue siendo la plantilla de Microsoft y el sitio tiene `lang="en"`.
- **Plan de integración:** sección por sección, sustituir `DatosPrueba` por llamadas a los servicios de Negocios. Empezar por Puestos.

## 6. Pendientes del proyecto

**Reparto propuesto dentro de Nominas.Web (falta acordarlo con el frontend):**
- Backend (alumno): `Program.cs` (registro del DbContext y de los servicios), `appsettings.json` (cadena de conexión) y la lógica de los PageModel (`.cshtml.cs`: OnGet y OnPost, que llaman a Negocios).
- Frontend: vistas `.cshtml` (HTML y Razor), CSS, layout y `wwwroot`.

**Con el equipo antes del 22-nov:**
- Pasarle al compañero de BD los cambios del modelo (sección 4), en especial: tablas nuevas, columnas nuevas o quitadas, `id_responsable`, `asistencia.id_obra` NULL, `puesto.descripcion` NULL, `dias_trabajados` TINYINT UNSIGNED (o ajustar el tipo en C#).
- Corregir la redacción de RN-04, RN-10, RN-12, RN-13 y RN-15 en el PDF.

**Resueltas al adoptar las recomendaciones de la propuesta (20-nov):**
- Cuentas: solo el responsable tiene cuenta; el supervisor le reporta las asistencias.
- Umbral de alerta: un monto en pesos por obra (`LimiteAlertaSaldo`), configurado por el responsable.
- Pago: siempre por día realmente trabajado, sin descuento adicional por faltas.
- Bono: importe fijo por trabajador elegible, configurado en cada obra. Es elegible quien tenga al menos una asistencia en esa obra. Se aplica una vez, al cierre de la obra, y no se repite por empleado y obra.
- Pagos pendientes: se entregan completos y se actualiza el PagoEmpleado original. **No hay pagos parciales** (sin tabla PAGO).
- Historial: solo se guardan las tarifas aplicadas en cada detalle, sin historial completo de sueldos.
- Faltas: se registran solo a nivel trabajador (`IdObra` NULL).

- **Abono mayor que el neto (decidido el 20-nov): se DIFIERE.** Si el abono semanal es mayor que el bruto del trabajador en esa nómina, no se cobra abono esa semana: no se crea AbonoPrestamo, TotalDescuentos = 0, el trabajador recibe su bruto completo y SaldoPendiente no cambia. A la semana siguiente se cobra el abono normal (no doble), así que el préstamo dura una semana más. Lo de "no doble" lo supuso Claude; falta que el equipo lo confirme.

**Sigue abierta (la propuesta no da una solución):**
- (9) ¿Cuándo pasa una nómina de autorizada a pagada?
- Los montos concretos de bono y umbral de cada obra son datos que se capturan, no decisiones de diseño.

**Técnicos:**
- Configurar EF para guardar los enums como texto, igual que el ENUM de MySQL.
- Configurar en EF las restricciones únicas y las longitudes. Incluir **único en `nomina.fecha_inicio`** (una nómina por semana, RN-09); además, `GenerarBorrador` debe devolver la nómina existente si ya hay una para esa semana.
- Decidir si Web referencia explícitamente a Entidades.
