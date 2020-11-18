using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollidable : Collidable {
    public float radius = 1;

    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        transform.localScale = 2 * new Vector3(radius, radius, radius);
    }

    public override bool CheckNoodleCollision(Vector3 noodleCenter, float noodleRadius) {
        return (transform.position - noodleCenter).sqrMagnitude <= Mathf.Pow(noodleRadius + this.radius, 2);
    }

    public override Tuple<Vector3, Vector3> NoodleCollisionResponse(Vector3 noodleCenter, float noodleRadius) {
        Vector3 normalVector = noodleCenter - transform.position;
        normalVector.Normalize();
        return new Tuple<Vector3, Vector3>(
            transform.position + normalVector * (radius + noodleRadius),
            normalVector
        );
    }
}
