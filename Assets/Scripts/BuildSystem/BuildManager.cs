using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Mecánica principal de construcción: elige casilla (BuildSlot), confirma torre desde UI (UI_BuildButton) y esta clase instancia la torre si hay oro.
public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;

    private UI ui;
    public BuildSlot selectedBuildSlot;

    public WaveManager waveManager;
    public GridBuilder currentGrid;
    private GameManager gameManager;
    private CameraEffects cameraEffects;


    [SerializeField] private LayerMask whatToIgnore;

    [Header("Build Materials")]
    [SerializeField] private Material attackRadiusMat;
    [SerializeField] private Material buildPreviewMat;

    [Header("Build details")]
    [SerializeField] private float towerCenterY = .5f;
    [SerializeField] private float camShakeDuration = .15f;
    [FormerlySerializedAs("camShakeMagnutiude")]
    [SerializeField] private float camShakeMagnitude = .02f;


    private bool isMouseOverUI;

    private void Awake()
    {
        // Si ya existe una instancia de otro objeto (p.ej. un duplicado en la escena
        // de nivel), este duplicado se destruye para que el BuildManager de MainScene
        // sea siempre el singleton válido. Sin esta guarda, el duplicado sobreescribe
        // 'instance' y cuando luego se destruye, 'instance' queda apuntando a un
        // objeto destruido → Unity lo evalúa como null → TriggerSelect crash.
        if (instance != null && instance != this)
        {
            enabled = false;
            Destroy(gameObject);
            return;
        }
        instance = this;

        ui = FindFirstObjectByType<UI>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();

        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager>();

        MakeBuildSlotNotAvalibleIfNeeded(waveManager, currentGrid);
    }

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            CancelBuildAction();

        bool hasInput = false;
        Vector3 inputPosition = Vector3.zero;
        int touchFingerId = -1;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                hasInput = true;
                inputPosition = touch.position;
                touchFingerId = touch.fingerId;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            hasInput = true;
            inputPosition = Input.mousePosition;
        }

        if (!hasInput || isMouseOverUI || Camera.main == null)
            return;

        // Mecanismo principal de selección: Physics.Raycast directo desde la cámara.
        // Este camino es el más confiable en Android porque NO depende del EventSystem
        // ni del PhysicsRaycaster en la cámara (que en algunos dispositivos/builds
        // puede no despachar OnPointerDown a objetos 3D correctamente).
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            BuildSlot slot = hit.collider.GetComponentInParent<BuildSlot>();
            if (slot != null)
            {
                slot.TriggerSelect();
                return; // Evento ya procesado; no cancelar.
            }
        }

        // El toque NO golpeó un BuildSlot. Solo cancelar si tampoco cayó sobre UI
        // (para no cerrar el menú de torres al presionar sus botones).
        bool overUI = UnityEngine.EventSystems.EventSystem.current != null &&
                      (touchFingerId >= 0
                          ? UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touchFingerId)
                          : UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());

        if (!overUI)
            CancelBuildAction();
    }

    public void UpdateBuildManager(WaveManager newWaveManager)
    {
        MakeBuildSlotNotAvalibleIfNeeded(newWaveManager, currentGrid);
    }

    /// <summary>
    /// Actualiza el WaveManager Y el grid activo del nivel.
    /// Llamado por LevelSetup al cargar un nivel para que la comparación
    /// de tiles use el grid correcto y no el que estaba en el inspector de MainScene.
    /// </summary>
    public void UpdateBuildManager(WaveManager newWaveManager, GridBuilder newCurrentGrid)
    {
        currentGrid = newCurrentGrid;
        MakeBuildSlotNotAvalibleIfNeeded(newWaveManager, currentGrid);
    }
    /// <summary>Confirma la torre en la casilla seleccionada: valida oro, gasta moneda, shake de cámara e Instantiate.</summary>
    public void BuildTower(GameObject towerToBuild,int towerPrice,Transform newPreviewTower)
    {
        if (gameManager == null || ui == null || ui.inGameUI == null)
        {
            Debug.LogWarning("BuildManager: GameManager o UI no inicializados.");
            return;
        }

        if (gameManager.HasEnoughCurrency(towerPrice) == false)
        {
            ui.inGameUI.ShakeCurrencyUI();
            return;
        }

        if (towerToBuild == null)
        {
            Debug.LogWarning("You did not assign tower to this button!");
            return;
        }

        if (ui.buildButtonsUI.GetLastSelectedButton() == null)
            return;

        gameManager.SpendCurrency(towerPrice);

        Transform previewTower = newPreviewTower;
        BuildSlot slotToUse = GetSelectedSlot();
        CancelBuildAction();

        slotToUse.SnapToDefaultPositionImmediately();
        slotToUse.SetSlotAvalibleTo(false);

        ui.buildButtonsUI.SetLastSelected(null,null);

        cameraEffects?.Screenshake(camShakeDuration, camShakeMagnitude);

        GameObject newTower = Instantiate(towerToBuild, slotToUse.GetBuildPosition(towerCenterY), Quaternion.identity);
        newTower.transform.rotation = newPreviewTower.rotation;
    }


    public void MouseOverUI(bool isOverUI) => isMouseOverUI = isOverUI;

    public void MakeBuildSlotNotAvalibleIfNeeded(WaveManager waveManager, GridBuilder currentGrid)
    {
        if (waveManager == null)
            return;

        if (currentGrid == null)
        {
            Debug.LogWarning("[BuildManager] MakeBuildSlotNotAvalibleIfNeeded: currentGrid es null. Asigna el grid del nivel correctamente.");
            return;
        }

        foreach (var wave in waveManager.GetLevelWaves())
        {
            if (wave.nextGrid == null)
                continue;

            List<GameObject> grid = currentGrid.GetTileSetup();
            List<GameObject> nextWaveGrid = wave.nextGrid.GetTileSetup();

            for (int i = 0; i < grid.Count; i++)
            {
                TileSlot currentTile = grid[i].GetComponent<TileSlot>();
                TileSlot nextTile = nextWaveGrid[i].GetComponent<TileSlot>();

                bool tileNotTheSame = currentTile.GetMesh() != nextTile.GetMesh() ||
                                      currentTile.GetMaterial() != nextTile.GetMaterial() ||
                                      currentTile.GetAllChildren().Count != nextTile.GetAllChildren().Count;

                if (tileNotTheSame == false)
                    continue;

                BuildSlot buildSlot = grid[i].GetComponent<BuildSlot>();

                if (buildSlot != null)
                    buildSlot.SetSlotAvalibleTo(false);
            }

        }
    }
    public void ClearBuildSelection()
    {
        if (selectedBuildSlot == null)
        {
            DisableBuildMenu();
            return;
        }

        SafeUnselectSlot(selectedBuildSlot);
        selectedBuildSlot = null;
        DisableBuildMenu();
    }

    public void CancelBuildAction()
    {
        if (selectedBuildSlot == null)
            return;

        ui?.buildButtonsUI?.GetLastSelectedButton()?.SelectButton(false);
        SafeUnselectSlot(selectedBuildSlot);
        selectedBuildSlot = null;
        DisableBuildMenu();
    }

    public void SelectBuildSlot(BuildSlot newSlot)
    {
        if (selectedBuildSlot != null && selectedBuildSlot != newSlot)
            SafeUnselectSlot(selectedBuildSlot);

        selectedBuildSlot = newSlot;
    }

    private void SafeUnselectSlot(BuildSlot slot)
    {
        if (slot == null)
            return;

        if (!slot.gameObject.activeInHierarchy)
            slot.SnapToDefaultPositionImmediately();
        else
            slot.UnselectTile();
    }
    public void EnableBuildMenu()
    {
        if (ui?.buildButtonsUI == null)
            return;

        ui.buildButtonsUI.ShowBuildButtons(true);
    }
    private void DisableBuildMenu()
    {
        if (ui?.buildButtonsUI == null)
            return;

        ui.buildButtonsUI.ShowBuildButtons(false);
    }
    public BuildSlot GetSelectedSlot() => selectedBuildSlot;
    public Material GetAttackRadiusMat() => attackRadiusMat;
    public Material GetBuildPreviewMat() => buildPreviewMat;
}
