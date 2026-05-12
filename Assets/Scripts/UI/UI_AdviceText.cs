using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_AdviceText : MonoBehaviour
{
    private TextMeshProUGUI myText;

    [SerializeField] private string[] advices;

    private void OnEnable()
    {
        if(myText == null)
            myText = GetComponent<TextMeshProUGUI>();

        int randomIndex = Random.Range(0, advices.Length);
        myText.text = advices[randomIndex];
    }
}
