using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDown : MonoBehaviour
{

    public float speed = 1;
    public float height = 20;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var pos = gameObject.transform.localPosition;
        pos.y = Mathf.Sin(Time.fixedTime * speed) * height;
        gameObject.transform.localPosition = pos;
    }
}
