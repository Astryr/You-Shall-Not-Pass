using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private UI_Animator uiAnim;
    private RectTransform myRect;

    [SerializeField] private float showcaseScale = 1.1f;
    [SerializeField] private float scaleUpDuration = .25f;

    private Coroutine scaleCoroutine;
    [Space]
    [SerializeField] private UI_TextBlinkEffect myTextBlinkEffect;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        uiAnim = GetComponentInParent<UI_Animator>();
        myRect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        AudioManager.instance?.PlaySFX(ui.onHoverSfx);

        scaleCoroutine = StartCoroutine(uiAnim.ChangeScaleCo(myRect,showcaseScale,scaleUpDuration));

        if (myTextBlinkEffect != null)
            myTextBlinkEffect.EnableBlink(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(uiAnim.ChangeScaleCo(myRect,1,scaleUpDuration));

        if(myTextBlinkEffect != null)
            myTextBlinkEffect.EnableBlink(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.instance?.PlaySFX(ui.onClickSfx);
        myRect.localScale = new Vector3(1, 1, 1);
    }
}
