using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowFrameRate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnGUI() {
        GUI.Label(new Rect(10.0f, 10.0f, 600.0f, 400.0f), "Tick time: " + Time.fixedTime + "\nReal time: " + Time.realtimeSinceStartup);
    }
}
