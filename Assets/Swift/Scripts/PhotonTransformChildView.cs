using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WS3
{
    [RequireComponent(typeof(PhotonView))]
    public class PhotonTransformChildView : MonoBehaviourPunCallbacks, IPunObservable
    {
        public bool SynchronizePosition = true;
        public bool SynchronizeRotation = true;
        public bool SynchronizeScale = false;

        public List<Transform> SynchronizedChildTransform;
        private List<Vector3> localPositionList;
        private List<Quaternion> localRotationList;
        private List<Vector3> localScaleList;

        // Start is called before the first frame update
        void Awake()
        {
            Debug.Log("Awake " + SynchronizedChildTransform.Count);
            localPositionList = new List<Vector3>(SynchronizedChildTransform.Count);
            for (int i = 0; i < SynchronizedChildTransform.Count; i++)
            {
                localPositionList.Add(Vector3.zero);
            }

            localRotationList = new List<Quaternion>(SynchronizedChildTransform.Count);
            for (int i = 0; i < SynchronizedChildTransform.Count; i++)
            {
                localRotationList.Add(Quaternion.identity);
            }

            localScaleList = new List<Vector3>(SynchronizedChildTransform.Count);
            for (int i = 0; i < SynchronizedChildTransform.Count; i++)
            {
                localScaleList.Add(Vector3.one);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!photonView.IsMine)
            {
                for (int i = 0; i < SynchronizedChildTransform.Count; i++)
                {
                    if (SynchronizePosition) SynchronizedChildTransform[i].localPosition = localPositionList[i];
                    if (SynchronizeRotation) SynchronizedChildTransform[i].localRotation = localRotationList[i];
                    if (SynchronizeScale) SynchronizedChildTransform[i].localScale = localScaleList[i];
                }
            }
        }


        #region IPUnObservable
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                Debug.Log("Writing " + SynchronizePosition + " " + SynchronizeRotation);
                if (this.SynchronizePosition)
                {
                    for (int i = 0; i < SynchronizedChildTransform.Count; i++)
                    {
                        stream.SendNext(SynchronizedChildTransform[i].localPosition);
                    }
                }
                if (this.SynchronizeRotation)
                {
                    for (int i = 0; i < SynchronizedChildTransform.Count; i++)
                    {
                        stream.SendNext(SynchronizedChildTransform[i].localRotation);
                    }
                }

                if (this.SynchronizeScale)
                {
                    for (int i = 0; i < SynchronizedChildTransform.Count; i++)
                    {
                        stream.SendNext(SynchronizedChildTransform[i].localScale);
                    }
                }
            }
            else
            {
                if (this.SynchronizePosition)
                {
                    for (int i = 0; i < SynchronizedChildTransform.Count; i++)
                    {
                        localPositionList[i] = (Vector3)stream.ReceiveNext();
                    }
                }
                if (this.SynchronizeRotation)
                {
                    for (int i = 0; i < SynchronizedChildTransform.Count; i++)
                    {
                        localRotationList[i] = (Quaternion)stream.ReceiveNext();
                    }
                }

                if (this.SynchronizeScale)
                {
                    for (int i = 0; i < SynchronizedChildTransform.Count; i++)
                    {
                        localScaleList[i] = (Vector3)stream.ReceiveNext();
                    }
                }
            }
        }
        #endregion
    }
}
