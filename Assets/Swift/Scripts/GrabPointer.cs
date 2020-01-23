using UnityEngine;
using Photon.Pun;

public class GrabPointer : MonoBehaviourPunCallbacks
{
	public float thickness = 0.002f;
	public float length = 100f;
	public GameObject targetedObject;
	public GameObject grabbedObject;
	public float pullSpeed = 1f;

	private GameObject holder;
	private GameObject pointer;
	private GameObject cursor;

	private Vector3 cursorScale = new Vector3(0.05f, 0.05f, 0.05f);
	private float contactDistance = 0f;
	private Transform contactTarget = null;

	private Color color = Color.black;
	private readonly float rotationSpeed = 2.0f;
	private float pitch = 0.0f;
	private float yaw1 = 0.0f;
	private float yaw2 = 0.0f;
	private float mult = 0.0f;

	void Awake()
	{
		if (photonView.IsMine)
			ActivatePointer();
	}

	void SetPointerTransform(float setLength, float setThicknes)
	{
		float beamPosition = setLength / (2 + 0.00001f);

		pointer.transform.localScale = new Vector3(setThicknes, setThicknes, setLength);
		pointer.transform.localPosition = new Vector3(0f, 0f, beamPosition);
		cursor.transform.localPosition = new Vector3(0f, 0f, setLength);
	}

	float GetBeamLength(bool bHit, RaycastHit hit)
	{
		float actualLength = length;

		if (!bHit || (contactTarget && contactTarget != hit.transform))
		{
			contactDistance = 0f;
			contactTarget = null;
		}
		if (bHit)
		{
			if (hit.distance <= 0)
			{

			}
			contactDistance = hit.distance;
			contactTarget = hit.transform;
		}

		if (bHit && contactDistance < length)
		{
			actualLength = contactDistance;
		}

		if (actualLength <= 0)
		{
			actualLength = length;
		}

		return actualLength; ;
	}

	void Update()
	{
		if (holder == null || pointer == null || cursor == null)
			return;

		Ray raycast = new Ray(transform.position, transform.forward);

		RaycastHit hitObject;
		bool rayHit = Physics.Raycast(raycast, out hitObject);
		if (rayHit)
		{
			if (hitObject.collider.gameObject.GetComponent<GrabbableObject>())
			{
				targetedObject = hitObject.collider.gameObject;
				UpdateColor(Color.blue);
			}
			else
			{
				targetedObject = null;
				if (grabbedObject == null)
					UpdateColor(Color.yellow);
			}
		}
		else
		{
			targetedObject = null;
			if (grabbedObject == null)
				UpdateColor(Color.yellow);
		}

		float beamLength = GetBeamLength(rayHit, hitObject);
		SetPointerTransform(beamLength, thickness);
		if (grabbedObject != null)
			grabbedObject.transform.forward = new Vector3(Mathf.Cos(yaw1) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Sin(yaw2) * Mathf.Cos(pitch));
	}

	void UpdateColor(Color color)
	{
		pointer.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
		cursor.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
	}

	void ActivatePointer()
	{
		Material newMaterial = new Material(Shader.Find("Unlit/Color"));
		newMaterial.SetColor("_Color", color);

		holder = new GameObject();
		holder.name = "Pointer";
		holder.transform.parent = this.transform;
		holder.transform.localPosition = Vector3.zero;


		pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
		pointer.name = "Laser";
		pointer.transform.parent = holder.transform;
		pointer.GetComponent<MeshRenderer>().material = newMaterial;

		pointer.GetComponent<BoxCollider>().isTrigger = true;
		pointer.AddComponent<Rigidbody>().isKinematic = true;
		pointer.layer = 2;

		cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		cursor.name = "Cursor";
		cursor.transform.parent = holder.transform;
		cursor.GetComponent<MeshRenderer>().material = newMaterial;
		cursor.transform.localScale = cursorScale;

		cursor.GetComponent<SphereCollider>().isTrigger = true;
		cursor.AddComponent<Rigidbody>().isKinematic = true;
		cursor.layer = 2;
		holder.transform.localRotation = new Quaternion(0, 0, 0, 0);
		SetPointerTransform(length, thickness);
	}

	public void DesactivatePointer()
	{
		Destroy(holder);
		Destroy(pointer);
		Destroy(cursor);
	}

	public void SetActivePointer(bool active)
	{
		holder.SetActive(active);
	}

	public void GrabSelectedObject()
	{
		if (targetedObject != null)
		{
			PhotonView grabbedObjectView = targetedObject.GetComponent<PhotonView>();
			if (grabbedObjectView.Owner != PhotonNetwork.LocalPlayer)
				grabbedObjectView.RequestOwnership();

			grabbedObject = targetedObject;
			grabbedObject.transform.parent = gameObject.transform;
			grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
			Vector3 grabbedDirection = grabbedObject.transform.forward;

			if (grabbedDirection.y <= -0.99f)
			{
				grabbedObject.transform.forward = new Vector3(0.0f, 0.0f, 1.0f);
				grabbedDirection = grabbedObject.transform.forward;
			}
			else
			{
				grabbedDirection.y = 0.0f;
				grabbedDirection.Normalize();
				grabbedObject.transform.forward = grabbedDirection;
			}
			mult = grabbedDirection.x <= -0.71f || grabbedDirection.z <= -0.71f ? 1.0f : -1.0f;
			pitch = Mathf.Asin(grabbedDirection.y);
			yaw1 = Mathf.Acos(grabbedDirection.x / Mathf.Cos(pitch));
			yaw2 = Mathf.Asin(grabbedDirection.z / Mathf.Cos(pitch));
		}
	}

	public void UngrabSelectedObject()
	{
		if (grabbedObject != null)
		{
			Vector3 grabbedDirection = grabbedObject.transform.forward;
			float dot1 = Vector3.Dot(grabbedDirection, new Vector3(0.0f, 0.0f, 1.0f));
			float dot2 = Vector3.Dot(grabbedDirection, new Vector3(1.0f, 0.0f, 0.0f));
			if (dot1 >= 0.5f)
				grabbedObject.transform.forward = new Vector3(0.0f, 0.0f, 1.0f);
			else if (dot1 <= -0.5f)
				grabbedObject.transform.forward = new Vector3(0.0f, 0.0f, -1.0f);
			else if (dot2 >= 0.5f)
				grabbedObject.transform.forward = new Vector3(1.0f, 0.0f, 0.0f);
			else if (dot2 <= -0.5f)
				grabbedObject.transform.forward = new Vector3(-1.0f, 0.0f, 0.0f);

			grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
			grabbedObject.transform.parent = null;
			grabbedObject = null;
		}
	}

	public void Pull()
	{
		if (grabbedObject != null)
		{
			Rigidbody grabbedRb = grabbedObject.GetComponent<Rigidbody>();
			if ((grabbedRb.transform.position - transform.position).magnitude >= 3.0f)
				grabbedRb.transform.position += (grabbedRb.transform.position - transform.position).magnitude * transform.forward * -pullSpeed * Time.deltaTime;
		}
	}

	public void Push()
	{
		if (grabbedObject != null)
		{
			Rigidbody grabbedRb = grabbedObject.GetComponent<Rigidbody>();
			if ((grabbedRb.transform.position - transform.position).magnitude <= 100.0f)
				grabbedRb.transform.position += (grabbedRb.transform.position - transform.position).magnitude * transform.forward * pullSpeed * Time.deltaTime;
		}
	}

	public void TurnLeft()
	{
		yaw1 -= rotationSpeed * mult * Time.deltaTime;
		yaw2 -= rotationSpeed * mult * Time.deltaTime;
	}

	public void TurnRight()
	{
		yaw1 += rotationSpeed * mult * Time.deltaTime;
		yaw2 += rotationSpeed * mult * Time.deltaTime;
	}
}