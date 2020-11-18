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

    public override bool CheckNoodleCollision(Vector3 noodleCenter, float noodleRadius) {
        Vector3 localVector = transform.InverseTransformPoint(noodleCenter);
        Vector3 directionToNoodle = noodleCenter - transform.position;

        localVector.x = localVector.x * transform.localScale.x;
        localVector.y = localVector.y * transform.localScale.y;
        localVector.z = localVector.z * transform.localScale.z;

        bool pointInBigBox = (
            localVector.x >= -0.5 * transform.localScale.x -noodleRadius && localVector.x <= 0.5 * transform.localScale.x  + noodleRadius &&
            localVector.y >= -0.5 * transform.localScale.y -noodleRadius && localVector.y <= 0.5 * transform.localScale.y  + noodleRadius &&
            localVector.z >= -0.5 * transform.localScale.z -noodleRadius && localVector.z <= 0.5 * transform.localScale.z  + noodleRadius);

        if (!pointInBigBox){
            return false;
        }

        // Add more cases if necessary

        return true;
    }

    public override Tuple<Vector3, Vector3> NoodleCollisionResponse(Vector3 noodleCenter, float noodleRadius) {
        Vector3 directionVector = noodleCenter - transform.position;
        Vector3 localVector = transform.InverseTransformDirection(directionVector);

        Vector3 normalVector;
        Vector3 positionVector;
        Vector3 tmpVector = new Vector3(
            Math.Abs(localVector.x / transform.localScale.x),
            Math.Abs(localVector.y / transform.localScale.y),
            Math.Abs(localVector.z / transform.localScale.z)
        );
        if (tmpVector.x > tmpVector.y && tmpVector.x > tmpVector.z) {
            positionVector = new Vector3(Math.Sign(localVector.x) * (transform.localScale.x / 2 + noodleRadius), localVector.y, localVector.z);
            normalVector = new Vector3(Math.Sign(localVector.x), 0, 0);
        } else if (tmpVector.x > tmpVector.z) {
            positionVector = new Vector3(localVector.x, Math.Sign(localVector.y) * (transform.localScale.y / 2 + noodleRadius), localVector.z);
            normalVector = new Vector3(0, Math.Sign(localVector.y), 0);
        } else {
            positionVector = new Vector3(localVector.x, localVector.y, Math.Sign(localVector.z) * (transform.localScale.z / 2 + noodleRadius));
            normalVector = new Vector3(0, 0, Math.Sign(localVector.z));
        }
        
        return new Tuple<Vector3, Vector3>(
            transform.position + transform.TransformDirection(positionVector),
            transform.position + transform.TransformDirection(normalVector)
        );
    }
}
