using UnityEngine;

public class Movement : MonoBehaviour
{

    [SerializeField] private float speed = 5f;
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float jumpForce = 40;
    [SerializeField] private bool grounded;
    [SerializeField] private GameObject groundCheckObj;
    private GroundChecker groundChecker;
    private bool jumpQueued;
    private int jumpAmount = 0;
    private Rigidbody rb;
    private Transform cameraHolder;
    private Transform cameraPosition;

    private float xRotation = 0f;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found!");
            enabled = false;
            return;
        }

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        cameraHolder = transform.Find("Camera Holder");
        if (cameraHolder == null)
        {
            Debug.LogError("Camera Holder not found!");
            enabled = false;
            return;
        }

        cameraPosition = cameraHolder.Find("Camera Position");
        if (cameraPosition == null)
        {
            Debug.LogError("Camera Position not found inside Camera Holder!");
            enabled = false;
            return;
        }

        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Main Camera not found in scene!");
            enabled = false;
            return;
        }

        groundChecker = groundCheckObj.GetComponent<GroundChecker>();

        mainCam.transform.SetParent(cameraPosition);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool wasGrounded;

    void Update()
    {
        grounded = groundChecker.isGrounded();

        if (grounded && !wasGrounded)
        {
            jumpAmount = 0;
        }
        wasGrounded = grounded;

        handleMouseLook();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpAmount < 2)
            {
                jumpQueued = true;
                jumpAmount++;
            }
        }
    }


    void FixedUpdate()
    {
        handleMovementAndRotation();
        if (jumpQueued)
        {
            handleJump();
        }
    }

    private void handleMouseLook()
    {
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 90f);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void handleMovementAndRotation()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity * Time.fixedDeltaTime;
        Quaternion rotationX = Quaternion.Euler(0f, mouseX, 0f);
        rb.MoveRotation(rb.rotation * rotationX);

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = rb.transform.right * horizontal + rb.transform.forward * vertical;
        rb.MovePosition(rb.position + move.normalized * speed * Time.fixedDeltaTime);
    }

    public void handleJump()
    {
        rb.AddForce(Vector3.up * jumpForce);
        jumpQueued = false;
    }

    public void setRb(Rigidbody newRb)
    {
        rb = newRb;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        xRotation = cameraHolder.localEulerAngles.x;
    }

    public void setCameraHolder(Transform _cameraHolder)
    {
        cameraHolder = _cameraHolder;
        cameraPosition = cameraHolder.Find("Camera Position");

        if (cameraPosition == null)
        {
            Debug.LogError("Camera Position not found inside new Camera Holder!");
            return;
        }

        mainCam.transform.SetParent(cameraPosition);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;

        xRotation = cameraHolder.localEulerAngles.x;
    }
}