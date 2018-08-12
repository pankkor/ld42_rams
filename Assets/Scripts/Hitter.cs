using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitter : MonoBehaviour {
    private PlayerControl playerControl;

    void Awake() {
        playerControl = GetComponentInParent<PlayerControl>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (playerControl.rb.velocity.y < 0.0f && other.gameObject.tag == "PlayerTopHitbox") {
            other.transform.parent.gameObject.SendMessage("TopHit", playerControl.calcKnockBackImpulse(other.transform.position));
        }
    }
}
