﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
//             other.gameObject.SendMessage("Die");
            Application.LoadLevel(Application.loadedLevel);
        }
    }
    
}
