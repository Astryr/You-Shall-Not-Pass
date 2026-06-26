using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    // Singletons: resueltos en O(1) sin FindFirstObjectByType.
    // Esto elimina ~150 búsquedas durante la activación de la escena
    // (antes era 3 FindFirstObjectByType por slot × N slots en el nivel).
    private UI ui => UI.instance;
    private TileAnimator tileAnim => TileAnimator.instance;
    private BuildManager buildManager => BuildManager.instance;

    private Vector3 defaultPosition;

    private bool tileCanBeMoved = true;
    private bool buildSlotAvalible = true;

    private Coroutine currentMovementUpCo;
    private Coroutine moveToDefaultCo;

    private void Awake()
    {
        // Solo cacheamos la posición inicial. Sin búsquedas de objetos.
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
            if (ui != null && ui.buildButtonsUI != null)
                ui.buildButtonsUI.RotatePreview(90);
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

        if (!isActiveAndEnabled)
        {
            transform.position = targetPosition;
            return;
        }

        currentMovementUpCo = StartCoroutine(tileAnim.MoveTileCo(transform, targetPosition));
    }

    private void MoveToDefaultPosition()
    {
        if (moveToDefaultCo != null)
        {
            StopCoroutine(moveToDefaultCo);
            moveToDefaultCo = null;
        }

        if (!isActiveAndEnabled)
        {
            transform.position = defaultPosition;
            return;
        }

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
