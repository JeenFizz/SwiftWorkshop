using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MachineLoadMenu : MonoBehaviour
{
    public GameObject SaveRow;
    private string saveDir;

    // Start is called before the first frame update
    void Start()
    {
        saveDir = Application.dataPath + "/StreamingAssets/MachineSaves";

        foreach(string save in Directory.GetFiles(saveDir, "*.json").Reverse())
        {
            AddSaveLine(save);
        }
    }

    public void AddSaveLine(string save)
    {
        var row = Instantiate(SaveRow, gameObject.transform);
        row.transform.Find("SaveLabel").GetComponent<UnityEngine.UI.Text>().text = save.Substring(saveDir.Length);
        row.transform.Find("Load").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
            GameObject.Find("FactoryMap").GetComponent<MachineLayoutSaver>().LoadFile(save);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
