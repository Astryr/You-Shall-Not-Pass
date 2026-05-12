using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer_Visuals : MonoBehaviour
{
    private Tower_Hammer myTower;

    [SerializeField] private ParticleSystem[] vfx;
    [SerializeField] private RotateObject valveRotation;

    [Header("Hammer Details")]
    [SerializeField] private Transform hammer;
    [SerializeField] private Transform hammerHolder;
    [Space]
    [SerializeField] private Transform sideWire;
    [SerializeField] private Transform sideHandle;

    [Header("Attack and Release Details")]
    [SerializeField] private float attackOffsetY;
    [SerializeField] private float attackDuration;
    [SerializeField] private float reloadDuration;

    private void Awake()
    {
        myTower = GetComponent<Tower_Hammer>();
        reloadDuration = myTower.GetAttackCooldown() - attackDuration;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            PlayAttackAnimation();
    }

    public void PlayAttackAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(HammerAttackCo());
    }

    private void PlayVFXs()
    {
        foreach (var p in vfx)
        {
            p.Play();
        }
    }


    private IEnumerator HammerAttackCo()
    {
        valveRotation.AdjustRotationSpeed(25);
        StartCoroutine(ChangePositionCo(hammer, -attackOffsetY, attackDuration));
        StartCoroutine(ChangeScaleCo(hammerHolder, 7, attackDuration));

        StartCoroutine(ChangePositionCo(sideHandle, .45f, attackDuration));
        StartCoroutine(ChangeScaleCo(sideWire, .1f,attackDuration));

        yield return new WaitForSeconds(attackDuration);
        PlayVFXs();

        valveRotation.AdjustRotationSpeed(3);
        StartCoroutine(ChangePositionCo(hammer, attackOffsetY, reloadDuration));
        StartCoroutine(ChangeScaleCo(hammerHolder, 1, reloadDuration));

        StartCoroutine(ChangePositionCo(sideHandle, -.45f, reloadDuration));
        StartCoroutine(ChangeScaleCo(sideWire, 1, reloadDuration));
    }

    private IEnumerator ChangePositionCo(Transform transform, float yOffset, float duration = .1f)
    {
        float time = 0;

        Vector3 initialPosition = transform.localPosition;
        Vector3 targetPosition = new Vector3(initialPosition.x, initialPosition.y + yOffset, initialPosition.z);

        while (time < duration)
        {
            transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
    }


    private IEnumerator ChangeScaleCo(Transform transform, float newScale, float duration = .25f)
    {
        float time = 0;

        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(1, newScale, 1);

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

}
