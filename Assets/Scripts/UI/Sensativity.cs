using UnityEngine;

public class SensitivitySettings : MonoBehaviour
{
    public static float sensitivity = 500f;
    public void SetSensitivity(float value)
    {
        sensitivity = value;
        Debug.Log("Sensitivity set to: " + sensitivity);
    }
}