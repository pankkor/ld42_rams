using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour {
    public float firstPlatformDestroyDelay = 4.0f;
    public float platformDestroyDelay = 4.0f;
    public GameObject platforms;
    public Shaker shaker;

    private int destroyIndex = 0;

    void Awake() {
        if (firstPlatformDestroyDelay > 0.0f) {
            InvokeRepeating("DestroyNextPlatforms", firstPlatformDestroyDelay, platformDestroyDelay);
        }
    }

    void DestroyNextPlatforms() {
        if (platforms != null) {
            platforms.BroadcastMessage("DestroyPlatform", destroyIndex);
        }
        
        if (shaker != null) {
            shaker.AddIntensity(0.6f);
        }

        destroyIndex++;
    }
}
