using UnityEngine;

public class SatelliteMovement : MonoBehaviour
{
    public float timeMult = 1.0f;
    int substeps = 1;
    const float safetyFactor = 0.007f;
    const float G = 6.6743e-11f;

    public GameObject moon;
    public LineRenderer orbitLine;
    public int orbitPoints = 200;

    public TMPro.TMP_Text timeMultValueLabel;
    public TMPro.TMP_Text timeValueLabel;
    float runtimeSec = 0f;
    public TMPro.TMP_InputField massCoefInput;
    public TMPro.TMP_InputField massExpInput;
    public TMPro.TMP_Text apsisLabel;
    public TMPro.TMP_Text speedLabel;
    public TMPro.TMP_Text altitudeLabel;
    public TMPro.TMP_Text radiusLabel;
    public TMPro.TMP_Text gravityAccelLabel;

    // Moon properties
    public float moonM = 7.35e22f;
    public float moonR = 1737f;

    // Satellite properties
    public float initVX_km;
    public float initVY_km;
    public Vector2 v_km;
    Vector2 a_km;
    float debugApoapsis;
    float debugPeriapsis;

    void Start()
    {
        v_km = new Vector2(initVX_km, initVY_km);
        a_km = ComputeAcceleration(transform.position);

        debugApoapsis = transform.position.magnitude;
        debugPeriapsis = transform.position.magnitude;
    }

    void FixedUpdate()
    {
        float dt_total = Time.fixedDeltaTime * timeMult;

        float rMag_km = transform.position.magnitude;
        float vMag_km = v_km.magnitude;

        float dt_step = safetyFactor * rMag_km / Mathf.Max(vMag_km, 1e-6f);     // arclength equals r * dtheta because fraction of circle = dtheta/2pi (partial angle over full angle) times full circumference
                                                                                // time to travel that arclength is over v which is dtheta*r/v. we have r and v so we plug in how much dtheta
        substeps = Mathf.CeilToInt(dt_total / dt_step);
        substeps = Mathf.Max(1, substeps);
        dt_step = dt_total / substeps;

        Vector2 pos = transform.position;
        for (int i = 0; i < substeps; i++)
        {
            StepVerlet(ref pos, dt_step);
        }
        transform.position = pos;

        // UI stuff
        DrawKeplerOrbit();

        runtimeSec += dt_total;
        float runtimeHour = runtimeSec / 3600f;
        timeValueLabel.text = $"Seconds: {FormatNumber(runtimeSec)}\nHours: {FormatNumber(runtimeHour)}";

        apsisLabel.text = $"Apoapsis: {FormatNumber(debugApoapsis)}\nPeriapsis: {FormatNumber(debugPeriapsis)}";

        speedLabel.text = $"Speed:\n{FormatNumber(vMag_km)} km/s";

        altitudeLabel.text = $"Altitude:\n{FormatNumber(rMag_km - moonR)} km";

        gravityAccelLabel.text = $"Gravity Accel:\n{FormatNumber(a_km.magnitude * 1000)} m/ss";
    }

    void StepVerlet(ref Vector2 pos, float dt)
    {
        Vector2 newPos = pos + v_km * dt + 0.5f * a_km * dt * dt;       // Velocity verlet equation (assumes constant a over dt, but averages old and new a)

        Vector2 newA_km = ComputeAcceleration(newPos);

        v_km += 0.5f * (a_km + newA_km) * dt;

        a_km = newA_km;

        pos = new Vector3(newPos.x, newPos.y, 0f);
    }

    Vector2 ComputeAcceleration(Vector2 pos_km)
    {
        float r_m = pos_km.magnitude * 1000f;
        float a_m;
        if (r_m / 1000 < moonR)        // Assumes even density
        {
            float density = moonM / ((4f / 3f) * Mathf.PI * Mathf.Pow(moonR * 1000f, 3f));
            a_m = G * (density * ((4f / 3f) * Mathf.PI * Mathf.Pow(r_m, 3f))) / (r_m * r_m);
        }
        else
        {
            a_m = G * moonM / (r_m * r_m);
        }


            Vector2 dir = -pos_km.normalized;
        return dir * (a_m / 1000f);
    }

    string FormatNumber(float value)
    {
        if ((Mathf.Abs(value) >= 1e6 || Mathf.Abs(value) < 1e-3f) && value != 0f)
            return value.ToString("0.##E0"); // scientific notation
        else
            return value.ToString("0.####"); // regular decimal
    }

    void DrawKeplerOrbit()
    {
        Vector2 r_m = transform.position * 1000f;
        Vector2 v_m = v_km * 1000f;

        // Gravitational parameter
        float mu = G * moonM; // m^3/s^2

        // Specific angular momentum (m^2/s)
        float h = r_m.x * v_m.y - r_m.y * v_m.x;

        // Eccentricity vector
        Vector2 eVec = new Vector2((v_m.y * h / mu) - r_m.normalized.x, (-v_m.x * h / mu) - r_m.normalized.y);
        float e = eVec.magnitude;

        // Orbital energy
        float energy = 0.5f * v_m.sqrMagnitude - mu / r_m.magnitude;

        // Semi-major axis
        float a = -mu / (2f * energy); // meters

        // LineRenderer
        orbitLine.positionCount = orbitPoints;

        float angleOffset = Mathf.Atan2(eVec.y, eVec.x);

        for (int i = 0; i < orbitPoints; i++)
        {
            float theta = i * 2f * Mathf.PI / orbitPoints;
            float rOrbit_m = a * (1 - e * e) / (1 + e * Mathf.Cos(theta));

            float x_km = rOrbit_m * Mathf.Cos(theta + angleOffset) / 1000f;
            float y_km = rOrbit_m * Mathf.Sin(theta + angleOffset) / 1000f;

            orbitLine.SetPosition(i, new Vector3(x_km, y_km, 0f));
        }

        debugApoapsis = a * (1 + e) / 1000f;
        debugPeriapsis = a * (1 - e) / 1000f;
    }




    public void SetTimeMult(float sliderValue)
    {
        timeMult = 100000 * Mathf.Pow(sliderValue, 8.39905f);
        timeMultValueLabel.text = $"x{timeMult:F1}";
    }

    public void SetRadius(float sliderValue)
    {
        moonR = 10000f * sliderValue;
        radiusLabel.text = $"{FormatNumber(moonR)} km";
        
        moon.transform.localScale = new Vector3(moonR * 2, moonR * 2, 1f);
    }

    public void ResetTimeLog()
    {
        runtimeSec = 0f;
    }

    public void ResetMoonMass()
    {
        moonM = 7.35e22f;
        massCoefInput.text = "7.35";
        massExpInput.text = "22";

        SetRadius(1737f/10000f);
    }

    public void MassEdited()
    {
        if (float.Parse(massCoefInput.text) < 0)
        {
            massCoefInput.text = "0";
        }
        if (float.Parse(massExpInput.text) > 35)
        {
            massExpInput.text = "35";
        }
        if (float.Parse(massExpInput.text) < -35)
        {
            massExpInput.text = "-35";
        }
        moonM = float.Parse(massCoefInput.text) * Mathf.Pow(10f, float.Parse(massExpInput.text));
    }

    public void ResetApsis()
    {
        debugApoapsis = 0f;
        debugPeriapsis = 1e36f;
    }

    public void ResetSatellite()
    {
        transform.position = new Vector2(0f, moonR + 100f);
        v_km = new Vector2(0f, 0f);
        ResetApsis();
    }
}
// TODO LIST
// Velocity player controller (hard)