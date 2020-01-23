using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMenuHandler : MonoBehaviour
{
    private bool showUI = true;
    public GameObject UI;
    // Start is called before the first frame update
    void Start()
    {
        UI = GameObject.Find("Interface");
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Tab)) return;

        showUI = !showUI;
        UI.SetActive(showUI);
        Cursor.lockState = showUI ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = showUI;
    }
}
