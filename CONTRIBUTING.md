# Contribuir a DiagCore

¡Gracias por interesarte por DiagCore! Este proyecto está abierto a
contribuciones de cualquier tamaño: un typo, un bug, una mejora
visual, o un módulo completo del roadmap.

---

## 🎯 Antes de empezar

DiagCore es un proyecto **personal de aprendizaje** que se libera
como open source. Eso significa dos cosas:

1. La estética y la arquitectura están **decididas y documentadas**
   en [`PLAN.md`](PLAN.md). Léelo antes de proponer cambios grandes.
2. Las decisiones de stack ya están cerradas (.NET 10, WPF + WPF-UI,
   MVVM con CommunityToolkit, etc.). Si quieres proponer cambiar
   alguna, **abre primero una issue** antes de invertir tiempo.

---

## 🐛 Reportar un bug

1. Verifica que no exista ya en
   [Issues](https://github.com/RochyDev/DiagCore/issues).
2. Abre una issue con:
   - Versión de DiagCore (puedes leerla en `Configuración → Sobre el producto`).
   - Versión de Windows.
   - Pasos para reproducirlo.
   - Comportamiento esperado vs observado.
   - Captura si aplica.
   - El log relevante de `%LOCALAPPDATA%\DiagCore\logs\diagcore-YYYY-MM-DD.log`
     (revísalo antes de pegarlo — solo lleva mensajes técnicos, sin
     datos personales).

---

## 💡 Proponer una mejora

Para cualquier cambio que vaya más allá de un typo o un bug
trivial, **abre primero una issue** describiendo:

- Qué problema resuelve.
- Cómo encaja en el [roadmap](README.md#-roadmap) actual.
- Bocetos o pantallazos si es visual.

Esto evita que dediques tiempo a una PR que no encaja con la
dirección del proyecto.

---

## 🔧 Setup de desarrollo

Mira el [README](README.md#-requisitos) para los requisitos y el
flujo de build. Resumen:

```powershell
git clone https://github.com/RochyDev/DiagCore.git
cd DiagCore
dotnet restore DiagCore.slnx
dotnet build DiagCore.slnx
dotnet test
dotnet run --project src/DiagCore.App
```

> **Tip**: trabaja desde una rama de feature, no desde `main`:
>
> ```powershell
> git switch -c feature/mi-cambio
> ```

---

## 📐 Convenciones de código

- **Naming** estándar .NET (`PascalCase` para públicos,
  `_camelCase` para campos privados).
- **`Nullable<T>` habilitado** en toda la solución. No marques `!.`
  porque sí; razona el contexto.
- **`TreatWarningsAsErrors = true`** está activado en
  `Directory.Build.props`. Un warning rompe el build.
- **Servicios de diagnóstico** devuelven `DiagnosticResult<T>`,
  nunca lanzan al caller.
- **WMI / I/O lento** siempre dentro de `Task.Run`, con
  `CancellationToken` en la firma.
- **Strings de UI** van a `Resources/Strings.resx`. No
  hardcoded en XAML ni en code-behind.
- **Layout en cards** con `CardStyle`, etiquetas con
  `KeyValueRow`, big numbers con `StatTile`.
- **XAML**: una propiedad por línea cuando hay más de tres, atributos
  ordenados alfabéticamente excepto los obvios (`Grid.Row`,
  `Grid.Column`, etc.).

---

## ✅ Tests

Si tu cambio toca lógica de `DiagCore.Core` (parsers, mappers,
helpers puros), **añade tests**. El proyecto está en
`src/DiagCore.Tests/`.

Ejecuta el conjunto completo antes de abrir la PR:

```powershell
dotnet test
```

Los 80 tests actuales deben seguir verdes.

---

## 📝 Commits

- **Mensajes en inglés**, formato convencional:
  - `feat:` nueva feature
  - `fix:` corrección de bug
  - `chore:` cambios de infraestructura
  - `docs:` documentación
  - `test:` cambios o adiciones de tests
  - `refactor:` refactor sin cambio de comportamiento
- **Mensajes pequeños y descriptivos.** Un commit por cambio
  lógico. Si necesitas más de una frase en el resumen, el cambio
  probablemente sea varios commits.

Ejemplo:

```
feat(network): add cancellable traceroute via Ping with TTL

Mirrors Test-NetConnection -TraceRoute from the legacy PowerShell
script. Uses System.Net.NetworkInformation.Ping with explicit TTL
instead of shelling out to tracert.exe so the hop list comes back
typed and the CancellationToken actually cancels mid-trace.
```

---

## 🔀 Pull request

1. Abre la PR contra `main`.
2. En el cuerpo:
   - Qué hace.
   - Qué issue cierra (`Closes #N`).
   - Cómo probarlo (`dotnet test`, pasos manuales, screenshots).
3. Espera CI verde antes de pedir revisión.

---

## 🛡️ Privacidad — invariantes del producto

Cualquier cambio debe respetar las **tres reglas inviolables** de
DiagCore:

1. **Cero telemetría.** El binario no envía nada al autor ni a
   terceros.
2. **Cero datos del usuario fuera del equipo.** Informes,
   históricos, logs — todo local.
3. **Llamadas salientes**: solo `api.ipify.org`, solo cuando el
   usuario lo pida explícitamente.

Una PR que introduzca telemetría no se aceptará.

---

## 📜 Licencia

Al contribuir aceptas que tu código se libera bajo la misma
licencia [MIT](LICENSE) del proyecto.

¡Gracias! 🙌
