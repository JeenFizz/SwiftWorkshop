using UnityEngine;
using Valve.VR;

public class ControllerInput : MonoBehaviour
{
	public delegate void OnGrabPressed(GameObject controller);
	public static event OnGrabPressed OnGrabPressedEvent;
	public delegate void OnGrabReleased(GameObject controller);
	public static event OnGrabReleased OnGrabReleasedEvent;

	private SteamVR_Behaviour_Pose behaviourPose;
	private SteamVR_Input_Sources inputSource;
	private GameObject selectedObject;
	private FixedJoint joint;
	private ControllerPointer contPointer;

	void Awake()
	{
		behaviourPose = GetComponent<SteamVR_Behaviour_Pose>();
		inputSource = behaviourPose.inputSource;
	}

	void Update()
	{
		if (SteamVR_Actions._default.GrabPinch.GetStateDown(inputSource))
		{
			if (selectedObject != null)
				GrabSelectedObject();

			OnGrabPressedEvent?.Invoke(gameObject);
		}
		if (SteamVR_Actions._default.GrabPinch.GetStateUp(inputSource))
		{
			if (selectedObject != null)
				UngrabSelectedObject();

			OnGrabReleasedEvent?.Invoke(gameObject);
		}

		if (SteamVR_Actions._default.Teleport.GetStateDown(inputSource))
			TeleportPressed();
		if (SteamVR_Actions._default.Teleport.GetStateUp(inputSource))
			TeleportReleased();
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<GrabbableObject>())
			selectedObject = other.gameObject;
	}

	void OnTriggerExit(Collider other)
	{
		if (other.gameObject == selectedObject)
			selectedObject = null;
	}

	void GrabSelectedObject()
	{
		if (joint == null)
		{
			joint = gameObject.AddComponent<FixedJoint>();
			joint.connectedBody = selectedObject.GetComponent<Rigidbody>();
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
			selectedObject.GetComponent<Rigidbody>().velocity = behaviourPose.GetVelocity();
			selectedObject.GetComponent<Rigidbody>().angularVelocity = behaviourPose.GetAngularVelocity();
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
}
