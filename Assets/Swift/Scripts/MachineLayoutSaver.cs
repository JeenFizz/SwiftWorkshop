using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Photon.Realtime;
using System.Threading;

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
        public float[] position;
        public float[] rot;
        public string name;
    }

    [Serializable]
    public struct FactorySave
    {
        public MachineData[] machines;
    }

    private string saveDir;
    private FactorySave save;
    private string file;
    // Start is called before the first frame update
    void Start()
    {
        saveDir = Application.dataPath + "/StreamingAssets/MachineSaves";
        BetterStreamingAssets.Initialize();
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

        SaveConfig();
    }

    public void SaveConfig()
    {
        IEnumerable<MachineData> machineInfos = machines
            .Aggregate(new List<MachineData>() as IEnumerable<MachineData>, (prev, next) =>
                prev.Concat(
                    GameObject.FindGameObjectsWithTag(next.tag)
                        .Select(machine => new MachineData() { 
                            machineType = next.tag, position = new float[] {
                                machine.transform.position.x,
                                machine.transform.position.y,
                                machine.transform.position.z
                            }, rot = new float[] {
                                machine.transform.rotation.x,
                                machine.transform.rotation.y,
                                machine.transform.rotation.z,
                                machine.transform.rotation.w
                            }, name = machine.name 
                        })
                )
            );

        DateTime now = DateTime.Now;

        string path = saveDir + $"/Swift {now.Year}-{now.Month}-{now.Day} {now.Hour}-{now.Minute}-{now.Second}.json";
        Debug.Log($"Saving to {path}");

        string saveContent = JsonUtility.ToJson(new FactorySave() { machines = machineInfos.ToArray() });
        Debug.Log(saveContent);

        File.WriteAllText(path, saveContent);

        var cont = GameObject.Find("Content");
        if (cont != null) cont.GetComponent<MachineLoadMenu>().AddSaveLine(path);

        path = saveDir + $"/Swift {now.Year}-{now.Month}-{now.Day} {now.Hour}-{now.Minute}-{now.Second}.jpg";
        ScreenCapture.CaptureScreenshot(path);
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
        save = JsonUtility.FromJson<FactorySave>(File.ReadAllText(file));

        /*foreach()
        GetComponent<PhotonView>().RPC("DeleteMachines", RpcTarget.MasterClient);*/

        foreach(var machineGroups in save.machines.GroupBy(m => m.machineType))
        {
            var g = machineGroups.Select((mData, i) =>
            {
                GetComponent<PhotonView>().RPC("PlaceMachine", RpcTarget.MasterClient, mData.machineType, mData.position, mData.rot, mData.machineType + (i + 1).ToString());
                return mData;
            }).ToList();
        }
        /*foreach (MachineData mData in save.machines)
            GetComponent<PhotonView>().RPC("PlaceMachine", RpcTarget.MasterClient, mData.machineType, mData.position, mData.rot, mData.name);*/
    }

    [PunRPC]
    public void PlaceMachine(string machineType, float[] position, float[] rot, string name)
    {
        var delMachine = GameObject.Find(name.Substring(0, 2));

        if(delMachine != null) PhotonNetwork.Destroy(delMachine);

        string machineName = machines.First(m => m.tag == machineType).prefab.name;
        GameObject machine = PhotonNetwork.InstantiateSceneObject(
            machineName,
            new Vector3(position[0], position[1], position[2]),
            new Quaternion(rot[0], rot[1], rot[2], rot[3])
        );
        machine.tag = machineType;

        /*var pView = machine.GetComponent<PhotonView>();
        pView.RPC("NameMachine", RpcTarget.All, pView.ViewID, name.Substring(0, 2));*/

        machine.name = name.Substring(0, 2);
    }

    /*[PunRPC]
    public void NameMachine(int viewId, string name)
    {
        PhotonView.Find(viewId).transform.name = name;
    }*/


    /*[PunRPC]
    public void DeleteMachines()
    {
        foreach (string tag in machines.Select(m => m.tag))
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
                PhotonNetwork.Destroy(obj);

        foreach (MachineData mData in save.machines)
            GetComponent<PhotonView>().RPC("PlaceMachine", RpcTarget.MasterClient, mData.machineType, mData.position, mData.rot, mData.name);
    }

    private IEnumerator wait(GameObject obj)
    {
        yield return new WaitForSeconds(0.2f);
    }*/

    public void LoadFileAR()
    {
        foreach (string file in BetterStreamingAssets.GetFiles("MachineSaves", "*.json"))
        {
            this.file = file;
        }
        save = JsonUtility.FromJson<FactorySave>(BetterStreamingAssets.ReadAllText(this.file));

        foreach (MachineData mData in save.machines)
            GetComponent<PhotonView>().RPC("PlaceMachine", RpcTarget.MasterClient, mData.machineType, mData.position, mData.rot, mData.name);
    }
}
