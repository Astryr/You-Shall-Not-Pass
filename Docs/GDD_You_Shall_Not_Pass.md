# GDD — You Shall Not Pass!

**Versión:** 2.8 (entrega final — revisión completa)
**Fecha:** 14/07/2026
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

**Orientación:** landscape (horizontal) con auto-rotación habilitada entre **LandscapeLeft** y **LandscapeRight**. Si el jugador gira el teléfono 180°, la pantalla acompaña. La rotación a portrait está bloqueada a nivel de Project Settings y reforzada en runtime por `MobileBootstrap.cs`. El canvas escala correctamente en ambas orientaciones al ser `ScreenSpaceOverlay`.

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
- El contador de FPS en el **costado izquierdo** (centro vertical) usa colores: **verde** ≥60, **amarillo** 45-59, **rojo** <45. Posicionado a la izquierda para evitar que la cámara frontal del teléfono (ubicada a la derecha en landscape) lo tape.

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
| **Render Scale** | **0.75** (URP-Performant.asset + forzado en runtime por MobileBootstrap) | Renderiza al 75% de la resolución nativa del TCL 408 (540×900 px efectivos); reduce el fill rate en ~31% respecto a full resolution. Recomendado por el docente para alcanzar 60 FPS de forma consistente. |
| **Sombras** | `shadowDistance = 15 m` en Android | Sombras solo a 15 m de la cámara, no a distancia completa |
| **Una sola luz direccional** | `LevelEnvironmentOptimizer.Apply()` | Desactiva todas las luces puntuales/spot; conserva la direccional más intensa |
| **LOD Bias** | `0.7` en Android | Activa LODs de baja poli antes, reduciendo tris en GPU |
| **MSAA desactivado** | `QualitySettings.antiAliasing = 0` + `cam.allowMSAA = false` | MSAA resuelve múltiples samples por pixel; en gama baja el costo supera el beneficio visual |
| **HDR desactivado** | `cam.allowHDR = false` en Android | HDR necesita render target de 16-32 bits por canal; reduce significativamente la presión sobre la memoria de GPU |
| **Skybox eliminado en Android** | `cam.clearFlags = SolidColor` + `cam.backgroundColor = black` + `RenderSettings.skybox = null` | Elimina el pase de renderizado del skybox (-1 drawcall). Los niveles son interiores/industriales; el fondo negro encaja con la estética y no hay pérdida visual. La iluminación bakeada no depende del skybox en runtime. |
| **Far clip plane reducido** | `cam.farClipPlane = 80 m` (default era 1000 m) | El área de juego no supera ~30 u de ancho. Con la cámara a máx. 16 u de altura, 80 m cubre todo el nivel con margen; valores más altos solo añaden overdraw innecesario. |
| **Lens Flare desactivado** | `m_SupportDataDrivenLensFlare: 0`, `m_SupportScreenSpaceLensFlare: 0` | El juego no usa lens flares; tenerlos habilitados compila shader variants innecesarios. |
| **Light Cookies desactivado** | `m_SupportsLightCookies: 0` | No se usan cookies en ninguna luz del proyecto; desactivar elimina el sampler y las variantes de shader. |
| **LOD Cross-fade desactivado** | `m_EnableLODCrossFade: 0` | Las transiciones dithered de LOD no aportan calidad perceptible en este juego; desactivar elimina un shader pass y variables de cómputo. |
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
| **Selección de tiles via Physics.Raycast directo** | `BuildManager.Update()` lanza `Camera.main.ScreenPointToRay` al toque y llama `TriggerSelect()` si golpea un BuildSlot | Mecanismo principal para Android. No depende del EventSystem ni del PhysicsRaycaster, que en algunos dispositivos/compilaciones no despacha eventos a objetos 3D correctamente |

### 7.4 Manejo de assets

#### Audio — configuración de importación

Todos los archivos de audio usan **Vorbis** como formato de compresión (verificado en los `.meta` de cada clip). A continuación la configuración exacta de cada tipo:

| Archivo | Formato | Load Type | `preloadAudioData` | `forceToMono` | `loadInBackground` | Justificación |
|---------|---------|-----------|-------------------|--------------|-------------------|---------------|
| `bg_example_1.mp3` `bg_example_2.mp3` `bg_example_3.mp3` | Vorbis (`compressionFormat: 1`) | **Streaming** (`loadType: 2`) | 0 | 0 | **1** | BGM de larga duración: el streaming decodifica en tiempo real y nunca ocupa más de un pequeño buffer en RAM. `loadInBackground: 1` evita que la primera lectura del disco bloquee el hilo principal. |
| `ui_click_1.mp3` `ui_click_2.wav` `ui_onHover_1.mp3` `ui_onHover_2.wav` `sfx_beam_1.mp3` `sfx_beam_2.mp3` | Vorbis (`compressionFormat: 1`) | **Decompress On Load** (`loadType: 0`) | **1** | **1** | 0 | SFX cortos: se descomprimen una sola vez al cargar y quedan en RAM como PCM sin comprimir. Esto garantiza latencia mínima al dispararse (crítico para feedback táctil). `forceToMono: 1` descarta el canal derecho y reduce el uso de RAM al 50%. `preloadAudioData: 1` asegura que el clip esté en memoria antes de que se necesite. |

**Decisión técnica — por qué Vorbis para SFX cortos y no ADPCM:**
Vorbis en modo Decompress On Load no tiene latencia de decodificación en runtime (se decodifica al cargar, no al reproducir). La desventaja (mayor tiempo de carga inicial) se compensa con `preloadAudioData: 1`. ADPCM produciría archivos 3-5× más grandes por clip sin ventaja perceptible en este caso.

#### Texturas — configuración de importación

| Tipo de asset | Formato Android | Tamaño máximo | MipMaps | Notas |
|--------------|----------------|--------------|---------|-------|
| Paleta de textura 3D (`TD_main_texture_palette.png`) | **ETC2 RGBA8** (`textureFormat: 47`) | 1024 px | ✅ habilitados | Textura atlas compartida por torrets, caminos y estructuras. ETC2 RGBA8 es el estándar de compresión nativo en OpenGL ES 3.0+, presente en todos los Android modernos. MipMaps habilitados para reducir aliasing con la cámara alejada. |
| Iconos de torretas UI (Sprite Atlas) | ETC2 RGBA8 | 512 px | ❌ (UI fija en pantalla) | Agrupados en Sprite Atlas para reducir drawcalls. Sin mipmaps porque los sprites de UI no se escalan en profundidad. |
| UI general (bordes, barras, botones) | ETC2 RGBA8 | 512 px | ❌ | Sin mipmaps; escalan solo en 2D con el canvas scaler. |
| Texturas de VFX / efectos de partículas | ETC2 RGBA8 (Cartoon FX, Hovl Studio) | 1024 px | Según asset | Assets de terceros con sus propias configuraciones de compresión. Se mantienen las configuraciones originales para no romper los efectos visuales. |

**Por qué ETC2 y no ASTC:**
ETC2 es compatible con todos los dispositivos Android con OpenGL ES 3.0+, incluido el TCL 408 (Mediatek Helio A20). ASTC tiene mejor calidad de compresión pero requiere consulta de extensión en tiempo de carga; en dispositivos de gama baja puede caer a fallback sin compresión. ETC2 es el estándar seguro.

#### Materiales — shaders y configuración

| Material | Shader | Render Type | Textura | Configuración relevante |
|----------|--------|------------|---------|------------------------|
| `Main_mat.mat` | **URP/Lit** | Opaque | `TD_main_texture_palette.png` (1024px, ETC2) | Metallic 0.19, Smoothness 0.54, sin normal map. Recibe lightmaps (`m_LightmapFlags: 4`). Sin reflexiones de entorno (`_GlossyReflections: 0`) para ahorrar un sample de reflection probe por drawcall. |
| `Tiles_mat.mat` / `Tiles_mat 1-3.mat` | **URP/Lit** | Opaque | Sin textura (color sólido) | Color base por variante (verde disponible, gris ocupado, rojo no disponible). Sin textura → cero bytes de textura en GPU, 0 texture samplers. Metallic 0, Smoothness 0.4. |
| `Ground_Mat_1.mat` | **URP/Lit** | Opaque | — | Terreno base del mapa. Sin mapa normal para reducir el costo de píxel en fragmento shader. |
| `Emission_*.mat` (blue, green, red, etc.) | **URP/Lit** | Opaque | Sin textura | Emisión de color puro para señalización (radio de ataque, previsualización, waypoints). Sin textura, sin normal map. El color de emisión es constante → sin costo de sample de textura. |
| `Enemy_Transperent.mat` | **URP/Lit** | Transparent | — | Usado por el enemigo sigiloso (`Enemy_Stealth`). Renderiza en cola transparente; el alpha controla el nivel de invisibilidad. Más costoso que Opaque, pero solo se aplica a una unidad. |
| `BuildPreview_Mat.mat` | **URP/Lit** | Transparent | — | Previsualización semitransparente de la torre antes de construirla. Solo existe durante la fase de selección. |
| `AttackRadius_Mat.mat` | **URP/Lit** | Transparent | — | Círculo de rango de ataque de la torreta seleccionada. Solo visible al seleccionar un slot. |

**Decisión técnica — por qué URP/Lit y no URP/Unlit:**
El shader Lit de URP permite recibir iluminación bakeada (lightmaps), que es la fuente principal de luz en los niveles. Sin Lit, los modelos se verían sin sombras bakeadas, perdiendo la profundidad visual. URP/Lit con configuración mínima (sin normal maps, sin reflexiones, sin clearcoat) tiene un costo solo ligeramente mayor que Unlit pero mantiene compatibilidad total con el sistema de bake del proyecto.

**Decisión técnica — GPU Instancing:**
`m_EnableInstancingVariants: 0` (desactivado) en los materiales de tiles y de terreno. En este proyecto el número de materiales distintos es bajo y los objetos estáticos usan batching; el instancing manual no aporta mejora frente al Static Batching que Unity aplica automáticamente.

**Sprite Atlas:**
Todos los iconos de las 7 torretas están en un único Sprite Atlas. Esto reduce los draw calls de la UI de 7 a 1 cuando el panel de construcción está abierto. La policy es `Pack Together` (atlas cuadrado único, ninguna textura > 512px), compilado para `Android` target platform.

---

### 7.5 Técnicas evaluadas y NO aplicadas — justificación

La rúbrica exige que cada técnica no aplicada sea justificada explícitamente. Las siguientes técnicas fueron evaluadas y descartadas por las razones indicadas:

| Técnica | Motivo de no aplicación |
|---------|------------------------|
| **Occlusion Culling estático** | Los niveles son grillas abiertas con pocos objetos grandes. El área visible desde la cámara (fija, isométrica, ~30 u de diámetro) es casi la totalidad del nivel. Los portales de oclusión generarían overhead de CPU sin beneficio medible porque no hay paredes ni estructuras cerradas que dividan visualmente el mapa. Se optó por Far Clip Plane reducido (80 m) + frustum culling automático de Unity como alternativa más eficiente para este tipo de nivel. |
| **Light Probes para objetos dinámicos** | Los enemigos son objetos dinámicos que podrían recibir iluminación de Light Probes. Sin embargo, los niveles tienen iluminación bakeada de una única luz direccional; agregar una red de Light Probes añadiría complejidad de escena y memoria de textura sin cambio visual perceptible en modelos de estilo low-poly con materiales de color plano. La iluminación de los enemigos se resuelve con el Ambient Light global, suficiente para el estilo visual del juego. |
| **Topology / decimación de mallas** | Los modelos 3D del proyecto son assets de bajo polígono por diseño artístico (estilo low-poly). Las topologías ya tienen el mínimo de triángulos necesario para representar la forma. No se realizó decimación adicional porque hacerlo rompería la silueta visual del estilo elegido. |
| **GPU Instancing manual** | Los materiales tienen `m_EnableInstancingVariants: 0` porque el proyecto usa Static Batching (objetos del nivel marcados como estáticos). Unity combina automáticamente los batches estáticos en un único draw call por material, lo que es más eficiente que instancing manual para geometría fija como tiles, caminos y estructuras. GPU Instancing sería ventajoso para objetos dinámicos en gran cantidad (p. ej. 100+ enemigos idénticos); en este juego hay máximo 20-30 enemigos simultáneos de hasta 9 tipos distintos, por lo que el overhead de instancing no justifica el beneficio. |
| **Timeline (Cinemática)** | El juego no tiene cinematics ni secuencias animadas pre-definidas. Las transiciones de UI (menú, carga, victoria/derrota) se manejan con `UI_Animator` (corrutinas de posición/alpha) que son más livianas que un clip de Timeline para movimientos simples. El uso de Timeline para una pantalla de menú sería sobredimensionamiento sin mejora de experiencia. |
| **Post-procesado avanzado (DoF, Motion Blur, SSAO)** | Depth of Field y Motion Blur requieren la textura de profundidad (`m_RequireDepthTexture`) que está deliberadamente desactivada en URP-Performant para ahorrar un render target completo (~2 MB en 720p). SSAO (Screen Space Ambient Occlusion) añade un pass completo de renderizado; en el TCL 408 ese pass solo representa degradación de FPS. Se mantiene únicamente el Bloom que ya está en el perfil Performant y cuya implementación en URP es liviana (un blit post-procesado). |
| **Asset Bundles / Addressables** | La descarga dinámica de contenido no aplica a este proyecto; todo el contenido está incluido en el APK. Los Asset Bundles serían necesarios si hubiera DLC, actualizaciones OTA o contenido descargable. El tamaño del APK es manejable sin fragmentación de contenido. |

---

## 8. Rubrica — Autoevaluación

| Apartado | Estado | Evidencia |
|----------|--------|-----------|
| **Optimización en engine/off game (2/10)** | ✅ | IL2CPP/ARM64, async loading, backgroundLoadingPriority, object pool, URP Performant, render scale 0.75, minify, skybox eliminado, far clip plane 80m, singleton managers, GC LowLatency, lens flares/light cookies/LOD cross-fade desactivados. Técnicas no aplicadas y justificadas: oclusión estática (no aplica; ver sección 7.5). |
| **Iluminación (2/10)** | ✅ | `LevelEnvironmentOptimizer`: 1 luz direccional, sin puntuales/spot, shadow distance 15m, LOD bias 0.7, lightmaps bakeados en las 3 escenas, reflection probes por nivel. Técnicas no aplicadas y justificadas: Light Probes para objetos dinámicos (ver sección 7.5). |
| **Físicas (1/10)** | ✅ | Object pool (enemigos/proyectiles/VFX), NavMesh pre-baked, solver 4 iteraciones, TileSlot cached, `PhysicsRaycaster` en cámara para input táctil sobre objetos 3D |
| **Manejo de assets (2/10)** | ✅ | ETC2 RGBA8 para todas las texturas Android, Sprite Atlas para íconos de torretas, Vorbis/Streaming para BGM (loadInBackground), Vorbis/DecompressOnLoad para SFX (preloadAudioData, forceToMono), sin normal maps en materiales de juego, URP/Lit con configuración mínima; todo justificado en sección 7.4 |
| **Accesibilidad (2/10)** | ✅ | Tutorial con objetivo+controles+tips, HUD siempre visible, señalización de slots, SFX de feedback, pantallas de resultado claras, FPS counter en costado izquierdo libre de notch, auto-rotación entre ambos landscape, colocación de torres táctil (Physics.Raycast directo) |
| **Planificación (1/10)** | ✅ | Este GDD v2.8 con bitácora de 8 fases + High Concept v2.5 con justificaciones técnicas completas; todos los integrantes figuran en la portada y en créditos in-game |

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

### Fase 8 — Corrección docente: optimizaciones, cámara y rendimiento (14/07/2026)

A partir de la devolución del docente se aplica un conjunto de mejoras en cámara, renderizado, URP y documentación. El sistema de selección de tiles y colocación de torretas **no fue modificado** porque funciona correctamente mediante `Physics.Raycast` directo en `BuildManager.Update()`.

**Contador de FPS:**

Se crea `FPSCounter.cs` con auto-inicialización (`RuntimeInitializeOnLoadMethod + DontDestroyOnLoad`) para que aparezca en toda escena sin intervención manual. El panel se ancla al **borde izquierdo** de la pantalla (centro vertical) para evitar que la cámara frontal del teléfono —ubicada en el borde derecho en landscape— lo tape:

```csharp
panelRect.anchorMin        = new Vector2(0f, 0.5f);
panelRect.anchorMax        = new Vector2(0f, 0.5f);
panelRect.pivot            = new Vector2(0f, 0.5f);
panelRect.anchoredPosition = new Vector2(8f, 0f);
```

El texto cambia de color según el FPS medido: **verde** ≥ 60, **amarillo** 45-59, **rojo** < 45. `raycastTarget = false` en panel y texto para no bloquear el input táctil. El canvas usa `sortingOrder = 9999` para quedar siempre visible sobre cualquier otra UI.

**Cámara — zoom y límites:**

El gesto de pinch podía cubrir todo el rango min-max de zoom en un solo gesto rápido porque el multiplicador era `zoomSpeed * 0.01f`. Se reduce a `0.003f` para dar control fino y predecible. En `CameraController.Start()` se añade clamping que fuerza `minZoom ≥ 4` y `maxZoom ≤ 16` en runtime, independientemente del valor del Inspector.

Para limitar el área de desplazamiento, se añade el guard `maxDistanceFromCenter > 0.01f &&` en los tres manejadores (`HandleMovement`, `HandleMouseMovement`, `ApplyZoom`), evitando que un valor cero en el Inspector deshabilite el límite por error.

**Rotación de pantalla:**

`Screen.orientation = ScreenOrientation.LandscapeRight` fijaba un único lado. Se cambia a `ScreenOrientation.AutoRotation` con `autorotateToLandscapeLeft = true` y `autorotateToLandscapeRight = true`; el retrato queda deshabilitado. El jugador puede usar el teléfono en cualquier orientación landscape sin perder visibilidad del contador de FPS.

**Skybox eliminado y far clip plane reducido:**

En `MobileBootstrap.ApplyCameraSettings()` (Android):
- `cam.clearFlags = CameraClearFlags.SolidColor` + `cam.backgroundColor = Color.black` + `RenderSettings.skybox = null` → elimina el pase de renderizado del skybox (-1 drawcall). El fondo negro encaja con la estética industrial del juego y la iluminación bakeada no depende del skybox en runtime.
- `cam.farClipPlane = 80f` (default 1000 m) → los niveles no superan ~30 u de diámetro; 80 m cubre toda la geometría con margen y reduce el overhead de frustum culling y z-buffer.

**URP Performant — optimizaciones solicitadas por el docente:**

| Parámetro | Antes | Después | Justificación |
|-----------|-------|---------|---------------|
| `m_RenderScale` | 0.85 | **0.75** | Renderiza al 75% de la resolución nativa → -31% de fill rate. Recomendado explícitamente por el docente para alcanzar 60 FPS en gama baja. |
| `m_SupportDataDrivenLensFlare` | 1 | **0** | No se usan lens flares; desactivar elimina shader variants innecesarias. |
| `m_SupportScreenSpaceLensFlare` | 1 | **0** | Igual al anterior. |
| `m_SupportsLightCookies` | 1 | **0** | Sin cookies en ninguna luz del proyecto; desactivar elimina el texture sampler y variantes de shader. |
| `m_EnableLODCrossFade` | 1 | **0** | Las transiciones dithered de LOD no aportan calidad perceptible en este estilo low-poly. |

Parámetros verificados que ya estaban optimizados:
- `m_RequireDepthTexture: 0` (Depth Texture desactivada)
- `m_RequireOpaqueTexture: 0` (Opaque Texture desactivada)
- `m_SupportsHDR: 0` (HDR desactivado)
- `m_MainLightShadowsSupported: 0` (sombras de luz principal desactivadas)
- `m_AdditionalLightsRenderingMode: 0` (luces adicionales desactivadas)

El render scale también se fuerza en runtime desde `MobileBootstrap.ApplySettings()` como garantía:
```csharp
if (QualitySettings.renderPipeline is UniversalRenderPipelineAsset urpAsset)
    urpAsset.renderScale = 0.75f;
```

**Optimizaciones de carga y rendimiento de código:**

- `LevelManager` inicia `LoadSceneAsync` con `allowSceneActivation = false` antes de la animación del menú. La escena se activa cuando la animación termina Y el progreso llega al 90%, solapando I/O con animación y eliminando el freeze de carga.
- `Application.backgroundLoadingPriority = ThreadPriority.Low` durante la carga; pasa a `BelowNormal` en gameplay.
- `GridBuilder`: propiedad `_navMesh` con lazy-init + `cachedTileSlots` para evitar `GetComponent` en cada acceso por tile.
- `LevelSetup.Start()`: `yield return null` entre operaciones pesadas distribuye el trabajo en múltiples frames.
- `UI`, `BuildManager`, `TileAnimator`: campo `public static instance` en `Awake()`. Elimina 60-150 búsquedas `FindFirstObjectByType` O(n) por frame de activación de escena (una por `BuildSlot`, por escena).
- `MobileBootstrap`: `GCSettings.LatencyMode = LowLatency`, `Time.maximumDeltaTime = 0.05f`, `QualitySettings.antiAliasing = 0`, `cam.allowHDR = false`, `cam.allowMSAA = false`.
- `PhysicsRaycaster` añadido programáticamente a la cámara principal en `MobileBootstrap.ApplyCameraSettings()` para garantizar que el EventSystem puede despachar eventos táctiles a objetos 3D.

**Documentación:**

Se agrega a la sección 7.4 del GDD la tabla completa de configuración de importación de audio (formato, loadType, preloadAudioData, forceToMono por tipo de clip), texturas (ETC2 RGBA8, tamaños, mipmaps, justificación vs ASTC) y materiales (shader, render type, configuración). Se crea la sección 7.5 con las técnicas evaluadas y no aplicadas, cada una con justificación técnica.

---

- APK Android (build release) — enlace Google Drive: *(completar)*
- Proyecto Unity: `C:\Proyectos\You-Shall-Not-Pass`
- Este GDD (`Docs/GDD_You_Shall_Not_Pass.md`)
- High Concept (`Docs/HighConcept_You_Shall_Not_Pass.md`)
