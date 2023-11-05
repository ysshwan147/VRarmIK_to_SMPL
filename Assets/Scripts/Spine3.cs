using UnityEngine;

namespace VRArmIKtoSMPL
{
	public class Spine3 : MonoBehaviour
	{
        public AvatarVRTrackingReferences avatarTrackingReferences;

        public float maxDeltaHeadRotation = 80f;

        public bool autoDetectHandsBehindHead = true;
        public bool clampRotationToHead = true;

        bool handsBehindHead = false;

        [DisplayOnly][SerializeField] bool clampingHeadRotation = false;

        Vector3 lastAngle = Vector3.zero;

        void Update()
        {
            rotateUpperBodyAboutY();
        }

        /// <summary>
        /// 상체를 y축에 대하여 회전(yaw)
        /// </summary>
        private void rotateUpperBodyAboutY()
        {
            float angle = getCombinedDirectionAngleUp();

            Vector3 targetRotation = new Vector3(0f, angle, 0f);

            if (autoDetectHandsBehindHead)
            {
                detectHandsBehindHead(ref targetRotation);
            }

            if (clampRotationToHead)
            {
                clampHeadRotationDeltaUp(ref targetRotation);
            }

            transform.localEulerAngles = targetRotation;
        }

        /// <summary>
        /// 머리로부터 양 손까지의 두 벡터가 이루는 각이 180도를 넘어가면 상체가 바라보아야 할 forward vector의 방향이 뒤쪽을 향하게 됨
        /// 이를 확인해서 뒤 쪽을 향하게 되었을 때 180도를 더해 계속 앞으로 향할 수 있도록 함
        /// 양 손이 머리 앞 -> 뒤로 가면 handsBehindHead = false
        /// handsBehindHead = false면 targetRotation.y += 180f를 통해 neck이 뒤가 아닌 앞으로 계속 향하도록 함
        /// 양 손이 머리 뒤 -> 앞으로 가면 handsBehindHead = true
        /// </summary>
        private void detectHandsBehindHead(ref Vector3 targetRotation)
        {
            float delta = Mathf.Abs(targetRotation.y - lastAngle.y + 360f) % 360f;

            if (delta > 150f && delta < 210f && lastAngle.magnitude > 0.000001f && !clampingHeadRotation)
            {
                handsBehindHead = !handsBehindHead;
            }

            lastAngle = targetRotation;

            if (handsBehindHead)
            {
                targetRotation.y += 180f;
            }
        }

        /// <summary>
        /// head y축 rotation에 대해서 상체 y축 rotation을 maxDeltaHeadRotation만큼 clamp
        /// </summary>
        private void clampHeadRotationDeltaUp(ref Vector3 targetRotation)
        {
            float headUpRotation = (avatarTrackingReferences.head.transform.eulerAngles.y + 360f) % 360f;
            float targetUpRotation = (targetRotation.y + 360f) % 360f;

            float delta = headUpRotation - targetUpRotation;

            // 예를 들어 head가 forward vector가 정면을 향하고 있다고 가정했을 때(y축 회전값이 0일 때),
            // upper body의 target forward vector와의 delta가
            // maxDeltaHeadRotation ~ 180도에 있는 경우 확인(오른쪽 뒤를 향하고 있는 것)
            if (delta > maxDeltaHeadRotation && delta < 180f || delta < -180f && delta >= -360f + maxDeltaHeadRotation)
            {
                targetRotation.y = headUpRotation - maxDeltaHeadRotation;
                clampingHeadRotation = true;
            }
            // -maxDeltaHeadRotation ~ -180도에 있는 경우 확인(왼쪽 뒤를 향하고 있는 것)
            else if (delta < -maxDeltaHeadRotation && delta > -180 || delta > 180f && delta < 360f - maxDeltaHeadRotation)
            {
                targetRotation.y = headUpRotation + maxDeltaHeadRotation;
                clampingHeadRotation = true;
            }
            else
            {
                clampingHeadRotation = false;
            }
        }

        /// <summary>
        /// 양 손 위치에 대해 상체 forward vector의 y축 회전값 계산 및 반환
        /// </summary>
        private float getCombinedDirectionAngleUp()
        {
            Transform leftHand = avatarTrackingReferences.leftHand.transform, rightHand = avatarTrackingReferences.rightHand.transform;

            Vector3 distanceLeftHand = leftHand.position - transform.position,
                distanceRightHand = rightHand.position - transform.position;

            distanceLeftHand.y = 0;
            distanceRightHand.y = 0;

            Vector3 directionLeftHand = distanceLeftHand.normalized,
                directionRightHand = distanceRightHand.normalized;

            Vector3 combinedDirection = directionLeftHand + directionRightHand;

            return Mathf.Atan2(combinedDirection.x, combinedDirection.z) * 180f / Mathf.PI;
        }
    }
}
