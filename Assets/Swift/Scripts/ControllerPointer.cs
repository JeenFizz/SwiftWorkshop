using UnityEngine;

public class ControllerPointer : MonoBehaviour
{
	public Vector3 TargetPosition;
	public bool CanTeleport;
	public float thickness = 0.02f;
	public float length = 4.0f;

	private GameObject holder;
	private GameObject[] pointers;
	private GameObject cursor;

	private Vector3 cursorScale = new Vector3(0.5f, 0.5f, 0.5f);
	private float contactDistance = 0f;
	private Transform contactTarget = null;

	private Color color = Color.black;
	private readonly int maxNbOfPointers = 12;
	private readonly float directionChange = 0.05f;

	void SetPointerTransform(GameObject pointer, float setLength, Vector3 position, Vector3 direction, float setThickness)
	{
		pointer.transform.localScale = new Vector3(setThickness, setThickness, setLength);
		pointer.transform.position = position;
		pointer.transform.forward = direction;
	}

	void Awake()
	{
		ActivatePointer();
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
		Vector3 currentPosition = transform.position;
		Vector3 currentDirection = transform.forward;
		CanTeleport = false;
		cursor.SetActive(false);
		UpdateColor(Color.red);
		bool found = false;
		bool firstFound = false;
		for (int i = 0; i < maxNbOfPointers; i++)
		{
			Ray raycast = new Ray(currentPosition, currentDirection);

			RaycastHit hitObject;
			bool rayHit = Physics.Raycast(raycast, out hitObject, length);
			if (rayHit && !found)
			{
				if (hitObject.collider.gameObject.GetComponent<AllowTeleportation>())
				{
					CanTeleport = true;
					TargetPosition = hitObject.point;
					cursor.SetActive(true);
					cursor.transform.position = TargetPosition;
					UpdateColor(Color.green);
				}
				found = true;
				firstFound = true;
			}
			if (!found || firstFound)
			{
				pointers[i].SetActive(true);
				float beamLength = firstFound ? GetBeamLength(rayHit, hitObject) : length;
				SetPointerTransform(pointers[i], beamLength, currentPosition + currentDirection * beamLength / 2.00001f, currentDirection, thickness);
				currentPosition += currentDirection * beamLength;
				currentDirection.y -= directionChange;
				currentDirection.Normalize();
				firstFound = false;
			}
			else
				pointers[i].SetActive(false);
		}
	}

	void UpdateColor(Color color)
	{
		for (int i = 0; i < maxNbOfPointers; i++)
			pointers[i].GetComponent<MeshRenderer>().material.SetColor("_Color", color);
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
		holder.transform.localRotation = Quaternion.identity;

		cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		cursor.name = "Cursor";
		cursor.transform.parent = holder.transform;
		cursor.GetComponent<MeshRenderer>().material = newMaterial;
		cursor.transform.localScale = cursorScale;

		cursor.GetComponent<SphereCollider>().isTrigger = true;
		cursor.AddComponent<Rigidbody>().isKinematic = true;
		cursor.layer = 2;

		pointers = new GameObject[maxNbOfPointers];
		for (int i = 0; i < maxNbOfPointers; i++)
		{
			pointers[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
			pointers[i].name = "Laser";
			pointers[i].transform.parent = holder.transform;
			pointers[i].GetComponent<MeshRenderer>().material = newMaterial;

			pointers[i].GetComponent<BoxCollider>().isTrigger = true;
			pointers[i].AddComponent<Rigidbody>().isKinematic = true;
			pointers[i].layer = 2;
		}
	}

	public void DesactivatePointer()
	{
		for (int i = 0; i < maxNbOfPointers; i++)
			Destroy(pointers[i]);
		Destroy(cursor);
		Destroy(holder);
	}
}