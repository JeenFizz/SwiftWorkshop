using UnityEngine;
using Photon.Pun;

public class GrabPointer : MonoBehaviourPunCallbacks
{
    public float thickness = 0.002f;
    public float length = 100f;
    public GameObject targetedObject;
    public GameObject grabbedObject;
    public GameObject sphere;
    public float pullSpeed = 1f;

    private GameObject holder;
    private GameObject pointer;
    private GameObject cursor;

    private Vector3 cursorScale = new Vector3(0.05f, 0.05f, 0.05f);
    private float contactDistance = 0f;
    private Transform contactTarget = null;

    private Color color = Color.black;
    private float grabbedX = 0f;
    private float grabbedZ = 0f;
    private float grabbedY = 0f;
    public float rotationSpeed = 120.0f;

    void SetPointerTransform(float setLength, float setThicknes)
    {
        float beamPosition = setLength / (2 + 0.00001f);

        pointer.transform.localScale = new Vector3(setThicknes, setThicknes, setLength);
        pointer.transform.localPosition = new Vector3(0f, 0f, beamPosition);
        cursor.transform.localPosition = new Vector3(0f, 0f, setLength);
    }

    void Awake()
    {
        if(photonView.IsMine)
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
                UpdateColor(Color.yellow);
            }
        }
        else
        {
            targetedObject = null;
            UpdateColor(Color.yellow);
        }

        float beamLength = GetBeamLength(rayHit, hitObject);
        SetPointerTransform(beamLength, thickness);
        if (grabbedObject != null)
            grabbedObject.transform.rotation = (new Quaternion(grabbedX, grabbedY, grabbedZ, 0));
    }

    public void UpdateColor(Color color)
    {
        pointer.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        cursor.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }

    public void ActivatePointer()
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

    public void GrabSelectedObject()
    {
        if (targetedObject != null)
        {
            PhotonView grabbedObjectView = targetedObject.GetComponent<PhotonView>();
            if (grabbedObjectView.Owner == PhotonNetwork.LocalPlayer)
            {
                grabbedObject = targetedObject;
                grabbedObject.transform.parent = gameObject.transform;

                Quaternion identity = Quaternion.identity;
                Quaternion grabbedRotation = grabbedObject.transform.rotation;
                grabbedX = identity.x;
                grabbedY = grabbedRotation.y;
                grabbedZ = identity.z;
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
            }
            else
            {
                grabbedObjectView.RequestOwnership();
                grabbedObject = targetedObject;
                grabbedObject.transform.parent = gameObject.transform;

                Quaternion identity = Quaternion.identity;
                Quaternion grabbedRotation = grabbedObject.transform.rotation;
                grabbedX = identity.x;
                grabbedY = grabbedRotation.y;
                grabbedZ = identity.z;
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
            }
            
        }
    }

    public void UngrabSelectedObject()
    {
        if (grabbedObject != null)
        {
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
            grabbedRb.transform.position = (grabbedRb.transform.position + ((grabbedRb.transform.position - transform.position).magnitude * transform.forward * -pullSpeed * Time.deltaTime));
        }
    }

    public void Push()
    {
        if (grabbedObject != null)
        {
            Rigidbody grabbedRb = grabbedObject.GetComponent<Rigidbody>();
            grabbedRb.transform.position = (grabbedRb.transform.position + ((grabbedRb.transform.position - transform.position).magnitude * transform.forward * pullSpeed * Time.deltaTime));
        }
    }

    public void TurnRight()
    {
        //grabbedObject.transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
        //Quaternion grabbedRotation = grabbedObject.transform.rotation;
        //grabbedY = grabbedRotation.y;
        grabbedY += rotationSpeed * Time.deltaTime;
    }

    public void TurnLeft()
    {
        //grabbedObject.transform.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime);
        //Quaternion grabbedRotation = grabbedObject.transform.rotation;
        //grabbedY = grabbedRotation.y;
        grabbedY -= rotationSpeed * Time.deltaTime;
    }
}