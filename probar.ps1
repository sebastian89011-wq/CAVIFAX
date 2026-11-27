<#
.SYNOPSIS
    Revisa el estado de todo el proyecto CAVIFAX.

.DESCRIPTION
    1. Pruebas unitarias (Nominas.Pruebas): reglas de negocio y ciclo de la nómina
       sobre una base SQLite en memoria. No tocan MySQL.
    2. Prueba de humo: compila la aplicación en una carpeta temporal, la levanta contra
       la base real (constructora_nominas), revisa que sin sesión pida el login, inicia sesión
       y abre cada página. Fuera del login, solo hace GET: no modifica datos.

.PARAMETER SinHumo
    Corre solo las pruebas unitarias (sirve sin MySQL).

.PARAMETER Puerto
    Puerto para la prueba de humo (por defecto 5199, distinto del de Visual Studio).

.PARAMETER Usuario
    Cuenta para iniciar sesión en la prueba de humo (la de database/02_datos.sql).

.PARAMETER Contrasena
    Contraseña de esa cuenta. Por defecto la de prueba de 02_datos.sql, o la variable
    de entorno CAVIFAX_CONTRASENA si ya la cambiaste.

.EXAMPLE
    .\probar.ps1
    .\probar.ps1 -SinHumo
#>
param(
    [switch]$SinHumo,
    [int]$Puerto = 5199,
    [string]$Usuario = 'rmendoza',
    [string]$Contrasena = $(if ($env:CAVIFAX_CONTRASENA) { $env:CAVIFAX_CONTRASENA } else { 'Cavifax2026!' })
)

$ErrorActionPreference = 'Stop'
$raiz = $PSScriptRoot
$resumen = [System.Collections.Generic.List[object]]::new()

function Escribir-Titulo([string]$texto) {
    Write-Host ""
    Write-Host "=== $texto ===" -ForegroundColor Cyan
}

function Agregar-Resultado([string]$prueba, [bool]$ok, [string]$detalle = "") {
    $resumen.Add([pscustomobject]@{ Prueba = $prueba; Estado = $(if ($ok) { 'OK' } else { 'FALLA' }); Detalle = $detalle })
}

# Sesión web: guarda la cookie de inicio de sesión entre peticiones
$script:sesionWeb = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# Devuelve el código HTTP y el contenido sin lanzar excepción en 4xx/5xx (PowerShell 5 y 7).
# EsLogin indica que la respuesta terminó en la página de inicio de sesión (sin permiso).
function Pedir-Pagina([string]$url, [switch]$SinSesion, [string]$Metodo = 'Get', [hashtable]$Cuerpo) {
    $parametros = @{ Uri = $url; UseBasicParsing = $true; TimeoutSec = 20; Method = $Metodo }
    if (-not $SinSesion) { $parametros.WebSession = $script:sesionWeb }
    if ($Cuerpo) { $parametros.Body = $Cuerpo }
    try {
        $r = Invoke-WebRequest @parametros
        $html = [string]$r.Content
        return @{ Codigo = [int]$r.StatusCode; Html = $html; EsLogin = ($html -match 'id="Contrasena"') }
    }
    catch {
        $respuesta = $_.Exception.Response
        if ($null -ne $respuesta) { return @{ Codigo = [int]$respuesta.StatusCode; Html = ""; EsLogin = $false } }
        return @{ Codigo = 0; Html = ""; EsLogin = $false }
    }
}

# Inicia sesión como lo haría el navegador: pide el formulario, toma el token
# antifalsificación y envía usuario y contraseña. Devuelve $true si entró.
function Iniciar-Sesion([string]$base) {
    $formulario = Pedir-Pagina "$base/Cuenta/Login"
    $token = [regex]::Match($formulario.Html, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
    $respuesta = Pedir-Pagina "$base/Cuenta/Login" -Metodo Post -Cuerpo @{
        __RequestVerificationToken = $token
        NombreUsuario              = $Usuario
        Contrasena                 = $Contrasena
    }
    return ($respuesta.Codigo -eq 200 -and -not $respuesta.EsLogin)
}

# ------------------------------------------------------------------ 1. Pruebas unitarias
Escribir-Titulo "1. Pruebas unitarias (SQLite en memoria)"
$carpetaResultados = Join-Path ([System.IO.Path]::GetTempPath()) "cavifax-resultados"
Remove-Item $carpetaResultados -Recurse -Force -ErrorAction SilentlyContinue
Push-Location $raiz
try {
    $salida = & dotnet test "Nominas.Pruebas/Nominas.Pruebas.csproj" --nologo `
        --logger "trx;LogFileName=resultados.trx" --results-directory $carpetaResultados 2>&1
    $codigo = $LASTEXITCODE
}
finally { Pop-Location }

$trx = Join-Path $carpetaResultados "resultados.trx"
if (Test-Path $trx) {
    # Cada prueba, agrupada por archivo de pruebas (módulo)
    [xml]$xml = Get-Content $trx -Raw -Encoding UTF8
    $definiciones = @{}
    foreach ($def in $xml.TestRun.TestDefinitions.UnitTest) {
        $definiciones[$def.id] = $def.TestMethod.className -replace '^Nominas\.Pruebas\.', ''
    }
    $resultados = foreach ($r in $xml.TestRun.Results.UnitTestResult) {
        [pscustomobject]@{
            Modulo  = $definiciones[$r.testId]
            Nombre  = ($r.testName -split '\.')[-1]
            Ok      = ($r.outcome -eq 'Passed')
            Mensaje = $r.Output.ErrorInfo.Message
        }
    }

    foreach ($grupo in ($resultados | Sort-Object Modulo, Nombre | Group-Object Modulo)) {
        $okGrupo = @($grupo.Group | Where-Object Ok).Count
        Write-Host ""
        Write-Host ("{0}  ({1}/{2})" -f $grupo.Name, $okGrupo, $grupo.Count) -ForegroundColor White
        foreach ($t in $grupo.Group) {
            if ($t.Ok) { Write-Host "  [OK]    $($t.Nombre)" -ForegroundColor Green }
            else {
                Write-Host "  [FALLA] $($t.Nombre)" -ForegroundColor Red
                if ($t.Mensaje) { Write-Host ("          " + ($t.Mensaje -split "`n")[0].Trim()) -ForegroundColor DarkRed }
            }
        }
    }

    $correctas = @($resultados | Where-Object Ok).Count
    $total = @($resultados).Count
    Agregar-Resultado "Pruebas unitarias" ($codigo -eq 0) "$correctas de $total correctas"
}
else {
    # No se generaron resultados: casi siempre es un error de compilación
    $salida | ForEach-Object { "$_" } | Where-Object { $_ -match 'error' } | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    Agregar-Resultado "Pruebas unitarias" $false "no compilaron (ver errores arriba)"
}

# ------------------------------------------------------------------ 2. Prueba de humo
if (-not $SinHumo) {
    Escribir-Titulo "2. Prueba de humo contra la base real (solo lectura)"

    $carpeta = Join-Path ([System.IO.Path]::GetTempPath()) "cavifax-prueba-humo"
    Write-Host "Compilando la aplicación en $carpeta ..."
    $compilacion = & dotnet build (Join-Path $raiz "Nominas.Web.csproj") -o $carpeta --nologo -v q 2>&1
    $compilo = ($LASTEXITCODE -eq 0)
    Agregar-Resultado "Compilación de la aplicación" $compilo ""
    if (-not $compilo) {
        $compilacion | Where-Object { "$_" -match 'error' } | ForEach-Object { Write-Host "$_" -ForegroundColor Red }
    }
    else {
        $base = "http://localhost:$Puerto"
        $env:ASPNETCORE_ENVIRONMENT = 'Development'   # para que lea la contraseña de User Secrets
        $app = Start-Process -FilePath "dotnet" `
            -ArgumentList @("`"$(Join-Path $carpeta 'Nominas.Web.dll')`"", "--urls", $base, "--contentRoot", "`"$raiz`"") `
            -WorkingDirectory $raiz -PassThru -WindowStyle Hidden

        try {
            # Esperar a que la aplicación responda (máximo 40 s)
            $lista = $false
            for ($i = 0; $i -lt 40 -and -not $lista; $i++) {
                Start-Sleep -Seconds 1
                $lista = ((Pedir-Pagina "$base/").Codigo -ne 0)
            }
            Agregar-Resultado "La aplicación arranca" $lista $base

            if ($lista) {
                # Sin sesión, cualquier página debe mandar al login
                $sinSesion = Pedir-Pagina "$base/Empleados" -SinSesion
                Agregar-Resultado "Sin sesión pide el login" ($sinSesion.EsLogin) "GET /Empleados"
                Write-Host ("  Sin sesión, /Empleados lleva al login: {0}" -f $(if ($sinSesion.EsLogin) { 'sí' } else { 'NO' })) `
                    -ForegroundColor $(if ($sinSesion.EsLogin) { 'Green' } else { 'Red' })

                $entro = Iniciar-Sesion $base
                Agregar-Resultado "Inicio de sesión" $entro "usuario $Usuario"
                Write-Host ("  Inicio de sesión como {0}: {1}" -f $Usuario, $(if ($entro) { 'correcto' } else { 'FALLÓ' })) `
                    -ForegroundColor $(if ($entro) { 'Green' } else { 'Red' })
                if (-not $entro) {
                    Write-Host "  Revisa la contraseña (parámetro -Contrasena o variable CAVIFAX_CONTRASENA)." -ForegroundColor Yellow
                }
            }

            if ($lista -and $entro) {
                $paginas = @(
                    '/', '/Puestos', '/Puestos/Crear', '/Clientes', '/Clientes/Crear',
                    '/Empleados', '/Empleados/Crear', '/Empleados?Buscar=a',
                    '/Obras', '/Obras/Crear', '/Obras?Estatus=0',
                    '/Asistencias', '/NominaSemanal', '/Prestamos', '/Prestamos/Crear',
                    '/Cuenta/CambiarContrasena', '/Ayuda', '/img/manual/02-inicio.png',
                    '/css/site.css', '/css/glass.css'
                )

                # Páginas de detalle y edición: se toma el primer registro que aparezca en cada lista
                $detalles = @(
                    @{ Lista = '/Empleados'; Patron = '/Empleados/Detalle/(\d+)'; Rutas = @('/Empleados/Detalle/{0}', '/Empleados/Editar/{0}') },
                    @{ Lista = '/Obras'; Patron = '/Obras/Detalle/(\d+)'; Rutas = @('/Obras/Detalle/{0}', '/Obras/Editar/{0}') },
                    @{ Lista = '/Prestamos'; Patron = '/Prestamos/Detalle/(\d+)'; Rutas = @('/Prestamos/Detalle/{0}') },
                    @{ Lista = '/NominaSemanal'; Patron = '/NominaSemanal/Detalle/(\d+)'; Rutas = @('/NominaSemanal/Detalle/{0}') },
                    @{ Lista = '/Puestos'; Patron = '/Puestos/Editar/(\d+)'; Rutas = @('/Puestos/Editar/{0}') },
                    @{ Lista = '/Clientes'; Patron = '/Clientes/Editar/(\d+)'; Rutas = @('/Clientes/Editar/{0}') }
                )
                foreach ($d in $detalles) {
                    $html = (Pedir-Pagina "$base$($d.Lista)").Html
                    $m = [regex]::Match($html, $d.Patron)
                    if ($m.Success) {
                        foreach ($ruta in $d.Rutas) { $paginas += ($ruta -f $m.Groups[1].Value) }
                    }
                    else {
                        Write-Host "  (sin registros en $($d.Lista); se omiten sus páginas de detalle)" -ForegroundColor DarkYellow
                    }
                }

                foreach ($p in $paginas) {
                    $r = Pedir-Pagina "$base$p"
                    $ok = ($r.Codigo -eq 200 -and -not $r.EsLogin)
                    $detalle = $(if ($r.EsLogin) { "HTTP $($r.Codigo) (mandó al login)" } else { "HTTP $($r.Codigo)" })
                    $color = $(if ($ok) { 'Green' } else { 'Red' })
                    Write-Host ("  {0,-34} {1}" -f $p, $detalle) -ForegroundColor $color
                    Agregar-Resultado "GET $p" $ok $detalle
                }

                if (($resumen | Where-Object { $_.Prueba -like 'GET*' -and $_.Detalle -eq 'HTTP 500' }).Count -gt 0) {
                    Write-Host "  Hay páginas con error 500: revisa que MySQL esté encendido y que la contraseña esté en User Secrets (ver README)." -ForegroundColor Yellow
                }
            }
        }
        finally {
            if ($app -and -not $app.HasExited) { Stop-Process -Id $app.Id -Force }
        }
    }
}

# ------------------------------------------------------------------ Resumen
Escribir-Titulo "Resumen"
$resumen | Format-Table -AutoSize | Out-String | Write-Host

$fallas = @($resumen | Where-Object { $_.Estado -eq 'FALLA' }).Count
if ($fallas -eq 0) {
    Write-Host "Todo bien: $($resumen.Count) revisiones correctas." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "$fallas de $($resumen.Count) revisiones fallaron." -ForegroundColor Red
    exit 1
}
