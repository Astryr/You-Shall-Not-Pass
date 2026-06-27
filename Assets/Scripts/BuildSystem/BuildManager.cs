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
        instance = this;
        ui = FindFirstObjectByType<UI>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();

        // Autocompletar WaveManager si olvidaste asignarlo en el inspector
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }

        MakeBuildSlotNotAvalibleIfNeeded(waveManager,currentGrid);
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

        Ray ray = Camera.main.ScreenPointToRay(inputPosition);

        // IMPORTANTE: raycast SIN restricción de capas (no usar ~whatToIgnore aquí).
        // whatToIgnore está configurado en el inspector para ignorar ciertos objetos al
        // cancelar la selección, pero puede excluir accidentalmente la capa en la que
        // están los tiles BuildSlot (p.ej. Default/0). Si se usa ~whatToIgnore aquí,
        // el raycast nunca golpea los tiles y la selección nunca se activa.
        if (Physics.Raycast(ray, out RaycastHit buildHit, Mathf.Infinity))
        {
            // GetComponentInParent cubre el caso donde el Collider está en un hijo
            // y el componente BuildSlot está en el raíz del tile.
            BuildSlot hitSlot = buildHit.collider.GetComponentInParent<BuildSlot>();
            if (hitSlot != null)
            {
                hitSlot.TriggerSelect();
                return;
            }
        }

        // No se tocó ningún BuildSlot. Solo cancelar si el toque tampoco está sobre UI
        // (evita cerrar el menú al presionar botones de torre).
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
