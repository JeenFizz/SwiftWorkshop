using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppConfigLoaderBehaviour : MonoBehaviour {

    public string SceneToLoadAfterAppConfigLoaded;
    /// <summary>
    /// The file path or use the value by defaults otherwise
    /// </summary>
    public string AppConfigFilePath;
    // Use this for initialization
    void Start () {
        Debug.Log(Application.productName + ".AppConfig.json BEFORE LOADING");
        Debug.Log(AppConfig.Inst.ToJsonString());

        if (string.IsNullOrEmpty(AppConfigFilePath)) AppConfig.Inst.UpdateValuesFromJsonFile();
        else AppConfig.Inst.UpdateValuesFromJsonFile(AppConfigFilePath);

        if (!string.IsNullOrEmpty(SceneToLoadAfterAppConfigLoaded))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneToLoadAfterAppConfigLoaded);
        }
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
