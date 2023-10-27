using UnityEngine;

namespace VRArmIKtoSMPL
{
    public class Elbow : MonoBehaviour
    {
        public UpperBodyTransform upperBody;
        public Transform target;
        public Transform shoulder, wrist;

        public bool isLeft = false;

        float innerAngle;

        Quaternion startRotation;

        Quaternion elbowRotation => transform.rotation * Quaternion.Inverse(startRotation);

        void setRotation(Quaternion rotation) => transform.rotation = rotation * startRotation;
        void setLocalRotation(Quaternion rotation) => transform.rotation = transform.parent.rotation * rotation * startRotation;

        // Use this for initialization
        void Awake()
        {
            startRotation = transform.rotation;
        }

        void FixedUpdate()
        {
            innerAngle = calcInnerAngle();
        }

        void LateUpdate()
        {
            Vector3 eulerAngles = new Vector3();
            eulerAngles.y = innerAngle;

            setLocalRotation(Quaternion.Euler(eulerAngles));
        }

        float calcInnerAngle()
        {
            float targetShoulderDistance = (target.position - shoulder.position).magnitude;
            float angle = 0.0f;

            if (targetShoulderDistance < upperBody.armLength)
            {
                angle = Mathf.Acos(Mathf.Clamp((Mathf.Pow(upperBody.upperArmLength, 2f) + Mathf.Pow(upperBody.lowerArmLength, 2f) -
                                                Mathf.Pow(targetShoulderDistance, 2f)) / (2f * upperBody.upperArmLength * upperBody.lowerArmLength), -1f, 1f)) * Mathf.Rad2Deg;
                if (isLeft)
                    angle = 180f - angle;
                else
                    angle = 180f + angle;

                if (float.IsNaN(angle))
                {
                    angle = 180f;
                }
            }

            return angle;
        }
    }
}
