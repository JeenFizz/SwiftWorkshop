using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ConfigurationChooser : MonoBehaviour
{
	public Image screenshot;
	public Text date;
	public bool done = false;

	private ControllerInput.JsonConfig jsonConfig;
	private string path;
	private string configPath;
	private MachineLayoutSaver machineLayoutSaver;

	void Awake()
	{
		path = Application.dataPath + "/StreamingAssets/";
		configPath = path + "MachineSaves/";
		machineLayoutSaver = GameObject.FindWithTag("FactoryMap").GetComponent<MachineLayoutSaver>();
	}

	public void SetJsonConfig(ControllerInput.JsonConfig jsonConfig)
	{
		this.jsonConfig = jsonConfig;
		if (jsonConfig.screenshotName != null)
			StartCoroutine(GetTexture());
		date.text = jsonConfig.name;
	}

	IEnumerator GetTexture()
	{
		using (UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + configPath + jsonConfig.screenshotName))
		{
			yield return request.SendWebRequest();

			if (!request.isNetworkError && !request.isHttpError)
			{
				Texture2D texture = DownloadHandlerTexture.GetContent(request);
				screenshot.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			}
			else
				Debug.Log(request.error);
		}
	}

	public void ResetJsonConfig()
	{
		jsonConfig.fileName = "";
		jsonConfig.name = "";
		jsonConfig.screenshotName = "";
		Destroy(screenshot.sprite);
		screenshot.sprite = null;
		date.text = "No configuration";
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("InteractionTool") && jsonConfig.fileName != null && jsonConfig.fileName.Length > 0)
		{
			machineLayoutSaver.LoadFile(configPath + jsonConfig.fileName);
			done = true;
		}
	}

	void OnDestroy()
	{
		Destroy(screenshot.sprite);
	}
}
