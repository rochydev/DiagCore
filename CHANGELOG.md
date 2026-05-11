# Changelog

Todos los cambios destacables de este proyecto se documentan aquí.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y este proyecto se adhiere a [Semantic Versioning](https://semver.org/lang/es/).

---

## [Unreleased]

### Próximamente — Fase 3.5

- Acciones de reparación de sistema (SFC, DISM, chkdsk) con
  consola embebida y streaming de stdout en tiempo real.
- Reset de pila de red, vaciado de caché DNS, reparación de caché
  de Windows Update.
- Limpieza de archivos temporales con previsualización del espacio
  liberable.
- Informe de batería y de eficiencia energética (`powercfg /batteryreport`,
  `/energy`).

### Próximamente — Fase 4

- Generación de informes en PDF con [QuestPDF](https://www.questpdf.com/).
- Histórico local en SQLite + EF Core.
- Wizard de exportación con preselección de secciones y datos del
  técnico.
- Comparación entre dos informes del mismo equipo (v1.1).

### Próximamente — Fase 5

- Auto-update con [Velopack](https://github.com/velopack/velopack)
  desde GitHub Releases.
- Workflow `release.yml` en GitHub Actions disparado por tag
  `v*.*.*`.

### Próximamente — Fase 6

- Pulido visual: estados vacíos diseñados, mensajes de error
  amigables, splash screen.
- Icono `.ico` multi-resolución para el `.exe`.
- Screenshots para el README.
- Release **v1.0.0**.

---

## [0.1.0] — 2026-05-11

Primera release pública. Incluye toda la arquitectura base más las
cuatro primeras fases del plan: andamiaje, sistema de diseño,
núcleo de diagnóstico y vistas funcionales con datos reales.

### Added

- **Solución .NET 10** con tres proyectos:
  - `DiagCore.App` (WPF, x64)
  - `DiagCore.Core` (lógica de diagnóstico, sin UI)
  - `DiagCore.Tests` (xUnit + FluentAssertions)
- **Custom chrome estilo Windows 11**: `FluentWindow` con Mica
  backdrop, sin `ui:TitleBar`, botones de minimizar / maximizar /
  cerrar implementados a mano con hover azul (cerrar en rojo).
- **Sidebar de navegación** con un único `ListBox` + `DockPanel`:
  seis secciones arriba y Settings anclado al pie. Item activo
  con icono en azul (`AccentBrush`) y barra vertical de 3 px.
- **Sistema de diseño** en `Resources/Themes/`:
  - Paleta oscura propia (#0F1115 / #161922 / #1E2230 / #232838
    + acento #3B82F6 + semánticos verde/ámbar/rojo/cian).
  - Tipografía: Inter (variable) y JetBrains Mono embebidas como
    `Resource`, licencias OFL incluidas.
  - Estilos base: Card, Button, PrimaryButton, TextBox, badges,
    barras de uso.
- **Controles reutilizables**: `KeyValueRow`, `StatTile`
  (tile-héroe con número grande + barra coloreada por umbral),
  `StatusBadge` con cinco variantes semánticas.
- **Servicios de diagnóstico** (`DiagCore.Core/Diagnostics/`):
  1. `SystemDiagnostics` — OS, máquina, BIOS/UEFI, placa base,
     Secure Boot, modo de arranque.
  2. `HardwareDiagnostics` — CPU, RAM (totales + módulos), GPU,
     batería, sensores ACPI.
  3. `StorageDiagnostics` — volúmenes lógicos, discos físicos con
     SMART, particiones.
  4. `NetworkDiagnostics` — adaptadores con IPs, ping, traceroute,
     port test TCP, DNS, IP pública opt-in.
  5. `SecurityDiagnostics` — Microsoft Defender, firewall, usuarios
     locales, administradores.
  6. `ProcessDiagnostics` — procesos, servicios, autoruns,
     hotfixes (KB instalados).
  7. `EventLogDiagnostics` — eventos Critical + Error de las
     últimas 24 horas.
- **Tipo `DiagnosticResult<T>`** (record struct con
  `[MemberNotNullWhen]`) para devolver éxito / fallo sin lanzar
  excepciones desde los servicios.
- **Helper `WmiQuery`** que centraliza
  `ManagementObjectSearcher`, disposal y proyección a modelos.
- **80 tests unitarios** sobre los parsers puros (fechas CIM,
  códigos enum, formateadores).
- **Vistas funcionales con datos reales**:
  - 🏠 **Inicio** — health score 0-100, cuatro KPI tiles
    (CPU%, RAM%, disco sistema, eventos críticos 24h), top 5
    procesos por CPU y por RAM, lista de eventos críticos.
  - 🖥️ **Hardware** — tres tiles héroe (CPU%, RAM, uptime) más
    seis cards en dos columnas.
  - 💾 **Almacenamiento** — tiles, lista de volúmenes con barras
    coloreadas según uso, lista de discos físicos con badge SMART,
    tabla de particiones.
  - 🌐 **Red** — tile IP local, adaptadores con sus IPs, herramienta
    de ping con estadísticas, consulta opcional de IP pública.
  - 🛡️ **Seguridad y Sistema** — Defender, perfiles de firewall,
    usuarios locales, miembros del grupo Administradores.
  - 📄 **Informes** — skeleton del wizard (export deferred a Fase 4).
  - ⚙️ **Configuración** — about, créditos, privacidad, botón para
    re-abrir la bienvenida.
- **Ventana de bienvenida** con custom chrome, logo grande,
  changelog inline, créditos al autor, resumen de privacidad y
  checkbox "no volver a mostrar al iniciar". Sólo aparece la
  primera vez por versión.
- **`PreferencesService`** persistido como JSON en
  `%LOCALAPPDATA%\DiagCore\preferences.json` (sin Registry, sin
  red — cumple la política de privacidad).
- **Logging** con Serilog a archivos diarios rotativos en
  `%LOCALAPPDATA%\DiagCore\logs\`.
- **GitHub Actions** workflow de build + tests en `windows-latest`.
- Documentación: `README.md`, `CHANGELOG.md`, `CONTRIBUTING.md`,
  `PLAN.md`, `docs/legacy/` con el script PowerShell origen.

### Privacidad

- Cero telemetría a servidores propios.
- Ninguna llamada de red al arrancar.
- Única salida posible: `api.ipify.org`, opt-in por gesto explícito
  del usuario.
