using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class ControllerInput : MonoBehaviour
{
	public enum ToolState
	{
		None,
		Screenshot,
		Save,
		Load
	}

	public struct JsonConfig
	{
		public string fileName;
		public string name;
		public string screenshotName;
	}

	private readonly List<string> toolNames = new List<string>
	{
		"Screenshot",
		"Save",
		"Load"
	};
	private int toolIndex = 0;

	[Serializable]
	public struct ToolObject
	{
		public ToolState state;
		public GameObject gameObject;
	}

	public Transform cameraRig;
	public float rotationSpeed = 120.0f;
	public Text toolNameText;
	public List<ToolObject> toolObjects;
	public List<ConfigurationChooser> configurationChoosers;
	public GameObject interactionTool;

	private SteamVR_Behaviour_Pose behaviourPose;
	private SteamVR_Input_Sources inputSource;
	private GrabPointer grabPointer;
	private ControllerPointer contPointer = null;
	private ToolState currentState = ToolState.None;
	private readonly Dictionary<ToolState, Action> toolUpdateCallbacks = new Dictionary<ToolState, Action>();
	private readonly Dictionary<ToolState, Action> toolOpeningCallbacks = new Dictionary<ToolState, Action>();
	private readonly Dictionary<ToolState, Action> toolClosingCallbacks = new Dictionary<ToolState, Action>();
	private bool isTurningLeft = false;
	private bool isTurningRight = false;
	private bool isPulling = false;
	private bool isPushing = false;

	private MachineLayoutSaver machineLayoutSaver;
	private float timeBeforeToolNameTextDisabling = 0.0f;
	private readonly List<JsonConfig> jsonConfigs = new List<JsonConfig>();
	private string path;
	private string configPath;
	private string screenshotPath;

	void Awake()
	{
		behaviourPose = GetComponent<SteamVR_Behaviour_Pose>();
		inputSource = behaviourPose.inputSource;
		grabPointer = GetComponent<GrabPointer>();
		machineLayoutSaver = GameObject.FindWithTag("FactoryMap").GetComponent<MachineLayoutSaver>();
	}

	void Start()
	{
		path = Application.dataPath + "/StreamingAssets/";
		configPath = path + "MachineSaves/";
		screenshotPath = path + "Screenshots/";

		toolUpdateCallbacks.Add(ToolState.Screenshot, OnScreenshotUpdate);
		toolUpdateCallbacks.Add(ToolState.Save, OnSaveUpdate);
		toolUpdateCallbacks.Add(ToolState.Load, OnLoadUpdate);

		toolOpeningCallbacks.Add(ToolState.Load, OnLoadOpening);

		toolClosingCallbacks.Add(ToolState.Load, OnLoadClosing);

		SetToolNameText();
	}

	void Update()
	{
		if (currentState == ToolState.None)
		{
			if (SteamVR_Actions._default.GrabPinch.GetStateDown(inputSource))
				if (grabPointer.targetedObject != null)
					grabPointer.GrabSelectedObject();
			if (SteamVR_Actions._default.GrabPinch.GetStateUp(inputSource))
				grabPointer.UngrabSelectedObject();

			if (SteamVR_Actions._default.Teleport.GetStateDown(inputSource))
			{
				if (grabPointer.grabbedObject == null)
				{
					grabPointer.SetActivePointer(false);
					TeleportPressed();
				}
				else
					isPushing = true;
			}
			if (SteamVR_Actions._default.Teleport.GetStateUp(inputSource))
			{
				if (grabPointer.grabbedObject == null)
				{
					TeleportReleased();
					grabPointer.SetActivePointer(true);
				}
				isPushing = false;
			}

			if (SteamVR_Actions._default.JoystickDown.GetStateDown(inputSource))
				if (grabPointer.grabbedObject != null)
					isPulling = true;
			if (SteamVR_Actions._default.JoystickDown.GetStateUp(inputSource))
				isPulling = false;

			if (grabPointer.grabbedObject != null)
			{
				if (isPulling)
					grabPointer.Pull();
				else if (isPushing)
					grabPointer.Push();
			}

			if (SteamVR_Actions._default.SnapTurnLeft.GetStateDown(inputSource))
				isTurningLeft = true;
			if (SteamVR_Actions._default.SnapTurnLeft.GetStateUp(inputSource))
				isTurningLeft = false;
			if (SteamVR_Actions._default.SnapTurnRight.GetStateDown(inputSource))
				isTurningRight = true;
			if (SteamVR_Actions._default.SnapTurnRight.GetStateUp(inputSource))
				isTurningRight = false;

			if (isTurningLeft)
			{
				if (grabPointer.grabbedObject != null)
					grabPointer.TurnLeft();
				else
					cameraRig.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime);
			}
			else if (isTurningRight)
			{
				if (grabPointer.grabbedObject != null)
					grabPointer.TurnRight();
				else
					cameraRig.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
			}

			if (SteamVR_Actions._default.TouchLeft.GetStateDown(inputSource))
			{
				toolIndex--;
				if (toolIndex < 0)
					toolIndex = toolNames.Count - 1;
				SetToolNameText();
			}
			if (SteamVR_Actions._default.TouchRight.GetStateDown(inputSource))
			{
				toolIndex++;
				if (toolIndex >= toolNames.Count)
					toolIndex = 0;
				SetToolNameText();
			}
			if (SteamVR_Actions._default.TouchDown.GetStateDown(inputSource))
				SetToolNameText();

			if (timeBeforeToolNameTextDisabling > 0.0f && toolNameText != null)
			{
				timeBeforeToolNameTextDisabling -= Time.deltaTime;
				if (timeBeforeToolNameTextDisabling <= 0.0f)
					toolNameText.gameObject.SetActive(false);
			}
		}

		bool closeMenu = false;
		foreach (ConfigurationChooser chooser in configurationChoosers)
		{
			if (chooser.done)
			{
				closeMenu = true;
				chooser.done = false;
				break;
			}
		}

		if ((SteamVR_Actions._default.Menu.GetStateDown(inputSource) || closeMenu) && toolNameText != null)
		{
			if (currentState == ToolState.None)
			{
				switch (toolIndex)
				{
					case 0:
						currentState = ToolState.Screenshot;
						break;
					case 1:
						currentState = ToolState.Save;
						break;
					case 2:
						currentState = ToolState.Load;
						break;
					default:
						break;
				}

				foreach (ToolObject toolObject in toolObjects)
				{
					if (toolObject.state == currentState)
					{
						toolObject.gameObject.SetActive(true);
						toolNameText.gameObject.SetActive(false);
						if (toolOpeningCallbacks.ContainsKey(currentState))
							toolOpeningCallbacks[currentState].Invoke();
						break;
					}
				}
			}
			else
			{
				foreach (ToolObject toolObject in toolObjects)
				{
					if (toolObject.state == currentState)
					{
						toolObject.gameObject.SetActive(false);
						if (toolClosingCallbacks.ContainsKey(currentState))
							toolClosingCallbacks[currentState].Invoke();
						break;
					}
				}
				currentState = ToolState.None;
			}
		}

		if (currentState != ToolState.None)
			toolUpdateCallbacks[currentState]?.Invoke();
	}

	void TeleportPressed()
	{
		if (contPointer == null)
			contPointer = gameObject.AddComponent<ControllerPointer>();
	}

	void TeleportReleased()
	{
		if (contPointer != null)
		{
			if (contPointer.CanTeleport)
				transform.parent.position = contPointer.TargetPosition;

			contPointer.DesactivatePointer();
			Destroy(contPointer);
		}
	}

	void OnDestroy()
	{
		grabPointer.DesactivatePointer();
	}

	void SetToolNameText()
	{
		if (toolNameText != null)
		{
			toolNameText.gameObject.SetActive(true);
			toolNameText.text = toolNames[toolIndex];
			timeBeforeToolNameTextDisabling = 2.0f;
		}
	}

	void OnScreenshotUpdate()
	{
		if (SteamVR_Actions._default.GrabPinch.GetStateDown(inputSource))
			ScreenCapture.CaptureScreenshot(screenshotPath + "Screen-" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".jpg");
	}

	void OnSaveUpdate()
	{
		if (SteamVR_Actions._default.GrabPinch.GetStateDown(inputSource))
			machineLayoutSaver.SaveConfig();
	}

	void OnLoadOpening()
	{
		if (interactionTool != null)
			interactionTool.SetActive(true);

		jsonConfigs.Clear();
		DirectoryInfo dirInfo = new DirectoryInfo(configPath);
		FileInfo[] files = dirInfo.GetFiles("*.json");
		foreach (FileInfo file in files)
		{
			string screenshotFileName = file.Name.Substring(0, file.Name.LastIndexOf('.')) + ".jpg";
			JsonConfig jsonConfig = new JsonConfig
			{
				fileName = file.Name,
				name = file.Name.Substring(6, file.Name.LastIndexOf('.') - 6),
				screenshotName = File.Exists(screenshotFileName) ? screenshotFileName : null
			};
			jsonConfigs.Add(jsonConfig);
		}

		int count = 0;
		for (int i = jsonConfigs.Count - 1; i >= 0; i--)
		{
			configurationChoosers[count].SetJsonConfig(jsonConfigs[i]);
			count++;
			if (count >= 4)
				break;
		}
	}

	void OnLoadUpdate()
	{

	}

	void OnLoadClosing()
	{
		if (interactionTool != null)
			interactionTool.SetActive(false);
	}
}
