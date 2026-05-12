using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private BuildManager buildManager;
    private CameraEffects cameraEffects;
    private GameManager gameManager;
    private UI_BuildButtonsHolder buildButtonsHolder;
    private UI_BuildButtonOnHoverEffect onHoverEffect;


    [SerializeField] private string towerName;
    [SerializeField] private int towerPrice = 50;
    [Space]
    [SerializeField] private GameObject towerToBuild;
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerPriceText;


    //It is used to preview tower before building it
    public TowerPreview towerPreview;
    public bool buttonUnlocked { get; private set; }

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        onHoverEffect = GetComponent<UI_BuildButtonOnHoverEffect>();
        buildButtonsHolder = GetComponentInParent<UI_BuildButtonsHolder>();

        buildManager = FindFirstObjectByType<BuildManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>(); 
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        CreateTowerPreview();
    }

    private void CreateTowerPreview()
    {
        GameObject newPreview = Instantiate(towerToBuild,Vector3.zero, Quaternion.identity);

        towerPreview = newPreview.AddComponent<TowerPreview>();
        towerPreview.SetupTowerPreview(newPreview);
        towerPreview.transform.parent = buildManager.transform;
    }

    public void SelectButton(bool select)
    {
        BuildSlot slotToUse = buildManager.GetSelectedSlot();

        if (slotToUse == null)
            return;

        Vector3 previewPosition = slotToUse.GetBuildPosition(1f);


        towerPreview.gameObject.SetActive(select);
        towerPreview.ShowPreview(select, previewPosition);
        onHoverEffect.ShowcaseButton(select);
        buildButtonsHolder.SetLastSelected(this, towerPreview.transform);
    }

    public void UnlockTowerIfNeeded(string towerNameToCheck, bool unlockStatus)
    {
        if (towerNameToCheck != towerName)
            return;

        buttonUnlocked = unlockStatus;
        gameObject.SetActive(unlockStatus);
    }

    public void ConfirmTowerBuild()
    {
        buildManager.BuildTower(towerToBuild,towerPrice,towerPreview.transform);
    }




    public void OnPointerEnter(PointerEventData eventData)
    {
        buildManager.MouseOverUI(true);

        foreach (var button in buildButtonsHolder.GetBuildButtons())
        {
            if(button.gameObject.activeSelf)
                button.SelectButton(false);
        }

        SelectButton(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildManager.MouseOverUI(false);
    }

    private void OnValidate()
    {
        towerNameText.text = towerName;
        towerPriceText.text = towerPrice + "";
        gameObject.name = "BuildButton_UI - " + towerName;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SelectButton(true); // Asegura que la torreta tenga preview position
        ConfirmTowerBuild();
    }
}
