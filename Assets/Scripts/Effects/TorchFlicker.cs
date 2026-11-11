using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchFlicker : MonoBehaviour
{
    private Light2D light2D;
    [SerializeField] private float minIntensity = 0.8f;
    [SerializeField] private float maxIntensity = 1.4f;
    [SerializeField] private float speed = 5.0f;

    private float baseIntensity;

    private void Start()
    {
        light2D = GetComponent<Light2D>();
        if (light2D != null)
        {
            baseIntensity = light2D.intensity;
        }
    }

    private void Update()
    {
        if (light2D == null) return;
        float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
        light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
