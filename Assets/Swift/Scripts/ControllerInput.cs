using UnityEngine;
using Valve.VR;

public class ControllerInput : MonoBehaviour
{
	public Transform cameraRig;
	public float rotationSpeed = 120.0f;

	private SteamVR_Behaviour_Pose behaviourPose;
	private SteamVR_Input_Sources inputSource;
	private GrabPointer grabPointer;
	private ControllerPointer contPointer = null;
	private bool isTurningLeft = false;
	private bool isTurningRight = false;
	private bool isPulling = false;
	private bool isPushing = false;

	void Awake()
	{
		behaviourPose = GetComponent<SteamVR_Behaviour_Pose>();
		inputSource = behaviourPose.inputSource;
		grabPointer = GetComponent<GrabPointer>();
	}

	void Update()
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

	void OnDestroy()
	{
		grabPointer.DesactivatePointer();
	}
}
