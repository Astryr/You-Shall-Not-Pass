HIGH CONCEPT — You Shall Not Pass!
Versión 2.5 — Entrega Final — 14/07/2026

Integrantes:
  Herrera, Oriana    — Project Manager · Artista 3D
  Muiños, Guadalupe  — Game Designer · Artista 3D
  Lima, Thiago       — Game Designer · QA · Audio
  Jorge, Santino     — Programador

Plataforma de destino:  Mobile (Android). iOS como extensión futura posible.
Dispositivo de referencia: TCL 408 (720×1600 px, gama baja, ~2 GB RAM).
Motor gráfico: Unity 6000.3.11f1 + Universal Render Pipeline (URP).
Género: Tower Defense / Estrategia en tiempo real.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

RESUMEN DEL JUEGO

"You Shall Not Pass!" es un tower defense ambientado en un mundo post-apocalíptico.
El jugador defiende el castillo (Núcleo) de oleadas de robots de chatarra colocando
distintos tipos de torretas sobre una grilla táctica. Tres niveles con dificultad
progresiva; el Nivel 1 funciona como tutorial interactivo obligatorio.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

GAME LOOP

  1. PREPARACIÓN
     El jugador recibe una cantidad fija de chatarra al iniciar el nivel.
     Puede mover la cámara, hacer zoom e inspeccionar el mapa antes de colocar
     torres en los slots habilitados de la grilla.

  2. OLEADA (FORCE WAVE)
     El jugador inicia la oleada manualmente. No existe temporizador automático:
     se le da control total para no penalizar a jugadores más lentos o con
     dispositivos menos potentes. Los enemigos aparecen en portales y siguen
     el camino hacia el castillo; las torretas disparan automáticamente.

  3. RECOMPENSA
     Al eliminar enemigos se obtiene chatarra proporcional a su tipo. Ese recurso
     se puede usar para construir más torres antes de la siguiente oleada.

  4. FIN DE PARTIDA
     Victoria: completar todas las oleadas del nivel.
     Derrota: el contador Threat llega a 0 (demasiados enemigos alcanzaron el castillo).

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

INTEGRANTES Y ROLES

  Rol                    Integrante(s)
  ─────────────────────  ───────────────────────────────────
  Project Manager        Herrera, Oriana
  Game Designer          Lima, Thiago · Muiños, Guadalupe
  Programador            Jorge, Santino
  Artista 3D             Herrera, Oriana · Muiños, Guadalupe
  QA / Audio             Lima, Thiago

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

CONTENIDO IMPLEMENTADO

  NIVELES: 3
  ┌─────────┬─────────┬──────────────────────────────────────────────────┐
  │ Nivel   │ Oleadas │ Descripción                                      │
  ├─────────┼─────────┼──────────────────────────────────────────────────┤
  │ Level 1 │ 3       │ Tutorial automático. Básico + Rápido.            │
  │ Level 2 │ 5       │ Mayor densidad. Pesado + Enjambre.               │
  │ Level 3 │ 7       │ Dificultad máxima. Sigilosos + Voladores + Jefes │
  └─────────┴─────────┴──────────────────────────────────────────────────┘

  TORRETAS: 7
    Ballesta, Cañón, Ametralladora, Martillo, Nido de Araña,
    Arpón Antiaéreo, Ventilador.
    Algunas torretas se desbloquean por nivel (configurado en LevelSetup).

  ENEMIGOS: 9
    Básico, Rápido, Pesado (escudo), Enjambre, Sigiloso, Volador,
    Jefe Volador, Jefe Araña, Unidad de Jefe.

  INTERFAZ:
    Menú principal, selección de nivel, pantalla de carga con barra de
    progreso, HUD in-game (Threat, moneda, oleada, FORCE WAVE), tutorial
    interactivo, botón de ayuda (?), pausa, pantallas de victoria/derrota,
    créditos, ajustes de audio y sensibilidad.
    Contador de FPS permanente en el costado izquierdo de la pantalla
    (verde ≥60 / amarillo 45-59 / rojo <45). Posicionado a la izquierda
    para que la cámara frontal del teléfono (derecha en landscape) no lo tape.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

TÉCNICAS DE OPTIMIZACIÓN — JUSTIFICACIONES TÉCNICAS

──────────────────────────────────────────────────────────────
A) OPTIMIZACIÓN EN ENGINE / BUILD
──────────────────────────────────────────────────────────────

  IL2CPP + ARM64
  El backend de scripting IL2CPP compila C# a C++ nativo antes de la build.
  En Android ARM64 (arquitectura del TCL 408), esto mejora el rendimiento
  de ejecución un 20-30 % respecto a Mono. Se eligió ARM64 exclusivamente
  porque es la arquitectura de todos los dispositivos Android modernos y
  eliminar x86/ARMv7 reduce el tamaño del APK.

  Carga asíncrona de escenas con pre-carga paralela (v2.0 → v2.1)
  Antes de la v2.0, la escena empezaba a cargarse DESPUÉS de la animación
  del menú — freeze de 1-3 s visible con FPS cayendo a ~2 en el TCL 408.

  Solución v2.0 (LevelManager.cs):
    1. LoadSceneAsync se llama inmediatamente con allowSceneActivation = false.
    2. backgroundLoadingPriority = ThreadPriority.Low durante la carga.
    3. Animación del menú + lectura del disco ocurren en paralelo.
    4. Al terminar la animación + escena al 90% → se activa la escena.
    5. LevelSetup.Start() distribuye trabajo con "yield return null".
  Resultado: FPS durante carga mejoró de ~2 a ~24.

  Eliminación del spike de activación de escena (v2.1)
  Causa raíz del 24 FPS restante: cada BuildSlot.Awake() llamaba 3 veces
  a FindFirstObjectByType (búsqueda O(n) sobre todos los objetos activos).
  Con 20-50 BuildSlots por nivel, el frame de activación ejecutaba 60-150
  búsquedas costosas en un solo frame.

  Solución (BuildSlot.cs, BuildManager.cs, TileAnimator.cs, UI.cs):
    - Se añadió "public static instance" a BuildManager, TileAnimator y UI.
    - BuildSlot.Awake() reemplazado: ahora solo asigna defaultPosition.
    - Los managers se acceden como UI.instance, BuildManager.instance, etc.
    - Cada acceso es O(1) sin ninguna búsqueda.

  Configuraciones adicionales (MobileBootstrap.cs v2.1):
    - GCSettings.LatencyMode = LowLatency: el runtime .NET prioriza pausas
      cortas de GC en lugar de throughput máximo. Evita hitches de 20-50 ms
      durante oleadas intensas causados por colecciones completas de GC.
    - Time.maximumDeltaTime = 0.05f: si un frame tarda más de 50 ms,
      la física y animaciones reciben máximo 50 ms de delta. Evita que
      objetos físicos "salten" durante el frame de activación de la escena.
    - QualitySettings.antiAliasing = 0 + cam.allowMSAA = false: MSAA
      resuelve múltiples samples por pixel; en gama baja el costo de memoria
      de framebuffer supera el beneficio visual.
    - cam.allowHDR = false: HDR requiere render targets de 16-32 bits por
      canal, aumentando el uso de memoria de GPU en el TCL 408.

    GC Incremental (ProjectSettings)
    - gcIncremental: 1 ya estaba habilitado: el recolector de Unity
      distribuye el trabajo de GC en slices por frame en lugar de pausar.

  Minify Release + Managed Stripping (Low)
  Reduce el tamaño del APK eliminando bytecode no referenciado. El nivel
  "Low" de stripping es suficiente para no romper reflexión ni serializadores.

  Render Scale reducido a 0.75 (v2.4 — URP-Performant.asset + MobileBootstrap.cs)
  El docente indicó explícitamente reducir el Render Scale para alcanzar 60 FPS
  de manera consistente. Cambio aplicado de dos formas complementarias:
    1. En el asset URP-Performant.asset: m_RenderScale = 0.75 (valor persistente).
    2. En MobileBootstrap.ApplySettings(): urpAsset.renderScale = 0.75f (runtime
       override; garantiza el valor aunque el asset en el build sea diferente).
  Efecto: el framebuffer se genera al 75% de la resolución del TCL 408
  (540×900 px efectivos en lugar de 720×1200), reduciendo el fill rate ~31%.

  Features URP desactivadas en Performant (v2.4):
    - Lens Flares (data-driven y screen-space): m_SupportDataDrivenLensFlare = 0,
      m_SupportScreenSpaceLensFlare = 0. Sin lens flares en el juego; desactivar
      elimina shader variants compilados innecesariamente.
    - Light Cookies: m_SupportsLightCookies = 0. Sin cookies en ninguna luz.
    - LOD Cross-fade: m_EnableLODCrossFade = 0. Sin transiciones dithered de LOD.
  Features ya desactivadas y verificadas en esta revisión:
    - Depth Texture: m_RequireDepthTexture = 0 ✓
    - Opaque Texture: m_RequireOpaqueTexture = 0 ✓
    - HDR: m_SupportsHDR = 0 ✓
    - Main Light Shadows: m_MainLightShadowsSupported = 0 ✓
    - Additional Lights: m_AdditionalLightsRenderingMode = 0 (Off) ✓

  Skybox eliminado en Android (v2.3 — MobileBootstrap.cs)
  Los niveles son entornos industriales cerrados; el fondo negro encaja con
  la estética post-apocalíptica. Al setear clearFlags = SolidColor + black y
  RenderSettings.skybox = null, se elimina el pase de renderizado del skybox
  (-1 drawcall), y Unity no necesita samplear la cubemap de ambiente en runtime.
  La iluminación bakeada no depende del skybox en runtime, así que no hay
  pérdida visual en objetos estáticos con lightmap.

  Far clip plane reducido: 1000 → 80 m (v2.3 — MobileBootstrap.cs)
  El área de juego de cualquier nivel no supera ~30 u de diámetro.
  Con la cámara hasta 16 u de altura, el far plane a 80 m cubre todo con margen.
  Reducir el far clip plane mejora la precisión del z-buffer (menos z-fighting)
  y reduce la carga de frustum culling al descartar antes la geometría lejana.

  Auto-rotación landscape (v2.3 — MobileBootstrap.cs)
  Antes: Screen.orientation = LandscapeRight (orientación fija).
  Ahora: Screen.orientation = AutoRotation + LandscapeLeft/Right habilitados.
  El jugador puede usar el teléfono con el cargador a cualquier lado sin que
  la interfaz quede al revés. Portrait deshabilitado en ambos casos.

  Zoom pinch calibrado (v2.3 — CameraController.cs)
  El multiplicador de sensibilidad del gesto pinch se redujo de ×0.01 a ×0.003.
  Antes, en un gesto de 200 px de delta el targetZoomDist saltaba 20 unidades
  (cubriendo todo el rango min-max de una sola vez). Con el nuevo valor salta
  ~6 unidades: suficiente para zoom rápido pero controlable para zoom fino.
  Además se añaden límites en runtime: minZoom ≥ 4, maxZoom ≤ 16.

──────────────────────────────────────────────────────────────
B) ILUMINACIÓN
──────────────────────────────────────────────────────────────

  LevelEnvironmentOptimizer.cs se ejecuta al cargar cada nivel:
    - Busca todas las luces de la escena.
    - Desactiva todas las luces puntuales y spot (costosas en móvil:
      cada punto de luz adiciona 1 draw call y sombras de 6 caras).
    - Conserva únicamente la luz direccional más intensa para mantener
      la calidad visual.
    - Asigna esa luz como RenderSettings.sun para que URP la reconozca.

  URP Performant (perfil Android):
    - Render Scale: 0.85 (renderiza a 85% de resolución, reescala antes de
      presentar en pantalla — menor carga de fill rate para la GPU).
    - Shadow Distance: 15 m (en runtime via QualitySettings.shadowDistance).
      Las sombras se calculan solo para objetos a menos de 15 m de la cámara.
    - Additional Lights: configurado en "Per Vertex" para el perfil mobile
      (mucho más barato que "Per Pixel").
    - LOD Bias: 0.7 en Android — los modelos de menor poligonaje se activan
      antes de lo normal, reduciendo la carga de GPU en oleadas intensas.

──────────────────────────────────────────────────────────────
C) FÍSICAS
──────────────────────────────────────────────────────────────

  Object Pooling (ObjectPoolManager.cs)
  Motivo: en oleadas con 20+ enemigos activos, instanciar y destruir objetos
  en cada spawn/muerte genera garbage collection (GC) que en el TCL 408
  se traduce en hitches de 20-50 ms. El pool pre-crea los objetos, los
  activa/desactiva en lugar de crearlos/destruirlos, y elimina el GC overhead.
  Cubre: enemigos, proyectiles de todas las torretas y VFX de impacto.

  Physics Solver reducido
  Physics.defaultSolverIterations = 4 (default Unity: 6).
  Reduce la carga del FixedUpdate ~30% sin afectar el gameplay (las colisiones
  del juego no requieren la precisión máxima del solver).
  Physics.defaultSolverVelocityIterations = 1 (ya es el mínimo por defecto,
  se establece explícitamente para garantizarlo).

  NavMesh pre-baked
  El navmesh de cada nivel está calculado en el editor y guardado en la escena.
  No se recalcula en runtime (salvo cuando se construye una torre, que bloquea
  una celda — actualización mínima localizada). Esto evita el freeze de
  BuildNavMesh() que en Unity puede durar 200-500 ms según la complejidad.

  Caché de componentes (GridBuilder.cs)
    - NavMeshSurface: antes era una property expression (=> GetComponent<>())
      que llamaba GetComponent en cada acceso. Ahora es un campo con lazy-init.
    - TileSlot: antes cada llamada a MakeTilesNonInteractable() hacía
      GetComponent<TileSlot>() para cada tile de la grilla. Ahora se cachea
      en List<TileSlot> en el primer acceso.

  Fix de race condition: Arpón vs Pool de enemigos (26/06/2026)
  El proyectil del arpón (Projectile_Harpoon) volaba hacia un enemigo que
  podía morir y volver al pool (SetActive false) antes del impacto.
  Al intentar AttachToEnemy() sobre un objeto inactivo:
    - StartCoroutine() tiraba "Coroutine on inactive object"
    - Los accesos a currentEnemy tiraban NullReferenceException
  Solución: verificar enemy.gameObject.activeInHierarchy en Update() y en
  AttachToEnemy(). Si el enemigo está inactivo, llamar tower.ResetAttack().

──────────────────────────────────────────────────────────────
D) MANEJO DE ASSETS
──────────────────────────────────────────────────────────────

  AUDIO — selección de formatos justificada:

    Vorbis + Streaming (BGM: bg_example_1/2/3.mp3)
    Los tracks de música duran 2-4 minutos. Cargarlos completos en RAM
    ocuparía 10-20 MB por track. Streaming lee el archivo en chunks pequeños;
    loadInBackground = 1 hace que la lectura ocurra en un thread secundario
    sin bloquear el frame del menú o el nivel.

    Vorbis + Decompress On Load (todos los SFX)
    Todos los archivos de efectos usan Vorbis con Decompress On Load: el clip
    se descomprime una sola vez al cargar la escena y queda en RAM como PCM.
    Esto garantiza latencia de reproducción mínima al toque del usuario.
    preloadAudioData = 1: el clip está listo antes del primer uso.
    forceToMono = 1: los SFX de UI no necesitan información estéreo;
    reducirlos a mono divide el tamaño en RAM a la mitad sin pérdida perceptible.

    Archivos SFX afectados:
    ui_click_1.mp3, ui_click_2.wav, ui_onHover_1.mp3, ui_onHover_2.wav,
    sfx_beam_1.mp3, sfx_beam_2.mp3

  TEXTURAS — configuración de importación:
    ETC2 (RGBA8) para todas las texturas Android (textureFormat: 47 en .meta):
    formato nativo de OpenGL ES 3.0+, decompresión directa en GPU sin overhead
    de CPU. Resolución máxima 1024 px para modelos 3D y 512 px para UI.
    MipMaps habilitados en texturas 3D para reducir aliasing; desactivados en
    UI (elementos 2D fijos en pantalla que no se alejan).

    Sprite Atlas: todos los iconos de las 7 torretas están en un único atlas.
    Esto reduce los draw calls del panel de construcción de 7 a 1.

  MATERIALES — shaders y configuración:
    Shader principal: Universal Render Pipeline/Lit (URP/Lit).
    Justificación: URP/Lit permite recibir iluminación bakeada (lightmaps).
    Los niveles tienen iluminación pre-calculada; sin Lit los modelos no
    mostrarían las sombras bakeadas y perderían toda la profundidad visual.

    Configuración optimizada aplicada a todos los materiales de juego:
    - Sin mapas de normales (_BumpMap vacío): ahorrar 1 texture sample por
      fragmento. El estilo low-poly no requiere detalle de relieve.
    - Sin reflexiones de entorno (_GlossyReflections = 0): evitar el sample
      de reflection probe por drawcall donde no aporta calidad visual.
    - Opaque render type (_Surface = 0) para todos los materiales de juego
      excepto Enemy_Transperent (sigiloso) y los de previsualización de torre.
      La cola opaque es procesada sin blending, mucho más eficiente en GPU.
    - GPU Instancing desactivado (m_EnableInstancingVariants = 0): los
      objetos estáticos usan Static Batching de Unity, que es más eficiente
      para geometría fija que el instancing manual.

    Materiales especiales:
    - Tiles_mat (1/2/3): sin textura, color sólido codificado en _BaseColor.
      Cero bytes de textura, cero texture samplers en el shader.
    - Emission_*.mat: color de emisión puro sin textura ni normal map.
      Usado para señalización visual (radio de ataque, waypoints).
    - Enemy_Transperent.mat: transparente, cola de render Transparent.
      Solo aplicado al enemigo sigiloso.

  MODELOS 3D:
    Arte low-poly con geometría limpia (sin caras duplicadas, sin ngons).
    Un solo material por tipo de enemigo para aprovechar batching y minimizar
    state changes en el render pipeline.

──────────────────────────────────────────────────────────────
E) ACCESIBILIDAD
──────────────────────────────────────────────────────────────

  Tutorial (Nivel 1):
    Aparece automáticamente al entrar. Contiene tres secciones claramente
    diferenciadas: OBJETIVO (qué es el Threat y cómo se pierde), CONTROLES
    (cómo mover la cámara, construir y lanzar oleadas) y CONSEJOS (economía
    y cómo reabrir el tutorial). Al cerrarse con "Entendido!" no vuelve a
    aparecer automáticamente (PlayerPrefs). Se puede reabrir con el botón ?
    en el HUD, que pausa el juego para que el usuario lo lea sin presión.

  HUD siempre informativo:
    Threat actual/máximo, recursos actuales, número de oleada y botón
    FORCE WAVE visibles en todo momento durante la partida. La información
    clave nunca está oculta.

  Señalización de slots de construcción:
    Los slots interactuables tienen feedback visual de hover (highlight)
    para que el usuario sepa qué puede tocar sin necesidad de instrucción previa.

  Feedback de sonido como refuerzo:
    Hover en botones → SFX de selección.
    Click en botones → SFX de confirmación.
    Disparos de torre → SFX por tipo.
    El juego es completamente jugable sin audio (el tutorial y las señales
    visuales son suficientes); el audio es refuerzo adicional, no requisito.

  Pantallas de fin claras:
    Victoria y derrota tienen overlays dedicados que ocultan el HUD para
    máxima legibilidad. El usuario no necesita interpretar el estado del juego.

  Controles ajustables:
    Sensibilidad de cámara configurable desde el menú de ajustes.
    Sliders separados de BGM y SFX en el menú de opciones.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

BITÁCORA DE DESARROLLO

  Fecha / Fase              Actividades
  ─────────────────────────────────────────────────────────────────────
  Preproducción             Concepto, roles, game loop base, elección Unity+URP
                            para mobile, análisis del dispositivo de referencia
                            (TCL 408).

  Prototipo                 Grilla de construcción con GridBuilder (editor tool),
                            primer spawn de enemigos, economía simple, pantalla
                            de juego básica.

  Core gameplay             WaveManager (oleadas configurables), GameManager
                            (Threat/moneda/victoria-derrota), tres niveles,
                            botón FORCE WAVE manual (se descartó temporizador
                            automático por razones de accesibilidad).

  Enemigos y torretas       9 tipos de enemigos con comportamientos distintos.
                            7 torretas con proyectiles. Object Pooling
                            implementado al detectar GC spikes de 30-50 ms en
                            oleadas intensas.

  Interfaz y flujo          Menú principal, selección de nivel, tutorial Nivel 1,
                            How to Play, pantallas de fin de partida, ajustes de
                            audio y sensibilidad.

  Corrección docente        Pantalla de carga (UI_LoadingScreen + barra de
  27/05/2026                progreso), AudioManager con BGM/SFX, orientación
                            landscape bloqueada, LevelEnvironmentOptimizer
                            (una luz direccional + desactivación de puntuales/spot).

  Auditoría de bugs         80 scripts revisados. Null-checks en GameManager,
  23/06/2026                LevelSetup, UI_Pause, TowerPreview, RadiusDisplay,
                            Waypoint, SelfRemoveToPool. Fix race condition arpón
                            vs pool de enemigos. Optimización de audio (ADPCM +
                            Vorbis streaming + loadInBackground). Contador FPS.
                            targetFrameRate = 90, solver física = 4 iteraciones.

  Opt. carga: 1ª ronda      Stutter detectado (~2 FPS) durante pantalla de
  26/06/2026                carga en TCL 408. Causas: escena cargaba tarde,
                            GetComponent sin caché en GridBuilder, operaciones
                            pesadas síncronas en LevelSetup.
                            Solución: LoadSceneAsync+allowSceneActivation=false
                            en paralelo con animación; ThreadPriority.Low;
                            NavMeshSurface y TileSlot cacheados; yield return null.
                            Resultado: 2 FPS → 24 FPS.

  Opt. carga: 2ª ronda      Causa raíz del 24 FPS identificada: BuildSlot.Awake()
  26/06/2026                llamaba 3 FindFirstObjectByType por tile (60-150 por
                            nivel). Solución: static instance en BuildManager,
                            TileAnimator y UI; acceso O(1) desde BuildSlot.
                            Además: GCLatencyMode=LowLatency, maximumDeltaTime=
                            0.05f, antiAliasing=0, HDR/MSAA desactivados en cámara.

  Bug crítico Android:          Síntoma: toque en tiles no abría menú de torres.
  construcción de torres         Causa capa 1: BuildSlot usa IPointerDownHandler
  26/06/2026                    que necesita PhysicsRaycaster para 3D objects.
                                Causa capa 2 (raíz real): Canvas activo con
                                GraphicRaycaster en pantalla hace que
                                IsPointerOverGameObject devuelva true para TODO
                                toque, bloqueando el fallback via Physics.Raycast
                                antes de que se ejecute.
                                Solución v2.3: BuildManager.Update reestructurado —
                                Physics.Raycast se ejecuta PRIMERO sin pasar por
                                IsPointerOverGameObject. Si golpea BuildSlot →
                                TriggerSelect(). Solo si no golpea BuildSlot se
                                consulta IsPointerOverGameObject para decidir si
                                cancelar. GetComponent→GetComponentInParent en
                                BuildManager y CameraController.

  Bug Android persistente:      Síntoma idéntico a fase anterior, pero tras v2.3
  causa raíz real               y v2.4. Análisis del historial git confirmó que
  27/06/2026                    PhysicsRaycaster NUNCA existió en ninguna escena.
                                Sin él, el EventSystem no puede llamar
                                OnPointerDown en objetos 3D (como los BuildSlots).
                                Todos los intentos previos de fallback via
                                BuildManager.Update() fallaron por distintas
                                razones (IsPointerOverGameObject bloqueaba, o
                                el layermask excluía la capa de los tiles).

                                Solución v2.5 (definitiva):
                                  MobileBootstrap.EnsurePhysicsRaycasterOnMainCamera()
                                  agrega PhysicsRaycaster a Camera.main en runtime
                                  (AfterSceneLoad). LevelSetup también lo llama
                                  como fallback al cargar cada nivel.
                                  Con PhysicsRaycaster presente:
                                  - OnPointerDown en BuildSlot es disparado ✓
                                  - IsPointerOverGameObject devuelve true para
                                    tiles 3D y UI (correcto en ambos casos)
                                  - CameraController no pan al tocar tiles ✓
                                  - BuildManager.Update() solo cancela cuando
                                    IsPointerOverGameObject = false (vacío) ✓
                                TriggerSelect(). Solo si no hay BuildSlot se
                                consulta IsPointerOverGameObject para decidir si
                                cancelar. GetComponentInParent para cubrir colliders
                                en hijos del tile.

  Bug Android persistente:      Síntoma idéntico a fase anterior. Análisis
  causa raíz real               de historial git confirmó que PhysicsRaycaster
  27/06/2026                    nunca existió en ninguna escena. Solución v2.5:
                                MobileBootstrap.EnsurePhysicsRaycasterOnMainCamera()
                                agrega PhysicsRaycaster en runtime (AfterSceneLoad).
                                Con él, EventSystem llama OnPointerDown en tiles 3D.

  Pulido final y correcciones   Zoom pinch reducido (×0.003), minZoom ≥ 4 /
  de cámara y UX                maxZoom ≤ 16. Rotación auto entre ambos landscape.
  12/07/2026                    FPS counter movido al costado izquierdo. Skybox
                                eliminado en Android (SolidColor negro). Far clip
                                plane reducido a 80 m. Bug de pan con
                                maxDistanceFromCenter=0 corregido (guard >0.01f).
                                Documentación actualizada: formatos de audio reales
                                (Vorbis, no ADPCM), materiales y shaders detallados.

  Corrección docente:           Tiles de construcción aún no respondían en Android
  URP + tiles Android           APK. Diagnóstico: BuildManager.Update() (v2.5) había
  13/07/2026                    eliminado el Physics.Raycast directo y confiaba
                                únicamente en PhysicsRaycaster + EventSystem, que
                                no funciona de forma consistente en Android builds.
                                Solución (v2.7): restablecer Physics.Raycast manual
                                → TriggerSelect() como camino PRIMARIO en
                                BuildManager.Update(). CameraController también usa
                                el mismo raycast como fallback para bloquear el pan
                                al tocar tiles.
                                URP-Performant actualizado por indicación del docente:
                                Render Scale 0.85→0.75, lens flares/light cookies/
                                LOD cross-fade desactivados. Render scale también
                                forzado en runtime via MobileBootstrap.

  Estado actual                 Juego listo para entrega final. Tiles de
  14/07/2026                    construcción operativos en Android (Physics.Raycast
                                directo, sin dependencia del EventSystem). Tres
                                niveles funcionales, sin bugs inhabilitantes, URP
                                optimizado al máximo según indicaciones del docente.
                                Revisión final completa: todos los puntos del docente
                                implementados y verificados. Sección 7.5 del GDD
                                documenta las 7 técnicas evaluadas y no aplicadas con
                                justificación técnica completa.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

ENTREGABLES

  - APK Android (build release)          → Google Drive: (completar con link)
  - Proyecto Unity                       → C:\Proyectos\You-Shall-Not-Pass
  - GDD completo                         → Docs/GDD_You_Shall_Not_Pass.md
  - Este High Concept                    → Docs/HighConcept_You_Shall_Not_Pass.md
