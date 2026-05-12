using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    private CameraController camController;
    private Coroutine cameraCo;
    private Coroutine enableControlsCo;

    [Header("Transition details")]
    [SerializeField] private float transitionDuration = 3;
    [Space]
    [SerializeField] private Vector3 inMenuPosition;
    [SerializeField] private Quaternion inMenuRotation;
    [Space]
    [SerializeField] private Vector3 inGamePosition;
    [SerializeField] private Quaternion inGameRotation;
    [Space]
    [SerializeField] private Vector3 levelSelectPosition;
    [SerializeField] private Quaternion levelSelectRotation;

    [Header("Screenshake details")]
    [Range(0.01f, .5f)]
    [SerializeField] private float shakeMagnutide;
    [Range(0.1f, 3f)]
    [SerializeField] private float shakeDuration;

    [Header("Castle Focus Details")]
    [SerializeField] private float focusOnCastleDuration = 2;
    [SerializeField] private float hightOffset = 3;
    [SerializeField] private float distanceToCastle = 7;

    private void Awake()
    {
        camController = GetComponent<CameraController>();
    }

    private void Start()
    {
        if (GameManager.instance != null && GameManager.instance.IsTestingLevel())
        {
            camController.EnableCameraConrolls(true);
            return;
        }

        SwitchToMenuView();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            Screenshake(shakeDuration, shakeMagnutide);

        if (Input.GetKeyDown(KeyCode.B))
            FocusOnCastle();
    }

    public void Screenshake(float newDuration, float newMagnitude)
    {
        StartCoroutine(ScreenshakeFX(newDuration, newMagnitude));
    }

    public void FocusOnCastle()
    {
        Transform castle = FindFirstObjectByType<Castle>().transform;

        if (castle == null)
        {
            Debug.Log("There is no castle to focus on!");
            return;
        }

        Vector3 directionToCastle = (castle.position - transform.position).normalized;
        Vector3 targetPosition = castle.position - (directionToCastle * distanceToCastle);
        targetPosition.y = castle.position.y + hightOffset;

        Quaternion targetRotation = Quaternion.LookRotation(castle.position - targetPosition);

        if(cameraCo != null)
            StopCoroutine(cameraCo);

        CancelScheduledEnableControls();
        cameraCo = StartCoroutine(ChangePositionAndRotation(targetPosition, targetRotation,focusOnCastleDuration));
        ScheduleEnableCameraControlsAfter(focusOnCastleDuration + .1f);
    }

    public void SwitchToMenuView()
    {
        CancelScheduledEnableControls();
        if (cameraCo != null)
            StopCoroutine(cameraCo);

        camController.EnableCameraConrolls(false);
        cameraCo = StartCoroutine(ChangePositionAndRotation(inMenuPosition, inMenuRotation, transitionDuration));
        camController.AdjustPitchValue(inMenuRotation.eulerAngles.x);
    }

    public void SwitchToGameView()
    {
        if (cameraCo != null)
            StopCoroutine(cameraCo);

        cameraCo = StartCoroutine(ChangePositionAndRotation(inGamePosition, inGameRotation,transitionDuration));
        camController.AdjustPitchValue(inGameRotation.eulerAngles.x);

        ScheduleEnableCameraControlsAfter(transitionDuration + .1f);
    }

    public void SwitchToLevelSelectView()
    {
        CancelScheduledEnableControls();
        if (cameraCo != null)
            StopCoroutine(cameraCo);

        camController.EnableCameraConrolls(false);
        cameraCo = StartCoroutine(ChangePositionAndRotation(levelSelectPosition, levelSelectRotation,transitionDuration));
        camController.AdjustPitchValue(levelSelectRotation.eulerAngles.x);
    }

    private IEnumerator ChangePositionAndRotation(Vector3 targetPosition, Quaternion targetRotation, float duration = 3, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        camController.EnableCameraConrolls(false);


        float time = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    private void CancelScheduledEnableControls()
    {
        if (enableControlsCo == null)
            return;

        StopCoroutine(enableControlsCo);
        enableControlsCo = null;
    }

    private void ScheduleEnableCameraControlsAfter(float delay)
    {
        CancelScheduledEnableControls();
        enableControlsCo = StartCoroutine(EnableCameraControllsAfter(delay));
    }

    private IEnumerator EnableCameraControllsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        enableControlsCo = null;
        camController.EnableCameraConrolls(true);
    }

    private IEnumerator ScreenshakeFX(float duration, float magnitude)
    {
        Vector3 originalPosition = camController.transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float x = Random.Range(-1, 1) * magnitude;
            float y = Random.Range(-1, 1) * magnitude;

            camController.transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camController.transform.position = originalPosition;
    }

    public Coroutine GetActiveCamCo() => cameraCo;
}
