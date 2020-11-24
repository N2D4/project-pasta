using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSpaghet : MonoBehaviour
{
    public Spaghet originalNoodle;
    public Transform PastaParent;
    public AttachmentPoint[] attachmentPoints;
    public Collidable[] collidables;
    private Spaghet spaghet;

    // Start is called before the first frame update
    void Start()
    {
        spaghet = GameObject.Instantiate(originalNoodle, PastaParent);
        SetAttachmentPoints();
        SetCollidables();
    }

    void OnEnable() {
        if(spaghet != null) {
            spaghet.gameObject.SetActive(true);
        }
    }

    void OnDisable() {
        if(spaghet != null) {
            spaghet.gameObject.SetActive(false);
        }
    }

    void OnDestroy() {
        if(spaghet != null) {
            GameObject.Destroy(spaghet.gameObject);
        }
    }

    void SetAttachmentPoints(){
        if (attachmentPoints.Length != 0) {
            spaghet.attachmentPoints = attachmentPoints;
        }
        else {
            spaghet.attachmentPoints = new AttachmentPoint[1];
            spaghet.attachmentPoints[0] = new AttachmentPoint() {nodeIndex = 0, transform = transform};
        }
    }

    void SetCollidables() {
        if (collidables.Length != 0) {
            spaghet.collidables = collidables;
        }
        else {
            spaghet.collidables = new Collidable[0];
        }
    }
}
