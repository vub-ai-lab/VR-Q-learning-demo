using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a simple script to make an object rotate on its z axis.
public class Spin : MonoBehaviour {

    public float speed = 100f;

	void Update () {
        transform.Rotate(Vector3.back, speed * Time.smoothDeltaTime);	
	}
}
