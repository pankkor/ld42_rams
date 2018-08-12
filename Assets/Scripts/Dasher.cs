using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dasher : MonoBehaviour {

    private PlayerControl playerControl;

    void Awake() {
        playerControl = GetComponentInParent<PlayerControl>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        InTrigger(other);
    }

    void OnTriggerStay2D(Collider2D other) {
        // we want to use stay just in case we didn't
        // leave a trigger durine subsequent dash
        InTrigger(other);
    }

    void InTrigger(Collider2D other) {
        if (other.gameObject.tag == "PlayerDashHitboxLeft"
            || other.gameObject.tag == "PlayerDashHitboxRight") {
            if (playerControl.isDashing) {
                other.transform.parent.gameObject.SendMessage("DashHit", playerControl.calcKnockBackImpulse(other.transform.position));
            }
        }
    }
}
