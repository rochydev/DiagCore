# DiagCore — Plan de Proyecto

> **Documento maestro del proyecto.**
> Contiene contexto, arquitectura, stack, fases y especificaciones de UI.
> Léelo entero antes de empezar a escribir código.

---

## 🎯 Visión del producto

**DiagCore** es una aplicación de escritorio para Windows orientada a administradores técnicos de sistemas, IT helpdesk y MSPs. Sustituye el paso de ejecutar manualmente decenas de comandos (`sfc`, `dism`, `chkdsk`, `Get-PhysicalDisk`, etc.) por una interfaz moderna, oscura y profesional que centraliza todo el diagnóstico en un único lugar.

**Filosofía:**
- 100% local: ningún dato sale del equipo
- 100% gratuita y sin autenticación
- Sin telemetría
- Open source (licencia MIT)
- Distribuida vía GitHub Releases

**No es:** un anticheat, un antivirus, un agente de monitorización en tiempo real, ni un SaaS. Es una herramienta de escritorio para una intervención técnica puntual.

**Autor:** Roger ([RochyDev](https://github.com/RochyDev))
**Origen:** evolución del script PowerShell `sysadmin-diagnosticador-script`.

---

## 🛠️ Stack técnico (decisiones cerradas)

| Capa | Tecnología | Motivo |
|------|------------|--------|
| **Lenguaje** | C# 14 | Madurez, Windows nativo, gran ecosistema |
| **Framework UI** | WPF (.NET 10) | Control total sobre estilos, animaciones potentes |
| **Componentes UI** | [WPF-UI](https://github.com/lepoco/wpfui) | Componentes Fluent modernos, mantenido, gratuito |
| **Iconos** | [Lucide.Wpf](https://github.com/khalidabuhakmeh/Lucide.Avalonia) o WPF-UI SymbolIcon | Iconografía minimalista coherente |
| **Gráficos** | [LiveCharts2](https://livecharts.dev/) | Animaciones suaves, gauges, donuts, líneas |
| **Acceso a sistema** | `System.Management` (WMI) + `Microsoft.Win32` + ejecución de procesos | Lo que ya usa el script PowerShell pero nativo |
| **Generación de PDF** | [QuestPDF](https://www.questpdf.com/) | API fluida, gratis para uso comercial bajo cierto revenue |
| **Persistencia local** | SQLite + EF Core | Histórico de informes |
| **Auto-update** | [Velopack](https://github.com/velopack/velopack) | Update desde GitHub Releases, sin backend |
| **Logging** | Serilog → archivo local | Diagnóstico de fallos de la propia app |
| **DI** | `Microsoft.Extensions.DependencyInjection` | Estándar .NET |
| **Testing** | xUnit + FluentAssertions | Para servicios críticos |
| **Build/CI** | GitHub Actions | Build, test y release automáticos |
| **Instalador** | Velopack genera `.exe` y `Setup.exe` | Una sola fuente |

**Versión .NET objetivo:** .NET 10 (LTS hasta noviembre 2028).
**Plataforma objetivo:** Windows 10 1809+ y Windows 11. Windows Server 2019/2022/2025.
**Arquitectura binaria:** x64 (única, no se publica x86 ni ARM en MVP).

---

## 🎨 Sistema de diseño

### Paleta de colores (modo oscuro único en MVP)

```
/* Fondos */
--bg-primary:    #0F1115   /* fondo general */
--bg-secondary:  #161922   /* cards, paneles */
--bg-tertiary:   #1E2230   /* hover, inputs */
--bg-elevated:   #232838   /* tooltips, popovers */

/* Acentos */
--accent:        #3B82F6   /* azul primario (botones, focos, activos) */
--accent-hover:  #2563EB
--accent-soft:   rgba(59, 130, 246, 0.12)

/* Estados semafóricos */
--ok:            #10B981   /* verde */
--warning:       #F59E0B   /* ámbar */
--danger:        #EF4444   /* rojo */
--info:          #06B6D4   /* cian */

/* Texto */
--text-primary:  #E5E7EB
--text-secondary:#9CA3AF
--text-muted:    #6B7280
--text-disabled: #4B5563

/* Bordes */
--border:        #2A2F3F
--border-strong: #3A4055
--border-focus:  #3B82F6
```

### Tipografía

- **Principal:** Inter (incluida como recurso embebido)
- **Monoespaciada:** JetBrains Mono (para outputs de comandos, valores numéricos en cards)
- Tamaños: 12 / 13 / 14 / 16 / 20 / 28 (scale)

### Espaciado y radios

- Spacing scale (px): 4, 8, 12, 16, 24, 32, 48
- Radio bordes: 6px (inputs, botones), 10px (cards), 14px (modales)
- Sombras: muy sutiles, solo en elementos elevados

### Animaciones (críticas para el "feel")

- Transiciones de tab: 200ms ease-out, fade + slide ligero (8px)
- Hover en botones/cards: 150ms ease
- Aparición de gráficos: 600ms con easing curvado
- Entrada de listas: stagger 30ms entre items
- Spinner de carga: rotación 1s infinita
- **Importante:** todas las animaciones deben respetar la preferencia del sistema "reducir movimiento" (`SystemParameters.ClientAreaAnimation`)

### Layout principal

Estructura inspirada en la captura de referencia (RedEngine):
navegación únicamente por **sidebar de iconos** a la izquierda, sin tabs en el
topbar. El title bar custom solo lleva la marca (logo + título) y los controles
de ventana.

```
┌──────────────────────────────────────────────────────────────┐
│  [Logo] DiagCore                              [_] [▢] [×]    │ ← TitleBar (52px)
├─────┬────────────────────────────────────────────────────────┤
│     │                                                        │
│ 🏠  │                                                        │
│ 🖥️  │              ÁREA DE CONTENIDO                         │
│ 💾  │              (cambia según sección)                    │
│ 🌐  │                                                        │
│ 🛡️  │                                                        │
│ 📄  │                                                        │
│     │                                                        │
│ ⚙️  │ ← Settings (anclado al pie del sidebar)                │
├─────┴────────────────────────────────────────────────────────┤
│  v1.0.0  •  DiagCore  •  developed by RochyDev    [Estado]   │ ← StatusBar (28px)
└──────────────────────────────────────────────────────────────┘
   ↑
 Sidebar
 (60px)
```

**Sidebar:** solo iconos, tooltip al hacer hover. Ítem activo con barra azul
vertical de 3px a la izquierda y fondo `--bg-tertiary`. El icono de
Configuración va anclado al pie.

**Ventana:** sin chrome de Windows estándar. Implementar **custom title bar**
con botones minimizar/maximizar/cerrar al estilo Windows 11 (animación de hover
sutil, rojo solo en cerrar al pasar por encima).

---

## 📐 Arquitectura del proyecto

### Estructura de solución

```
DiagCore.sln
├── src/
│   ├── DiagCore.App/                    # Proyecto WPF principal
│   │   ├── App.xaml(.cs)
│   │   ├── MainWindow.xaml(.cs)
│   │   ├── Views/                       # Vistas (UserControl por sección)
│   │   ├── ViewModels/                  # MVVM
│   │   ├── Controls/                    # Controles personalizados
│   │   ├── Converters/                  # IValueConverter
│   │   ├── Resources/                   # Estilos, plantillas, fuentes, iconos
│   │   │   ├── Themes/
│   │   │   │   ├── Colors.xaml
│   │   │   │   ├── Typography.xaml
│   │   │   │   └── Controls.xaml
│   │   │   └── Fonts/
│   │   ├── Services/                    # Servicios de aplicación (UI-side)
│   │   └── Assets/                      # Imágenes, iconos
│   │
│   ├── DiagCore.Core/                   # Lógica de negocio, sin UI
│   │   ├── Diagnostics/                 # Servicios de diagnóstico
│   │   │   ├── HardwareDiagnostics.cs
│   │   │   ├── StorageDiagnostics.cs
│   │   │   ├── SystemDiagnostics.cs
│   │   │   ├── NetworkDiagnostics.cs
│   │   │   ├── SecurityDiagnostics.cs
│   │   │   └── ProcessDiagnostics.cs
│   │   ├── Models/                      # POCOs / DTOs
│   │   ├── Repair/                      # Acciones reparadoras (SFC, DISM...)
│   │   ├── Reports/                     # Generación de PDF, exportación
│   │   ├── History/                     # SQLite, EF Core
│   │   └── Common/                      # Helpers, extensiones
│   │
│   └── DiagCore.Tests/                  # Tests unitarios
│
├── docs/                                # Documentación
│   ├── architecture.md
│   ├── ui-spec.md
│   └── screenshots/
│
├── .github/
│   └── workflows/
│       ├── build.yml
│       └── release.yml
│
├── README.md
├── LICENSE
├── .gitignore
└── DiagCore.sln
```

### Patrón arquitectónico

- **MVVM estricto** con `CommunityToolkit.Mvvm` (atributos `[ObservableProperty]`, `[RelayCommand]`)
- **Inyección de dependencias** con `Microsoft.Extensions.DependencyInjection`
- **Services por capa**:
  - `DiagCore.Core/Diagnostics/*` — recolección de datos del sistema (interfaz + implementación)
  - `DiagCore.App/Services/*` — servicios de UI (navegación, dialogs, theme, config)
- **Operaciones largas async** con `IProgress<T>` para reportar progreso a la UI
- **CancellationToken** en todos los métodos largos (escaneos, SFC, DISM, etc.)

### Convenciones de código

- Naming estándar .NET (`PascalCase` para públicos, `_camelCase` para campos privados)
- Comentarios XML en interfaces y servicios públicos
- Nada de `Thread.Sleep` — siempre `await Task.Delay`
- No bloquear el hilo de UI con WMI o procesos: todo `Task.Run` o async desde el primer momento
- Strings de UI en `Resources.resx` para i18n futuro (MVP solo español)

---

## 🧩 Funcionalidades por sección

Cada tab del MVP es una sección independiente. Listadas en orden de aparición en la TopBar.

### Tab 1 — 🏠 Inicio (Dashboard)

Vista de aterrizaje al abrir la app. Resumen visual rápido del estado del equipo.

**Componentes:**
- **Header con info del equipo**: nombre, usuario, dominio, modelo, OS, uptime
- **Score de salud general** (0-100, calculado en `Core`) con gauge animado
- **4 cards principales con gauges**:
  - CPU (% uso actual)
  - RAM (% uso)
  - Disco C: (% lleno)
  - Estado SMART agregado (todos los discos: OK/Warning/Critical)
- **Card "Procesos top"**: top 5 por CPU, top 5 por RAM, en dos columnas
- **Card "Eventos críticos últimas 24h"**: contador con badge rojo si hay
- **Botón grande "Generar informe completo"** que lleva al Tab 6 con el wizard

**Carga:** la primera carga del dashboard ejecuta los escaneos básicos en paralelo con un loader sutil. Refresco manual con botón ↻ en la esquina superior derecha.

### Tab 2 — 🖥️ Hardware

Detalle completo del hardware del equipo. Vista en columnas.

**Subsecciones (acordeones o subtabs):**
- **Sistema**: OS, build, arquitectura, fabricante, modelo, S/N, dominio, último arranque, uptime
- **CPU**: modelo, fabricante, núcleos físicos, lógicos, velocidades, socket, carga actual
- **Memoria RAM**: total, usado, libre, módulos físicos (capacidad, velocidad, fabricante, slot)
- **Discos físicos**: cada disco con modelo, tipo medio, bus, tamaño, salud SMART, estado operativo
- **GPU**: modelo, procesador, driver, fecha driver, VRAM, resolución
- **BIOS/UEFI**: fabricante, versión, fecha, S/N, modo arranque (UEFI/Legacy), placa base
- **Temperaturas** (si están disponibles vía ACPI WMI)
- **Batería** (solo si existe): nombre, carga actual, estado, botón "Generar batteryreport"

### Tab 3 — 💾 Almacenamiento

Volúmenes y particiones, herramientas de reparación de disco.

**Componentes:**
- **Lista de volúmenes** con barra de progreso por unidad, espacio total/usado/libre, sistema de archivos, etiqueta
- **Visualización de discos físicos** con sus particiones (estilo barra con segmentos coloreados)
- **Acciones** (botones con confirmación):
  - Comprobar errores (chkdsk /scan)
  - Programar chkdsk /f /r
  - Optimizar/desfragmentar
  - Limpiar archivos temporales (mostrar previsualización del espacio a liberar)
  - Abrir Administración de discos

### Tab 4 — 🌐 Red

Diagnóstico completo de red.

**Componentes:**
- **Adaptadores de red**: lista con estado (Up/Down con punto de color), descripción, MAC, velocidad, IPv4, IPv6
- **Configuración IP completa** (panel con copia rápida)
- **IP pública** (con botón refresh, llamada a `api.ipify.org`)
- **Herramientas**:
  - Ping (input host, output con estadísticas en vivo)
  - Tracert (input host, tabla de saltos)
  - Test de puerto TCP (input host + puerto, indicador OK/FAIL)
  - Resolver DNS (input dominio, lista de IPs)
- **Conexiones activas** (netstat parseado en tabla)
- **Acciones de reparación**:
  - Vaciar caché DNS
  - Reset de pila de red (con warning grande)

### Tab 5 — 🛡️ Seguridad y Sistema

Mezcla de Defender, firewall y herramientas de reparación del sistema.

**Componentes:**
- **Estado de Defender**: antivirus habilitado, RT, versión motor, definiciones, última actualización, último escaneo
- **Estado del firewall** (perfiles Domain/Private/Public con switches visuales)
- **Usuarios locales** (tabla)
- **Grupo Administradores** (lista de miembros)
- **Acciones**:
  - Actualizar firmas Defender
  - Escaneo rápido / completo (con progress bar real)
  - SFC /scannow (output streaming en consola embebida)
  - DISM CheckHealth / ScanHealth / RestoreHealth
  - Reparar caché de Windows Update (con paso a paso visual)

**Importante:** ejecutar SFC y DISM con captura de stdout y mostrar el progreso real en una vista de consola estilizada (fondo `--bg-elevated`, fuente JetBrains Mono).

### Tab 6 — 📄 Informes

Generación de informes y histórico.

**Componentes:**
- **Wizard de informe**:
  1. Seleccionar secciones a incluir (checkboxes)
  2. Datos del cliente (opcional): nombre, empresa, contacto del técnico (se guarda como preset)
  3. Vista previa
  4. Exportar (PDF / TXT / JSON)
- **Histórico local** (SQLite): tabla con fecha, equipo escaneado, score, acciones (Ver / Eliminar / Reabrir)
- **Comparar dos informes** del mismo equipo en distintos momentos (función v1.1, dejar punto de extensión)

**Diseño del PDF:**
- Portada con logo DiagCore + datos del equipo + fecha
- Índice
- Una sección por cada bloque seleccionado
- Tablas con estilos coherentes con la app (header azul, filas alternas)
- Footer con número de página y "Generated by DiagCore"
- **Marca personal:** "Generated with DiagCore — by RochyDev — github.com/RochyDev/DiagCore"

### Configuración (icono ⚙️ en sidebar)

Modal o vista lateral. Contiene:
- Tema (oscuro forzado en MVP, dejar punto de extensión para claro en v1.1)
- Idioma (solo español en MVP)
- Comprobar actualizaciones (manual)
- Activar/desactivar auto-update
- Preset de datos del técnico para informes
- Carpeta de informes guardados (botón "Abrir carpeta")
- Versión, créditos, enlaces a GitHub e Issues

---

## 🚀 Auto-update (desde el día 1)

Implementación con **Velopack** + **GitHub Releases**.

### Flujo

1. La app se instala con un `Setup.exe` generado por Velopack.
2. Al arrancar, en background, comprueba si hay nueva release en `github.com/RochyDev/DiagCore/releases`.
3. Si hay update: descarga el delta o el paquete, lo aplica al cerrar.
4. Notificación discreta en la StatusBar: "Nueva versión disponible · Reiniciar para aplicar".

### Setup técnico

```csharp
// En App.xaml.cs - OnStartup
VelopackApp.Build().Run();

// Comprobación de updates (servicio)
var updateManager = new UpdateManager("https://github.com/RochyDev/DiagCore");
var newVersion = await updateManager.CheckForUpdatesAsync();
if (newVersion != null) {
    await updateManager.DownloadUpdatesAsync(newVersion);
    // Mostrar notificación al usuario
}
```

### Workflow de release (GitHub Actions)

`.github/workflows/release.yml` se dispara con un tag `v*.*.*`:

1. Build de la solución en modo Release
2. Tests
3. Publish con `dotnet publish` self-contained
4. `vpk pack` genera el instalador y los deltas
5. Sube los artefactos a la GitHub Release como assets

---

## 📅 Fases de desarrollo

Cada fase debe quedar **funcional y commiteable** antes de pasar a la siguiente. Se hace commit al final de cada fase, con verificación humana antes de avanzar.

### Fase 0 — Andamiaje (1-2 sesiones)

- [ ] Crear solución `DiagCore.sln` con los 3 proyectos
- [ ] Configurar `.gitignore`, `Directory.Build.props`, `nuget.config`
- [ ] Añadir paquetes NuGet base: `WPF-UI`, `CommunityToolkit.Mvvm`, `Microsoft.Extensions.Hosting`, `Serilog`
- [ ] Crear `App.xaml` con configuración de DI básica
- [ ] `MainWindow` mínima (custom chrome, sidebar y topbar vacíos)
- [ ] Pipeline GitHub Actions para `build.yml` (compilar y testear en cada push)
- [ ] README inicial del repo apuntando a este plan

**Criterio de hecho:** la app abre, muestra ventana custom oscura, cierra correctamente.

### Fase 1 — Sistema de diseño y navegación (1-2 sesiones)

- [ ] Definir `ResourceDictionary` con paleta y tipografía (`Colors.xaml`, `Typography.xaml`)
- [ ] Embebido de fuentes Inter y JetBrains Mono
- [ ] Estilos base de Button, ToggleButton, TextBox, Card
- [ ] Sidebar con iconos y navegación entre vistas placeholder
- [ ] TopBar con tabs estilo pill animados
- [ ] StatusBar con versión y créditos
- [ ] Animaciones de transición entre tabs

**Criterio de hecho:** se puede navegar por las 6 tabs (con vistas placeholder), todo se ve coherente con el sistema de diseño.

### Fase 2 — Núcleo de diagnósticos (3-4 sesiones)

Portar la lógica del script PowerShell a `DiagCore.Core/Diagnostics/*`:

- [ ] `HardwareDiagnostics`: SO, CPU, RAM, GPU, BIOS, batería, temperatura
- [ ] `StorageDiagnostics`: volúmenes, discos físicos, particiones, SMART
- [ ] `NetworkDiagnostics`: adaptadores, IPs, ping, tracert, puertos, DNS
- [ ] `SecurityDiagnostics`: Defender, firewall, usuarios
- [ ] `ProcessDiagnostics`: procesos, servicios, autoruns
- [ ] Tests unitarios para parsers y mapeos de WMI

**Criterio de hecho:** los servicios devuelven modelos correctos sin tocar UI. Tests verdes.

### Fase 3 — Vistas funcionales (4-5 sesiones)

Una sesión por tab aproximadamente:

- [ ] Tab Inicio (dashboard con gauges)
- [ ] Tab Hardware
- [ ] Tab Almacenamiento (con acciones de reparación)
- [ ] Tab Red (con herramientas)
- [ ] Tab Seguridad y Sistema (con consola embebida)
- [ ] Tab Informes (placeholder de export)

**Criterio de hecho:** cada tab muestra datos reales, acciones funcionan, animaciones de carga visibles.

### Fase 4 — Informes PDF y persistencia (2 sesiones)

- [ ] Integración de QuestPDF
- [ ] Plantilla de informe completa
- [ ] Wizard de exportación
- [ ] SQLite + EF Core para histórico
- [ ] Vista de histórico con tabla

**Criterio de hecho:** se puede generar PDF, queda guardado en histórico, se puede reabrir.

### Fase 5 — Auto-update y release (1 sesión)

- [ ] Integración de Velopack
- [ ] Comprobación al arranque
- [ ] Notificación en StatusBar
- [ ] Workflow `release.yml` en GitHub Actions
- [ ] Test manual: publicar v0.1.0, simular v0.1.1, verificar update

**Criterio de hecho:** publicar una nueva release en GitHub hace que las instalaciones existentes detecten y apliquen el update.

### Fase 6 — Pulido y v1.0 (2 sesiones)

- [ ] Iconos consistentes en toda la app
- [ ] Estados vacíos diseñados (cuando no hay datos)
- [ ] Mensajes de error amigables
- [ ] Verificación de comportamiento sin admin (mostrar avisos)
- [ ] Splash screen con animación de carga
- [ ] Manifest UAC (`requireAdministrator` opcional / asInvoker)
- [ ] Documentación final
- [ ] Screenshots para README
- [ ] Release v1.0.0

---

## ⚠️ Consideraciones importantes de implementación

### Privilegios

- La app debe arrancar **sin requerir admin** (asInvoker). Las funciones que necesiten elevación detectan la ausencia de privilegios y:
  - Muestran badge "Requiere admin" en el botón
  - Al pulsar, ofrecen "Reiniciar como administrador" (usando `Process.Start` con `Verb = "runas"`)
- Funciones que requieren admin: SFC, DISM, reset red, reparar Windows Update, escaneo completo Defender, chkdsk /f /r.

### Operaciones largas

- **Nunca bloquear UI**: usar siempre `Task.Run` para WMI/procesos pesados.
- Mostrar feedback visual: skeletons al cargar, progress determinista cuando se conoce el avance, indeterminado si no.
- Permitir cancelar (ej: ping continuo, escaneo completo Defender) con `CancellationToken`.

### Llamadas a procesos externos

Para SFC, DISM, chkdsk, ipconfig, etc.: usar `Process.Start` con redirección de stdout/stderr y mostrar la salida en tiempo real en la consola embebida. Conservar el código de salida y mostrarlo al final.

### WMI vs nativo

- WMI es lento. Para refrescos en vivo (CPU%, RAM%) usar `PerformanceCounter` o `System.Diagnostics`.
- Para info estática (modelo CPU, BIOS, etc.) WMI es aceptable, pero cachear los resultados.

### Manejo de errores

- Nunca crashear por un fallo de WMI o de un comando externo.
- Cada servicio devuelve un `Result<T>` o equivalente con éxito/fallo y mensaje legible.
- Errores se loggean con Serilog y se muestran al usuario como toast no intrusivo.

### Internacionalización

- MVP solo en español, pero todos los strings van a `Resources.resx`. Nada de strings hardcoded en XAML/código.
- Dejar la infraestructura preparada para añadir inglés en v1.1.

### Antivirus / Smartscreen

- La app, al hacer cosas que parecen "intrusivas" (escanear procesos, llamar a netsh, etc.), puede ser marcada por antivirus.
- **Mitigación:** firmar los binarios con un certificado de code signing en cuanto haya presupuesto. Para v1.0 sin firma, documentar claramente en el README que Smartscreen puede pedir confirmación la primera vez.

### Privacidad

- **Cero telemetría**. No se envía nada a ningún servidor (excepto la llamada a api.ipify.org que es opcional y solo cuando el usuario pulsa "ver IP pública").
- **Cero recogida de datos**. Los informes se quedan en el equipo del usuario.
- Esto se documenta en el README como argumento de venta.

---

## 📦 Entregables al final del desarrollo

1. Repositorio público `github.com/RochyDev/DiagCore` con código completo
2. Releases con instalador (`.exe`) y portable (`.zip`)
3. Auto-update funcionando contra Releases
4. README con screenshots, instalación, uso, FAQ
5. CHANGELOG.md
6. CONTRIBUTING.md (cómo contribuir)
7. Etiquetas de GitHub para issues (`bug`, `enhancement`, `good first issue`)

---

## 💡 Cómo usar este documento

Este plan está pensado como **referencia maestra** durante el desarrollo:

1. **Lee el documento entero antes de tocar código.** Ahorra rehacer
   decisiones que ya están tomadas (stack, estética, fases).
2. **Avanza por fases, no toda la app de golpe.** Cada fase tiene un
   criterio de hecho explícito al final.
3. **Revisa cada fase antes de aprobar la siguiente.** Compila, abre la
   app, navega, ejecuta tests.
4. **Estética**: si algún detalle visual no está en este plan,
   discútelo antes de implementarlo.
5. **Commits pequeños y descriptivos**: uno por feature o cambio
   lógico, no uno gigante por fase. Mensajes en formato convencional
   (`feat:`, `fix:`, `chore:`, `docs:`, etc.).
6. **Tests desde el día 1**: a partir de la Fase 2, todo servicio de
   `Core` lleva tests unitarios.

---

## 🗓️ Estimación temporal realista

Trabajando en sesiones de 2-3 horas:

| Fase | Sesiones | Horas |
|------|----------|-------|
| 0 — Andamiaje | 1-2 | 3-5 |
| 1 — Sistema de diseño | 2 | 5-7 |
| 2 — Núcleo diagnósticos | 4 | 10-14 |
| 3 — Vistas funcionales | 5 | 15-20 |
| 4 — Informes y persistencia | 2 | 6-8 |
| 5 — Auto-update y release | 1 | 3-4 |
| 6 — Pulido | 2 | 6-8 |
| **TOTAL** | **~17 sesiones** | **~50-65h** |

Si trabajas 5-6 horas a la semana, **MVP listo en ~10 semanas**. Razonable.

---

## 📝 Resumen ejecutivo

**Producto:** DiagCore — app Windows de diagnóstico técnico para sysadmins. Gratuita, open source, todo local, sin login.

**Stack:** C# 14, WPF, .NET 10, WPF-UI, LiveCharts2, QuestPDF, SQLite + EF Core, Velopack para auto-update, GitHub Actions para CI/CD.

**Estética:** oscura con acento azul, layout sidebar + topbar + content + statusbar, animaciones suaves, custom title bar Windows 11.

**6 secciones:** Inicio (dashboard), Hardware, Almacenamiento, Red, Seguridad y Sistema, Informes.

**Distribución:** GitHub Releases con instalador Velopack y auto-update.

**Privacidad:** cero telemetría, cero datos enviados a servidores, todo en el equipo del usuario.

**Plan de ejecución:** 7 fases (0 a 6), aprobación humana al final de cada una.
