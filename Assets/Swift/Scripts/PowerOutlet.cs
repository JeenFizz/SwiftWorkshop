using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerOutlet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTrigger(other, true);
    }

    private void OnTriggerExit(Collider other)
    {
        OnTrigger(other, false);
    }

    private void OnTrigger(Collider other, bool enter)
    {
        if (new string[] { "P", "T", "G", "F" }.Contains(other.tag))
        {
            foreach (Transform border in other.transform.Find("FreeBorder"))
            {
                MeshRenderer mesh = border.GetComponent<MeshRenderer>();
                mesh.material.color = enter ? Color.cyan : new Color() { a=.5f, r=.853f, g=.512f, b=0 };
            }
            other.transform.Find("PowerLight").GetComponent<Light>().enabled = enter;
        }
    }
}
