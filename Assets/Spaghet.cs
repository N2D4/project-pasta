using UnityEngine;
using System;
using System.Collections.Generic;

public class NoodleNode {
    /**
     * Local position
     */
    public Vector3 position;
    public float radius;
    public Vector3 velocity;
    private Vector3 accel;
    public GameObject obj;

    public NoodleNode(Vector3 position, float radius, GameObject obj) {
        this.position = position;
        this.radius = radius;
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

    public void collideWith(Vector3 parentPos, Collidable collidable) {
        Vector3 pos = parentPos + position;
        if (!collidable.CheckNoodleCollision(pos, radius)) {
            return;
        }
        Tuple<Vector3, Vector3> collisionResponse = collidable.NoodleCollisionResponse(pos, radius);
        this.position = collisionResponse.Item1 - parentPos;
        this.velocity = Vector3.ProjectOnPlane(this.velocity, collisionResponse.Item2);
    }
}

public class NoodleConnection {
    /**
     * Local position
     */
    public NoodleNode node1;
    public NoodleNode node2;
    public GameObject obj;
    public Vector3 direction;
    public float distance;
    public float radius;

    public NoodleConnection(NoodleNode node1, NoodleNode node2, GameObject obj) {
        this.node1 = node1;
        this.node2 = node2;
        this.obj = obj;
        UpdateConnection();
    }

    public void UpdateConnection() {
        Vector3 v = node2.position - node1.position;
        this.direction = v.normalized;
        this.distance = v.magnitude;
        this.radius = node1.radius;
        obj.transform.localPosition = node1.position + direction * (distance / 2);
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
        obj.transform.localScale = new Vector3(radius * 2, distance / 2, radius * 2);
    }
}

[Serializable]
public struct AttachmentPoint {
    public int nodeIndex;
    public Transform transform;
}


public class Spaghet : MonoBehaviour {
    public float gravity = 9.8065f;
    public float springConstantContraction = 0.01f;
    public float springConstantExpansion = 0.01f;
    public float totalMass = 0.01f;
    public float damping = 0.1f;
    public float noodleRadius = 0.5f;
    public bool keepLengthConstant = true;
    public float totalLength = 10;
    public int nodeCount = 10;
    public GameObject nodeOriginal;
    public GameObject connectionOriginal;
    public AttachmentPoint[] attachmentPoints;
    public Collidable[] collidables;
    public bool resetNoodle = false;
    public bool enableStepTime = true;
    public bool addConnetions = true;

    private Dictionary<int, Transform> attachPoints;
    private List<NoodleNode> nodes;
    private List<NoodleConnection> connections;

    private float nodeMass {
        get {
            return totalMass / nodeCount;
        }
    }
    private float springLength {
        get {
            if (keepLengthConstant) {
                return totalLength / nodeCount;
            }
            return noodleRadius * 2;
        }
    }

    void ResetNoodle() {
        attachPoints = new Dictionary<int, Transform>();
        foreach (var ap in attachmentPoints) {
            attachPoints[ap.nodeIndex] = ap.transform;
        }

        if (nodes != null) {
            foreach (NoodleNode node in nodes) {
                GameObject.Destroy(node.obj);
            }
        }
        nodes = new List<NoodleNode>();
        for (int i = 0; i < nodeCount; i++) {
            GameObject obj = GameObject.Instantiate(nodeOriginal, gameObject.transform);
            obj.transform.localScale = new Vector3(1, 1, 1) * noodleRadius * 2;
            nodes.Add(new NoodleNode(new Vector3(0, i, i), noodleRadius, obj));
        }

        if (connections != null) {
            foreach (NoodleConnection connection in connections) {
                GameObject.Destroy(connection.obj);
            }
        }
        connections = new List<NoodleConnection>();
        if(addConnetions) {
            for (int i = 0; i < nodeCount - 1; i++) {
                GameObject obj = GameObject.Instantiate(connectionOriginal, gameObject.transform);
                connections.Add(new NoodleConnection(nodes[i], nodes[i+1], obj));
            }
        }
        
    }

    void Start() {
        ResetNoodle();
    }

    void FixedUpdate() {
        if (resetNoodle) {
            ResetNoodle();
            resetNoodle = false;
        }

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

            Vector3 dif = n2.position - n1.position;
            float forceFac = (dif.magnitude - springLength) / dif.magnitude;
            Vector3 force = dif * forceFac * (forceFac >= 0 ? springConstantExpansion : springConstantContraction);
            
            if (!attachPoints.ContainsKey(i)) {
                n1.addAcceleration(force / nodeMass);
            }
            if (!attachPoints.ContainsKey(i+1)) {
                n2.addAcceleration(- force / nodeMass);
            }
        }

        // apply damping
        foreach (var node in nodes) {
            node.applyDamping(damping, Time.fixedDeltaTime);
        }

        // step time
        if (enableStepTime) {
            foreach (var node in nodes) {
                node.stepTime(Time.fixedDeltaTime);
            }
        }

        // collisions
        foreach (var node in nodes) {
            foreach (var collidable in collidables) {
                node.collideWith(transform.position, collidable);
            }
        }
    }

    void Update() {
        foreach (var node in nodes) {
            node.obj.transform.localPosition = node.position;
        }
        foreach (NoodleConnection connection in connections){
            connection.UpdateConnection();
        }
    }
}
