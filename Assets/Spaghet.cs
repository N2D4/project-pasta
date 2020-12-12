using UnityEditor;
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
    public float mass;

    public NoodleNode(Vector3 position, float radius, GameObject obj, float mass) {
        this.position = position;
        this.radius = radius;
        this.obj = obj;
        this.mass = mass;
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
        Vector3 pos = parentPos + this.position;
        var (newPos, newVel) = collidable.collideWith(pos, this.velocity, this.mass, this.radius, 0);
        this.position = newPos - parentPos;
        this.velocity = newVel;
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
    public float curvatureConstantContraction = 0f;
    public float curvatureConstantExpansion = 0.005f;
    public float targetCurvature = 1.9f;
    public float timeScale = 1f;
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
    public bool addConnections;

    private Dictionary<int, Transform> attachPoints;
    private List<NoodleNode> nodes;
    private List<NoodleConnection> connections;

    private float nodeMass {
        get {
            return totalMass / nodes.Count;
        }
    }
    private float springLength {
        get {
            if (keepLengthConstant) {
                return totalLength / nodes.Count;
            }
            return noodleRadius * 2;
        }
    }

    private bool _connectionsActive;
    private bool connectionsActive {
        get {return _connectionsActive; }
        set {
            foreach (NoodleConnection connection in connections) {
                connection.obj.SetActive(value);
            }
                _connectionsActive = value;
        }
    }

    void ResetNoodle() {
        // AttachmentPoints
        attachPoints = new Dictionary<int, Transform>();
        foreach (var ap in attachmentPoints) {
            attachPoints[ap.nodeIndex] = ap.transform;
        }

        // NoodleNodes
        if (nodes != null) {
            foreach (NoodleNode node in nodes) {
                GameObject.Destroy(node.obj);
            }
        }
        nodes = new List<NoodleNode>();
        for (int i = 0; i < nodeCount; i++) {
            GameObject obj = GameObject.Instantiate(nodeOriginal, gameObject.transform);
            obj.transform.localScale = new Vector3(1, 1, 1) * noodleRadius * 2;
            nodes.Add(new NoodleNode(new Vector3(0, i, i), noodleRadius, obj, nodeMass));
        }

        // NoodleConnections
        if (connections != null) {
            foreach (NoodleConnection connection in connections) {
                GameObject.Destroy(connection.obj);
            }
        }
        connections = new List<NoodleConnection>();
        for (int i = 0; i < nodes.Count - 1; i++) {
            GameObject obj = GameObject.Instantiate(connectionOriginal, gameObject.transform);
            connections.Add(new NoodleConnection(nodes[i], nodes[i+1], obj));
        }
        connectionsActive = true;
    }

    void Start() {
        ResetNoodle();
    }

    void FixedUpdate() {
        if (resetNoodle || attachmentPoints == null || nodes == null || connections == null) {
            foreach (Transform child in transform) {
               Destroy(child.gameObject);
            }
            ResetNoodle();
            resetNoodle = false;
        }

        // attachment points
        foreach (int key in attachPoints.Keys) {
            if (key < nodes.Count) {
                nodes[key].position = attachPoints[key].transform.position - gameObject.transform.position;
            }
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
            Vector3 force = dif * forceFac * (forceFac >= 0 ? springConstantContraction : springConstantExpansion);
            
            if (!attachPoints.ContainsKey(i)) {
                n1.addAcceleration(force / nodeMass);
            }
            if (!attachPoints.ContainsKey(i+1)) {
                n2.addAcceleration(- force / nodeMass);
            }
        }

        // curvature force
        for (int i = 0; i < nodes.Count - 2; i++) {
            NoodleNode n1 = nodes[i];
            NoodleNode n2 = nodes[i+2];

            Vector3 dif = n2.position - n1.position;
            float forceFac = (dif.magnitude - targetCurvature * springLength) / dif.magnitude;
            Vector3 force = dif * forceFac * (forceFac >= 0 ? curvatureConstantContraction : curvatureConstantExpansion);
            
            if (!attachPoints.ContainsKey(i)) {
                n1.addAcceleration(force / nodeMass);
            }
            if (!attachPoints.ContainsKey(i+2)) {
                n2.addAcceleration(- force / nodeMass);
            }
        }

        // apply damping
        foreach (var node in nodes) {
            node.applyDamping(damping, Time.fixedDeltaTime * timeScale);
        }

        // step time
        if (enableStepTime) {
            foreach (var node in nodes) {
                node.stepTime(Time.fixedDeltaTime * timeScale);
            }
        }

        // collision
        for (int i = 0; i < nodes.Count; i++) {
            if (!attachPoints.ContainsKey(i)) {
                foreach (var collidable in collidables) {
                    nodes[i].collideWith(transform.position, collidable);
                }
            }
        }
    }

    void Update() {
        //Debug.Log("attachPoints.Count: " + attachPoints.Count + " | nodes.Count: " + nodes.Count + " | connections.Count: " + connections.Count);
        foreach (var node in nodes) {
            node.obj.transform.localPosition = node.position;
        }

        if(!addConnections) {
            if(connectionsActive) {
                connectionsActive = false;
            }
        } 
        else {
            if (!connectionsActive) {
                connectionsActive = true;
            }
            foreach (NoodleConnection connection in connections) {
                connection.UpdateConnection();
            }
        }
    }
}
