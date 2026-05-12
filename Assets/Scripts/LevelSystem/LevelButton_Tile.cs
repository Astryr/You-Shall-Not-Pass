using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButton_Tile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler,IPointerExitHandler
{
    private LevelManager levelManager;
    private TileAnimator tileAnimator;
    private TextMeshPro myText => GetComponentInChildren<TextMeshPro>();

    [SerializeField] private int levelIndex;

    private Vector3 defaultPosition;
    private Coroutine currentMoveCo;
    private Coroutine moveToDefaultCo;

    private bool canClick;
    private bool unlocked;

    private void Awake()
    {
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        levelManager = FindAnyObjectByType<LevelManager>();
        defaultPosition = transform.position;
        CheckIfLevelUnlocked();
    }

    public void CheckIfLevelUnlocked()
    {
        if (levelIndex == 1)
            PlayerPrefs.SetInt("Level_1" + "unlocked", 1);

        unlocked = PlayerPrefs.GetInt("Level_" + levelIndex + "unlocked", 0) == 1;
        UpdateLevelButtonText();
    }

    private void UpdateLevelButtonText()
    {
        if (unlocked == false)
            myText.text = "Locked";
        else
            myText.text = "Level " + levelIndex;
    }

    public void EnableClickOnButton(bool enable) => canClick = enable;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canClick == false)
            return;

        if (unlocked == false)
        {
            Debug.Log("Level locked!");
            // play sound
            return;
        }

        transform.position = defaultPosition;
        levelManager.LoadLevelFromMenu("Level_" + levelIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tileAnimator.IsGridMoving())
            return;

        if (currentMoveCo != null)
            Invoke(nameof(MoveToDefault), tileAnimator.GetTravelDuration());
        else
            MoveToDefault();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tileAnimator.IsGridMoving())
            return;

        MoveTileUp();
    }

    private void MoveTileUp()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, tileAnimator.GetBuildOffset(), 0);
        currentMoveCo = StartCoroutine(tileAnimator.MoveTileCo(transform,targetPosition));
    }

    private void MoveToDefault()
    {
        moveToDefaultCo = StartCoroutine(tileAnimator.MoveTileCo(transform,defaultPosition));
    }

    private void OnValidate()
    {
        levelIndex = transform.GetSiblingIndex() + 1;

        if (myText != null)
            myText.text = "Level " + levelIndex;
    }
}
