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
    [SerializeField] private float zoomSpeed = 10;
    [SerializeField] private float minZoom = 3;
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

        HandleRotation();
        HandleZoom();
        //HandleEdgeMovement();
        HandleMouseMovement();
        HandleMovement();

        focusPoint.position = transform.position + (transform.forward * GetFocusPointDistance());

    }

    public void EnableCameraConrolls(bool enable) => canControll = enable;

    public float AdjustPitchValue(float value) => pitch = value;
    public float AdjustKeyboardSenseitivty(float value) => movementSpeed = value;
    public float AdjustMouseSensetivity(float value) => mouseMovementSpeed = value;

    private void HandleZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            float prevMag = (t1.position - t1.deltaPosition - (t2.position - t2.deltaPosition)).magnitude;
            float currentMag = (t1.position - t2.position).magnitude;
            float difference = currentMag - prevMag;

            Vector3 touchZoomDir = transform.forward * difference * (zoomSpeed * 0.05f);
            Vector3 touchTargetPos = transform.position + touchZoomDir;

            if (!(transform.position.y < minZoom && difference > 0) && !(transform.position.y > maxZoom && difference < 0))
                transform.position = Vector3.SmoothDamp(transform.position, touchTargetPos, ref zoomVelocity, smoothTime);

            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;
        Vector3 targetPosition = transform.position + zoomDirection;

        if (transform.position.y < minZoom && scroll > 0)
            return;

        if (transform.position.y > maxZoom && scroll < 0)
            return;

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
        if (Input.GetMouseButton(1)) // Right mouse button for rotation
        {
            float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float verticalRotation = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            pitch = Mathf.Clamp(pitch - verticalRotation, minPitch, maxPitch);

            transform.RotateAround(focusPoint.position, Vector3.up, horizontalRotation);
            transform.RotateAround(focusPoint.position, transform.right, pitch - transform.eulerAngles.x);

            transform.LookAt(focusPoint);
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
