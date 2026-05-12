using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Final tower build step is done on UI_BuildButton
public class BuildManager : MonoBehaviour
{
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

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                hasInput = true;
                inputPosition = touch.position;

                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            hasInput = true;
            inputPosition = Input.mousePosition;

            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
        }

        if (hasInput)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(inputPosition), out RaycastHit hit, Mathf.Infinity, ~whatToIgnore))
            {
                bool clickedNotOnBuildSlot = hit.collider.GetComponent<BuildSlot>() == null;

                if (clickedNotOnBuildSlot)
                    CancelBuildAction();
            }
            else
            {
                // Clicked outside any collider, cancel build menu
                CancelBuildAction();
            }
        }
    }

    public void UpdateBuildManager(WaveManager newWaveManager)
    {
        MakeBuildSlotNotAvalibleIfNeeded(newWaveManager, currentGrid);
    }
    public void BuildTower(GameObject towerToBuild,int towerPrice,Transform newPreviewTower)
    {
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

        slotToUse.SnapToDefaultPositionImmidiatly();
        slotToUse.SetSlotAvalibleTo(false);

        ui.buildButtonsUI.SetLastSelected(null,null);

        cameraEffects.Screenshake(camShakeDuration, camShakeMagnitude);

        GameObject newTower = Instantiate(towerToBuild, slotToUse.GetBuildPosition(towerCenterY), Quaternion.identity);
        newTower.transform.rotation = newPreviewTower.rotation;
    }


    public void MouseOverUI(bool isOverUI) => isMouseOverUI = isOverUI;

    public void MakeBuildSlotNotAvalibleIfNeeded(WaveManager waveManager, GridBuilder currentGrid)
    {
        if (waveManager == null)
            return;

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
    public void CancelBuildAction()
    {
        if (selectedBuildSlot == null)
            return;
    
        ui.buildButtonsUI.GetLastSelectedButton()?.SelectButton(false);

        selectedBuildSlot.UnselectTile();
        selectedBuildSlot = null;
        DisableBuildMenu();
    }
    public void SelectBuildSlot(BuildSlot newSlot)
    {
        if (selectedBuildSlot != null)
            selectedBuildSlot.UnselectTile();

        selectedBuildSlot = newSlot;
    }
    public void EnableBuildMenu()
    {
        if (selectedBuildSlot != null)
            return;

        ui.buildButtonsUI.ShowBuildButtons(true);
    }
    private void DisableBuildMenu()
    {
        ui.buildButtonsUI.ShowBuildButtons(false);
    }
    public BuildSlot GetSelectedSlot() => selectedBuildSlot;
    public Material GetAttackRadiusMat() => attackRadiusMat;
    public Material GetBuildPreviewMat() => buildPreviewMat;
}
