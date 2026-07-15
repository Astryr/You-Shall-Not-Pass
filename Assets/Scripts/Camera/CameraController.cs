using UnityEngine;

// Control de cámara para PC y móvil:
//   PC     → teclado (pan), botón derecho (rotación), rueda (zoom), botón central (pan libre).
//   Móvil  → 1 dedo (pan), 2 dedos (pinch = zoom).
// Los controles se habilitan/deshabilitan desde CameraEffects según la fase del juego.
public class CameraController : MonoBehaviour
{
    [SerializeField] private bool canControll;
    [SerializeField] private Vector3 levelCenterPoint;
    [SerializeField] private float maxDistanceFromCenter;

    [Header("Movement Details")]
    [SerializeField] private float movementSpeed = 120;
    [SerializeField] private float mouseMovementSpeed = 5;

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
    private Vector3 zoomVelocity = Vector3.zero;
    private Vector3 lastMousePosition;

    // Distancia objetivo cámara↔punto de foco; se actualiza con scroll/pinch y se aplica cada frame.
    private float targetZoomDist = -1f;

    // Durante el gesto de pinch (dos dedos) se fija el punto de foco del PRIMER frame
    // del gesto para que ApplyZoom no derive la cámara conforme el raycast cambia de hit.
    private bool isPinching = false;
    private Vector3 pinnedFocusPoint;

    private void Start()
    {
        // Reforzar límites de zoom seguros en runtime para evitar que el jugador
        // se salga del área visible (independientemente de los valores del inspector).
        minZoom = Mathf.Max(minZoom, 4f);
        maxZoom = Mathf.Min(maxZoom, 16f);

        targetZoomDist = Mathf.Clamp(
            Vector3.Distance(transform.position, GetVirtualFocusPoint()),
            minZoom, maxZoom);
    }

    void Update()
    {
        if (canControll == false)
            return;

        RefreshFocusPoint();

        HandleRotation();
        RefreshFocusPoint();

        HandleZoom();
        ApplyZoom();          // se llama CADA frame para suavizar aunque no haya input
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

    // Solo lee el input y actualiza targetZoomDist; no mueve la cámara directamente.
    private void HandleZoom()
    {
        float delta = 0f;

        // Pinch con dos dedos (móvil).
        if (Input.touchCount == 2)
        {
            // Primer frame del gesto: fijamos el punto de foco actual.
            // A partir de aquí ApplyZoom() usará este punto fijo, evitando que
            // el raycast de RefreshFocusPoint() derive la cámara a posiciones aleatorias.
            if (!isPinching)
            {
                pinnedFocusPoint = GetVirtualFocusPoint();
                isPinching = true;
            }

            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            Vector2 t1Prev = t1.position - t1.deltaPosition;
            Vector2 t2Prev = t2.position - t2.deltaPosition;
            float prevMag = Vector2.Distance(t1Prev, t2Prev);
            float curMag  = Vector2.Distance(t1.position, t2.position);

            // 0.003f en lugar de 0.01f: reduce la sensibilidad del pinch ~70%.
            delta = (prevMag - curMag) * zoomSpeed * 0.003f;
        }
        else
        {
            isPinching = false;

            // Rueda del mouse (PC); scroll hacia adelante = zoom in = delta negativo en distancia.
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(scroll, 0f))
                scroll = Input.mouseScrollDelta.y * 0.08f;

            delta = -scroll * zoomSpeed;
        }

        if (Mathf.Abs(delta) < 1e-5f)
            return;

        targetZoomDist = Mathf.Clamp(targetZoomDist + delta, minZoom, maxZoom);
    }

    // Mueve la cámara suavemente hacia la distancia de zoom objetivo; se llama cada frame.
    private void ApplyZoom()
    {
        // Durante el pinch usamos el punto de foco fijado al inicio del gesto;
        // esto impide que el raycast variable derive la posición de la cámara.
        Vector3 fp = isPinching ? pinnedFocusPoint : GetVirtualFocusPoint();
        Vector3 dir = transform.position - fp;

        if (dir.sqrMagnitude < 0.0001f)
            dir = -transform.forward;

        dir.Normalize();

        Vector3 targetPos = fp + dir * targetZoomDist;

        // Solo aplica límite de distancia al centro si el valor fue configurado en Inspector.
        if (maxDistanceFromCenter > 0.01f &&
            Vector3.Distance(levelCenterPoint, targetPos) > maxDistanceFromCenter)
        {
            targetPos = levelCenterPoint +
                        (targetPos - levelCenterPoint).normalized * maxDistanceFromCenter;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position, targetPos, ref zoomVelocity, smoothTime);
    }

    // Devuelve el punto de foco asignado, o uno calculado con raycast si no hay Transform asignado.
    private Vector3 GetVirtualFocusPoint()
    {
        if (focusPoint != null)
            return focusPoint.position;

        if (Physics.Raycast(transform.position, transform.forward,
                            out RaycastHit hit, maxFocusPointDistance))
            return hit.point;

        return transform.position + transform.forward * (maxFocusPointDistance * 0.5f);
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


        if (maxDistanceFromCenter > 0.01f &&
            Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
        {
            targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref movementVelocity, smoothTime);
    }

    

    private bool isTouchDraggingUI = false;

    // Pan con 1 dedo (móvil) o botón central del mouse (PC). Ignora el gesto si empezó sobre UI.
    private void HandleMouseMovement()
    {
        // Con dos o más dedos en pantalla (gesto de pinch/zoom), bloquear el pan completamente.
        // Sin esta guarda, la llegada o salida del segundo dedo puede disparar un pan involuntario.
        if (Input.touchCount >= 2)
            return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastMousePosition = touch.position;

                isTouchDraggingUI = UnityEngine.EventSystems.EventSystem.current != null &&
                                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId);

                // Fallback manual: si el EventSystem no detectó objetos 3D
                // (PhysicsRaycaster inactivo o no configurado en la cámara),
                // verificar directamente si el toque cayó sobre un BuildSlot.
                // Evita que la cámara haga pan al tocar una casilla de construcción.
                if (!isTouchDraggingUI && Camera.main != null)
                {
                    Ray r = Camera.main.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(r, out RaycastHit rHit, Mathf.Infinity) &&
                        rHit.collider.GetComponentInParent<BuildSlot>() != null)
                        isTouchDraggingUI = true;
                }
            }
            else if (touch.phase == TouchPhase.Moved && !isTouchDraggingUI)
            {
                Vector3 positionDifference = new Vector3(touch.position.x, touch.position.y, 0) - lastMousePosition;
                Vector3 moveRight = transform.right * (-positionDifference.x) * (mouseMovementSpeed * 0.2f) * Time.deltaTime;
                Vector3 moveForawrd = transform.forward * (-positionDifference.y) * (mouseMovementSpeed * 0.2f) * Time.deltaTime;

                moveRight.y = 0;
                moveForawrd.y = 0;

                Vector3 targetPosition = transform.position + moveRight + moveForawrd;

                if (maxDistanceFromCenter > 0.01f &&
                    Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
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


            if (maxDistanceFromCenter > 0.01f &&
                Vector3.Distance(levelCenterPoint, targetPosition) > maxDistanceFromCenter)
            {
                targetPosition = levelCenterPoint + (targetPosition - levelCenterPoint).normalized * maxDistanceFromCenter;
            }


            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref mouseMovementVelocity, smoothTime);
            lastMousePosition = Input.mousePosition;
        }
    }

}
