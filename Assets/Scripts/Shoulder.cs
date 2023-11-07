using UnityEngine;
using UnityEngine.UI;

namespace VRArmIKtoSMPL
{
    public class Shoulder : MonoBehaviour
    {
        public UpperBodyTransform upperBody;
        public Transform target;
        public Transform elbow, wrist;

        public bool isLeft = false;

        public float offsetAngle = 15.0f;

        public float xBias = 30.0f;
        public float yBias = 120.0f;
        public float zBias = 65.0f;

        public float yWeight = -60.0f;
        public float zWeight = 260.0f;
        public float zWeightTop = 260.0f;
        public float zWeightBottom = -100.0f;
        public float zStartDistance = 0.5f;
        public float xWeight = -50.0f;
        public float xStartDistance = 0.1f;

        public float minAngle = 13.0f;
        public float maxAngle = 175.0f;

        public bool correctElbowOutside = true;
        public float weight1 = -0.5f;
        public float startBelowZ = 0.4f;
        public float startAboveY = 0.1f;


        public float startBelowDistance = 0.5f;
        public float startBelowY = 0.1f;
        public float weight2 = 2.0f;
        public Vector3 localElbowPos = new Vector3(0.3f, -1.0f, -2.0f);


        public float handDeltaPow = 1.5f, handDeltaFactor = -.3f, handDeltaOffset = 45f;
        // todo fix rotateElbowWithHandForward with factor != 1 -> horrible jumps. good value would be between [0.4, 0.6]
        public float handDeltaForwardPow = 2f, handDeltaForwardFactor = 1f, handDeltaForwardOffset = 0f, handDeltaForwardDeadzone = .3f;
        public float rotateElbowWithHandDelay = .08f;

        float interpolatedDeltaElbow;
        float interpolatedDeltaElbowForward;

        Quaternion startRotation;
        Quaternion startElbowRotation;
        Vector3 armDirection => isLeft ? Vector3.left : Vector3.right;
        Quaternion shoulderRotation => transform.rotation * Quaternion.Inverse(startRotation);
        Quaternion elbowRotation => elbow.rotation * Quaternion.Inverse(startElbowRotation);

        void setRotation(Quaternion rotation) => transform.rotation = rotation * startRotation;
        void setLocalRotation(Quaternion rotation) => transform.rotation = transform.parent.rotation * rotation * startRotation;


        void Awake()
        {
            startRotation = transform.rotation;
            startElbowRotation = elbow.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            rotateShoulderAboutElbowAngle();
            correctElbowRotation();

            rotateShoulderAboutHandShoulderAxis();
            correctElbowAfterPositioning();
            rotateElbowWithWristRight();
            rotateElbowWithWristFoward();
        }

        /// <summary>
        /// elbow의 위치를 나타내기 위한 shoulder의 rotation
        /// 타겟(손목), 어깨, 팔꿈치 삼각형에서 어깨 쪽 각도만큼 계산해서 회전
        /// </summary>
        void rotateShoulderAboutElbowAngle()
        {
            Vector3 targetShoulderDirection = (target.position - transform.position).normalized;
            float targetAngleAboutElbowAngle = calcAngleAboutElbowAngle();

            Quaternion toTargetRotation = Quaternion.FromToRotation(armDirection, targetShoulderDirection);
            setRotation(toTargetRotation);

            transform.rotation = Quaternion.AngleAxis(targetAngleAboutElbowAngle, elbowRotation * Vector3.up) * transform.rotation;
        }

        /// <summary>
        /// elbow의 위치를 나타내기 위한 shoulder의 rotation
        /// 어깨-손 축 기준 angle 계산 후 회전
        /// </summary>
        void rotateShoulderAboutHandShoulderAxis()
        {
            float targetAngleAboutHandShoulderAxis = calcAngleAboutHandShoulderAxis();

            rotateWithAngle(targetAngleAboutHandShoulderAxis);
        }

        /// <summary>
        /// 어깨-손 축 기준 angle 만큼 회전
        /// </summary>
        void rotateWithAngle(float angle)
        {
            Vector3 shoulderWristDirection = (transform.position - wrist.position).normalized;

            Quaternion rotation = Quaternion.AngleAxis(angle, shoulderWristDirection);
            setRotation(rotation * shoulderRotation);
        }

        /// <summary>
        /// target이 축 근처에서 움직일 때의 보정
        /// startBelowZ = 0.4, startAboveY = 0.1, weight1 = -0.5
        /// </summary>
        void correctElbowRotation()
        {
            Vector3 localTargetPos = transform.InverseTransformPoint(target.position) / upperBody.armLength;

            float elbowOutsideFactor = Mathf.Clamp01(
                                     Mathf.Clamp01((startBelowZ - localTargetPos.z) /
                                                   Mathf.Abs(startBelowZ) * .5f) *
                                     Mathf.Clamp01((localTargetPos.y - startAboveY) /
                                                   Mathf.Abs(startAboveY)) *
                                     Mathf.Clamp01(1f - localTargetPos.x * (isLeft ? -1f : 1f))
                                 ) * weight1;

            Vector3 shoulderWristDirection = (transform.position - wrist.position).normalized;
            Vector3 targetDir = upperBody.transform.rotation * (Vector3.up + (correctElbowOutside ? (armDirection + Vector3.forward * -.2f) * elbowOutsideFactor : Vector3.zero));
            Vector3 cross = Vector3.Cross(shoulderWristDirection, targetDir * 1000f);

            Vector3 shoulderUp = shoulderRotation * Vector3.up;

            float elbowTargetUp = Vector3.Dot(shoulderUp, targetDir);
            float elbowAngle = Vector3.Angle(cross, shoulderUp) + (isLeft ? 0f : 180f);
            Quaternion rotation = Quaternion.AngleAxis(elbowAngle * Mathf.Sign(elbowTargetUp), shoulderWristDirection);
            transform.rotation = rotation * transform.rotation;
        }

        /// <summary>
        /// target이 축 근처에서 움직일 때의 보정
        /// threshold 이내에서 고정 벡터 localElbowPos로 linearly blend함
        /// startBelowY = 0.1, weight2 = 2.0f, startBelowDistance = 0.5
        /// </summary>
        void correctElbowAfterPositioning()
        {
            Vector3 localTargetPos = transform.InverseTransformPoint(target.position) / upperBody.armLength;
            Vector3 shoulderWristDirection = (transform.position - wrist.position).normalized;
            Vector3 elbowPos = localElbowPos;

            if (isLeft)
                elbowPos.x *= -1f;

            Vector3 targetDir = upperBody.transform.rotation * elbowPos.normalized;
            Vector3 cross = Vector3.Cross(shoulderWristDirection, targetDir);

            Vector3 shoulderUp = shoulderRotation * Vector3.up;


            Vector3 distance = target.position - transform.position;
            distance = distance.magnitude * upperBody.transform.InverseTransformDirection(distance / distance.magnitude);

            float weight = Mathf.Clamp01(Mathf.Clamp01((startBelowDistance - distance.xz().magnitude / upperBody.armLength) /
                           startBelowDistance) * weight2 + Mathf.Clamp01((-distance.z + .1f) * 3)) *
                           Mathf.Clamp01((startBelowY - localTargetPos.y) /
                                         startBelowY);

            float elbowTargetUp = Vector3.Dot(shoulderUp, targetDir);
            float elbowAngle2 = Vector3.Angle(cross, shoulderUp) + (isLeft ? 0f : 180f);
            Quaternion rotation = Quaternion.AngleAxis((elbowAngle2 * Mathf.Sign(elbowTargetUp)).toSignedEulerAngle() * Mathf.Clamp(weight, 0, 1f), shoulderWristDirection);
            transform.rotation = rotation * transform.rotation;
        }

        /// <summary>
        /// right 축 기준 wrist의 rotation에 따라 팔꿈치 위치 조정(어깨 회전각도 조정)
        /// </summary>
        void rotateElbowWithWristRight()
        {
            Vector3 wristUpVec = target.rotation * Vector3.up;
            float forwardAngle = VectorHelpers.getAngleBetween(elbowRotation * Vector3.right, target.rotation * Vector3.right,
                elbowRotation * Vector3.up, elbowRotation * Vector3.forward);

            // todo reduce influence if hand local forward rotation is high (hand tilted inside)
            Quaternion handForwardRotation = Quaternion.AngleAxis(-forwardAngle, elbowRotation * Vector3.forward);
            wristUpVec = handForwardRotation * wristUpVec;

            float elbowTargetAngle = VectorHelpers.getAngleBetween(elbowRotation * Vector3.up, wristUpVec,
                elbowRotation * Vector3.forward, elbowRotation * armDirection);

            float deltaElbow = (elbowTargetAngle + (isLeft ? -handDeltaOffset : handDeltaOffset)) / 180f;

            deltaElbow = Mathf.Sign(deltaElbow) * Mathf.Pow(Mathf.Abs(deltaElbow), handDeltaPow) * 180f * handDeltaFactor;
            interpolatedDeltaElbow =
                Mathf.LerpAngle(interpolatedDeltaElbow, deltaElbow, Time.deltaTime / rotateElbowWithHandDelay);

            rotateWithAngle(interpolatedDeltaElbow);
        }

        /// <summary>
        /// forward 축 기준 wrist의 rotation에 따라 팔꿈치 위치 조정(어깨 회전각도 조정)
        /// </summary>
        void rotateElbowWithWristFoward()
        {
            Vector3 wristRightVec = target.rotation * armDirection;

            float elbowTargetAngleForward = VectorHelpers.getAngleBetween(elbowRotation * armDirection, wristRightVec,
                elbowRotation * Vector3.up, elbowRotation * Vector3.forward);

            float deltaElbowForward = (elbowTargetAngleForward + (isLeft ? -handDeltaForwardOffset : handDeltaForwardOffset)) / 180f;

            if (Mathf.Abs(deltaElbowForward) < handDeltaForwardDeadzone)
                deltaElbowForward = 0f;
            else
            {
                deltaElbowForward = (deltaElbowForward - Mathf.Sign(deltaElbowForward) * handDeltaForwardDeadzone) / (1f - handDeltaForwardDeadzone);
            }

            deltaElbowForward = Mathf.Sign(deltaElbowForward) * Mathf.Pow(Mathf.Abs(deltaElbowForward), handDeltaForwardPow) * 180f;
            interpolatedDeltaElbowForward = Mathf.LerpAngle(interpolatedDeltaElbowForward, deltaElbowForward, Time.deltaTime / rotateElbowWithHandDelay);

            float signedInterpolated = interpolatedDeltaElbowForward.toSignedEulerAngle();

            rotateWithAngle(signedInterpolated * handDeltaForwardFactor);
        }

        /// <summary>
        /// 어깨-손 축 기준 회전 각도 반환
        /// 논문 수식 참고
        /// </summary>
        private float calcAngleAboutHandShoulderAxis()
        {
            Vector3 localWristPosNormalized = transform.InverseTransformDirection(wrist.position) / upperBody.armLength;
            float angle = offsetAngle;

            // angle from Y
            angle += yWeight * localWristPosNormalized.y;

            // angle from Z
            if (localWristPosNormalized.y > 0)
                angle += zWeightTop * (Mathf.Max(zStartDistance - localWristPosNormalized.z, 0f) * Mathf.Max(localWristPosNormalized.y, 0f));
            else
                angle += zWeightBottom * (Mathf.Max(zStartDistance - localWristPosNormalized.z, 0f) * Mathf.Max(-localWristPosNormalized.y, 0f));

            // angle from X
            angle += xWeight * Mathf.Max(localWristPosNormalized.x * (isLeft ? 1.0f : -1.0f) + xStartDistance, 0f);

            // clamp angle
            angle = Mathf.Clamp(angle, minAngle, maxAngle);

            return angle * (isLeft ? -1.0f : 1.0f);
        }

        /// <summary>
        /// 타겟(손목), 어깨, 팔꿈치 삼각형에 대해 코사인법칙을 이용해 어깨쪽 각도 구하기
        /// </summary>
        private float calcAngleAboutElbowAngle()
        {
            float angle;
            float targetShoulderDistance = (target.position - transform.position).magnitude;

            angle = (isLeft ? -1.0f : 1.0f) *
                Mathf.Acos(Mathf.Clamp((Mathf.Pow(targetShoulderDistance, 2.0f) + Mathf.Pow(upperBody.upperArmLength, 2.0f) -
                            Mathf.Pow(upperBody.lowerArmLength, 2.0f)) / (2.0f * targetShoulderDistance * upperBody.upperArmLength), -1.0f, 1.0f)) * Mathf.Rad2Deg;
            if (float.IsNaN(angle))
                angle = 0.0f;

            return angle;
        }
    }
}
