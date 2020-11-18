using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collidable : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    public abstract bool CheckNoodleCollision(Vector3 noodleCenter, float noodleRadius);

    public abstract Tuple<Vector3, Vector3> NoodleCollisionResponse(Vector3 noodleCenter, float noodleRadius);
}
