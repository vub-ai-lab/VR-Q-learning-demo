using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {

    public float speed = 100f;

	void Update () {
        transform.Rotate(Vector3.back, speed * Time.smoothDeltaTime);	
	}
}
