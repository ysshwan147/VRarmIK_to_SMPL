using UnityEngine;

namespace VRArmIKtoSMPL
{
    public class Wrist : MonoBehaviour
    {
        public Transform target;
        public Transform elbow;

        public bool isLeft = false;

        Quaternion startRotation;
        Vector3 armDirection => isLeft ? Vector3.left : Vector3.right;

        Quaternion wristRotation => transform.rotation * Quaternion.Inverse(startRotation);

        void setRotation(Quaternion rotation) => transform.rotation = rotation * startRotation;
        void setLocalRotation(Quaternion rotation) => transform.rotation = transform.parent.rotation * rotation * startRotation;

        // Use this for initialization
        void Awake()
        {
            startRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            rotateWrist();
        }

        public void rotateWrist()
        {
            //Vector3 handUpVec = target.rotation * Vector3.up;
            //float forwardAngle = VectorHelpers.getAngleBetween(elbow.rotation * Vector3.right, target.rotation * Vector3.right,
            //    elbow.rotation * Vector3.up, elbow.rotation * Vector3.forward);

            //// todo reduce influence if hand local forward rotation is high (hand tilted inside)
            //Quaternion handForwardRotation = Quaternion.AngleAxis(-forwardAngle, elbow.rotation * Vector3.forward);
            //handUpVec = handForwardRotation * handUpVec;

            //float elbowTargetAngle = VectorHelpers.getAngleBetween(elbow.rotation * Vector3.up, handUpVec,
            //    elbow.rotation * Vector3.forward, elbow.rotation * armDirection);

            //elbowTargetAngle = Mathf.Clamp(elbowTargetAngle, -90f, 90f);

            setRotation(target.rotation);
        }
    }
}
