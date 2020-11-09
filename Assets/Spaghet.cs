using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NoodleNode {
    public Vector3 position;
    public Vector3 velocity;
    private Vector3 accel;
    public GameObject obj;

    public NoodleNode(Vector3 position, GameObject obj) {
        this.position = position;
        this.obj = obj;
        this.velocity = new Vector3();
        this.accel = new Vector3();
    }

    public void addAcceleration(Vector3 accel) {
        this.accel += accel;
    }

    public void applyDamping(float damping, float timestep) {
        float factor = Mathf.Pow(1 - damping, timestep);
        this.velocity *= factor;
    }

    public void stepTime(float timestep) {
        this.velocity += this.accel * timestep;
        this.accel = new Vector3();
        this.position += this.velocity * timestep;
    }

    public override String ToString() {
        return "{position: " + position + ", velocity: " + velocity + ", accel: " + accel + "}";
    }
}

[Serializable]
public struct AttachmentPoint {
    public int nodeIndex;
    public Transform transform;
}

public class Spaghet : MonoBehaviour {
    public float gravity = 9.8065f;
    public float springConstant = 0.01f;
    public float totalMass = 0.01f;
    public float damping = 0.1f;
    public int nodeCount = 10;
    public GameObject nodeOriginal;
    public AttachmentPoint[] attachmentPoints;

    private Dictionary<int, Transform> attachPoints;
    private List<NoodleNode> nodes;

    private float nodeMass {
        get {
            return totalMass / nodeCount;
        }
    }

    void Start() {
        attachPoints = new Dictionary<int, Transform>();
        foreach (var ap in attachmentPoints) {
            attachPoints[ap.nodeIndex] = ap.transform;
        }

        nodes = new List<NoodleNode>();
        for (int i = 0; i < nodeCount; i++) {
            GameObject obj = GameObject.Instantiate(nodeOriginal, gameObject.transform);
            nodes.Add(new NoodleNode(new Vector3(0, i, i), obj));
        }
    }

    void FixedUpdate() {
        // attachment points
        foreach (int key in attachPoints.Keys) {
            NoodleNode node = nodes[key];
            node.position = attachPoints[key].transform.position - gameObject.transform.position;
        }

        // gravity
        for (int i = 0; i < nodes.Count; i++) {
            if (!attachPoints.ContainsKey(i)) {
                nodes[i].addAcceleration(new Vector3(0, -gravity, 0));
            }
        }

        // spring force
        for (int i = 0; i < nodes.Count - 1; i++) {
            NoodleNode n1 = nodes[i];
            NoodleNode n2 = nodes[i+1];
            Vector3 force = (n2.position - n1.position) * springConstant;
            if (!attachPoints.ContainsKey(i)) {
                n1.addAcceleration(force / nodeMass);
            }
            if (!attachPoints.ContainsKey(i+1)) {
                n2.addAcceleration(- force / nodeMass);
            }
        }

        // step time
        foreach (var node in nodes) {
            node.applyDamping(damping, Time.fixedDeltaTime);
            node.stepTime(Time.fixedDeltaTime);
        }
    }

    void Update() {
        foreach (var node in nodes) {
            node.obj.transform.localPosition = node.position;
        }
    }
}
