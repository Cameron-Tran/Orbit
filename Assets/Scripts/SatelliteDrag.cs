using UnityEngine;

public class SatelliteDrag : MonoBehaviour
{
    public SatelliteMovement sat;
    public float velSensitivity = 0.1f;

    Vector3 offset;
    bool draggingPos = false;
    bool draggingVel = false;

    void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;
        Vector2 mouse2D = new Vector2(mouseWorld.x, mouseWorld.y);

        // --- LEFT CLICK: drag position ---
        if (Input.GetMouseButtonDown(0))
        {
            if (Vector2.Distance(mouse2D, transform.position) < 50f) // adjust radius
            {
                offset = transform.position - mouseWorld;
                draggingPos = true;
            }
        }
        if (Input.GetMouseButtonUp(0)) draggingPos = false;

        if (draggingPos)
        {
            transform.position = mouseWorld + offset;
            sat.ResetApsis(); // reset apo/periapsis tracking
        }

        // --- RIGHT CLICK: drag velocity ---
        if (Input.GetMouseButtonDown(1))
        {
            if (Vector2.Distance(mouse2D, transform.position) < 50f) // start only if click on satellite
            {
                draggingVel = true;
            }
        }
        if (Input.GetMouseButtonUp(1)) draggingVel = false;

        if (draggingVel)
        {
            Vector3 dir = mouseWorld - transform.position;
            sat.v_km = new Vector2(dir.x * velSensitivity, dir.y * velSensitivity); // set velocity in km/s
        }
    }
}
