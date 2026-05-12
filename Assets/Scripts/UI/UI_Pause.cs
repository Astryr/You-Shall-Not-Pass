using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Pause : MonoBehaviour
{
    private UI ui;
    private UI_InGame inGameUI;

    [SerializeField] private GameObject[] pauseUiElements;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        inGameUI = ui.GetComponentInChildren<UI_InGame>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
            ui.SwitchTo(inGameUI.gameObject);
    }

    public void SwitchPauseUIElements(GameObject elementToEnable)
    {
        foreach (GameObject obj in pauseUiElements)
        {
            obj.SetActive(false);
        }

        elementToEnable.SetActive(true);
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }
}
