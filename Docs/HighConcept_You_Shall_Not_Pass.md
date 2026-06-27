HIGH CONCEPT — You Shall Not Pass!
Versión 2.2 — Entrega Final — 26/06/2026

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
    Contador de FPS permanente (verde ≥60 / amarillo 45-59 / rojo <45).

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

    ADPCM + Decompress On Load (SFX cortos: ui_click_1, ui_onHover_1, sfx_beam_2)
    Para sonidos de UI que se disparan al toque del usuario, la latencia de
    decodificación importa. ADPCM decodifica en ~1 ms; Vorbis necesita 5-10 ms.
    preloadAudioData = 1 asegura que el clip esté descomprimido en RAM desde
    el inicio, sin ningún delay al reproducir.
    forceToMono = 1: los SFX de UI no necesitan información estéreo.
    Reducir a mono divide el tamaño en RAM a la mitad.

    Vorbis + Decompress On Load (SFX medianos: sfx_beam_1, ui_click_2, ui_onHover_2)
    Para clips > 30 KB, Vorbis ofrece mejor ratio de compresión que ADPCM
    con calidad perceptiblemente igual. preloadAudioData = 1 y forceToMono = 1.

  TEXTURAS:
    ETC2 (RGBA8) para todas las texturas Android: formato nativo de OpenGL ES 3.0,
    decompresión en GPU sin overhead de CPU. Resolución máxima 1024 px para
    modelos 3D y 512 px para UI (la UI no se acerca/aleja, no necesita mipmaps).

    Sprite Atlas: todos los iconos de las 7 torretas están en un único atlas.
    Esto reduce los draw calls del panel de construcción de 7 a 1.

  MODELOS 3D:
    Arte low-poly con geometría limpia (sin caras duplicadas, sin ngons).
    Texturas de 512×512 por personaje/enemigo. Un solo material por tipo
    de enemigo para aprovechar GPU instancing y minimizar state changes.

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

  Estado actual                 Juego listo para entrega final. Tiles de
  26/06/2026                    construcción operativos en Android, tres
                                niveles funcionales, sin bugs inhabilitantes,
                                FPS estables en verde (≥60) en TCL 408.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

ENTREGABLES

  - APK Android (build release)          → Google Drive: (completar con link)
  - Proyecto Unity                       → C:\Proyectos\You-Shall-Not-Pass
  - GDD completo                         → Docs/GDD_You_Shall_Not_Pass.md
  - Este High Concept                    → Docs/HighConcept_You_Shall_Not_Pass.md
