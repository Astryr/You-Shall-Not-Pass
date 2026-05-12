using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_TextBlinkEffect : MonoBehaviour
{
    private TextMeshProUGUI myText;

    [SerializeField] private float changeValueSpeed;
    [SerializeField] private float currentValue;
    [SerializeField] private float targetValue;
    private float targetAlpha;
    private bool canBlink;

    private void Awake()
    {
        myText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (canBlink == false)
            return;

        if (Mathf.Abs(myText.color.a - targetAlpha) > .01f)
        {
            float newAlpha = Mathf.Lerp(myText.color.a, targetAlpha, changeValueSpeed * Time.deltaTime);
            ChangeColorAlpha(newAlpha);
        }
        else
        {
            ChangeTargetAlpha();
        }
    }

    public void EnableBlink(bool enable)
    {
        canBlink = enable;

        if (canBlink == false)
            ChangeColorAlpha(1);
    }

    private void ChangeTargetAlpha() => targetAlpha = (targetAlpha == 1) ? 0 : 1;

    private void ChangeColorAlpha(float newAlpha)
    {
        Color myColor = myText.color;
        myText.color = new Color(myColor.r,myColor.g, myColor.b, newAlpha);
    }
}
