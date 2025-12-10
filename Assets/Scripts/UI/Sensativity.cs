using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettings : MonoBehaviour
{
    public static float sensitivity = 500f;
    public Slider slider;

    void Start()
    {
        slider.minValue = 1f;   
        slider.maxValue = 2000f;
        slider.value = sensitivity;
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;
        Debug.Log("Sensitivity set to: " + sensitivity);
    }
}
