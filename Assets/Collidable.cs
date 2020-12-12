using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collidable : MonoBehaviour {

    public float mass = float.PositiveInfinity;
    public float gravity = 0;
    public float damping = 0.9f;
    public Vector3 velocity;

    public virtual void FixedUpdate() {
        this.velocity *= Mathf.Pow(1 - damping, Time.fixedDeltaTime);
        this.velocity += new Vector3(0, -gravity * Time.fixedDeltaTime, 0);
        transform.position += velocity * Time.fixedDeltaTime;
    }

    public (Vector3, Vector3) collideWith(Vector3 spherePos, Vector3 sphereVelocity, float sphereMass, float sphereRadius, float elasticity) {
        if (!this.CheckSphereCollision(spherePos, sphereRadius)) {
            return (spherePos, sphereVelocity);
        }
        (Vector3, Vector3) collisionResponse = this.SphereCollisionResponse(spherePos, sphereRadius);
        Vector3 posResult = collisionResponse.Item1;
        
        Vector3 tangentialSphereVelocity = Vector3.ProjectOnPlane(sphereVelocity, collisionResponse.Item2);
        Vector3 directSphereVelocity = sphereVelocity - tangentialSphereVelocity;
        Vector3 tangentialCollidableVelocity = Vector3.ProjectOnPlane(this.velocity, collisionResponse.Item2);
        Vector3 directCollidableVelocity = this.velocity - tangentialCollidableVelocity;

        Vector3 velResult;
        if (float.IsInfinity(this.mass)) {
            velResult = tangentialSphereVelocity - elasticity * directSphereVelocity;
        } else {
            Vector3 newInelasticVelocity = 1 / (sphereMass + this.mass) * (sphereMass * directSphereVelocity + this.mass * directCollidableVelocity);
            Vector3 newElasticSphereVelocity = (sphereMass - this.mass) / (sphereMass + this.mass) * directSphereVelocity + 2 * this.mass / (sphereMass + this.mass) * directCollidableVelocity;
            Vector3 newElasticCollidableVelocity = (this.mass - sphereMass) / (sphereMass + this.mass) * directCollidableVelocity + 2 * sphereMass / (sphereMass + this.mass) * directSphereVelocity;
            velResult = tangentialSphereVelocity + (1 - elasticity) * newInelasticVelocity + elasticity * newElasticSphereVelocity;
            this.velocity = tangentialCollidableVelocity + (1 - elasticity) * newInelasticVelocity + elasticity * newElasticCollidableVelocity;
        }

        return (posResult, velResult);
    }

    public abstract bool CheckSphereCollision(Vector3 center, float radius);

    public abstract (Vector3, Vector3) SphereCollisionResponse(Vector3 center, float radius);
}
