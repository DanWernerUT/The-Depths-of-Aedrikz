using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FOV : MonoBehaviour
{
    public Camera cam;
    public Slider slider;
    public TMP_Text fovText;

    void Start()
    {
        slider.minValue = 30f;
        slider.maxValue = 120f;
        slider.value = 60f;

        UpdateFOV(slider.value);
    }

    public void UpdateFOV(float value)
    {
        cam.fieldOfView = value;
        fovText.text = "FOV value: " + Mathf.RoundToInt(value);
    }
}
