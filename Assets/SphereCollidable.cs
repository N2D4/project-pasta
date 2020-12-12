using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollidable : Collidable {
    public float radius = 1;
    public float elasticity = 0.8f;
    public Collidable[] collidables;

    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        transform.localScale = 2 * new Vector3(radius, radius, radius);
    }

    public override void FixedUpdate() {
        base.FixedUpdate();
        foreach (var collidable in collidables) {
            (transform.position, this.velocity) = collidable.collideWith(transform.position, this.velocity, this.mass, this.radius, elasticity);
        }
    }

    public override bool CheckSphereCollision(Vector3 center, float radius) {
        return (transform.position - center).sqrMagnitude <= Mathf.Pow(radius + this.radius, 2);
    }

    public override (Vector3, Vector3) SphereCollisionResponse(Vector3 center, float otherRadius) {
        Vector3 normalVector = center - transform.position;
        normalVector.Normalize();
        return (
            transform.position + normalVector * (this.radius + otherRadius),
            normalVector
        );
    }
}
