using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private TileAnimator tileAnim;
    private BuildManager buildManager;
    private Vector3 defaultPosition;

    private bool tileCanBeMoved = true;
    private bool buildSlotAvalible = true;

    private Coroutine currentMovementUpCo;
    private Coroutine moveToDefaultCo;

    

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        tileAnim = FindFirstObjectByType<TileAnimator>();
        buildManager = FindFirstObjectByType<BuildManager>();
        defaultPosition = transform.position;
    }

    private void Start()
    {
        if (buildSlotAvalible == false)
            transform.position += new Vector3(0, .1f);
    }

    public void SetSlotAvalibleTo(bool value) => buildSlotAvalible = value;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buildSlotAvalible == false || tileAnim.IsGridMoving())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (buildManager.GetSelectedSlot() == this)
        {
            // On mobile, tap the same slot repeatedly to rotate the preview tower
            UI uiScript = FindFirstObjectByType<UI>();
            if (uiScript != null && uiScript.buildButtonsUI != null)
                uiScript.buildButtonsUI.RotatePreview(90);
            return;
        }

        SnapToBeforeBuildPosition();
        buildManager.EnableBuildMenu();
        buildManager.SelectBuildSlot(this);
        MoveTileUp();

        tileCanBeMoved = false;

        ui.buildButtonsUI.GetLastSelectedButton()?.SelectButton(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildSlotAvalible == false || tileAnim.IsGridMoving())
            return;

        if (tileCanBeMoved == false)
            return;

        MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buildSlotAvalible == false || tileAnim.IsGridMoving())
            return;

        if (tileCanBeMoved == false)
            return;

        if (currentMovementUpCo != null)
        {
            Invoke(nameof(MoveToDefaultPosition), tileAnim.GetTravelDuration());
        }
        else
            MoveToDefaultPosition();
    }

    public void UnselectTile()
    {
        MoveToDefaultPosition();
        tileCanBeMoved = true;
    }

    private void MoveTileUp()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, tileAnim.GetBuildOffset(), 0);
        currentMovementUpCo = StartCoroutine(tileAnim.MoveTileCo(transform, targetPosition));
    }
    
    private void MoveToDefaultPosition()
    {
        moveToDefaultCo = StartCoroutine(tileAnim.MoveTileCo(transform, defaultPosition));
    }
    public void SnapToDefaultPositionImmediately()
    {
        if(moveToDefaultCo != null)
            StopCoroutine(moveToDefaultCo);

        transform.position = defaultPosition;
    }

    public void SnapToBeforeBuildPosition()
    {
        Vector3 targetPosition = defaultPosition + new Vector3(0, tileAnim.GetBuildOffset(), 0);
        transform.position = targetPosition;
    }

    public Vector3 GetBuildPosition(float yOffset) => defaultPosition + new Vector3(0, yOffset);
}
