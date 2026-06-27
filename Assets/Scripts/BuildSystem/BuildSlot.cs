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
        if (!buildSlotAvalible) return;
        if (tileAnim == null || tileAnim.IsGridMoving()) return;
        if (buildManager == null) return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (buildManager.GetSelectedSlot() == this)
        {
            ui?.buildButtonsUI?.RotatePreview(90);
            return;
        }

        SnapToBeforeBuildPosition();
        buildManager.EnableBuildMenu();
        buildManager.SelectBuildSlot(this);
        MoveTileUp();
        tileCanBeMoved = false;
        ui?.buildButtonsUI?.GetLastSelectedButton()?.SelectButton(true);
    }

    /// <summary>
    /// Selección directa sin EventSystem. Llamada por BuildManager via Physics.Raycast.
    /// Es el camino principal en Android: evita depender de IsPointerOverGameObject
    /// y de PhysicsRaycaster en la cámara.
    /// </summary>
    public void TriggerSelect()
    {
        if (!buildSlotAvalible)
        {
            Debug.LogWarning($"[BuildSlot] TriggerSelect ignorado: slot '{name}' marcado como no disponible.");
            return;
        }

        // Si el TileAnimator no está disponible omitimos el check de animación pero
        // intentamos proceder igual; MoveTileUp ya tiene su propia guarda.
        if (tileAnim != null && tileAnim.IsGridMoving())
        {
            Debug.LogWarning("[BuildSlot] TriggerSelect ignorado: el grid está animándose.");
            return;
        }

        if (buildManager == null)
        {
            Debug.LogError("[BuildSlot] TriggerSelect: BuildManager.instance es null. Verifica que BuildManager está en la escena y su Awake() se ejecutó.");
            return;
        }

        if (buildManager.GetSelectedSlot() == this)
        {
            ui?.buildButtonsUI?.RotatePreview(90);
            return;
        }

        SnapToBeforeBuildPosition();
        buildManager.EnableBuildMenu();
        buildManager.SelectBuildSlot(this);
        MoveTileUp();
        tileCanBeMoved = false;
        ui?.buildButtonsUI?.GetLastSelectedButton()?.SelectButton(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!buildSlotAvalible || tileAnim == null || tileAnim.IsGridMoving())
            return;

        if (!tileCanBeMoved)
            return;

        MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!buildSlotAvalible || tileAnim == null || tileAnim.IsGridMoving())
            return;

        if (!tileCanBeMoved)
            return;

        if (currentMovementUpCo != null)
            Invoke(nameof(MoveToDefaultPosition), tileAnim.GetTravelDuration());
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
        if (tileAnim == null) return;

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
        if (tileAnim == null) return;
        Vector3 targetPosition = defaultPosition + new Vector3(0, tileAnim.GetBuildOffset(), 0);
        transform.position = targetPosition;
    }

    public Vector3 GetBuildPosition(float yOffset) => defaultPosition + new Vector3(0, yOffset);
}
