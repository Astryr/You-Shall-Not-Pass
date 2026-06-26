# GDD — You Shall Not Pass!

**Versión:** 2.1 (entrega final)
**Fecha:** 26/06/2026
**Motor:** Unity 6000.3.11f1 — Universal Render Pipeline (URP)
**Plataforma:** Android — dispositivo de referencia: TCL 408 (720×1600 px, gama baja)

---

## Integrantes

| Apellido y Nombre | Rol principal |
|-------------------|---------------|
| Herrera, Oriana | Project Manager · Artista 3D |
| Muiños, Guadalupe | Game Designer · Artista 3D |
| Lima, Thiago | Game Designer · QA · Audio |
| Jorge, Santino | Programador |

---

## 1. Sinopsis y High Concept

"You Shall Not Pass!" es un tower defense post-apocalíptico para Android. El jugador defiende el **Núcleo** (castillo central) de oleadas de robots de chatarra colocando torretas sobre una grilla. Tres niveles con dificultad creciente; el primero funciona como tutorial interactivo.

**Propuesta de valor:** experiencia de tower defense compacta, jugable en una sesión corta en móvil, con controles táctiles intuitivos y mecánicas claras desde el primer minuto.

---

## 2. Game Loop

```
Inicio de nivel
      │
      ▼
 Preparación ──► El jugador coloca torretas con recursos (chatarra)
      │
      ▼
 FORCE WAVE ──► Inicia la oleada; los enemigos avanzan por el camino
      │
      ▼
 Resolución ──► Las torretas disparan; el jugador observa y no puede
      │          construir durante la oleada
      ▼
 Recompensa ──► Chatarra por enemigos eliminados → volver a Preparación
      │
      ▼
 Fin de nivel ──► Todas las oleadas completadas → Victoria
               └─► Threat llega a 0 → Derrota
```

**Threat:** contador de vida del castillo. Cada enemigo que llega reduce el Threat en 1. Si llega a 0, el jugador pierde.

---

## 3. Controles

| Acción | PC (editor/prueba) | Móvil Android |
|--------|-------------------|---------------|
| Mover cámara | WASD o clic medio + arrastre | Un dedo arrastrando |
| Zoom | Rueda del mouse | Pinch con dos dedos |
| Construir torre | Clic izquierdo en casilla verde | Toque en casilla |
| Cancelar selección | Clic en zona vacía | Toque en zona vacía |
| Iniciar oleada | Botón FORCE WAVE | Botón FORCE WAVE |
| Abrir tutorial/ayuda | Botón ? en HUD | Botón ? en HUD |
| Pausa | F10 (solo editor) | Botón Pause en HUD |

**Orientación fija:** landscape (horizontal). La rotación a portrait está bloqueada a nivel de Project Settings y forzada en runtime por `MobileBootstrap.cs` para evitar que la UI se rompa al girar el dispositivo.

---

## 4. Contenido del juego

### Torretas (7 tipos)

| Nombre | Tipo de ataque | Particularidad |
|--------|---------------|----------------|
| Ballesta (Crossbow) | Proyectil directo | Torre básica, bajo costo |
| Cañón (Cannon) | Proyectil balístico | Área de impacto |
| Ametralladora (Machine Gun) | Ráfaga rápida | Alto DPS, corto rango |
| Martillo (Hammer) | Golpe en área | Stun a enemigos cercanos |
| Nido de araña (Spider Nest) | Spawner de arañas | Genera unidades propias |
| Arpón antiaéreo (AA Harpoon) | Proyectil guiado | Único que alcanza voladores |
| Ventilador (Fan) | Empuje de área | Revela y frena enemigos sigilosos |

### Enemigos (9 tipos)

| Tipo | Comportamiento especial |
|------|------------------------|
| Básico | Sin habilidades |
| Rápido | Alta velocidad de movimiento |
| Pesado (con escudo) | Alta vida, resistencia frontal |
| Enjambre (Swarm) | Muchas unidades de baja vida |
| Sigiloso | Se vuelve invisible fuera del rango del ventilador |
| Volador | Solo atacable por el arpón AA |
| Jefe volador | Volador con alta vida; genera unidades menores |
| Jefe araña | Boss con alta vida; genera arañas |
| Unidad de jefe | Generada por los jefes; muere con ellos |

### Niveles

| Nivel | Oleadas | Enemigos presentes | Descripción |
|-------|---------|-------------------|-------------|
| Level 1 | 3 | Básico, Rápido | Tutorial automático al entrar. Introduce las mecánicas base. |
| Level 2 | 5 | Básico, Rápido, Pesado, Enjambre | Mayor cantidad y diversidad. Requiere gestionar economía. |
| Level 3 | 7 | Todos los tipos | Máxima dificultad. Jefes aparecen en oleadas finales. |

---

## 5. Tutorial y accesibilidad

### Tutorial (Nivel 1)

Al entrar al Nivel 1 por primera vez, aparece automáticamente un overlay (`UI_Tutorial`) con tres secciones:

- **Objetivo:** explicación del Threat y qué pasa si los enemigos llegan al castillo.
- **Controles:** cómo mover la cámara, cómo construir una torre y cómo iniciar la oleada.
- **Consejo:** economía de recursos y cómo reabrir el tutorial con el botón `?`.

El tutorial se puede reabrir en cualquier momento desde el HUD durante la partida. Al reabrirlo pausa el juego para que el jugador pueda leerlo sin presión.

### Señalización visual

- Las casillas donde se pueden construir torretas cambian visualmente al pasar el dedo sobre ellas (feedback de hover).
- El HUD muestra en todo momento: Threat actual/máximo, recursos actuales, número de oleada y botón FORCE WAVE.
- Victoria y derrota tienen pantallas de overlay dedicadas que ocultan el HUD para máxima claridad.
- El contador de FPS en el centro-derecha usa colores: **verde** ≥60, **amarillo** 45-59, **rojo** <45.

### Feedback de sonido (refuerzo positivo/negativo)

- Hover en botones: SFX de selección.
- Click en botones: SFX de confirmación.
- Disparos de torre: SFX por tipo de torre (`Tower.cs` via `AudioManager.PlaySFX`).
- Música de menú (BGM track 0) y música de nivel (BGM track 1) diferenciadas.

**Nota de accesibilidad:** el juego es jugable sin audio. El tutorial, el HUD y todas las señales visuales funcionan independientemente del volumen. El feedback de audio es complementario, no reemplazable.

---

## 6. Arquitectura de código — Mecánicas principales

### 6.1 Sistema de grilla y construcción

El mapa de cada nivel es una grilla de `TileSlot` instanciados por `GridBuilder` en el editor (no en runtime). Cada tile puede ser de tipo "campo libre" o "slot de construcción". Al tocar un slot:

1. `BuildSlot.OnPointerDown()` notifica a `BuildManager`.
2. `BuildManager.SelectBuildSlot()` marca el slot seleccionado y abre el panel de torres (`UI_BuildButtonsHolder`).
3. Al elegir una torre, `BuildManager` llama a `Tower.Build()` en el slot.

```
BuildSlot (tap) → BuildManager → UI_BuildButtonsHolder → Tower prefab
```

**Decisión técnica:** el panel de torres se mueve con animación de posición (`UI_Animator.ChangePosition`) en lugar de activarse/desactivarse abruptamente. Esto da feedback visual claro de que el menú está disponible sin ser intrusivo.

### 6.2 Sistema de oleadas

`WaveManager` contiene la configuración de cada oleada como datos serializados (tipo de enemigo, cantidad, intervalo entre spawns). El jugador inicia cada oleada manualmente con FORCE WAVE; no existe temporizador automático para dar control total al jugador.

Al spawnar un enemigo:
1. `EnemyPortal.SpawnEnemy()` llama a `ObjectPoolManager.Get()`.
2. El enemigo recibe su ruta de waypoints del portal.
3. Cuando llega al castillo, `Castle.TakeDamage()` reduce el Threat en `GameManager`.

### 6.3 Object Pooling

Se usa `ObjectPoolManager` (singleton, `DontDestroyOnLoad`) para reutilizar enemigos, proyectiles y VFX. El pool pre-instancia N objetos al inicio; cuando se necesita uno se activa con `Get()` y al terminar se desactiva con `Remove()`.

```csharp
// En vez de Instantiate/Destroy:
GameObject obj = objectPool.Get(prefab, position, rotation, parent);
// Al terminar:
objectPool.Remove(obj);
```

**Justificación:** en oleadas con 20+ enemigos activos simultáneos, crear y destruir objetos genera allocations de GC que producen hitches de 20-50ms en dispositivos de gama baja. El pooling elimina ese overhead.

### 6.4 Movimiento de enemigos (NavMesh + Waypoints)

Los enemigos usan `NavMeshAgent` para moverse, combinado con waypoints explícitos de cada nivel. El NavMesh está **pre-baked** en cada escena de nivel; no se recalcula en runtime. La rotación del agente es manual (`Enemy.FaceTarget`) porque `agent.updateRotation = false` da resultados más fluidos y evita el giro brusco del NavMesh nativo.

### 6.5 Gestión de escenas (carga aditiva)

La `MainScene` siempre está cargada y contiene todos los managers globales (`GameManager`, `AudioManager`, `ObjectPoolManager`, `BuildManager`, `TileAnimator`, `UI`). Los niveles se cargan de forma **aditiva** sobre la `MainScene`.

**Flujo de carga optimizado (v2.0):**
1. La pantalla de carga se muestra.
2. `LoadSceneAsync` se inicia inmediatamente con `allowSceneActivation = false`.
3. La animación del menú juega mientras los datos de la escena se cargan en background con `ThreadPriority.Low`.
4. Cuando la animación termina Y la escena está al 90% → se activa la escena.
5. `LevelSetup.Start()` distribuye el setup entre frames con `yield return null` entre cada operación pesada.

**Decisión técnica:** sin este cambio, toda la carga ocurría después de la animación del menú, causando un freeze visible de 1-3 segundos en el TCL 408.

---

## 7. Optimizaciones técnicas implementadas

### 7.1 Optimización en engine / build

| Técnica | Implementación | Justificación |
|---------|---------------|---------------|
| **IL2CPP** | `scriptingBackend.Android: 1` | 20-30% más rápido que Mono en runtime en Android |
| **ARM64** | `AndroidTargetArchitectures: 2` | Arquitectura nativa del TCL 408 y cualquier Android moderno |
| **Minify Release** | `AndroidMinifyRelease: 1` | Reduce tamaño de APK |
| **Managed Stripping** | Level Low | Elimina código IL no usado sin romper reflexión |
| **GC Incremental** | `gcIncremental: 1` en ProjectSettings | El GC distribuye su trabajo en slices por frame en lugar de pausar todo el juego |
| **GC LatencyMode** | `GCSettings.LatencyMode = GCLatencyMode.LowLatency` | Prioriza pausas cortas sobre throughput; crítico para mantener 60 FPS en oleadas intensas |
| **Async scene loading** | `LoadSceneAsync` con `allowSceneActivation = false` | Carga mientras la animación juega, evitando freeze |
| **backgroundLoadingPriority** | `ThreadPriority.Low` durante carga, `BelowNormal` en gameplay | Evita que el hilo de carga interrumpa el hilo principal |
| **BuildSlot singletons** | `UI.instance`, `BuildManager.instance`, `TileAnimator.instance` | Elimina ~150 `FindFirstObjectByType` durante activación de escena (3 calls × N slots por nivel) |
| **maximumDeltaTime** | `Time.maximumDeltaTime = 0.05f` | Cap de 50ms de deltaTime: si el frame de activación tarda más, la física no explota |

### 7.2 Gráficos y renderizado

| Técnica | Valor configurado | Justificación |
|---------|------------------|---------------|
| **URP Performant** | Perfil índice 0 en Android | Pipeline de menor overhead para móvil |
| **Render Scale** | 0.85 | Renderiza a 85% de resolución nativa, reescala antes de mostrar |
| **Sombras** | `shadowDistance = 15 m` en Android | Sombras solo a 15 m de la cámara, no a distancia completa |
| **Una sola luz direccional** | `LevelEnvironmentOptimizer.Apply()` | Desactiva todas las luces puntuales/spot; conserva la direccional más intensa |
| **LOD Bias** | `0.7` en Android | Activa LODs de baja poli antes, reduciendo tris en GPU |
| **MSAA desactivado** | `QualitySettings.antiAliasing = 0` + `cam.allowMSAA = false` | MSAA resuelve múltiples samples por pixel; en gama baja el costo supera el beneficio visual |
| **HDR desactivado** | `cam.allowHDR = false` en Android | HDR necesita render target de 16-32 bits por canal; reduce significativamente la presión sobre la memoria de GPU |
| **Bloom activo** | Con URP Performant | El Bloom en URP Performant es una pass liviana; se mantiene por calidad visual |

### 7.3 Físicas y código

| Técnica | Implementación | Justificación |
|---------|---------------|---------------|
| **Object Pooling** | `ObjectPoolManager` (enemigos, proyectiles, VFX) | Elimina GC allocations en gameplay activo |
| **Solver iterations** | `Physics.defaultSolverIterations = 4` | Default 6→4: -30% carga de CPU por FixedUpdate sin afectar gameplay |
| **NavMesh pre-baked** | Bake en editor por nivel | Evita recálculo en runtime |
| **NavMeshSurface cached** | `GridBuilder._navMesh` con lazy-init | Evita `GetComponent<NavMeshSurface>()` en cada acceso a la property |
| **TileSlot cached** | `GridBuilder.cachedTileSlots` | Evita `GetComponent<TileSlot>()` × tiles en cada `MakeTilesNonInteractable` |
| **Singletons de managers** | `static instance` en BuildManager, TileAnimator, UI | `BuildSlot.Awake()` antes hacía 3 `FindFirstObjectByType` por tile; ahora acceso O(1) |

### 7.4 Manejo de assets

**Audio:**

| Archivo | Formato | Load Type | Configuración | Justificación |
|---------|---------|-----------|--------------|---------------|
| `bg_example_1/2/3.mp3` | Vorbis | Streaming | `loadInBackground: 1` | BGM largo → streaming no carga todo en RAM; `loadInBackground` evita bloquear el hilo principal |
| `ui_click_1.mp3`, `ui_onHover_1.mp3`, `sfx_beam_2.mp3` | ADPCM | Decompress On Load | `preloadAudioData: 1`, `forceToMono: 1` | SFX cortos → ADPCM decodifica instantáneamente; mono reduce RAM al 50% |
| `sfx_beam_1.mp3`, `ui_click_2.wav`, `ui_onHover_2.wav` | Vorbis | Decompress On Load | `preloadAudioData: 1`, `forceToMono: 1` | SFX medianos → Vorbis da mejor compresión para tamaños > 30 KB |

**Por qué ADPCM para SFX cortos y no Vorbis:**
Vorbis tiene mayor latencia de decodificación. En SFX de UI (clicks, hover) que se disparan al toque del usuario, una latencia de 5-10 ms es perceptible. ADPCM decodifica en 1 ms pero produce archivos más grandes; para clips de < 5 KB el trade-off es favorable.

**Texturas:**

| Tipo | Formato | Tamaño máx | Configuración |
|------|---------|-----------|--------------|
| Texturas 3D terreno | ETC2 (RGBA8) | 1024 px | MipMaps habilitados |
| Iconos de torretas UI | ETC2 (RGBA8) | 512 px | Sprite Atlas unificado |
| UI general | ETC2 (RGBA8) | 512 px | MipMaps deshabilitados (UI 2D, no se aleja) |

**Sprite Atlas:** todos los iconos de las 7 torretas están en un único Sprite Atlas. Esto reduce los draw calls de la UI de 7 a 1 cuando el panel de construcción está abierto.

---

## 8. Rubrica — Autoevaluación

| Apartado | Estado | Evidencia |
|----------|--------|-----------|
| **Optimización en engine/off game (2/10)** | ✅ | IL2CPP/ARM64, async loading, backgroundLoadingPriority, object pool, URP Performant, minify |
| **Iluminación (2/10)** | ✅ | `LevelEnvironmentOptimizer`: 1 luz direccional, sin puntuales/spot, shadow distance 15m, LOD bias 0.7 |
| **Físicas (1/10)** | ✅ | Object pool (enemigos/proyectiles), NavMesh pre-baked, solver 4 iteraciones, TileSlot cached |
| **Manejo de assets (2/10)** | ✅ | ETC2, Sprite Atlas, ADPCM/Vorbis justificado, loadInBackground, preloadAudioData |
| **Accesibilidad (2/10)** | ✅ | Tutorial con objetivo+controles+tips, HUD siempre visible, señalización de slots, SFX de feedback, pantallas de resultado claras |
| **Planificación (1/10)** | ✅ | Este GDD + bitácora detallada + High Concept con justificaciones técnicas |

---

## 9. Bitácora de desarrollo

### Fase 1 — Preproducción y concepto

Se define el género (tower defense), la plataforma objetivo (Android, dispositivo TCL 408 como referencia de gama baja), el motor (Unity + URP) y el loop de juego central. Se reparten roles: diseño de arte low-poly a cargo de Herrera y Muiños, programación a cargo de Jorge y Lima.

**Decisión temprana:** usar URP en lugar del pipeline Built-in porque URP tiene mejor soporte de optimización móvil y el perfil "Performant" reduce el overhead de rendering sin necesidad de ajustar shaders manualmente.

### Fase 2 — Prototipo inicial

Se construye el sistema de grilla con `GridBuilder` (herramienta de editor para generar tiles) y el primer loop: colocar torre → spawn enemigo → llega al castillo → reduce Threat. La economía básica (chatarra por enemigo) funciona desde esta fase.

**Problema encontrado:** los tiles se instanciaban en runtime en cada carga de nivel, causando un freeze de ~2 segundos. **Solución:** migrar la generación de la grilla al editor (método `BuildGrid()` en `GridBuilder`) y serializar la lista `createdTiles`. La escena ya tiene los tiles creados; solo se animan al cargar.

### Fase 3 — Core gameplay

Implementación de `WaveManager` (configuración de oleadas como datos), `GameManager` (Threat, economía, victoria/derrota), tres niveles con dificultad progresiva y botón FORCE WAVE manual.

**Decisión de diseño:** el jugador inicia cada oleada manualmente. Se descartó el temporizador automático porque en un dispositivo lento el jugador podría no tener tiempo de construir torres antes de que empiece la oleada. El FORCE WAVE da control total y reduce la frustración.

### Fase 4 — Enemigos, torretas y pooling

Se implementan los 9 tipos de enemigos y las 7 torretas. Se detecta que `Instantiate`/`Destroy` en cada spawn/muerte de enemigo causaba spikes de GC de 30-50 ms (medidos con el FPS counter). Se implementa `ObjectPoolManager` con `UnityEngine.Pool.ObjectPool<T>` para eliminar las allocations.

**Problema técnico:** el arpón (`Tower_Harpoon`) tenía una race condition: si el enemigo moría y volvía al pool mientras el proyectil estaba en vuelo, `Projectile_Harpoon.AttachToEnemy()` llamaba a métodos sobre un objeto inactivo, causando `StartCoroutine on inactive object` y `NullReferenceException`. **Solución:** verificar `enemy.gameObject.activeInHierarchy` en `Update()` y en `AttachToEnemy()` antes de continuar. Si el enemigo está inactivo, llamar `tower.ResetAttack()` para limpiar el estado.

### Fase 5 — Interfaz y flujo completo

Menú principal, selección de nivel, pantalla de carga, HUD in-game, tutorial del Nivel 1, How to Play, pantallas de victoria/derrota, créditos con nombres del equipo, ajustes de audio y sensibilidad.

**Problema:** al cargar un nivel, el canvas del MainScene (menú principal) aparecía superpuesto sobre la UI del nivel. La causa fue que `LevelSetup` encontraba cualquier `UI` en escena, incluyendo la del MainScene persistente. **Solución:** `LevelSetup.DestroyDuplicateUICanvases()` busca todos los `UI` components y destruye los que no pertenecen al MainScene, usando `scene.name` para distinguirlos.

### Fase 6 — Corrección docente (27/05/2026)

El profesor señala:
- Falta pantalla de carga → implementada con `UI_LoadingScreen` y barra de progreso.
- Falta audio → implementado `AudioManager` con BGM por contexto y SFX de UI/combate.
- Hay luz dinámica → se implementa `LevelEnvironmentOptimizer` que al cargar cada nivel desactiva todas las luces puntuales/spot y mantiene solo la direccional más intensa.
- Falta tutorial → implementado `UI_Tutorial` con objetivo, controles y tips. Aparece automáticamente en Nivel 1 y es reabrirle con `?`.
- La pantalla rota → orientación bloqueada en landscape a nivel de `ProjectSettings` y reforzada en runtime por `MobileBootstrap`.

### Fase 7 — Auditoría de bugs (23/06/2026)

Se realiza una auditoría completa de 80 scripts. Bugs críticos encontrados y corregidos:

**`GameManager.Start()` y `PrepareLevel()`:** llamadas directas a `inGameUI.UpdateHealthPointsUI()` sin null-check. Si el nivel se abre directamente en editor sin MainScene, crasheaba. Corregido con `?.` y re-lookup de `inGameUI` en `PrepareLevel()`.

**`SelfRemoveToPool`:** leía `ObjectPoolManager.instance` en `Awake()`. Si el pool todavía no existía (orden de inicialización no garantizado), la referencia era null y el `Remove()` crasheaba. Corregido: mover la lectura a `OnEnable()` con lazy-init.

**`Waypoint.Awake()`:** `GetComponent<MeshRenderer>().enabled = false` sin null-check. Si un waypoint no tiene MeshRenderer (posible en prefabs de nivel), crasheaba. Corregido con null-check.

**`RadiusDisplay` y `TowerPreview`:** `FindFirstObjectByType<BuildManager>()` sin null-check. Si se abre la escena directamente sin BuildManager, crash. Corregido.

### Fase 8 — Optimización de carga: primera ronda (26/06/2026)

Se detecta que el juego se traba (FPS cae a ~2) durante la pantalla de carga del nivel en el TCL 408. Análisis de causas:

1. **La escena empezaba a cargarse después de la animación del menú**, no durante. Todo el I/O y la activación de GameObjects ocurría de golpe mientras la pantalla de carga debería estar mostrándose.

2. **`GridBuilder.myNavMesh`** era una property expression (`=> GetComponent<NavMeshSurface>()`), llamando `GetComponent` en cada acceso.

3. **`LevelSetup.Start()`** ejecutaba `LevelEnvironmentOptimizer.Apply()`, `DeleteExtraObjects()` y varios `FindFirstObjectByType()` de forma síncrona consecutiva, bloqueando el renderer varios frames.

**Soluciones aplicadas:**

- `LevelManager`: se inicia `LoadSceneAsync` con `allowSceneActivation = false` antes de la animación. Se activa la escena cuando la animación termina Y la escena está al 90%.
- `Application.backgroundLoadingPriority = ThreadPriority.Low` durante la carga.
- `GridBuilder`: `_navMesh` con lazy-init + `cachedTileSlots` para evitar `GetComponent` por tile.
- `LevelSetup.Start()`: `yield return null` entre operaciones pesadas.

**Resultado:** FPS durante carga mejoró de ~2 a ~24.

### Fase 9 — Optimización de carga: segunda ronda (26/06/2026)

Con 24 FPS en el spike de activación de escena, se analiza la causa raíz restante: el frame de activación de Unity ejecuta todos los `Awake()` de los GameObjects del nivel en un solo frame.

**Cuello de botella identificado:** cada `BuildSlot.Awake()` llamaba:
```csharp
ui          = FindFirstObjectByType<UI>();          // O(n) sobre todos los objetos
tileAnim    = FindFirstObjectByType<TileAnimator>(); // O(n) sobre todos los objetos
buildManager = FindFirstObjectByType<BuildManager>(); // O(n) sobre todos los objetos
```
Con 20-50 BuildSlots por nivel → **60-150 búsquedas O(n) en un solo frame**.

**Soluciones aplicadas:**

- **`UI`, `BuildManager`, `TileAnimator`:** se añade campo `public static instance` que se asigna en `Awake()`. Son singletons de facto (uno por escena, en `MainScene` persistente).

- **`BuildSlot`:** se reemplaza el `Awake()` con 3 `FindFirstObjectByType` por propiedades que acceden al singleton directamente (`UI.instance`, `BuildManager.instance`, `TileAnimator.instance`). El `Awake()` ahora solo asigna `defaultPosition = transform.position`. Costo: O(1), sin búsqueda.

- **`MobileBootstrap`:** se agregan las siguientes configuraciones globales:
  - `GCSettings.LatencyMode = GCLatencyMode.LowLatency`: el runtime de .NET prioriza pausas cortas de GC.
  - `Time.maximumDeltaTime = 0.05f`: si un frame tarda más de 50ms (p.ej. el frame de activación), la física recibe máximo 50ms de delta, evitando que objetos físicos "salten" por el spike.
  - `QualitySettings.antiAliasing = 0`: MSAA desactivado globalmente en Android.
  - `cam.allowHDR = false` y `cam.allowMSAA = false`: desactivan render targets de alta precisión que no aportan calidad visible en el TCL 408 pero consumen bandwidth de GPU.

---

## 10. Entregables

- APK Android (build release) — enlace Google Drive: *(completar)*
- Proyecto Unity: `C:\Proyectos\You-Shall-Not-Pass`
- Este GDD (`Docs/GDD_You_Shall_Not_Pass.md`)
- High Concept (`Docs/HighConcept_You_Shall_Not_Pass.md`)
