using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BuildButtonOnHoverEffect : MonoBehaviour
{
    [SerializeField] private float adjustmnetSpeed = 10;

    [SerializeField] private float showcaseY;
    [SerializeField] private float defaultY;

    private float targetY;
    private bool canMove;

    private void Update()
    {
        if (Mathf.Abs(transform.position.y - targetY) > .01f && canMove)
        {
            float newPositionY = Mathf.Lerp(transform.position.y, targetY, adjustmnetSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newPositionY, transform.position.z);
        }
    }

    public void ToggleMovement(bool buttonsMenuActive)
    {
        canMove = buttonsMenuActive;
        SetTargetY(defaultY);

        if (buttonsMenuActive == false)
            SetPositionToDefault();
    }

    private void SetPositionToDefault()
    {
        transform.position = new Vector3(transform.position.x, defaultY, transform.position.z);
    }

    private void SetTargetY(float newY) => targetY = newY;

    public void ShowcaseButton(bool showcase)
    {
        if (showcase)
            SetTargetY(showcaseY);
        else
            SetTargetY(defaultY);
    }
}
