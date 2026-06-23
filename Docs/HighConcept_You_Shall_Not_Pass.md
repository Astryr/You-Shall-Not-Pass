HIGH CONCEPT

"You Shall Not Pass!"
Integrantes:

Herrera, Oriana.
Muiños, Guadalupe.
Lima, Thiago.
Jorge, Santino.

Plataforma de destino: Mobile (Android). iOS queda como posible extensión futura.
Dispositivo de referencia para pruebas: TCL 408 (720 x 1600, gama baja).
Motor gráfico: Unity 3D (Universal Render Pipeline).
Género: Tower Defense / Estrategia.

---

Resumen del juego

"You Shall Not Pass!" es un tower defense ambientado en un mundo post-apocalíptico. El jugador debe defender su base central, el castillo (Núcleo), de oleadas de robots de chatarra. Usando un sistema de grilla, coloca distintos tipos de torretas, administra su economía y resiste rondas de dificultad creciente a lo largo de tres niveles.
Cada nivel comienza con una cantidad limitada de recursos, lo que obliga al jugador a planificar bien dónde construir. Al eliminar enemigos gana más chatarra para ampliar su defensa. Si demasiados enemigos alcanzan el castillo, el jugador pierde.

---

Game Loop

Fase de preparación: El jugador usa chatarra (moneda del juego) para construir torretas en los slots disponibles del mapa. Puede mover la cámara y hacer zoom para revisar el terreno.
Fase de oleada: El jugador inicia la oleada manualmente con el botón "FORCE WAVE". Los enemigos aparecen en portales fijos, siguen un camino hacia el castillo y las torretas atacan automáticamente.
Fase de recompensa / pausa: Al terminar la oleada, el jugador recibe chatarra por enemigos eliminados y puede volver a construir antes de la siguiente ronda.
Victoria o derrota: Si completa todas las oleadas del nivel, gana. Si el contador de amenaza (Threat) llega a cero, pierde.

---

Integrantes del equipo y roles

Rol                 | Integrantes
--------------------|--------------------------------------------
Project Manager     | Herrera, Oriana
Game Designer       | Lima, Thiago y Muiños, Guadalupe
Programadores       | Jorge, Santino
Artistas 3D         | Herrera, Oriana y Muiños, Guadalupe
QA / Audio          | Lima, Thiago

---

Contenido implementado

Niveles: 3 (Level 1, Level 2, Level 3), con más oleadas y variedad de enemigos en cada uno. El Nivel 1 incluye tutorial obligatorio al entrar por primera vez y un botón "?" para releerlo en cualquier momento.

Torretas: Ballesta (Crossbow), Cañón, Ametralladora, Martillo, Nido de araña, Arpón antiaéreo, Ventilador, entre otras. Algunas se desbloquean según el nivel.

Enemigos: Básico, rápido, pesado (con escudo), enjambre (swarm), sigiloso, volador, jefe volador, jefe araña y unidades generadas por jefes.

Interfaz: Menú principal, selección de nivel, HUD in-game (vida/amenaza, moneda, botón de oleada), pantalla de carga entre menú y nivel, pantallas de victoria y derrota, tutorial en el Nivel 1, panel "How to Play" en el menú, pantalla de créditos con nombres del equipo, ajustes de audio y sensibilidad de cámara.

Contador de FPS: Visible en todo momento en el centro-derecha de la pantalla (verde ≥ 60, amarillo 45-59, rojo < 45). Persiste en menú, niveles y pantalla de carga.

---

Técnicas de optimización implementadas

Modelos y geometría

Los modelos 3D se mantienen con geometría ordenada, evitando caras duplicadas y detalle innecesario. La regla del equipo es simple: si no se ve en pantalla, no debería consumir recursos. Se evitan ngons en la preparación de assets para no forzar conversiones extra en el motor.

Texturas y draw calls

Se usa atlas de sprites para iconos de torretas (Sprite Atlas). Las texturas de interfaz se comprimen en formato ETC2 para Android, con tamaños máximos de 512 px (UI) y 1024 px (resto), siempre en potencias de 2 cuando corresponde, para un uso correcto de mipmaps y menor consumo de memoria.

Iluminación

En cada nivel se conserva una única luz direccional (la más intensa del nivel) y se desactivan las luces puntuales y spot, que son las más costosas para la GPU en mobile. Los materiales del terreno usan URP Lit Shader junto a esa luz direccional para mantener buena calidad visual sin overhead adicional. En Android se usa el perfil de calidad "Performant" del pipeline URP, con distancia de sombras reducida a 15 m y escala de render al 85%.

Culling y objetos estáticos

Suelo, muros, caminos y estructuras fijas se marcan como estáticos para ayudar al motor a dibujar solo lo visible. No se unifica todo el mapa en una sola malla gigante, para no perder las ventajas del frustum culling.

Build y configuración Android

Proyecto configurado con IL2CPP, arquitectura ARM64, compresión de assets, minificación en release y herramienta interna de optimización (AndroidOptimizer) para texturas, audio y ajustes de build. El script MobileBootstrap aplica en el dispositivo al iniciar: objetivo de 90 FPS (permite superar 60 si el hardware lo permite sin cap artificial), calidad Performant, LOD bias 0.7 para activar modelos de baja poli antes, solver de física reducido a 4 iteraciones (en lugar del default de 6) para bajar la carga de CPU por FixedUpdate, y SustainedPerformanceMode activo para rendimiento estable en partidas largas.

---

Explicación de las mecánicas

Oleadas y movimiento

Los enemigos spawnean desde portales y avanzan por un camino definido hacia el castillo. Usan NavMesh (malla de navegación precalculada) combinada con waypoints, para moverse de forma fluida sin salirse del recorrido. Todos los enemigos, incluidos los voladores, siguen el camino del suelo para que las torretas tengan tiempo de atacarlos. Las oleadas no arrancan solas: el jugador las inicia con "FORCE WAVE".

Disparo y spawn

Las torretas detectan enemigos en su rango y disparan automáticamente. No se crean ni destruyen objetos en tiempo real de forma libre: enemigos, proyectiles y efectos usan Object Pooling. Los objetos se reutilizan de un pool, lo que reduce tirones de memoria durante oleadas intensas.

Economía e interfaz

Cada nivel empieza con una cantidad fija de chatarra. Cada enemigo derrotado otorga una recompensa según su tipo. La UI se actualiza cuando cambian vida, moneda u oleada, no en cada frame sin necesidad. En móvil, la orientación se bloquea en landscape y se usan Canvas Scalers por escena. Al terminar la partida (victoria o derrota), se ocultan los demás elementos de interfaz y solo queda visible la pantalla final.

Cámara

En PC: teclado y mouse (movimiento, rotación, zoom con rueda). En móvil: un dedo para mover, dos dedos para zoom (pinch). Los controles se activan y desactivan según la fase del juego (menú vs. nivel).

Audio

La música de fondo (BGM) se carga en modo streaming con loadInBackground activo para no bloquear el hilo principal ni ocupar RAM innecesaria. Los efectos de sonido cortos (SFX de UI y combate de baja duración) se comprimen en formato ADPCM con preloadAudioData activado, lo que garantiza decodificación instantánea y sin stutters en el primer uso. Los SFX más largos usan Vorbis. Todos los SFX se fuerzan a mono en Android para reducir el consumo de memoria a la mitad. La música y los efectos son ajustables por separado a través del AudioMixer.

---

Bitácora de desarrollo

Etapa                        | Actividades realizadas
-----------------------------|-----------------------------------------------------------
Preproducción                | Definición del concepto, roles del equipo, game loop base, elección de Unity y plataforma mobile.
Prototipo inicial            | Grilla de construcción, colocación de torretas, spawn básico de enemigos, economía simple y pantalla de juego.
Core gameplay                | Sistema de oleadas (WaveManager), gestión global del nivel (GameManager), castillo con contador de amenaza (Threat), victoria/derrota, tres niveles con dificultad progresiva.
Enemigos y torretas          | Varios tipos de enemigos con comportamientos distintos (escudo, sigilo, vuelo, jefes). Torretas con proyectiles y pooling. Ajuste de vida, velocidad y recompensas por tipo.
Interfaz y flujo             | Menú principal, selección de nivel, tutorial solo en Nivel 1, How to Play, botón manual de oleada, eliminación del temporizador automático, pantallas de fin de partida aisladas del resto del HUD.
Optimización mobile          | Object pooling generalizado, reducción de consultas pesadas en runtime, compresión de texturas y audio, perfil URP Performant, script de arranque mobile (MobileBootstrap), herramienta de optimización Android en el editor, pruebas orientadas al TCL 408.
Corrección de bugs           | Fix de EventSystem duplicado al cargar niveles, interacción con slots de construcción, zoom de cámara, oleadas que terminaban antes de tiempo, balance de economía y amenaza del castillo, rutas NavMesh de enemigos voladores.
Build Android                | Migración del proyecto a ruta ASCII (C:\Proyectos\You-Shall-Not-Pass), configuración IL2CPP/ARM64, resolución de errores de Gradle y URP, generación exitosa de APK para prueba en dispositivo.
Corrección docente (27/05)   | Pantalla de carga (UI_LoadingScreen), música y SFX activos (AudioManager), orientación landscape bloqueada, tutorial integrado en Nivel 1, optimización de luces con LevelEnvironmentOptimizer (una luz direccional + desactivación de puntuales/spot).
Entrega final (23/06)        | Auditoría completa de 80 scripts con corrección de null-checks críticos (GameManager, LevelSetup, UI_Pause, TowerPreview, RadiusDisplay, Tower_Harpoon, Waypoint, SelfRemoveToPool); corrección de race condition entre proyectil de arpón y pool de enemigos; optimización de audio Android (ADPCM para SFX cortos, Vorbis streaming para BGM, loadInBackground, preloadAudioData); contador de FPS persistente con código de color; targetFrameRate elevado a 90; reducción de solver de física y distancia de sombras en Android.
Estado actual (23/06/2026)   | Juego listo para entrega final. Tres niveles funcionales, sin errores inhabilitantes, APK generado y probado en TCL 408, contador de FPS estable en verde (≥ 60 FPS) durante toda la partida.
