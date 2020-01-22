using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AppConfig {
    // HTC, force the device to HTC, PC, force device to PC Keyboard, AUTO: select the device according to the hardware connected.
    public string DeviceUsed = "AUTO"; 


    private static AppConfig inst;

    public static AppConfig Inst
    {
        get
        {
            if (inst == null) inst = new AppConfig();
            return inst;
        }
    }

    /// <summary>
    /// Update the values from a Json String
    /// </summary>
    /// <param name="jsonString"></param>
    public void UpdateValuesFromJsonString(string jsonString)
    {
        JsonUtility.FromJsonOverwrite(jsonString, Inst);
    }


    /// <summary>
    /// Read from a file path the JSON for deserializing
    /// use this 
    /// Appli
    /// </summary>
    /// <param name="filePath"></param>
    public void UpdateValuesFromJsonFile(string filePath)
    {
        UpdateValuesFromJsonString(System.IO.File.ReadAllText(filePath));
    }

    /// <summary>
    /// Read from the json file called StreamingAssets/{Application.productName+".AppConfig.json"}
    /// </summary>
    public void UpdateValuesFromJsonFile()
    {
        Debug.Log(Application.absoluteURL);
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, Application.productName+".AppConfig.json");
       
        UpdateValuesFromJsonFile(path);
    }

    public string ToJsonString()
    {
        return JsonUtility.ToJson(Inst, true);
    }

}
