using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private Camera cam;     // Initialize Camera and variables

    InputAction moveAction;
    InputAction scrollAction;
    InputAction quickMoveAction;
    public float speed = 5f;        // Camera movement speed
    public float initZoom = 2048f;
    public float initX = 0f;
    public float initY = 0f;
    public float zoomMult = 50f;        // How much zoom input is multiplied by on effect
    float dz = 0f;      // Scroll input (delta zoom)
    public float minZoomOut = 1f;
    public float maxZoomOut = 8192f;

    void Start()
    {
        cam = GetComponent<Camera>();       // Find Camera Object
        moveAction = InputSystem.actions.FindAction("Move");        // Assign input keys
        scrollAction = InputSystem.actions.FindAction("Zoom");
        quickMoveAction = InputSystem.actions.FindAction("Sprint");
        transform.position = new Vector3(initX, initY, -10f);       // Default position on startup
        cam.orthographicSize = initZoom;        // Default zoom on startup
    }


    void Update()
    {
        Vector2 moveVector = moveAction.ReadValue<Vector2>();       // Read movement input
        dz = scrollAction.ReadValue<float>();       // Set zoom var scrolling input
        dz = Mathf.Clamp(dz, -1, 1);     // Clamp delta zoom between -1 and 1
        zoomMult = cam.orthographicSize / 10;        // Set zoomMult based on Camera viewing size
        dz *= zoomMult;
        Vector2 xMoveVector = new Vector2(moveVector.x, 0f);        // Split movement input into x and y
        Vector2 yMoveVector = new Vector2(0f, moveVector.y);
        cam.orthographicSize -= dz;       // Zoom var affects Camera viewing size
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoomOut, maxZoomOut);       // Clamp Camera viewing size to corresponding vars
        speed = cam.orthographicSize + 5;   // Set Camera movement speed based on Camera viewing size
        if (quickMoveAction.IsPressed())        // Hold sprint button to double Camera movement speed
        {
            speed *= 2;
        }

        transform.Translate(xMoveVector * speed * Time.deltaTime);      // Apply movements to Camera object position
        transform.Translate(yMoveVector * speed * Time.deltaTime);
    }

    public void ResetCamera()
    {
        cam.orthographicSize = initZoom;
        transform.position = new Vector3(0f, 0f, -10f);
    }
}
