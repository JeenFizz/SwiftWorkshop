using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WS3;

public class IBelieveICanFly : MonoBehaviour
{
    private bool meIsFly = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        meIsFly = !meIsFly;
        GetComponent<UserManager>().goFreeLookCameraRig.transform.Find("Pivot").transform.position += (meIsFly ? Vector3.up : Vector3.down) * 20;
    }
}
