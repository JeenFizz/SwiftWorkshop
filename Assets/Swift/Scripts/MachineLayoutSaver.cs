using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MachineLayoutSaver : MonoBehaviour
{
    [Serializable]
    public struct MachineInfo
    {
        public string tag;
        public GameObject prefab;
    }
    public MachineInfo[] machines;

    [Serializable]
    public struct MachineData
    {
        public string machineType;
        public Vector3 position;
        public Quaternion rot;
        public string name;
    }

    [Serializable]
    public struct FactorySave
    {
        public MachineData[] machines;
    }

    private string saveDir;

    // Start is called before the first frame update
    void Start()
    {
        saveDir = Application.dataPath + "/StreamingAssets/MachineSaves";
    }

    // Update is called once per frame
    void Update()
    {
        CheckLoad();
        CheckSave();
    }

    void CheckSave()
    {
        if (!Input.GetKeyDown("m")) return;

        IEnumerable<MachineData> machineInfos = machines
            .Aggregate(new List<MachineData>() as IEnumerable<MachineData>, (prev, next) =>
                prev.Concat(
                    GameObject.FindGameObjectsWithTag(next.tag)
                        .Select(machine => new MachineData() { machineType = next.tag, position = machine.transform.position, rot = machine.transform.rotation, name = machine.name })
                )
            );

        DateTime now = DateTime.Now;

        string path = saveDir + $"/Swift {now.Year}-{now.Month}-{now.Day} {now.Hour}-{now.Minute}-{now.Second}.json";
        Debug.Log($"Saving to {path}");

        string saveContent = JsonUtility.ToJson(new FactorySave() { machines = machineInfos.ToArray() });
        Debug.Log(saveContent);

        File.WriteAllText(path, saveContent);
    }

    void CheckLoad()
    {
        if (!Input.GetKeyDown("l")) return;

        string lastSave = Directory.GetFiles(saveDir, "*.json").Last();
        Debug.Log($"Loading {lastSave}");

        LoadFile(lastSave);
    }

    public void LoadFile(string file)
    {
        FactorySave save = JsonUtility.FromJson<FactorySave>(File.ReadAllText(file));

        foreach (string tag in machines.Select(m => m.tag))
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
            {
                obj.GetComponent<PhotonView>().RequestOwnership();
                PhotonNetwork.Destroy(obj);
            }

        foreach (MachineData mData in save.machines)
        {
            string machineName = machines.First(m => m.tag == mData.machineType).prefab.name;
            GameObject machine = PhotonNetwork.InstantiateSceneObject(machineName, mData.position, mData.rot);
            machine.tag = mData.machineType;
            machine.name = mData.name.Substring(0, 2);
        }
    }
}
