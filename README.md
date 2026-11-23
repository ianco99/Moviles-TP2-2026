# GunJump

Trabajo práctico 2 de Desarrollo de Videojuegos para Móviles (2026). Juego para Android hecho en Unity 6000.3.11f1 (URP, IL2CPP).
Package: `com.Kuznicki2026.GunJump`.

| Entregable | Link |
| --- | --- |
| Página del juego en Google Play (beta cerrada) | _(completar)_ |
| APK | _(completar)_ |
| Documento de Game Design | `Docs/GDD_GunJump.docx` |

## El juego

El jugador no maneja un personaje: maneja un arma. Cada toque en la pantalla dispara, y el retroceso empuja el arma en sentido contrario al cañón y la hace girar. Disparar hacia atrás la hace avanzar, disparar hacia abajo la eleva. El objetivo es llegar a la meta de cada nivel. Se suman puntos eliminando enemigos, juntando monedas y acertando al multiplicador del final (x1 a x5).

- **5 niveles** de dificultad creciente. El nivel 1 es el tutorial (cámara lenta hasta el primer disparo y carteles de ayuda). Después aparecen huecos, altura, plataformas móviles, sierras, pinchos y munición limitada.
- **Una partida completa dura de 3 a 5 minutos.**
- **Monetización interna**: las monedas de cada nivel se gastan en la tienda en armas (cada una dispara distinto), skins, balas y efectos de disparo.
- **Progreso**: estrellas y mejor puntaje por nivel. Los niveles se desbloquean en orden.

## Requisitos de la consigna y dónde están

| Requisito | Cómo se cumple |
| --- | --- |
| Input táctil | `Gameplay/Input/InputReader.cs` (Enhanced Touch del Input System). |
| Característica del dispositivo además del touch | **Vibración** con el plugin nativo (`KuznickiVibrator.java`). Vibran el disparo, cada baja y el multiplicador. Se apaga con un botón junto a los de audio. |
| Niveles y dificultad creciente | Escenas `Level1` a `Level5`. Cada una tiene su munición y su tiempo objetivo en `LevelSettings`. |
| Tutorial | `Level1`: `LevelSettings` (cámara lenta y texto de inicio) y `TutorialZone` (ayudas a lo largo del nivel). |
| Monetización y progreso | Billetera, inventario y progreso de niveles guardados en PlayerPrefs (ver Memento). |
| Flujo de al menos 3 escenas | `MainMenu` → `Loading` → `Level1`…`Level5`. |
| UI adaptativa y áreas seguras | `CanvasScaler` por resolución de referencia y `UI/SafeArea/SafeAreaFitter.cs` en todo lo que toca los bordes. |
| Plugin nativo accesible desde la primera pantalla | Botón **LOGS** del menú principal: muestra los logs que guarda el plugin y permite borrarlos. |
| Logros y Leaderboard (Google Play Games) | 6 logros y el leaderboard *Total Score*. Se ven desde los botones **AWARDS** y **TOP** del menú. |
| Idioma | Todos los textos del juego están en inglés. |
| Botón Atrás de Android | En un nivel pausa y reanuda. En los submenús vuelve. En la pantalla principal pregunta antes de salir. |

## Patrones de diseño

La consigna pide al menos tres, distintos a los del TP1. Están implementados **Strategy**, **Command**, **Memento** y **Facade**, y además **Adapter** y **Template Method**. Las rutas son relativas a `TP2/Assets/_Game/Scripts/`.

| Patrón | Dónde | Para qué |
| --- | --- | --- |
| **Strategy** | `Gameplay/Weapons/IFireStrategy.cs`, `FireStrategy.cs`, `SingleShot.cs`, `SpreadShot.cs`, `BurstShot.cs`, `WeaponCatalog.cs`; `Gameplay/Gun/Gun.cs` (`SetStrategy`, `TryFire`) | Cada arma de la tienda dispara distinto (tiro simple, abanico, ráfaga). `Gun` no sabe cuál tiene: al cargar el nivel, `WeaponCatalog` le asigna la estrategia del arma equipada. Cada estrategia es un ScriptableObject con su ajuste. |
| **Command** | `Core/Commands/ICommand.cs`, `CommandInvoker.cs`; `Gameplay/Input/FireCommand.cs`, `InputReader.cs`; `Gameplay/Level/SetPausedCommand.cs`, `LevelBackCommand.cs`; `Core/Scenes/LoadSceneCommand.cs`, `RestartSceneCommand.cs`, `LoadMenuCommand.cs`; `UI/CommandButton.cs` | El toque, el botón Atrás y los botones del nivel (pausa, reintentar, siguiente, menú, tienda) no ejecutan acciones directamente: entregan un comando a `CommandInvoker`, que además puede grabarlos con su tiempo. |
| **Memento** | Fotos: `Economy/Save/EconomySnapshots.cs` (`WalletSnapshot`, `InventorySnapshot`, `LevelProgressSnapshot`). Originadores: `Economy/PlayerWalletManager.cs`, `PlayerInventoryManager.cs`, `PlayerLevelProgress.cs`. Cuidador: `Economy/Save/EconomySave.cs` | Cada originador arma y restaura su propia foto. Los campos de la foto son `internal`, así que nadie de afuera la lee. `EconomySave` las guarda como JSON en PlayerPrefs en cuanto algo cambia, sin mirar qué tienen adentro. |
| **Facade** | `Platform/MobilePlatform.cs` | Un único servicio para todo lo del teléfono: logs del plugin, vibración y Google Play Games. El resto del juego solo llama `Vibrate`, `UnlockAchievement`, `ReportTotalScore`, `GetLogs`… |
| **Adapter** | `Platform/Logs/AndroidNativeLogger.cs`, `Platform/Haptics/AndroidHaptics.cs` (y `SilentHaptics.cs`), `Platform/PlayGames/PlayGamesAccount.cs` | Adaptan las clases Java del plugin (`AndroidJavaObject`) y la API de Play Games a métodos C# (`SendLog`, `GetLogs`, `ClearLogs`) e interfaces chicas (`IHaptics`). |
| **Template Method** | `Gameplay/Enemies/Enemy.cs` (`TakeHit`), `StaticEnemy.cs` | La secuencia de recibir un impacto (vida, sonido, baja, monedas, vibración) es fija. Cada enemigo redefine `OnHit` y `OnDeath`. |

También se usan, pero **no cuentan** porque ya estaban en el TP1: Service Provider (`Bootstrap/Bootstrapper.cs`, ToolBox), Observer (`EventBus`) y Object Pool (`Gameplay/Bullets/BulletSpawner.cs` y la lista de la tienda).

## Optimización (Unidad 4)

- **Sprite Atlas**: toda la UI va en un solo Sprite Atlas V2 (`Art/Atlases/UI.spriteatlasv2`), en ASTC para Android y sin mipmaps.
- **Transición asincrónica entre escenas**: `Core/Scenes/LevelLoader.cs` carga `Loading` en forma aditiva con barra de progreso, descarga la escena anterior y activa la nueva cuando el arma terminó de cargar su equipamiento.
- **Carga y descarga de assets**: los modelos de armas, skins, balas, efectos y los prefabs que se instancian en ejecución (`Prefabs/Runtime`) salen de AssetBundles (`Core/Content/ContentLibrary.cs`). Al empezar cada carga de escena, `ContentLibrary` descarga los bundles y `Resources.UnloadUnusedAssets` libera lo que quedó sin usar.
- **Static batching**: la geometría de los niveles está marcada Batching Static y comparte materiales.
- **Audio**: la música se reproduce en streaming. Los efectos se descomprimen al cargar.
- Sin búsquedas ni reservas de memoria por frame durante el juego. Las balas y la lista de la tienda usan pools.

## Plugin nativo

`Plugin/KuznickiLogger` es una librería de Android en Java:

- `KuznickiLoggerManager` guarda los logs de Unity en un archivo de la sesión (`sendLog`), los devuelve para la pantalla LOGS (`getLogs`) y los borra después de confirmarlo en un `AlertDialog` nativo (`clearLogs`).
- `KuznickiVibrator` controla la vibración del teléfono.

El AAR compilado está en `TP2/Assets/Plugins/Android/`. Para recompilarlo:

```bash
cd Plugin
JAVA_HOME="C:/Program Files/Android/Android Studio/jbr" ./gradlew :KuznickiLogger:assembleRelease
# copiar KuznickiLogger/build/outputs/aar/KuznickiLogger-release.aar sobre TP2/Assets/Plugins/Android/
```

## Cómo compilar

1. Abrir `TP2/` con Unity 6000.3.11f1 y la plataforma Android activa.
2. Escena inicial: `Assets/_Game/Scenes/MainMenu.unity`.
3. Armar el APK o el AAB desde **File > Build Profiles > Build**. Ese botón arma antes los AssetBundles y los copia a StreamingAssets.

## Estructura

| Carpeta | Contenido |
| --- | --- |
| `TP2/Assets/_Game/Scripts/Core` | Servicios base: carga de escenas, contenido (AssetBundles), audio, comandos, economía de niveles |
| `TP2/Assets/_Game/Scripts/Gameplay` | Arma, balas, enemigos, armas (Strategy), nivel (`LevelCore`, peligros, tutorial) |
| `TP2/Assets/_Game/Scripts/Economy` | Billetera, inventario, equipamiento, progreso y guardado (Memento) |
| `TP2/Assets/_Game/Scripts/Platform` | `MobilePlatform` y sus adaptadores (logs, vibración, Play Games) |
| `TP2/Assets/_Game/Scripts/UI` | Menú, tienda, selección de nivel, HUD, pausa y resultado |
| `TP2/Assets/_Game/Scripts/Editor` | Menús `GunJump > …` que arman escenas, prefabs, atlas y bundles |
| `Plugin/` | Proyecto de Android Studio del plugin nativo |
