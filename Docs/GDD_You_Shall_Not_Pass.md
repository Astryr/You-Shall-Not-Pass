# GDD - You Shall Not Pass!

**Versión:** 1.2 (entrega final)  
**Fecha:** 23/06/2026  
**Plataforma:** Android (dispositivo de referencia: TCL 408, 720x1600)  
**Motor:** Unity 3D + Universal Render Pipeline (URP)

---

## Integrantes del equipo

| Nombre | Rol |
|--------|-----|
| Herrera Oriana | Diseño y programación |
| Muiños Guadalupe | Diseño y programación |
| Lima Thiago | Programación |
| Jorge Santino | Programación |

---

## 1. High Concept

Tower defense post-apocalíptico. El jugador defiende el castillo (Núcleo) contra oleadas de robots de chatarra usando torretas sobre una grilla. Tres niveles con dificultad progresiva. El Nivel 1 funciona como tutorial: muestra instrucciones al entrar y enseña controles, economía y oleadas manuales.

---

## 2. Game Loop

1. **Preparación:** el jugador coloca torretas con recursos limitados.
2. **Oleada:** inicia con el botón FORCE WAVE. Enemigos avanzan por el camino; las torretas disparan solas.
3. **Recompensa:** al limpiar la oleada, gana chatarra por enemigos eliminados y prepara la siguiente ronda.
4. **Fin:** victoria al completar todas las oleadas; derrota si el Threat (vida del castillo) llega a cero.

---

## 3. Controles

| Acción | PC | Móvil |
|--------|----|-------|
| Mover cámara | WASD / clic central | Un dedo |
| Zoom | Rueda del mouse | Pinch (dos dedos) |
| Construir torre | Clic en casilla | Tocar casilla |
| Iniciar oleada | FORCE WAVE | FORCE WAVE |
| Ayuda | Botón ? / How to Play | Igual |

**Orientación:** solo horizontal (landscape). La rotación a vertical está bloqueada para evitar que la UI se rompa.

---

## 4. Audio y musicalización

| Tipo | Implementación |
|------|----------------|
| Música de menú | `AudioManager` track 0, loop |
| Música de nivel | `AudioManager` track 1 al cargar nivel |
| SFX UI | Hover y click en botones |
| SFX combate | Disparos de torretas vía `AudioManager.PlaySFX` |
| Ajustes | Sliders de BGM y SFX en menú de opciones (AudioMixer) |
| Compresión Android | BGM: Vorbis streaming + `loadInBackground: 1` (no bloquea hilo principal); SFX cortos: ADPCM + `preloadAudioData: 1` (respuesta inmediata); SFX largos: Vorbis + `preloadAudioData: 1`; todos los SFX en mono |

---

## 5. Pantalla de carga

Mientras el nivel se construye en tiempo real (animación de tiles), se muestra un overlay con mensaje y barra de progreso. Cubre:

- Transición menú → nivel
- Animación de grilla (`TileAnimator`)
- Setup de `LevelSetup` (economía, oleadas, tutorial)

Scripts: `UI_LoadingScreen`, `LevelManager`, `LevelSetup`.

---

## 6. Tutorial e instrucciones

- **Nivel 1:** overlay de tutorial al entrar (`showTutorialIfFirstTime` + `ShowOnLevelStart`).
- **Durante partida:** botón ? en el HUD reabre el tutorial.
- **Menú principal:** panel How to Play (`UI_HowToPlay`).

---

## 7. Optimización técnica (implementada)

### Gráficos
- Perfil URP **Performant** en Android; render scale 0.85.
- Luces en niveles: se conserva **una sola luz direccional** (la más intensa del nivel) para mantener la calidad visual, y se desactivan las luces puntuales/spot que son costosas en rendimiento.
- `LevelEnvironmentOptimizer` aplica cambios al cargar cada nivel (`DisableExpensiveLights`, `ConfigureMainDirectionalLight`).
- Los materiales del terreno permanecen en modo **Lit** (URP Lit Shader) porque garantizan mejor calidad visual con la luz direccional única.
- Bloom activo (efecto visual limitado con URP Performant).
- Texturas UI/3D comprimidas ETC2; atlas de iconos de torretas.
- Distancia de sombras reducida a 15m en Android (`QualitySettings.shadowDistance`).
- LOD bias 0.7 en Android para activar modelos de baja poli antes (`QualitySettings.lodBias`).

### Código y runtime
- **Object Pooling** para enemigos, proyectiles y VFX (`ObjectPoolManager`).
- Pools reducidos en Android (menos RAM).
- Raycasts de enemigos limitados a cada N frames.
- `FindObjectOfType` cacheado donde impacta (oleadas, build slots).
- Cancelación de `InvokeRepeating` en objetos pooled.
- Solver de física reducido de 6 a 4 iteraciones (`Physics.defaultSolverIterations`).
- `targetFrameRate = 90` con `vSyncCount = 0` para permitir exceder 60fps y evitar throttling.
- `SustainedPerformanceMode` activo en Android para rendimiento estable en partidas largas.

### Build Android
- IL2CPP, ARM64, OpenGL ES 3, minify release.
- Proyecto en ruta ASCII: `C:\Proyectos\You-Shall-Not-Pass`.
- Package: `com.Astryr.YouShallNotPass`.
- Min SDK 25 (Android 7.1) / Target SDK 34 (Android 14).

### UI
- Canvas Scaler configurado por escena (reference resolution de los prefabs de Canvas en el proyecto).
- Pantalla de carga durante build del nivel.
- Rotación bloqueada en **landscape** (`ProjectSettings` + `MobileBootstrap`): `allowedAutorotateToPortrait: 0`, `allowedAutorotateToLandscapeRight: 1`, `allowedAutorotateToLandscapeLeft: 1`.
- Contador de FPS persistente (centro-derecha de pantalla) con código de color: verde ≥60, amarillo 45-59, rojo <45.
- Nombres del equipo visibles en la sección Credits del menú principal.

---

## 8. Scripts clave para demostración

| Script | Función |
|--------|---------|
| `ObjectPoolManager.cs` | Reutilización de enemigos y proyectiles |
| `WaveManager.cs` | Oleadas manuales y spawn |
| `GameManager.cs` | Economía, Threat, victoria/derrota |
| `LevelEnvironmentOptimizer.cs` | Optimización de luces (deshabilita puntuales/spot, conserva direccional) |
| `UI_LoadingScreen.cs` | Pantalla de carga |
| `MobileBootstrap.cs` | Ajustes Android al iniciar (FPS, orientación, física, calidad) |
| `AudioManager.cs` | Música y efectos de sonido |
| `FPSCounter.cs` | Contador de FPS persistente en pantalla |

---

## 9. Bitácora de desarrollo

| Fecha / fase | Hitos |
|--------------|-------|
| Preproducción | Concepto, roles, game loop, elección Unity + URP mobile |
| Prototipo | Grilla de tiles, torretas básicas, enemigos, economía |
| Core gameplay | Sistema de oleadas (`WaveManager`), castillo/Threat, 3 niveles, botón FORCE WAVE |
| Balance | Ajuste de vida de enemigos, recompensas y currency por nivel |
| Optimización v1 | Object pooling (`ObjectPoolManager`), URP Performant, `AndroidOptimizer`, build APK inicial |
| Corrección docente (27/05) | Pantalla de carga (`UI_LoadingScreen`), música y SFX activos (`AudioManager`), orientación landscape bloqueada, tutorial en Nivel 1, optimización de luces con `LevelEnvironmentOptimizer`, GDD v1.1 |
| Entrega final (23/06) | Auditoría completa de 80 scripts; null-checks críticos en `GameManager`, `LevelSetup`, `UI_Pause`, `TowerPreview`, `RadiusDisplay`, `Tower_Harpoon`, `Waypoint`, `SelfRemoveToPool`; fix race condition arpón vs. pool de enemigos (`Projectile_Harpoon` + `Enemy.SlowEnemy`); optimización audio Android (ADPCM + Vorbis streaming + `loadInBackground`); contador FPS persistente (`FPSCounter`); `targetFrameRate = 90` + reducción de solver de física y sombras; GDD v1.2 |

---

## 10. Entregables

- APK Android (build release) — enlace Google Drive: *(completar al subir)*
- Proyecto Unity: `C:\Proyectos\You-Shall-Not-Pass`
- Repositorio: `github.com/Astryr/You-Shall-Not-Pass`
- Este GDD (`Docs/GDD_You_Shall_Not_Pass.md`)
- Video de demostración con scripts y prueba en TCL 408
