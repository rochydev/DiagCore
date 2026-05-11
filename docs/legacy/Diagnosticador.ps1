<#
.SYNOPSIS
    Diagnosticador de Sistemas - Herramienta para administradores tecnicos.

.DESCRIPTION
    Menu interactivo en terminal para diagnostico, reparacion y analisis
    de equipos Windows / Windows Server.

.NOTES
    Autor      : RochyDev
    Version    : 1.0
    Compatible : Windows 10 / 11 / Server 2016+
    Ejecucion  : Doble clic en Diagnosticador.bat (eleva a admin)
                 o bien: powershell -ExecutionPolicy Bypass -File .\Diagnosticador.ps1
#>

# ============================================================
#  CONFIGURACION INICIAL
# ============================================================

$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$Host.UI.RawUI.WindowTitle = "Diagnosticador de Sistemas - by RochyDev"

# ============================================================
#  COMPROBACION DE PRIVILEGIOS DE ADMINISTRADOR
# ============================================================
function Test-Administrador {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# ============================================================
#  FUNCIONES DE INTERFAZ
# ============================================================

# IMPORTANTE: parametros NOMBRADOS para evitar problemas con switches en PowerShell
function Write-Color {
    param(
        [Parameter(Mandatory=$true)][string]$Texto,
        [string]$Color = "White",
        [switch]$NoSalto
    )
    if ($NoSalto) {
        Write-Host $Texto -ForegroundColor $Color -NoNewline
    } else {
        Write-Host $Texto -ForegroundColor $Color
    }
}

function Show-Cabecera {
    Clear-Host
    Write-Host ""
    Write-Color -Texto "  ============================================================" -Color "Cyan"
    Write-Color -Texto "                                                              " -Color "Cyan"
    Write-Color -Texto "         DIAGNOSTICADOR DE SISTEMAS  v1.0" -Color "Blue"
    Write-Color -Texto "               desarrollado por RochyDev" -Color "Blue"
    Write-Color -Texto "                                                              " -Color "Cyan"
    Write-Color -Texto "  ============================================================" -Color "Cyan"
    Write-Host ""

    $esAdmin = if (Test-Administrador) { "[ADMIN]" } else { "[USUARIO]" }
    $colorAdmin = if (Test-Administrador) { "Green" } else { "Yellow" }
    $fecha = Get-Date -Format 'dd/MM/yyyy HH:mm'

    Write-Color -Texto "  Equipo: " -Color "DarkGray" -NoSalto
    Write-Color -Texto "$env:COMPUTERNAME" -Color "White" -NoSalto
    Write-Color -Texto "  |  Usuario: " -Color "DarkGray" -NoSalto
    Write-Color -Texto "$env:USERNAME" -Color "White" -NoSalto
    Write-Color -Texto "  |  " -Color "DarkGray" -NoSalto
    Write-Color -Texto "$esAdmin" -Color $colorAdmin -NoSalto
    Write-Color -Texto "  |  $fecha" -Color "DarkGray"
    Write-Host ""
}

function Show-Separador {
    param([string]$Color = "DarkCyan")
    Write-Color -Texto "  ------------------------------------------------------------" -Color $Color
}

function Pausa {
    Write-Host ""
    Show-Separador
    Write-Color -Texto "  Pulsa cualquier tecla para volver al menu..." -Color "Yellow"
    [void][System.Console]::ReadKey($true)
}

function Show-TituloSeccion {
    param([string]$Titulo)
    Write-Host ""
    Write-Color -Texto "  >> $Titulo" -Color "Cyan"
    Show-Separador
    Write-Host ""
}

function Confirmar {
    param([string]$Mensaje)
    Write-Host ""
    Write-Color -Texto "  $Mensaje (S/N): " -Color "Yellow" -NoSalto
    $resp = Read-Host
    return ($resp -match '^[sSyY]')
}

function Write-Linea {
    param(
        [string]$Etiqueta,
        [string]$Valor,
        [string]$ColorValor = "White"
    )
    Write-Color -Texto $Etiqueta -Color "DarkGray" -NoSalto
    Write-Color -Texto $Valor -Color $ColorValor
}

# ============================================================
#  SECCION: HARDWARE
# ============================================================

function Menu-Hardware {
    while ($true) {
        Show-Cabecera
        Write-Color -Texto "  >> DIAGNOSTICO DE HARDWARE" -Color "Cyan"
        Show-Separador
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto " Informacion general del sistema" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto " CPU (procesador)" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto " Memoria RAM" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto " Discos fisicos y SMART" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto " Tarjeta grafica (GPU)" -Color "White"
        Write-Color -Texto "    [6]" -Color "Yellow" -NoSalto; Write-Color -Texto " Placa base y BIOS/UEFI" -Color "White"
        Write-Color -Texto "    [7]" -Color "Yellow" -NoSalto; Write-Color -Texto " Temperatura (si esta disponible)" -Color "White"
        Write-Color -Texto "    [8]" -Color "Yellow" -NoSalto; Write-Color -Texto " Bateria (portatiles)" -Color "White"
        Write-Color -Texto "    [9]" -Color "Yellow" -NoSalto; Write-Color -Texto " Lanzar diagnostico de memoria de Windows" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [0]" -Color "Red" -NoSalto; Write-Color -Texto " Volver al menu principal" -Color "White"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        switch ($op) {
            "1" { Get-InfoSistema }
            "2" { Get-InfoCPU }
            "3" { Get-InfoRAM }
            "4" { Get-InfoDiscos }
            "5" { Get-InfoGPU }
            "6" { Get-InfoBIOS }
            "7" { Get-InfoTemperatura }
            "8" { Get-InfoBateria }
            "9" { Start-DiagMemoria }
            "0" { return }
            default {
                Write-Color -Texto "  Opcion no valida" -Color "Red"
                Start-Sleep -Seconds 1
            }
        }
    }
}

function Get-InfoSistema {
    Show-Cabecera
    Show-TituloSeccion "INFORMACION GENERAL DEL SISTEMA"
    try {
        $os = Get-CimInstance Win32_OperatingSystem
        $cs = Get-CimInstance Win32_ComputerSystem
        $bios = Get-CimInstance Win32_BIOS

        Write-Linea -Etiqueta "  Sistema Operativo  : " -Valor "$($os.Caption)"
        Write-Linea -Etiqueta "  Version            : " -Valor "$($os.Version) (Build $($os.BuildNumber))"
        Write-Linea -Etiqueta "  Arquitectura       : " -Valor "$($os.OSArchitecture)"
        Write-Linea -Etiqueta "  Fabricante         : " -Valor "$($cs.Manufacturer)"
        Write-Linea -Etiqueta "  Modelo             : " -Valor "$($cs.Model)"
        Write-Linea -Etiqueta "  Numero de serie    : " -Valor "$($bios.SerialNumber)"
        Write-Linea -Etiqueta "  Dominio/Workgroup  : " -Valor "$($cs.Domain)"
        Write-Linea -Etiqueta "  Ultima instalacion : " -Valor "$($os.InstallDate)"
        Write-Linea -Etiqueta "  Ultimo arranque    : " -Valor "$($os.LastBootUpTime)"

        $uptime = (Get-Date) - $os.LastBootUpTime
        $textoUp = [string]$uptime.Days + "d " + [string]$uptime.Hours + "h " + [string]$uptime.Minutes + "m"
        Write-Linea -Etiqueta "  Tiempo encendido   : " -Valor $textoUp -ColorValor "Green"
    } catch {
        Write-Color -Texto "  Error obteniendo datos: $_" -Color "Red"
    }
    Pausa
}

function Get-InfoCPU {
    Show-Cabecera
    Show-TituloSeccion "INFORMACION DE CPU"
    try {
        $cpus = Get-CimInstance Win32_Processor
        foreach ($cpu in $cpus) {
            Write-Linea -Etiqueta "  Modelo             : " -Valor "$($cpu.Name)"
            Write-Linea -Etiqueta "  Fabricante         : " -Valor "$($cpu.Manufacturer)"
            Write-Linea -Etiqueta "  Nucleos fisicos    : " -Valor "$($cpu.NumberOfCores)"
            Write-Linea -Etiqueta "  Nucleos logicos    : " -Valor "$($cpu.NumberOfLogicalProcessors)"
            Write-Linea -Etiqueta "  Velocidad maxima   : " -Valor "$($cpu.MaxClockSpeed) MHz"
            Write-Linea -Etiqueta "  Velocidad actual   : " -Valor "$($cpu.CurrentClockSpeed) MHz"
            Write-Linea -Etiqueta "  Socket             : " -Valor "$($cpu.SocketDesignation)"

            $colorLoad = if ($cpu.LoadPercentage -lt 50) { "Green" } elseif ($cpu.LoadPercentage -lt 80) { "Yellow" } else { "Red" }
            $textoCarga = [string]$cpu.LoadPercentage + "%"
            Write-Linea -Etiqueta "  Carga actual       : " -Valor $textoCarga -ColorValor $colorLoad
            Show-Separador
        }
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Get-InfoRAM {
    Show-Cabecera
    Show-TituloSeccion "INFORMACION DE MEMORIA RAM"
    try {
        $os = Get-CimInstance Win32_OperatingSystem
        $totalGB = [math]::Round($os.TotalVisibleMemorySize / 1MB, 2)
        $libreGB = [math]::Round($os.FreePhysicalMemory / 1MB, 2)
        $usadaGB = [math]::Round($totalGB - $libreGB, 2)
        $porcentaje = [math]::Round(($usadaGB / $totalGB) * 100, 1)

        Write-Linea -Etiqueta "  Total instalada : " -Valor "$totalGB GB"

        $colorRam = if ($porcentaje -lt 60) { "Green" } elseif ($porcentaje -lt 85) { "Yellow" } else { "Red" }
        $textoUso = [string]$usadaGB + " GB (" + [string]$porcentaje + "%)"
        Write-Linea -Etiqueta "  En uso          : " -Valor $textoUso -ColorValor $colorRam
        Write-Linea -Etiqueta "  Libre           : " -Valor "$libreGB GB" -ColorValor "Green"
        Write-Host ""

        Write-Color -Texto "  -- Modulos fisicos instalados --" -Color "DarkCyan"
        Write-Host ""
        $modulos = Get-CimInstance Win32_PhysicalMemory
        $i = 1
        foreach ($m in $modulos) {
            $cap = [math]::Round($m.Capacity / 1GB, 0)
            Write-Color -Texto "  Modulo $i" -Color "Yellow"
            Write-Linea -Etiqueta "    Capacidad   : " -Valor "$cap GB"
            Write-Linea -Etiqueta "    Velocidad   : " -Valor "$($m.Speed) MHz"
            Write-Linea -Etiqueta "    Fabricante  : " -Valor "$($m.Manufacturer)"
            Write-Linea -Etiqueta "    Slot        : " -Valor "$($m.DeviceLocator)"
            Write-Linea -Etiqueta "    Num. serie  : " -Valor "$($m.SerialNumber)"
            Write-Host ""
            $i++
        }
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Get-InfoDiscos {
    Show-Cabecera
    Show-TituloSeccion "DISCOS FISICOS Y ESTADO SMART"
    try {
        $discos = Get-PhysicalDisk
        foreach ($d in $discos) {
            $sizeGB = [math]::Round($d.Size / 1GB, 2)
            Write-Color -Texto "  Disco: $($d.FriendlyName)" -Color "Yellow"
            Write-Linea -Etiqueta "    Modelo       : " -Valor "$($d.Model)"
            Write-Linea -Etiqueta "    Tipo medio   : " -Valor "$($d.MediaType)"
            Write-Linea -Etiqueta "    Tipo bus     : " -Valor "$($d.BusType)"
            Write-Linea -Etiqueta "    Tamano       : " -Valor "$sizeGB GB"

            $colorSalud = switch ($d.HealthStatus) {
                "Healthy" { "Green" }
                "Warning" { "Yellow" }
                default   { "Red" }
            }
            Write-Linea -Etiqueta "    Salud (SMART): " -Valor "$($d.HealthStatus)" -ColorValor $colorSalud
            Write-Linea -Etiqueta "    Estado op.   : " -Valor "$($d.OperationalStatus)"
            Show-Separador
        }
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Get-InfoGPU {
    Show-Cabecera
    Show-TituloSeccion "TARJETAS GRAFICAS (GPU)"
    try {
        $gpus = Get-CimInstance Win32_VideoController
        foreach ($g in $gpus) {
            Write-Color -Texto "  $($g.Name)" -Color "Yellow"
            Write-Linea -Etiqueta "    Procesador  : " -Valor "$($g.VideoProcessor)"
            Write-Linea -Etiqueta "    Driver ver. : " -Valor "$($g.DriverVersion)"
            Write-Linea -Etiqueta "    Driver fecha: " -Valor "$($g.DriverDate)"

            if ($g.AdapterRAM -gt 0) {
                $vramGB = [math]::Round($g.AdapterRAM / 1GB, 2)
                Write-Linea -Etiqueta "    VRAM        : " -Valor "$vramGB GB"
            }

            $resol = [string]$g.CurrentHorizontalResolution + "x" + [string]$g.CurrentVerticalResolution + " @ " + [string]$g.CurrentRefreshRate + "Hz"
            Write-Linea -Etiqueta "    Resolucion  : " -Valor $resol
            Show-Separador
        }
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Get-InfoBIOS {
    Show-Cabecera
    Show-TituloSeccion "PLACA BASE Y BIOS / UEFI"
    try {
        $bios = Get-CimInstance Win32_BIOS
        $bb = Get-CimInstance Win32_BaseBoard

        Write-Color -Texto "  -- BIOS / UEFI --" -Color "DarkCyan"
        Write-Linea -Etiqueta "  Fabricante   : " -Valor "$($bios.Manufacturer)"
        Write-Linea -Etiqueta "  Version      : " -Valor "$($bios.SMBIOSBIOSVersion)"
        Write-Linea -Etiqueta "  Fecha        : " -Valor "$($bios.ReleaseDate)"
        Write-Linea -Etiqueta "  Num. serie   : " -Valor "$($bios.SerialNumber)"
        Write-Host ""
        Write-Color -Texto "  -- Placa base --" -Color "DarkCyan"
        Write-Linea -Etiqueta "  Fabricante   : " -Valor "$($bb.Manufacturer)"
        Write-Linea -Etiqueta "  Producto     : " -Valor "$($bb.Product)"
        Write-Linea -Etiqueta "  Num. serie   : " -Valor "$($bb.SerialNumber)"

        $modoArranque = "Desconocido"
        try {
            $info = Get-ComputerInfo -Property BiosFirmwareType -ErrorAction Stop
            if ($info.BiosFirmwareType -eq "Uefi") { $modoArranque = "UEFI" } else { $modoArranque = "Legacy/BIOS" }
        } catch { }
        Write-Linea -Etiqueta "  Modo arranque: " -Valor $modoArranque -ColorValor "Green"
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Get-InfoTemperatura {
    Show-Cabecera
    Show-TituloSeccion "TEMPERATURA DEL SISTEMA"
    Write-Color -Texto "  Nota: muchos equipos no exponen temperaturas via WMI." -Color "DarkYellow"
    Write-Color -Texto "        Para datos precisos usa HWiNFO64 o OpenHardwareMonitor." -Color "DarkYellow"
    Write-Host ""
    try {
        $temps = Get-CimInstance -Namespace "root\wmi" -ClassName MSAcpi_ThermalZoneTemperature -ErrorAction Stop
        if ($temps) {
            foreach ($t in $temps) {
                $celsius = [math]::Round(($t.CurrentTemperature / 10) - 273.15, 1)
                $colorTemp = if ($celsius -lt 60) { "Green" } elseif ($celsius -lt 80) { "Yellow" } else { "Red" }
                Write-Linea -Etiqueta "  Zona termica: " -Valor "$celsius C" -ColorValor $colorTemp
            }
        } else {
            Write-Color -Texto "  No se han detectado sensores accesibles." -Color "Yellow"
        }
    } catch {
        Write-Color -Texto "  No se puede leer la temperatura en este equipo." -Color "Yellow"
    }
    Pausa
}

function Get-InfoBateria {
    Show-Cabecera
    Show-TituloSeccion "ESTADO DE LA BATERIA"
    try {
        $bat = Get-CimInstance Win32_Battery -ErrorAction Stop
        if (-not $bat) {
            Write-Color -Texto "  Este equipo no tiene bateria (probablemente sobremesa o servidor)." -Color "Yellow"
        } else {
            foreach ($b in $bat) {
                Write-Linea -Etiqueta "  Nombre        : " -Valor "$($b.Name)"

                $colorBat = if ($b.EstimatedChargeRemaining -lt 25) { "Red" } elseif ($b.EstimatedChargeRemaining -lt 60) { "Yellow" } else { "Green" }
                $cargaTxt = [string]$b.EstimatedChargeRemaining + "%"
                Write-Linea -Etiqueta "  Carga actual  : " -Valor $cargaTxt -ColorValor $colorBat

                $estado = switch ($b.BatteryStatus) {
                    1 { "Descargando" }
                    2 { "Conectada a corriente" }
                    3 { "Totalmente cargada" }
                    4 { "Baja" }
                    5 { "Critica" }
                    default { "Estado $($b.BatteryStatus)" }
                }
                Write-Linea -Etiqueta "  Estado        : " -Valor $estado
            }
            Write-Host ""
            if (Confirmar "Generar informe completo de bateria (powercfg /batteryreport)?") {
                $ruta = "$env:USERPROFILE\Desktop\bateria-report.html"
                powercfg /batteryreport /output $ruta | Out-Null
                Write-Color -Texto "  Informe generado en: $ruta" -Color "Green"
            }
        }
    } catch {
        Write-Color -Texto "  No se ha detectado bateria." -Color "Yellow"
    }
    Pausa
}

function Start-DiagMemoria {
    Show-Cabecera
    Show-TituloSeccion "DIAGNOSTICO DE MEMORIA WINDOWS"
    Write-Color -Texto "  Esta herramienta requiere reiniciar el equipo para hacer la prueba." -Color "Yellow"
    Write-Host ""
    if (Confirmar "Lanzar la utilidad mdsched.exe?") {
        Start-Process "mdsched.exe"
        Write-Color -Texto "  Utilidad lanzada. Sigue las instrucciones en pantalla." -Color "Green"
    }
    Pausa
}

# ============================================================
#  SECCION: PARTICIONES Y DISCOS LOGICOS
# ============================================================

function Menu-Particiones {
    while ($true) {
        Show-Cabecera
        Write-Color -Texto "  >> PARTICIONES Y ALMACENAMIENTO" -Color "Cyan"
        Show-Separador
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto " Ver volumenes y espacio libre" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto " Ver particiones por disco" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto " Comprobar errores en disco (chkdsk /scan)" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto " Programar chkdsk /f /r en proximo arranque" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto " Optimizar / desfragmentar unidad" -Color "White"
        Write-Color -Texto "    [6]" -Color "Yellow" -NoSalto; Write-Color -Texto " Limpieza de archivos temporales" -Color "White"
        Write-Color -Texto "    [7]" -Color "Yellow" -NoSalto; Write-Color -Texto " Abrir Administracion de discos (diskmgmt)" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [0]" -Color "Red" -NoSalto; Write-Color -Texto " Volver al menu principal" -Color "White"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        switch ($op) {
            "1" { Get-Volumenes }
            "2" { Get-Particiones }
            "3" { Start-ChkdskScan }
            "4" { Start-ChkdskFull }
            "5" { Start-Optimizacion }
            "6" { Clear-Temporales }
            "7" {
                Start-Process "diskmgmt.msc"
                Write-Color -Texto "  Abriendo administrador de discos..." -Color "Green"
                Start-Sleep -Seconds 1
            }
            "0" { return }
            default {
                Write-Color -Texto "  Opcion no valida" -Color "Red"
                Start-Sleep -Seconds 1
            }
        }
    }
}

function Get-Volumenes {
    Show-Cabecera
    Show-TituloSeccion "VOLUMENES Y ESPACIO LIBRE"
    try {
        $vols = Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3"
        foreach ($v in $vols) {
            $totalGB = [math]::Round($v.Size / 1GB, 2)
            $libreGB = [math]::Round($v.FreeSpace / 1GB, 2)
            $usadoGB = [math]::Round($totalGB - $libreGB, 2)
            $porcentaje = if ($totalGB -gt 0) { [math]::Round(($usadoGB / $totalGB) * 100, 1) } else { 0 }

            Write-Color -Texto "  Unidad $($v.DeviceID) " -Color "Yellow" -NoSalto
            if ($v.VolumeName) {
                Write-Color -Texto "($($v.VolumeName))" -Color "DarkGray"
            } else {
                Write-Host ""
            }

            Write-Linea -Etiqueta "    Sistema archivos: " -Valor "$($v.FileSystem)"
            Write-Linea -Etiqueta "    Total           : " -Valor "$totalGB GB"

            $color = if ($porcentaje -lt 70) { "Green" } elseif ($porcentaje -lt 90) { "Yellow" } else { "Red" }
            $textoUso = [string]$usadaGB + " GB (" + [string]$porcentaje + "%)"
            $textoUso = [string]$usadoGB + " GB (" + [string]$porcentaje + "%)"
            Write-Linea -Etiqueta "    Usado           : " -Valor $textoUso -ColorValor $color
            Write-Linea -Etiqueta "    Libre           : " -Valor "$libreGB GB" -ColorValor "Green"

            # Barra de progreso visual con caracteres ASCII
            $barraLlena = [int]([math]::Round($porcentaje / 5))
            if ($barraLlena -gt 20) { $barraLlena = 20 }
            if ($barraLlena -lt 0)  { $barraLlena = 0 }
            $barraVacia = 20 - $barraLlena

            Write-Color -Texto "    [" -Color "DarkGray" -NoSalto
            if ($barraLlena -gt 0) {
                Write-Color -Texto ("#" * $barraLlena) -Color $color -NoSalto
            }
            if ($barraVacia -gt 0) {
                Write-Color -Texto ("." * $barraVacia) -Color "DarkGray" -NoSalto
            }
            $cierreBarra = "] " + [string]$porcentaje + "%"
            Write-Color -Texto $cierreBarra -Color "DarkGray"
            Show-Separador
        }
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Get-Particiones {
    Show-Cabecera
    Show-TituloSeccion "PARTICIONES POR DISCO"
    try {
        $discos = Get-Disk
        foreach ($d in $discos) {
            Write-Color -Texto "  Disco $($d.Number) - $($d.FriendlyName)" -Color "Yellow"
            Write-Linea -Etiqueta "    Estilo particion: " -Valor "$($d.PartitionStyle)"
            Write-Linea -Etiqueta "    Tamano total    : " -Valor "$([math]::Round($d.Size/1GB,2)) GB"

            $colSt = if ($d.HealthStatus -eq "Healthy") { "Green" } else { "Red" }
            Write-Linea -Etiqueta "    Estado          : " -Valor "$($d.HealthStatus)" -ColorValor $colSt

            $parts = Get-Partition -DiskNumber $d.Number -ErrorAction SilentlyContinue
            foreach ($p in $parts) {
                $letra = if ($p.DriveLetter) { "$($p.DriveLetter): " } else { "(sin letra) " }
                $sizeP = [math]::Round($p.Size/1GB,2)
                Write-Color -Texto "      - Particion $($p.PartitionNumber): " -Color "DarkCyan" -NoSalto
                Write-Color -Texto "$letra" -Color "White" -NoSalto
                Write-Color -Texto "$sizeP GB " -Color "White" -NoSalto
                Write-Color -Texto "[$($p.Type)]" -Color "DarkGray"
            }
            Show-Separador
        }
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Start-ChkdskScan {
    Show-Cabecera
    Show-TituloSeccion "ESCANEO DE DISCO (chkdsk /scan)"
    Write-Color -Texto "  Unidades disponibles:" -Color "DarkCyan"
    $unidades = (Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3").DeviceID
    Write-Color -Texto "  $($unidades -join ', ')" -Color "White"
    Write-Host ""
    Write-Color -Texto "  Indica la unidad a escanear (ej: C:): " -Color "Yellow" -NoSalto
    $u = Read-Host
    if ($u -match '^[A-Za-z]:$') {
        Write-Color -Texto "  Ejecutando chkdsk $u /scan ..." -Color "Green"
        Write-Host ""
        chkdsk $u /scan
    } else {
        Write-Color -Texto "  Formato no valido." -Color "Red"
    }
    Pausa
}

function Start-ChkdskFull {
    Show-Cabecera
    Show-TituloSeccion "PROGRAMAR CHKDSK /F /R"
    Write-Color -Texto "  Esta operacion marcara la unidad para revision completa en el" -Color "Yellow"
    Write-Color -Texto "  proximo arranque (puede tardar varias horas)." -Color "Yellow"
    Write-Host ""
    Write-Color -Texto "  Indica la unidad (ej: C:): " -Color "Yellow" -NoSalto
    $u = Read-Host
    if ($u -match '^[A-Za-z]:$') {
        if (Confirmar "Confirmas programar chkdsk $u /f /r?") {
            chkdsk $u /f /r
        }
    } else {
        Write-Color -Texto "  Formato no valido." -Color "Red"
    }
    Pausa
}

function Start-Optimizacion {
    Show-Cabecera
    Show-TituloSeccion "OPTIMIZACION DE UNIDAD"
    Write-Color -Texto "  Indica la letra (ej: C): " -Color "Yellow" -NoSalto
    $u = Read-Host
    if ($u -match '^[A-Za-z]$') {
        try {
            Write-Color -Texto "  Optimizando unidad $u ..." -Color "Green"
            Optimize-Volume -DriveLetter $u -Verbose
            Write-Color -Texto "  Optimizacion completada." -Color "Green"
        } catch {
            Write-Color -Texto "  Error: $_" -Color "Red"
        }
    }
    Pausa
}

function Clear-Temporales {
    Show-Cabecera
    Show-TituloSeccion "LIMPIEZA DE ARCHIVOS TEMPORALES"
    if (-not (Confirmar "Quieres eliminar los archivos temporales del usuario y del sistema?")) {
        return
    }

    $rutas = @(
        "$env:TEMP\*",
        "$env:WINDIR\Temp\*",
        "$env:LOCALAPPDATA\Temp\*"
    )

    $totalLiberado = 0
    foreach ($r in $rutas) {
        try {
            $size = (Get-ChildItem $r -Recurse -Force -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum
            Remove-Item $r -Recurse -Force -ErrorAction SilentlyContinue
            if ($size) { $totalLiberado += $size }
            Write-Color -Texto "  Limpiado: $r" -Color "Green"
        } catch {
            Write-Color -Texto "  Aviso en $r" -Color "DarkYellow"
        }
    }
    $mb = [math]::Round($totalLiberado / 1MB, 2)
    Write-Host ""
    Write-Color -Texto "  Espacio liberado aproximado: $mb MB" -Color "Cyan"
    Pausa
}

# ============================================================
#  SECCION: SISTEMA Y REPARACION
# ============================================================

function Menu-Sistema {
    while ($true) {
        Show-Cabecera
        Write-Color -Texto "  >> SISTEMA Y REPARACION" -Color "Cyan"
        Show-Separador
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto " SFC /scannow (verificar archivos sistema)" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto " DISM /CheckHealth" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto " DISM /ScanHealth" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto " DISM /RestoreHealth (reparacion completa)" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto " Reparar cache de Windows Update" -Color "White"
        Write-Color -Texto "    [6]" -Color "Yellow" -NoSalto; Write-Color -Texto " Restablecer pila de red (reset Winsock+TCP/IP)" -Color "White"
        Write-Color -Texto "    [7]" -Color "Yellow" -NoSalto; Write-Color -Texto " Vaciar cache DNS" -Color "White"
        Write-Color -Texto "    [8]" -Color "Yellow" -NoSalto; Write-Color -Texto " Comprobar Windows Update" -Color "White"
        Write-Color -Texto "    [9]" -Color "Yellow" -NoSalto; Write-Color -Texto " Lista de actualizaciones instaladas (KB)" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [0]" -Color "Red" -NoSalto; Write-Color -Texto " Volver al menu principal" -Color "White"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        switch ($op) {
            "1" { Run-SFC }
            "2" { Run-DISM "CheckHealth" }
            "3" { Run-DISM "ScanHealth" }
            "4" { Run-DISM "RestoreHealth" }
            "5" { Reset-WindowsUpdate }
            "6" { Reset-Red }
            "7" { Clear-DNS }
            "8" { Check-WindowsUpdate }
            "9" { Get-Hotfixes }
            "0" { return }
            default {
                Write-Color -Texto "  Opcion no valida" -Color "Red"
                Start-Sleep -Seconds 1
            }
        }
    }
}

function Run-SFC {
    Show-Cabecera
    Show-TituloSeccion "SFC /SCANNOW"
    if (-not (Test-Administrador)) {
        Write-Color -Texto "  Esta operacion requiere permisos de administrador." -Color "Red"
        Pausa; return
    }
    Write-Color -Texto "  Lanzando comprobacion de archivos del sistema..." -Color "Green"
    Write-Host ""
    sfc /scannow
    Pausa
}

function Run-DISM {
    param([string]$Modo)
    Show-Cabecera
    Show-TituloSeccion "DISM /$Modo"
    if (-not (Test-Administrador)) {
        Write-Color -Texto "  Esta operacion requiere permisos de administrador." -Color "Red"
        Pausa; return
    }
    Write-Color -Texto "  Ejecutando DISM /Online /Cleanup-Image /$Modo ..." -Color "Green"
    Write-Host ""
    DISM /Online /Cleanup-Image /$Modo
    Pausa
}

function Reset-WindowsUpdate {
    Show-Cabecera
    Show-TituloSeccion "REPARAR CACHE DE WINDOWS UPDATE"
    if (-not (Test-Administrador)) {
        Write-Color -Texto "  Necesita permisos de administrador." -Color "Red"
        Pausa; return
    }
    if (-not (Confirmar "Esto detendra los servicios de WU y limpiara la cache. Continuar?")) {
        return
    }

    Write-Color -Texto "  Deteniendo servicios..." -Color "Yellow"
    Stop-Service -Name wuauserv,bits,cryptsvc -Force -ErrorAction SilentlyContinue

    Write-Color -Texto "  Renombrando carpetas..." -Color "Yellow"
    Rename-Item "$env:WINDIR\SoftwareDistribution" "SoftwareDistribution.old" -ErrorAction SilentlyContinue
    Rename-Item "$env:WINDIR\System32\catroot2" "catroot2.old" -ErrorAction SilentlyContinue

    Write-Color -Texto "  Reiniciando servicios..." -Color "Yellow"
    Start-Service -Name wuauserv,bits,cryptsvc -ErrorAction SilentlyContinue

    Write-Color -Texto "  Cache de Windows Update reparada." -Color "Green"
    Pausa
}

function Reset-Red {
    Show-Cabecera
    Show-TituloSeccion "RESET DE PILA DE RED"
    if (-not (Test-Administrador)) {
        Write-Color -Texto "  Necesita permisos de administrador." -Color "Red"
        Pausa; return
    }
    if (-not (Confirmar "Esto reiniciara Winsock, IP y DNS. Recomendable reiniciar despues. Continuar?")) {
        return
    }
    netsh winsock reset
    netsh int ip reset
    ipconfig /flushdns
    ipconfig /registerdns
    Write-Host ""
    Write-Color -Texto "  Reset completado. Reinicia el equipo para aplicar los cambios." -Color "Green"
    Pausa
}

function Clear-DNS {
    Show-Cabecera
    Show-TituloSeccion "VACIAR CACHE DNS"
    ipconfig /flushdns
    Write-Color -Texto "  Cache DNS vaciada." -Color "Green"
    Pausa
}

function Check-WindowsUpdate {
    Show-Cabecera
    Show-TituloSeccion "BUSCAR ACTUALIZACIONES"
    Write-Color -Texto "  Abriendo Windows Update..." -Color "Green"
    Start-Process "ms-settings:windowsupdate"
    Start-Sleep -Seconds 1
}

function Get-Hotfixes {
    Show-Cabecera
    Show-TituloSeccion "ACTUALIZACIONES INSTALADAS"
    try {
        $hf = Get-HotFix | Sort-Object InstalledOn -Descending | Select-Object -First 25
        $hf | Format-Table HotFixID, Description, InstalledOn -AutoSize | Out-Host
        Write-Color -Texto "  Mostrando las ultimas 25 actualizaciones." -Color "DarkGray"
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

# ============================================================
#  SECCION: RED
# ============================================================

function Menu-Red {
    while ($true) {
        Show-Cabecera
        Write-Color -Texto "  >> DIAGNOSTICO DE RED" -Color "Cyan"
        Show-Separador
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto " Configuracion IP (ipconfig /all)" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto " Adaptadores de red" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto " Ping a un host" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto " Tracert a un host" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto " Conexiones activas (netstat)" -Color "White"
        Write-Color -Texto "    [6]" -Color "Yellow" -NoSalto; Write-Color -Texto " Test puerto TCP" -Color "White"
        Write-Color -Texto "    [7]" -Color "Yellow" -NoSalto; Write-Color -Texto " Mostrar IP publica" -Color "White"
        Write-Color -Texto "    [8]" -Color "Yellow" -NoSalto; Write-Color -Texto " Resolver DNS (nslookup)" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [0]" -Color "Red" -NoSalto; Write-Color -Texto " Volver al menu principal" -Color "White"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        switch ($op) {
            "1" {
                Show-Cabecera
                Show-TituloSeccion "ipconfig /all"
                ipconfig /all
                Pausa
            }
            "2" { Get-Adaptadores }
            "3" { Test-Ping }
            "4" { Test-Tracert }
            "5" {
                Show-Cabecera
                Show-TituloSeccion "Conexiones activas"
                netstat -ano | Select-Object -First 40
                Pausa
            }
            "6" { Test-Puerto }
            "7" { Get-IPPublica }
            "8" { Test-NSLookup }
            "0" { return }
            default {
                Write-Color -Texto "  Opcion no valida" -Color "Red"
                Start-Sleep -Seconds 1
            }
        }
    }
}

function Get-Adaptadores {
    Show-Cabecera
    Show-TituloSeccion "ADAPTADORES DE RED"
    try {
        $ads = Get-NetAdapter | Where-Object { $_.Status -ne "Disabled" }
        foreach ($a in $ads) {
            $colorEstado = if ($a.Status -eq "Up") { "Green" } else { "Yellow" }
            Write-Color -Texto "  $($a.Name) " -Color "Yellow" -NoSalto
            Write-Color -Texto "[$($a.Status)]" -Color $colorEstado
            Write-Linea -Etiqueta "    Descripcion : " -Valor "$($a.InterfaceDescription)"
            Write-Linea -Etiqueta "    MAC         : " -Valor "$($a.MacAddress)"
            Write-Linea -Etiqueta "    Velocidad   : " -Valor "$($a.LinkSpeed)"

            $ips = Get-NetIPAddress -InterfaceIndex $a.ifIndex -ErrorAction SilentlyContinue |
                   Where-Object { $_.AddressFamily -eq "IPv4" }
            foreach ($ip in $ips) {
                Write-Linea -Etiqueta "    IPv4        : " -Valor "$($ip.IPAddress)/$($ip.PrefixLength)" -ColorValor "Cyan"
            }
            Show-Separador
        }
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Test-Ping {
    Show-Cabecera
    Show-TituloSeccion "PING"
    Write-Color -Texto "  Indica el host (ej: 8.8.8.8 o google.com): " -Color "Yellow" -NoSalto
    $h = Read-Host
    if ($h) {
        ping $h
    }
    Pausa
}

function Test-Tracert {
    Show-Cabecera
    Show-TituloSeccion "TRACERT"
    Write-Color -Texto "  Indica el host: " -Color "Yellow" -NoSalto
    $h = Read-Host
    if ($h) {
        tracert $h
    }
    Pausa
}

function Test-Puerto {
    Show-Cabecera
    Show-TituloSeccion "TEST DE PUERTO TCP"
    Write-Color -Texto "  Host: " -Color "Yellow" -NoSalto
    $h = Read-Host
    Write-Color -Texto "  Puerto: " -Color "Yellow" -NoSalto
    $p = Read-Host
    if ($h -and $p) {
        try {
            $r = Test-NetConnection -ComputerName $h -Port $p -WarningAction SilentlyContinue
            if ($r.TcpTestSucceeded) {
                Write-Color -Texto "  [OK] Puerto $p ABIERTO en $h" -Color "Green"
            } else {
                Write-Color -Texto "  [FAIL] Puerto $p CERRADO/INACCESIBLE en $h" -Color "Red"
            }
            Write-Color -Texto "  Latencia: $($r.PingReplyDetails.RoundtripTime) ms" -Color "DarkGray"
        } catch {
            Write-Color -Texto "  Error: $_" -Color "Red"
        }
    }
    Pausa
}

function Get-IPPublica {
    Show-Cabecera
    Show-TituloSeccion "IP PUBLICA"
    try {
        $ip = (Invoke-RestMethod -Uri "https://api.ipify.org?format=json" -TimeoutSec 5).ip
        Write-Color -Texto "  Tu IP publica es: " -Color "DarkGray" -NoSalto
        Write-Color -Texto "$ip" -Color "Cyan"
    } catch {
        Write-Color -Texto "  No se pudo obtener la IP publica (sin conexion?)" -Color "Red"
    }
    Pausa
}

function Test-NSLookup {
    Show-Cabecera
    Show-TituloSeccion "NSLOOKUP"
    Write-Color -Texto "  Dominio: " -Color "Yellow" -NoSalto
    $d = Read-Host
    if ($d) {
        nslookup $d
    }
    Pausa
}

# ============================================================
#  SECCION: SEGURIDAD Y ESCANEO
# ============================================================

function Menu-Seguridad {
    while ($true) {
        Show-Cabecera
        Write-Color -Texto "  >> SEGURIDAD Y ESCANEO" -Color "Cyan"
        Show-Separador
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto " Estado de Windows Defender" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto " Actualizar firmas de Defender" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto " Escaneo rapido con Defender" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto " Escaneo completo con Defender" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto " Estado del firewall" -Color "White"
        Write-Color -Texto "    [6]" -Color "Yellow" -NoSalto; Write-Color -Texto " Listar usuarios locales" -Color "White"
        Write-Color -Texto "    [7]" -Color "Yellow" -NoSalto; Write-Color -Texto " Listar miembros del grupo Administradores" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [0]" -Color "Red" -NoSalto; Write-Color -Texto " Volver al menu principal" -Color "White"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        switch ($op) {
            "1" { Get-DefenderStatus }
            "2" {
                Show-Cabecera
                Show-TituloSeccion "Actualizando firmas..."
                try { Update-MpSignature; Write-Color -Texto "  Firmas actualizadas." -Color "Green" }
                catch { Write-Color -Texto "  Error: $_" -Color "Red" }
                Pausa
            }
            "3" {
                Show-Cabecera
                Show-TituloSeccion "Escaneo rapido"
                try { Start-MpScan -ScanType QuickScan; Write-Color -Texto "  Escaneo finalizado." -Color "Green" }
                catch { Write-Color -Texto "  Error: $_" -Color "Red" }
                Pausa
            }
            "4" {
                Show-Cabecera
                Show-TituloSeccion "Escaneo completo"
                if (Confirmar "El escaneo completo puede tardar horas. Continuar?") {
                    try { Start-MpScan -ScanType FullScan; Write-Color -Texto "  Escaneo finalizado." -Color "Green" }
                    catch { Write-Color -Texto "  Error: $_" -Color "Red" }
                }
                Pausa
            }
            "5" { Get-FirewallStatus }
            "6" { Get-UsuariosLocales }
            "7" { Get-AdminsGrupo }
            "0" { return }
            default {
                Write-Color -Texto "  Opcion no valida" -Color "Red"
                Start-Sleep -Seconds 1
            }
        }
    }
}

function Get-DefenderStatus {
    Show-Cabecera
    Show-TituloSeccion "ESTADO DE WINDOWS DEFENDER"
    try {
        $d = Get-MpComputerStatus
        $colAv = if ($d.AntivirusEnabled) { "Green" } else { "Red" }
        $colRt = if ($d.RealTimeProtectionEnabled) { "Green" } else { "Red" }

        Write-Linea -Etiqueta "  Antivirus habilitado    : " -Valor "$($d.AntivirusEnabled)" -ColorValor $colAv
        Write-Linea -Etiqueta "  Proteccion tiempo real  : " -Valor "$($d.RealTimeProtectionEnabled)" -ColorValor $colRt
        Write-Linea -Etiqueta "  Version motor           : " -Valor "$($d.AMEngineVersion)"
        Write-Linea -Etiqueta "  Version definiciones    : " -Valor "$($d.AntivirusSignatureVersion)"
        Write-Linea -Etiqueta "  Ultima actualizacion    : " -Valor "$($d.AntivirusSignatureLastUpdated)"
        Write-Linea -Etiqueta "  Ultimo escaneo rapido   : " -Valor "$($d.QuickScanEndTime)"
        Write-Linea -Etiqueta "  Ultimo escaneo completo : " -Valor "$($d.FullScanEndTime)"
    } catch {
        Write-Color -Texto "  No se puede consultar Defender (deshabilitado o usando otro AV?)" -Color "Yellow"
    }
    Pausa
}

function Get-FirewallStatus {
    Show-Cabecera
    Show-TituloSeccion "ESTADO DEL FIREWALL"
    try {
        $perfiles = Get-NetFirewallProfile
        foreach ($p in $perfiles) {
            $col = if ($p.Enabled) { "Green" } else { "Red" }
            Write-Linea -Etiqueta "  Perfil $($p.Name) : " -Valor "$($p.Enabled)" -ColorValor $col
        }
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Get-UsuariosLocales {
    Show-Cabecera
    Show-TituloSeccion "USUARIOS LOCALES"
    try {
        Get-LocalUser | Format-Table Name, Enabled, LastLogon, Description -AutoSize | Out-Host
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

function Get-AdminsGrupo {
    Show-Cabecera
    Show-TituloSeccion "GRUPO ADMINISTRADORES"
    try {
        $grupo = (Get-LocalGroup | Where-Object { $_.SID -like "*-544" }).Name
        Get-LocalGroupMember -Group $grupo | Format-Table Name, ObjectClass, PrincipalSource -AutoSize | Out-Host
    } catch {
        Write-Color -Texto "  Error: $_" -Color "Red"
    }
    Pausa
}

# ============================================================
#  SECCION: PROCESOS Y SERVICIOS
# ============================================================

function Menu-Procesos {
    while ($true) {
        Show-Cabecera
        Write-Color -Texto "  >> PROCESOS Y SERVICIOS" -Color "Cyan"
        Show-Separador
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto " Top 15 procesos por CPU" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto " Top 15 procesos por RAM" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto " Buscar proceso por nombre" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto " Matar proceso por nombre" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto " Servicios en ejecucion" -Color "White"
        Write-Color -Texto "    [6]" -Color "Yellow" -NoSalto; Write-Color -Texto " Servicios detenidos (con inicio automatico)" -Color "White"
        Write-Color -Texto "    [7]" -Color "Yellow" -NoSalto; Write-Color -Texto " Programas de inicio (autorun)" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [0]" -Color "Red" -NoSalto; Write-Color -Texto " Volver al menu principal" -Color "White"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        switch ($op) {
            "1" {
                Show-Cabecera
                Show-TituloSeccion "TOP 15 procesos por CPU"
                Get-Process | Sort-Object CPU -Descending | Select-Object -First 15 ProcessName, Id, CPU, @{N="RAM(MB)";E={[math]::Round($_.WorkingSet64/1MB,2)}} | Format-Table -AutoSize | Out-Host
                Pausa
            }
            "2" {
                Show-Cabecera
                Show-TituloSeccion "TOP 15 procesos por RAM"
                Get-Process | Sort-Object WorkingSet64 -Descending | Select-Object -First 15 ProcessName, Id, @{N="RAM(MB)";E={[math]::Round($_.WorkingSet64/1MB,2)}}, CPU | Format-Table -AutoSize | Out-Host
                Pausa
            }
            "3" {
                Show-Cabecera
                Show-TituloSeccion "BUSCAR PROCESO"
                Write-Color -Texto "  Nombre (sin .exe): " -Color "Yellow" -NoSalto
                $n = Read-Host
                Get-Process -Name "*$n*" -ErrorAction SilentlyContinue | Format-Table ProcessName, Id, @{N="RAM(MB)";E={[math]::Round($_.WorkingSet64/1MB,2)}} -AutoSize | Out-Host
                Pausa
            }
            "4" {
                Show-Cabecera
                Show-TituloSeccion "MATAR PROCESO"
                Write-Color -Texto "  Nombre del proceso: " -Color "Yellow" -NoSalto
                $n = Read-Host
                if ($n -and (Confirmar "Seguro que quieres matar TODOS los procesos llamados '$n'?")) {
                    try {
                        Stop-Process -Name $n -Force -ErrorAction Stop
                        Write-Color -Texto "  Proceso(s) terminado(s)." -Color "Green"
                    } catch {
                        Write-Color -Texto "  Error: $_" -Color "Red"
                    }
                }
                Pausa
            }
            "5" {
                Show-Cabecera
                Show-TituloSeccion "SERVICIOS EN EJECUCION"
                Get-Service | Where-Object { $_.Status -eq "Running" } | Format-Table Name, DisplayName -AutoSize | Out-Host
                Pausa
            }
            "6" {
                Show-Cabecera
                Show-TituloSeccion "SERVICIOS AUTOMATICOS DETENIDOS"
                Get-CimInstance Win32_Service | Where-Object { $_.StartMode -eq "Auto" -and $_.State -ne "Running" } |
                    Format-Table Name, DisplayName, State -AutoSize | Out-Host
                Pausa
            }
            "7" {
                Show-Cabecera
                Show-TituloSeccion "PROGRAMAS DE INICIO"
                try {
                    Get-CimInstance Win32_StartupCommand | Format-Table Name, Command, Location, User -AutoSize -Wrap | Out-Host
                } catch {
                    Write-Color -Texto "  Error: $_" -Color "Red"
                }
                Pausa
            }
            "0" { return }
            default {
                Write-Color -Texto "  Opcion no valida" -Color "Red"
                Start-Sleep -Seconds 1
            }
        }
    }
}

# ============================================================
#  SECCION: INFORMES
# ============================================================

function Menu-Informes {
    while ($true) {
        Show-Cabecera
        Write-Color -Texto "  >> INFORMES Y EXPORTACION" -Color "Cyan"
        Show-Separador
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto " Generar informe completo del sistema (.txt)" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto " Informe de bateria (powercfg)" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto " Informe de eficiencia energetica" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto " Eventos criticos (ultimas 24h)" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto " Abrir Visor de Eventos" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [0]" -Color "Red" -NoSalto; Write-Color -Texto " Volver al menu principal" -Color "White"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        switch ($op) {
            "1" { New-InformeCompleto }
            "2" {
                $r = "$env:USERPROFILE\Desktop\bateria-report.html"
                powercfg /batteryreport /output $r | Out-Null
                Write-Color -Texto "  Informe en: $r" -Color "Green"
                Pausa
            }
            "3" {
                if (-not (Test-Administrador)) {
                    Write-Color -Texto "  Requiere admin." -Color "Red"
                    Pausa
                } else {
                    $r = "$env:USERPROFILE\Desktop\energy-report.html"
                    powercfg /energy /output $r /duration 30
                    Write-Color -Texto "  Informe en: $r" -Color "Green"
                    Pausa
                }
            }
            "4" { Get-EventosCriticos }
            "5" {
                Start-Process "eventvwr.msc"
                Start-Sleep -Seconds 1
            }
            "0" { return }
            default {
                Write-Color -Texto "  Opcion no valida" -Color "Red"
                Start-Sleep -Seconds 1
            }
        }
    }
}

function New-InformeCompleto {
    Show-Cabecera
    Show-TituloSeccion "GENERANDO INFORME COMPLETO"
    $fecha = Get-Date -Format 'yyyyMMdd_HHmm'
    $ruta = "$env:USERPROFILE\Desktop\Informe_$($env:COMPUTERNAME)_$fecha.txt"
    Write-Color -Texto "  Generando..." -Color "Yellow"

    $contenido = @()
    $contenido += "=========================================================="
    $contenido += "   INFORME DE DIAGNOSTICO - $(Get-Date)"
    $contenido += "   Generado por Diagnosticador (RochyDev)"
    $contenido += "=========================================================="
    $contenido += ""
    $contenido += "[ SISTEMA OPERATIVO ]"
    $contenido += (Get-CimInstance Win32_OperatingSystem | Format-List Caption, Version, OSArchitecture, InstallDate, LastBootUpTime | Out-String)
    $contenido += "[ EQUIPO ]"
    $contenido += (Get-CimInstance Win32_ComputerSystem | Format-List Manufacturer, Model, Domain, TotalPhysicalMemory | Out-String)
    $contenido += "[ BIOS ]"
    $contenido += (Get-CimInstance Win32_BIOS | Format-List Manufacturer, SMBIOSBIOSVersion, ReleaseDate, SerialNumber | Out-String)
    $contenido += "[ CPU ]"
    $contenido += (Get-CimInstance Win32_Processor | Format-List Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed | Out-String)
    $contenido += "[ RAM ]"
    $contenido += (Get-CimInstance Win32_PhysicalMemory | Format-Table @{N="GB";E={[math]::Round($_.Capacity/1GB,0)}}, Speed, Manufacturer, DeviceLocator -AutoSize | Out-String)
    $contenido += "[ DISCOS ]"
    $contenido += (Get-PhysicalDisk | Format-Table FriendlyName, MediaType, BusType, @{N="GB";E={[math]::Round($_.Size/1GB,2)}}, HealthStatus -AutoSize | Out-String)
    $contenido += "[ VOLUMENES ]"
    $contenido += (Get-CimInstance Win32_LogicalDisk -Filter "DriveType=3" | Format-Table DeviceID, VolumeName, FileSystem, @{N="GBTotal";E={[math]::Round($_.Size/1GB,2)}}, @{N="GBLibres";E={[math]::Round($_.FreeSpace/1GB,2)}} -AutoSize | Out-String)
    $contenido += "[ RED ]"
    $contenido += (ipconfig /all | Out-String)
    $contenido += "[ ULTIMAS 25 ACTUALIZACIONES ]"
    $contenido += (Get-HotFix | Sort-Object InstalledOn -Descending | Select-Object -First 25 | Format-Table HotFixID, InstalledOn, Description -AutoSize | Out-String)

    $contenido | Out-File -FilePath $ruta -Encoding UTF8
    Write-Color -Texto "  Informe guardado en: $ruta" -Color "Green"
    Pausa
}

function Get-EventosCriticos {
    Show-Cabecera
    Show-TituloSeccion "EVENTOS CRITICOS Y ERRORES (24H)"
    try {
        $desde = (Get-Date).AddHours(-24)
        $eventos = Get-WinEvent -FilterHashtable @{LogName='System','Application'; Level=1,2; StartTime=$desde} -ErrorAction Stop | Select-Object -First 30
        if ($eventos.Count -eq 0) {
            Write-Color -Texto "  No hay eventos criticos en las ultimas 24h." -Color "Green"
        } else {
            $eventos | Format-Table TimeCreated, LevelDisplayName, ProviderName, Id, Message -AutoSize -Wrap | Out-Host
        }
    } catch {
        Write-Color -Texto "  Error o sin eventos: $_" -Color "Yellow"
    }
    Pausa
}

# ============================================================
#  SECCION: HERRAMIENTAS DE WINDOWS
# ============================================================

function Menu-Herramientas {
    while ($true) {
        Show-Cabecera
        Write-Color -Texto "  >> HERRAMIENTAS DEL SISTEMA" -Color "Cyan"
        Show-Separador
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto " Administrador de tareas (taskmgr)" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto " Administrador de dispositivos (devmgmt)" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto " Servicios (services.msc)" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto " Informacion del sistema (msinfo32)" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto " Editor del registro (regedit)" -Color "White"
        Write-Color -Texto "    [6]" -Color "Yellow" -NoSalto; Write-Color -Texto " Configuracion del sistema (msconfig)" -Color "White"
        Write-Color -Texto "    [7]" -Color "Yellow" -NoSalto; Write-Color -Texto " Programador de tareas (taskschd)" -Color "White"
        Write-Color -Texto "    [8]" -Color "Yellow" -NoSalto; Write-Color -Texto " GPEDIT (directivas de grupo locales)" -Color "White"
        Write-Color -Texto "    [9]" -Color "Yellow" -NoSalto; Write-Color -Texto " Monitor de recursos (resmon)" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [0]" -Color "Red" -NoSalto; Write-Color -Texto " Volver al menu principal" -Color "White"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        $tools = @{
            "1"="taskmgr"; "2"="devmgmt.msc"; "3"="services.msc"; "4"="msinfo32";
            "5"="regedit"; "6"="msconfig"; "7"="taskschd.msc"; "8"="gpedit.msc"; "9"="resmon"
        }

        if ($op -eq "0") { return }
        if ($tools.ContainsKey($op)) {
            try {
                Start-Process $tools[$op]
                Write-Color -Texto "  Lanzando $($tools[$op])..." -Color "Green"
                Start-Sleep -Seconds 1
            } catch {
                Write-Color -Texto "  No se pudo abrir: $_" -Color "Red"
                Start-Sleep -Seconds 2
            }
        } else {
            Write-Color -Texto "  Opcion no valida" -Color "Red"
            Start-Sleep -Seconds 1
        }
    }
}

# ============================================================
#  MENU PRINCIPAL
# ============================================================

function Menu-Principal {
    while ($true) {
        Show-Cabecera

        Write-Color -Texto "  +-- MENU PRINCIPAL --------------------------------------------+" -Color "DarkCyan"
        Write-Host ""
        Write-Color -Texto "    [1]" -Color "Yellow" -NoSalto; Write-Color -Texto "  Diagnostico de Hardware" -Color "White"
        Write-Color -Texto "    [2]" -Color "Yellow" -NoSalto; Write-Color -Texto "  Particiones y Almacenamiento" -Color "White"
        Write-Color -Texto "    [3]" -Color "Yellow" -NoSalto; Write-Color -Texto "  Sistema y Reparacion (SFC, DISM, etc.)" -Color "White"
        Write-Color -Texto "    [4]" -Color "Yellow" -NoSalto; Write-Color -Texto "  Diagnostico de Red" -Color "White"
        Write-Color -Texto "    [5]" -Color "Yellow" -NoSalto; Write-Color -Texto "  Seguridad y Escaneo" -Color "White"
        Write-Color -Texto "    [6]" -Color "Yellow" -NoSalto; Write-Color -Texto "  Procesos y Servicios" -Color "White"
        Write-Color -Texto "    [7]" -Color "Yellow" -NoSalto; Write-Color -Texto "  Informes y Exportacion" -Color "White"
        Write-Color -Texto "    [8]" -Color "Yellow" -NoSalto; Write-Color -Texto "  Herramientas del Sistema (taskmgr, etc.)" -Color "White"
        Write-Host ""
        Write-Color -Texto "    [R]" -Color "Magenta" -NoSalto; Write-Color -Texto "  Reiniciar equipo" -Color "White"
        Write-Color -Texto "    [A]" -Color "Magenta" -NoSalto; Write-Color -Texto "  Apagar equipo" -Color "White"
        Write-Color -Texto "    [Q]" -Color "Red"     -NoSalto; Write-Color -Texto "  Salir" -Color "White"
        Write-Host ""
        Write-Color -Texto "  +--------------------------------------------------------------+" -Color "DarkCyan"
        Write-Host ""
        Show-Separador
        Write-Color -Texto "  Selecciona una opcion: " -Color "Cyan" -NoSalto
        $op = Read-Host

        switch ($op.ToUpper()) {
            "1" { Menu-Hardware }
            "2" { Menu-Particiones }
            "3" { Menu-Sistema }
            "4" { Menu-Red }
            "5" { Menu-Seguridad }
            "6" { Menu-Procesos }
            "7" { Menu-Informes }
            "8" { Menu-Herramientas }
            "R" {
                if (Confirmar "Seguro que quieres REINICIAR el equipo?") {
                    Restart-Computer -Force
                }
            }
            "A" {
                if (Confirmar "Seguro que quieres APAGAR el equipo?") {
                    Stop-Computer -Force
                }
            }
            "Q" {
                Show-Cabecera
                Write-Host ""
                Write-Color -Texto "  Hasta luego, RochyDev!" -Color "Blue"
                Write-Host ""
                Start-Sleep -Seconds 1
                exit
            }
            default {
                Write-Color -Texto "  Opcion no valida" -Color "Red"
                Start-Sleep -Seconds 1
            }
        }
    }
}

# ============================================================
#  ARRANQUE
# ============================================================
if (-not (Test-Administrador)) {
    Show-Cabecera
    Write-Color -Texto "  AVISO: NO se esta ejecutando como administrador." -Color "Yellow"
    Write-Color -Texto "  Algunas funciones (SFC, DISM, reset de red, reparaciones)" -Color "DarkYellow"
    Write-Color -Texto "  pueden no funcionar correctamente." -Color "DarkYellow"
    Write-Host ""
    Write-Color -Texto "  Recomendacion: cierra y abre PowerShell como Administrador." -Color "DarkYellow"
    Write-Host ""
    Pausa
}

Menu-Principal
