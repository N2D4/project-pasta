using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCollidable : Collidable
{
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    private static float TowardsZero(float a, float b) {
        return Mathf.Max(Mathf.Abs(a) - b, 0) * Mathf.Sign(a);
    }

    private static float CapAt(float a, float b) {
        return Mathf.Min(Mathf.Abs(a), b) * Mathf.Sign(a);
    }

    private (Vector3, Vector3) DecomposeVector(Vector3 localVector) {
        return (
            new Vector3(
                CapAt(localVector.x, transform.localScale.x / 2),
                CapAt(localVector.y, transform.localScale.y / 2),
                CapAt(localVector.z, transform.localScale.z / 2)
            ),
            new Vector3(
                TowardsZero(localVector.x, transform.localScale.x / 2),
                TowardsZero(localVector.y, transform.localScale.y / 2),
                TowardsZero(localVector.z, transform.localScale.z / 2)
            )
        );
    }

    private static float sq(float f) {
        return f * f;
    }

    public override bool CheckSphereCollision(Vector3 center, float radius) {
        Vector3 localVector = transform.InverseTransformPoint(center);
        localVector.x = localVector.x * transform.localScale.x;
        localVector.y = localVector.y * transform.localScale.y;
        localVector.z = localVector.z * transform.localScale.z;

        Vector3 collapsedVector = new Vector3(
            Mathf.Max(Mathf.Abs(localVector.x) - transform.localScale.x / 2, 0),
            Mathf.Max(Mathf.Abs(localVector.y) - transform.localScale.y / 2, 0),
            Mathf.Max(Mathf.Abs(localVector.z) - transform.localScale.z / 2, 0)
        );

        return collapsedVector.sqrMagnitude <= sq(radius);
    }

    public override (Vector3, Vector3) SphereCollisionResponse(Vector3 center, float radius) {
        Vector3 localVector = transform.InverseTransformPoint(center);
        localVector.x = localVector.x * transform.localScale.x;
        localVector.y = localVector.y * transform.localScale.y;
        localVector.z = localVector.z * transform.localScale.z;
        
        (Vector3, Vector3) decomposed = DecomposeVector(localVector);
        if ((decomposed.Item1 + decomposed.Item2 - localVector).sqrMagnitude > 0.0000001) {
            throw new Exception("Vectors not equal! " + decomposed + " " + localVector);
        }

        Vector3 normalVector = decomposed.Item2;
        if (normalVector.sqrMagnitude < 0.000001) {
            normalVector = decomposed.Item1; // TODO fix
        }
        normalVector.Normalize();

        Vector3 newBoxOffset = decomposed.Item1 * Mathf.Min(
            Mathf.Abs(transform.localScale.x / 2 / decomposed.Item1.x),
            Mathf.Abs(transform.localScale.y / 2 / decomposed.Item1.y),
            Mathf.Abs(transform.localScale.z / 2 / decomposed.Item1.z)
        );
        Vector3 newEdgeOffset = normalVector * radius;
        Vector3 newOffset = newBoxOffset + newEdgeOffset;

        return (
            transform.TransformPoint(new Vector3(
                newOffset.x / transform.localScale.x,
                newOffset.y / transform.localScale.y,
                newOffset.z / transform.localScale.z
            )),
            transform.TransformDirection(normalVector)
        );
    }
}
