using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private bool canControll;
    [SerializeField] private Vector3 levelCenterPoint;
    [SerializeField] private float maxDistanceFromCenter;

    [Header("Movement Details")]
    [SerializeField] private float movementSpeed = 120;
    [SerializeField] private float mouseMovementSpeed = 5;
    [SerializeField] private float edgeMovementSpeed = 50;
    [SerializeField] private float edgeTreshold = 10;
    private float screenWidth;
    private float screenHeight;

    [Header("Rotation details")]
    [SerializeField] private Transform focusPoint;
    [SerializeField] private float maxFocusPointDistance = 15;
    [Space]
    [SerializeField] private float rotationSpeed = 200;
    [Space]
    private float pitch;
    [SerializeField] private float minPitch = 5f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Zoom details")]
    [Tooltip("Multiplicador para rueda / pinch.")]
    [SerializeField] private float zoomSpeed = 10;
    [Tooltip("Distancia mínima cámara ↔ punto de foco (world).")]
    [SerializeField] private float minZoom = 3;
    [Tooltip("Distancia máxima cámara ↔ punto de foco (world).")]
    [SerializeField] private float maxZoom = 15;


    private float smoothTime = .1f;
    private Vector3 movementVelocity = Vector3.zero;
    private Vector3 mouseMovementVelocity = Vector3.zero;
    private Vector3 edgeMovementVelocity = Vector3.zero;
    private Vector3 zoomVelocity = Vector3.zero;
    private Vector3 lastMousePosition;

    private void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    void Update()
    {
        if (canControll == false)
            return;

        RefreshFocusPoint();

        HandleRotation();
        RefreshFocusPoint();

        HandleZoom();
        //HandleEdgeMovement();
        HandleMouseMovement();
        HandleMovement();

        RefreshFocusPoint();
    }

    public void EnableCameraConrolls(bool enable) => canControll = enable;

    public float AdjustPitchValue(float value) => pitch = value;
    public float AdjustKeyboardSenseitivty(float value) => movementSpeed = value;
    public float AdjustMouseSensetivity(float value) => mouseMovementSpeed = value;

    private void RefreshFocusPoint()
    {
        if (focusPoint != null)
            focusPoint.position = transform.position + (transform.forward * GetFocusPointDistance());
    }

    private void HandleZoom()
    {
        // Pinch con dos dedos (móvil).
        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            Vector2 t1Prev = t1.position - t1.deltaPosition;
            Vector2 t2Prev = t2.position - t2.deltaPosition;
            float prevMag = Vector2.Distance(t1Prev, t2Prev);
            float currentMag = Vector2.Distance(t1.position, t2.position);
            float difference = currentMag - prevMag;

            float delta = difference * zoomSpeed * 0.05f;
            TryApplyZoom(delta);
            return;
        }

        // Rueda del mouse (PC) + respaldo con mouseScrollDelta.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f))
            scroll = Input.mouseScrollDelta.y * 0.08f;

        if (Mathf.Approximately(scroll, 0f))
            return;

        TryApplyZoom(scroll * zoomSpeed);
    }

    // Devuelve el punto de foco asignado, o uno calculado con raycast si no hay Transform asignado.
    private Vector3 GetVirtualFocusPoint()
    {
        if (focusPoint != null)
            return focusPoint.position;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxFocusPointDistance))
            return hit.point;

        return transform.position + transform.forward * (maxFocusPointDistance * 0.5f);
    }

    /// <summary>Mueve la cámara a lo largo del eje de vista y limita por distancia al punto de foco.</summary>
    private void TryApplyZoom(float deltaAlongForward)
    {
        if (Mathf.Abs(deltaAlongForward) < 1e-5f)
            return;

        Vector3 fp = GetVirtualFocusPoint();
        Vector3 offset = transform.position - fp;
        float dist = offset.magnitude;

        if (dist < 0.01f)
        {
            offset = -transform.forward.normalized;
            dist = minZoom;
        }
        else
            offset /= dist;

        float newDist = Mathf.Clamp(dist - deltaAlongForward, minZoom, maxZoom);
        Vector3 targetPosition = fp + offset * newDist;

        if (Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
            targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref zoomVelocity, smoothTime);
    }

    private float GetFocusPointDistance()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxFocusPointDistance))
            return hit.distance;

        return maxFocusPointDistance;
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float verticalRotation = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            pitch = Mathf.Clamp(pitch - verticalRotation, minPitch, maxPitch);

            Vector3 pivot = GetVirtualFocusPoint();
            transform.RotateAround(pivot, Vector3.up, horizontalRotation);
            transform.RotateAround(pivot, transform.right, pitch - transform.eulerAngles.x);
            transform.LookAt(pivot);
        }
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = transform.position;

        float vInput = Input.GetAxisRaw("Vertical");
        float hInput = Input.GetAxisRaw("Horizontal");

        if (vInput == 0 && hInput == 0)
            return;

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        if (vInput > 0)
            targetPosition += flatForward * movementSpeed * Time.deltaTime;
        if (vInput < 0)
            targetPosition -= flatForward * movementSpeed * Time.deltaTime;


        if (hInput > 0)
            targetPosition += transform.right * movementSpeed * Time.deltaTime;
        if (hInput < 0)
            targetPosition -= transform.right * movementSpeed * Time.deltaTime;


        if (Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
        {
            targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref movementVelocity, smoothTime);
    }

    

    private bool isTouchDraggingUI = false;

    private void HandleMouseMovement()
    { 
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastMousePosition = touch.position;
                isTouchDraggingUI = UnityEngine.EventSystems.EventSystem.current != null && 
                                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
            else if (touch.phase == TouchPhase.Moved && !isTouchDraggingUI)
            {
                Vector3 positionDifference = new Vector3(touch.position.x, touch.position.y, 0) - lastMousePosition;
                Vector3 moveRight = transform.right * (-positionDifference.x) * (mouseMovementSpeed * 0.2f) * Time.deltaTime;
                Vector3 moveForawrd = transform.forward * (-positionDifference.y) * (mouseMovementSpeed * 0.2f) * Time.deltaTime;

                moveRight.y = 0;
                moveForawrd.y = 0;

                Vector3 targetPosition = transform.position + moveRight + moveForawrd;

                if (Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
                    targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;


                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref mouseMovementVelocity, smoothTime);
                lastMousePosition = touch.position;
            }
            return;
        }

        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 positionDifference = Input.mousePosition - lastMousePosition;
            Vector3 moveRight = transform.right * (-positionDifference.x) * mouseMovementSpeed * Time.deltaTime;
            Vector3 moveForawrd = transform.forward * (-positionDifference.y) * mouseMovementSpeed * Time.deltaTime;

            moveRight.y = 0;
            moveForawrd.y = 0;

            Vector3 movememnt = moveRight + moveForawrd;
            Vector3 targetPosition = transform.position + movememnt;


            if (Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
            {
                targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;
            }


            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref mouseMovementVelocity, smoothTime);
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleEdgeMovement()
    {
        Vector3 targetPosition = transform.position;
        Vector3 mousePosition = Input.mousePosition;
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        if (mousePosition.x > screenWidth - edgeTreshold)
            targetPosition += transform.right * edgeMovementSpeed * Time.deltaTime;

        if(mousePosition.x < edgeTreshold)
            targetPosition -= transform.right * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.y > screenHeight - edgeTreshold)
            targetPosition += flatForward * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.y < edgeTreshold)
            targetPosition -= flatForward * edgeMovementSpeed * Time.deltaTime;

        if (Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
        {
            targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;
        }


        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref edgeMovementVelocity, smoothTime);
    }
}
