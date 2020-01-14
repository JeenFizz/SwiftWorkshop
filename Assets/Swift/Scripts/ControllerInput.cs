using UnityEngine;
using Valve.VR;

public class ControllerInput : MonoBehaviour
{
	public delegate void OnGrabPressed(GameObject controller);
	public static event OnGrabPressed OnGrabPressedEvent;
	public delegate void OnGrabReleased(GameObject controller);
	public static event OnGrabReleased OnGrabReleasedEvent;
	public Transform cameraRig;
	public float rotationSpeed = 120.0f;

	private SteamVR_Behaviour_Pose behaviourPose;
	private SteamVR_Input_Sources inputSource;
	private FixedJoint joint;
	private ControllerPointer contPointer;
	private bool isTurningLeft = false;
	private bool isTurningRight = false;
	private GrabPointer grabPointer;

	void Awake()
	{
		behaviourPose = GetComponent<SteamVR_Behaviour_Pose>();
		inputSource = behaviourPose.inputSource;
	}

	private void Start()
	{
		grabPointer = GetComponent<GrabPointer>();
	}

	void Update()
	{
		if (SteamVR_Actions._default.GrabPinch.GetStateDown(inputSource))
		{
			if (grabPointer.targetedObject != null)
				GrabSelectedObject();

			OnGrabPressedEvent?.Invoke(gameObject);
		}
		if (SteamVR_Actions._default.GrabPinch.GetStateUp(inputSource))
		{
			if (grabPointer.targetedObject != null)
				UngrabSelectedObject();

			OnGrabReleasedEvent?.Invoke(gameObject);
		}

		if (SteamVR_Actions._default.Teleport.GetStateDown(inputSource))
			TeleportPressed();
		if (SteamVR_Actions._default.Teleport.GetStateUp(inputSource))
			TeleportReleased();

		if (SteamVR_Actions._default.SnapTurnLeft.GetStateDown(inputSource))
			isTurningLeft = true;

		if (SteamVR_Actions._default.SnapTurnLeft.GetStateUp(inputSource))
			isTurningLeft = false;

		if (SteamVR_Actions._default.SnapTurnRight.GetStateDown(inputSource))
			isTurningRight = true;

		if (SteamVR_Actions._default.SnapTurnRight.GetStateUp(inputSource))
			isTurningRight = false;

		if (isTurningLeft)
			TurnLeft();
		if (isTurningRight)
			TurnRight();
	}

	void GrabSelectedObject()
	{
		if (joint == null)
		{
			joint = gameObject.AddComponent<FixedJoint>();
			joint.connectedBody = grabPointer.targetedObject.GetComponent<Rigidbody>();
			joint.breakForce = 20000;
			joint.breakTorque = 20000;
		}
	}

	void UngrabSelectedObject()
	{
		if (joint != null)
		{
			joint.connectedBody = null;
			Destroy(joint);
		}
	}

	void TeleportPressed()
	{
		if (contPointer == null)
		{
			contPointer = gameObject.AddComponent<ControllerPointer>();
			contPointer.UpdateColor(Color.green);
		}
	}

	void TeleportReleased()
	{
		if (contPointer.CanTeleport)
		{
			GameObject cameraRig = GameObject.Find("[CameraRig]");
			cameraRig.transform.position = contPointer.TargetPosition;
			contPointer.DesactivatePointer();
			Destroy(contPointer);
		}
	}

	void TurnRight()
	{
		cameraRig.transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
	}

	void TurnLeft()
	{
		cameraRig.transform.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime);

	}
}
