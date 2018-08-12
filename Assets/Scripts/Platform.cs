using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform: MonoBehaviour {

    [Range(0, 50)]
    public int destroyIndex = 0;

    public float fallTorque = 3.0f;

    private bool isDestroyed = false;

    public Rigidbody2D rb;
    private Animator animator;

    void Awake() {
        animator = GetComponent<Animator>();
    }
    
    void DestroyPlatform(int index) {
        if (!isDestroyed && destroyIndex == index) {
            animator.SetTrigger("destroy");
            isDestroyed = true;
        }
    }

    public void PhysicallyFall() {
        animator.enabled = false;
        rb.bodyType = RigidbodyType2D.Dynamic;

        float torque = Random.Range(-1.0f, 1.0f) * fallTorque;
        rb.AddTorque(torque, ForceMode2D.Impulse);
    }
}
