using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class ControllerInput : MonoBehaviour
{
	public enum ToolState
	{
		None,
		Screenshot
	}

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

	private SteamVR_Behaviour_Pose behaviourPose;
	private SteamVR_Input_Sources inputSource;
	private GrabPointer grabPointer;
	private ControllerPointer contPointer = null;
	private ToolState currentState = ToolState.None;
	private readonly Dictionary<ToolState, Action> toolCallbacks = new Dictionary<ToolState, Action>();
	private bool isTurningLeft = false;
	private bool isTurningRight = false;
	private bool isPulling = false;
	private bool isPushing = false;
	private int callCount = 0;

	private readonly List<string> toolNames = new List<string>
	{
		"Screenshot",
		"Tool 2",
		"Tool 3"
	};
	private int toolIndex = 0;

	void Awake()
	{
		behaviourPose = GetComponent<SteamVR_Behaviour_Pose>();
		inputSource = behaviourPose.inputSource;
		grabPointer = GetComponent<GrabPointer>();
	}

	void Start()
	{
		toolCallbacks.Add(ToolState.Screenshot, OnScreenshot);

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
		}

		if (SteamVR_Actions._default.Menu.GetStateDown(inputSource))
		{
			if (currentState == ToolState.None)
			{
				switch (toolIndex)
				{
					case 0:
						currentState = ToolState.Screenshot;
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
						break;
					}
				}
				currentState = ToolState.None;
			}
		}

		if (currentState != ToolState.None)
			toolCallbacks[currentState]?.Invoke();
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
				cameraRig.position = contPointer.TargetPosition;

			contPointer.DesactivatePointer();
			Destroy(contPointer);
		}
	}

	void SetToolNameText()
	{
		toolNameText.gameObject.SetActive(true);
		toolNameText.text = toolNames[toolIndex];
		callCount++;
		StartCoroutine(HideToolNameText(callCount));
	}

	System.Collections.IEnumerator HideToolNameText(int callCount)
	{
		yield return new WaitForSeconds(1.0f);
		if (callCount == this.callCount)
		{
			toolNameText.gameObject.SetActive(false);
			this.callCount = 0;
		}
	}

	void OnScreenshot()
	{
		if (SteamVR_Actions._default.GrabPinch.GetStateDown(inputSource))
			ScreenCapture.CaptureScreenshot("Assets/StreamingAssets/Screenshots/Screen-" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".jpg");
	}

	void OnDestroy()
	{
		grabPointer.DesactivatePointer();
	}
}
