using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour 
{
    public Vector3 shakePos = Vector3.one;
    public Vector3 shakeRotation = Vector3.one * 15.0f;
    public float frequency = 25;
    public float intensityDecay = 1;

    private float intensity = 0.0f;

    private void Update() {
        transform.localPosition = new Vector3(
            shakePos.x * (Mathf.PerlinNoise(0, Time.time * frequency) * 2 - 1),
            shakePos.y * (Mathf.PerlinNoise(1, Time.time * frequency) * 2 - 1),
            shakePos.z * (Mathf.PerlinNoise(2, Time.time * frequency) * 2 - 1)
        ) * intensity;

        transform.localRotation = Quaternion.Euler(new Vector3(
            shakeRotation.x * (Mathf.PerlinNoise(3, Time.time * frequency) * 2 - 1),
            shakeRotation.y * (Mathf.PerlinNoise(4, Time.time * frequency) * 2 - 1),
            shakeRotation.z * (Mathf.PerlinNoise(5, Time.time * frequency) * 2 - 1)
        ) * intensity);

        intensity = Mathf.Clamp01(intensity - intensityDecay * Time.deltaTime);
    }

    public void AddIntensity(float aIntensity) {
        intensity = Mathf.Clamp01(intensity + aIntensity);
    }
}
