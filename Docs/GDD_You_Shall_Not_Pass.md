# GDD - You Shall Not Pass!

**Versión:** 1.1 (post-corrección)  
**Fecha:** 27/05/2026  
**Equipo:** Herrera Oriana, Muiños Guadalupe, Lima Thiago, Jorge Santino  
**Plataforma:** Android (dispositivo de referencia: TCL 408, 720x1600)  
**Motor:** Unity 3D + Universal Render Pipeline (URP)

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
|--------|-----|-------|
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
| Compresión Android | BGM en streaming (Vorbis); SFX cortos comprimidos y mono |

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
- Sin luces dinámicas en niveles: se desactivan y el terreno usa materiales **Unlit** (sin depender de iluminación).
- `LevelEnvironmentOptimizer` aplica cambios al cargar cada nivel.
- Herramienta de editor: `Tools → Android Optimizer → 7. Convert Tile Materials to Unlit`.
- Bloom desactivado en Android (`MobileBootstrap`).
- Texturas UI/3D comprimidas ETC2; atlas de iconos de torretas.

### Código y runtime
- **Object Pooling** para enemigos, proyectiles y VFX (`ObjectPoolManager`).
- Pools reducidos en Android (menos RAM).
- Raycasts de enemigos limitados a cada N frames.
- `FindObjectOfType` cacheado donde impacta (oleadas, build slots).
- Cancelación de `InvokeRepeating` en objetos pooled.

### Build Android
- IL2CPP, ARM64, OpenGL ES 3, minify release.
- Proyecto en ruta ASCII: `C:\Proyectos\You-Shall-Not-Pass`.
- Package: `com.Astryr.YouShallNotPass`.

### UI
- Canvas Scaler a 1600x720 (landscape, TCL 408).
- Pantalla de carga durante build del nivel.
- Rotación bloqueada en landscape.

---

## 8. Scripts clave para demostración

| Script | Función |
|--------|---------|
| `ObjectPoolManager.cs` | Reutilización de enemigos y proyectiles |
| `WaveManager.cs` | Oleadas manuales y spawn |
| `GameManager.cs` | Economía, Threat, victoria/derrota |
| `LevelEnvironmentOptimizer.cs` | Sin luces dinámicas + materiales unlit |
| `UI_LoadingScreen.cs` | Pantalla de carga |
| `MobileBootstrap.cs` | Ajustes Android al iniciar |
| `AudioManager.cs` | Música y efectos |
| `AndroidOptimizer.cs` | Optimización de assets en editor |

---

## 9. Bitácora de desarrollo

| Fecha / fase | Hitos |
|--------------|-------|
| Preproducción | Concepto, roles, game loop, elección Unity mobile |
| Prototipo | Grilla, torretas, enemigos básicos, economía |
| Core gameplay | Oleadas, castillo/Threat, 3 niveles, FORCE WAVE |
| Balance | Vida de enemigos, recompensas, currency por nivel |
| Optimización v1 | Pooling, URP Performant, AndroidOptimizer, build APK |
| Corrección docente (27/05) | Pantalla de carga, audio activo, portrait lock, tutorial Nivel 1, luces off + unlit, GDD actualizado |

---

## 10. Entregables

- APK Android (build release)
- Proyecto Unity: `C:\Proyectos\You-Shall-Not-Pass`
- Repositorio: `github.com/Astryr/You-Shall-Not-Pass`
- Este GDD (`Docs/GDD_You_Shall_Not_Pass.md`)
- Video de demostración con scripts y prueba en TCL 408
