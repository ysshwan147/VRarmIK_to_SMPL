using UnityEngine;

namespace VRArmIKtoSMPL
{
    public class Wrist : MonoBehaviour
    {
        public Transform target;

        public bool isLeft = false;

        Quaternion startRotation;

        void setRotation(Quaternion rotation) => transform.rotation = rotation * startRotation;
        void setLocalRotation(Quaternion rotation) => transform.rotation = transform.parent.rotation * rotation * startRotation;

        // Use this for initialization
        void Awake()
        {
            startRotation = transform.rotation;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            rotateWrist();
        }

        /// <summary>
        /// target의 rotation 그대로 wrist 회전
        /// </summary>
        public void rotateWrist()
        {
            /// 이 부분은 VRarmIK github 코드로 참고용
            //Vector3 handUpVec = target.rotation * Vector3.up;
            //float forwardAngle = VectorHelpers.getAngleBetween(elbow.rotation * Vector3.right, target.rotation * Vector3.right,
            //    elbow.rotation * Vector3.up, elbow.rotation * Vector3.forward);

            //// todo reduce influence if hand local forward rotation is high (hand tilted inside)
            //Quaternion handForwardRotation = Quaternion.AngleAxis(-forwardAngle, elbow.rotation * Vector3.forward);
            //handUpVec = handForwardRotation * handUpVec;

            //float elbowTargetAngle = VectorHelpers.getAngleBetween(elbow.rotation * Vector3.up, handUpVec,
            //    elbow.rotation * Vector3.forward, elbow.rotation * armDirection);

            //elbowTargetAngle = Mathf.Clamp(elbowTargetAngle, -90f, 90f);

            //if (arm.wrist1 != null)
            //    setWrist1Rotation(Quaternion.AngleAxis(elbowTargetAngle * .3f, lowerArmRotation * armDirection) * lowerArmRotation);
            //if (arm.wrist2 != null)
            //    setWrist2Rotation(Quaternion.AngleAxis(elbowTargetAngle * .8f, lowerArmRotation * armDirection) * lowerArmRotation);

            setRotation(target.rotation);
        }
    }
}
