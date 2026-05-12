using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_EnemiesKilledText : MonoBehaviour
{
    private TextMeshProUGUI myText;
    private GameManager gameManager;

    private void Awake()
    {
        myText = GetComponent<TextMeshProUGUI>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        myText.text = "Enemies killed: " + gameManager.enemiesKilled;
    }
}
