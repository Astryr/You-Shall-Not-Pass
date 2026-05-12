# You Shall Not Pass! — Documento de Desarrollo Técnico

**Materia:** Optimización de Videojuegos  
**Plataforma objetivo:** Android (TCL 408 — MTK Helio G25, 4 GB RAM, 720×1612, Android 12)  
**Motor:** Unity 6, Universal Render Pipeline (URP)

---

## 1. Descripción del proyecto

Tower defense 3D en el que el jugador construye torres sobre casillas del mapa para detener oleadas de enemigos que avanzan hacia su castillo. El objetivo es sobrevivir todas las oleadas; si los enemigos alcanzan el castillo, se pierden vidas (amenaza) y al llegar a cero, la partida termina en derrota.

---

## 2. Mecánica principal — Construcción de torres

**Script:** `BuildManager.cs` + `BuildSlot.cs` + `UI_BuildButton.cs`

### Flujo de colocación:
1. El jugador **toca una casilla** (`BuildSlot`) → `BuildManager.SelectBuildSlot()` guarda la referencia y abre el menú de torres.
2. El jugador **elige una torre** del menú → `BuildManager.BuildTower()`:
   - Valida que tenga suficientes recursos (`GameManager.HasEnoughCurrency`).
   - Descuenta el costo (`GameManager.SpendCurrency`).
   - Hace `Instantiate` de la torre sobre la casilla y la desactiva para construcción (`SetSlotAvalibleTo(false)`).
3. Si el jugador toca fuera o presiona Escape, `CancelBuildAction()` cierra el menú sin construir.

### Decisión de diseño:
Se usa `Physics.Raycast` con `ScreenPointToRay` para detectar toque/clic (compatible PC y móvil), con verificación de `EventSystem.IsPointerOverGameObject` para no capturar toques sobre la UI.

---

## 3. Sistema de enemigos

**Script principal:** `Enemy.cs`  
**Scripts relacionados:** `EnemyPortal.cs`, subclases (`Enemy_Fast.cs`, `Enemy_Heavy.cs`, etc.)

### Movimiento por waypoints con NavMeshAgent:
- Cada `EnemyPortal` tiene hijos tipo `Waypoint` que definen la ruta.
- Al instanciar un enemigo, `SetupEnemy(portal)` copia los waypoints, calcula la distancia total hasta la meta (`CollectTotalDistance`) y arranca el movimiento (`BeginMovement`).
- `Update()` verifica via `ShouldChangeWaypoint()` si el agente llegó lo suficientemente cerca del punto actual para pasar al siguiente.
- La rotación se maneja manualmente con `FaceTarget()` (suavizado con `Quaternion.Lerp`) porque `agent.updateRotation = false`.

### Object Pool (Físicas — Reciclado de objetos):
Se usa `UnityEngine.Pool.ObjectPool<GameObject>` en `ObjectPoolManager`.  
- Los enemigos, proyectiles y VFX se **reciclan** en lugar de destruirse/crearse en cada ola.
- Se precargan en `Start()` via corrutina repartida en frames (`yield return null`) para evitar un spike de CPU al inicio del nivel.
- Justificación: en mobile, `Instantiate`/`Destroy` frecuentes generan GC alloc que causa micro-stutters. El pool elimina esa presión de memoria.

---

## 4. Sistema de oleadas

**Script:** `WaveManager.cs`

- El `WaveManager` maneja un array de `WaveDetails`, cada uno con cantidad de enemigos por tipo y (opcionalmente) un nuevo `GridBuilder` que transforma el mapa.
- Puede ser configurado para oleadas automáticas (timer) o manuales (botón del jugador).
- Cuando todos los enemigos de una oleada son eliminados o retirados, `CheckIfWaveCompleted()` evalúa si quedan activos y, de no haberlos, pasa a la siguiente ola o llama a `GameManager.LevelCompleted()`.

---

## 5. Manejo de estado (GameManager)

**Script:** `GameManager.cs` (Singleton)

- Gestiona vidas (amenaza), moneda (recursos) y el estado ganado/perdido.
- Al perder todas las vidas: `LevelFailedCo()` → detiene oleadas, mueve la cámara al castillo, activa `gameOverUI`.
- Al completar todas las oleadas: `LevelCompletedCo()` → activa `victoryUI` o `levelCompletedUI` y desbloquea el siguiente nivel via `PlayerPrefs`.

---

## 6. Optimizaciones aplicadas

### 6.1 Engine (Unity)

| Técnica | Estado | Detalle |
|---|---|---|
| URP Performant | Aplicado | `Assets/Settings/URP-Performant.asset` — sin sombras en luces adicionales, MSAA=1, sin HDR |
| SRP Batcher | Activado | `m_UseSRPBatcher: 1` — reduce draw calls agrupando materiales con el mismo shader |
| Baked Lighting | Habilitado en escenas | `m_EnableBakedLightmaps: 1`, `m_EnableRealtimeLightmaps: 0` — luz precalculada para objetos estáticos |
| Occlusion Culling | Datos bakeados | Window → Rendering → Occlusion Culling → Bake (ejecutar en Editor) |
| Static Flags | Marcados via script | `AndroidOptimizer.cs` → "Mark Scene Terrain+Tiles as Static" |
| Adaptive Performance | Activado | `m_UseAdaptivePerformance: 1` en URP-Performant |

### 6.2 Físicas y lógica

| Técnica | Estado | Detalle |
|---|---|---|
| Object Pool | Implementado | `ObjectPoolManager.cs` — enemigos, proyectiles y VFX reutilizados |
| NavMeshAgent sin física de Rigidbody | Implementado | El agente maneja el movimiento; el Rigidbody es solo para detección de colisiones |
| `FixedUpdate` para torretas | Implementado | La lógica de ataque corre en `FixedUpdate` (60 Hz fijo) en vez de `Update` |
| Pre-asignación de colliders | Implementado | `Collider[] allocatedColliders = new Collider[100]` en `Tower.cs` — evita alloc por frame |

### 6.3 Assets

| Asset | Optimización |
|---|---|
| Texturas UI (`Assets/Graphics/UI`) | Override Android: ETC2_RGBA8, max 512px (ejecutar `AndroidOptimizer.cs` desde Unity) |
| Íconos de torres | Sprite Atlas generado por `AndroidOptimizer.cs` (1 draw call en lugar de N) |
| BGM (`.mp3`) | Android override: Streaming Vorbis (no carga todo el archivo en memoria) |
| SFX (`.wav`/`.mp3`) | Android override: DecompressOnLoad Vorbis + ForceToMono (ahorra ~50% memoria en clips estéreo) |
| Modelos 3D | Assets de tienda (low-poly) |

### 6.4 Pendiente (requiere acción en Unity Editor)

1. **Bake de Lighting:** Window → Rendering → Lighting → Generate Lighting (requiere objetos marcados como Static).
2. **Bake de Occlusion Culling:** Window → Rendering → Occlusion Culling → Bake.
3. **LOD Groups:** Agregar `LOD Group` component en prefabs de enemigos y objetos del escenario de mayor densidad poligonal.
4. **Eliminar assets de terceros no usados de la build:** TMP Examples, CFXR Demo, Ultimate 10 Shaders Scenes → excluir del Build Settings.

---

## 7. Accesibilidad

- **Tutorial** (`UI_Tutorial.cs`): aparece automáticamente la primera vez que se carga un nivel (controlado por `PlayerPrefs`). Explica el objetivo y los controles. Accesible durante la partida con el botón "?".
- **Feedback visual continuo:** barra de amenaza (vidas restantes), contador de recursos, timer de oleada con efecto blink.
- **Feedback de derrota/victoria:** pantallas dedicadas con canvas isolation (toda la UI de juego se oculta; solo queda el overlay).
- **Controles táctiles:** un toque abre el menú de torres; segundo toque en la misma casilla rota el preview; toque fuera cancela.
- **AudioMixer:** control de volumen SFX y BGM independientes desde Settings.
- **Font TMP:** fuente `NewAmsterdam` grande y legible en pantallas de 720p.

---

## 8. Integrantes

*(Completar con nombres del grupo)*

---

*Documento generado para la entrega de Optimización de Videojuegos — UADE, 2026.*
