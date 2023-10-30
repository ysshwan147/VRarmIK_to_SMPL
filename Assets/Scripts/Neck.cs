using UnityEngine;

namespace VRArmIKtoSMPL
{
	public class Neck : MonoBehaviour
	{

		public AvatarVRTrackingReferences avatarTrackingReferences;

        public UpperBodyTransform upperBody;

        public float headNeckDistance = 0.03f;
        public Vector3 headNeckDirectionVector = new Vector3(0f, -1f, -.05f);

        // for pitch, beta_{n,0} and b
        public float weight1OfRotationAboutX = 135.3f;
        public float weight2OfRotationAboutX = 0.333f;

        public float maxDeltaHeadRotation = 80f;

        public float rightRotationStartHeight = 0f;
        public float rightRotationHeightFactor = 142f;
        public float rightRotationHeadRotationFactor = 0.3f;
        public float rightRotationHeadRotationOffset = -20f;

        public bool ignoreYPos = true;
        public bool autoDetectHandsBehindHead = true;
        public bool clampRotationToHead = true;

        bool handsBehindHead = false;

        [DisplayOnly][SerializeField] bool clampingHeadRotation = false;

        public float neckRightRotation;

        Vector3 lastAngle = Vector3.zero;

        // Use this for initialization
        void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
            //positionNeck();
            rotateNeckAboutY();
            rotateNeckAboutX();
		}


        void positionNeck()
        {
            Vector3 headNeckOffset = avatarTrackingReferences.hmd.transform.rotation * headNeckDirectionVector;
            Vector3 targetPosition = avatarTrackingReferences.head.transform.position + headNeckOffset * headNeckDistance;
            transform.parent.localPosition = transform.parent.InverseTransformPoint(targetPosition);
        }


        // pitch of hmd(beta_H) 를 구할 때,
        // 새로 작성한 코드는 hmd의 transform.eulerAngles.x로 그대로 사용
        // 깃헙 코드는 상체 forward vector와 hmd forward vector의 차이를 사용
        // positionShoulderRelative()는 hmd의 위치와 rotation 정도를 고려하여 몸통을 앞으로 숙인 상태를 표현하는 것으로 보임
        // hmd -90~90 밖에서 오류
        void rotateNeckAboutX()
        {
            float referencePlayerHeightHmd = upperBody.playerHeightHmd;
            float playerHeightHmd = avatarTrackingReferences.hmd.transform.position.y;
            float hmdRotationAboutX = avatarTrackingReferences.hmd.transform.eulerAngles.x;

            // from 0~360 to -180~180
            if (hmdRotationAboutX > 180.0f)
            {
                hmdRotationAboutX -= 360.0f;
            }

            float ratio = (referencePlayerHeightHmd - playerHeightHmd) / referencePlayerHeightHmd;
            ratio = Mathf.Clamp(ratio, 0.0f, 1.0f);

            float rotationAngle = 0.0f;

            rotationAngle = ratio * (weight1OfRotationAboutX + weight2OfRotationAboutX * hmdRotationAboutX);

            Quaternion deltaRot = Quaternion.AngleAxis(rotationAngle, transform.right);
            transform.rotation = deltaRot * transform.rotation;


            //------------------------------------
            //github code
            //------------------------------------
            //float heightDiff = avatarTrackingReferences.hmd.transform.position.y;
            //float relativeHeightDiff = heightDiff / upperBody.playerHeightHmd;

            //float headRightRotation = VectorHelpers.getAngleBetween(transform.forward,
            //                              avatarTrackingReferences.hmd.transform.forward,
            //                              Vector3.up, transform.right) + rightRotationHeadRotationOffset;
            //float heightFactor = Mathf.Clamp(relativeHeightDiff - rightRotationStartHeight, 0f, 1f);

            //neckRightRotation = heightFactor * rightRotationHeightFactor;
            //neckRightRotation += Mathf.Clamp(headRightRotation * rightRotationHeadRotationFactor * heightFactor, 0f, 50f);

            //Quaternion deltaRot = Quaternion.AngleAxis(neckRightRotation, transform.right);

            //transform.rotation = deltaRot * transform.rotation;
            //positionNeckRelative();
        }

        void positionNeckRelative()
        {
            Quaternion deltaRot = Quaternion.AngleAxis(neckRightRotation, transform.right);
            Vector3 shoulderHeadDiff = transform.position - avatarTrackingReferences.head.transform.position;
            transform.position = deltaRot * shoulderHeadDiff + avatarTrackingReferences.head.transform.position;
        }

        /// <summary>
        /// neck joint를 y축에 대하여 회전(yaw)
        /// 논문에서는 neck joint의 회전이 목과 양 어깨가 모두 회전하는 것을 의미(목 관절의 하위 오브젝트가 어깨 관절)
        /// 즉, 상체 전제가 회전하는 것이므로 smpl 모델에서 적절한 관절에게 회전값을 줘야 함
        /// </summary>
        void rotateNeckAboutY()
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

            transform.eulerAngles = targetRotation;
        }
        
        /// <summary>
        /// 양 손이 머리 앞 -> 뒤로 가면 handsBehindHead = false
        /// handsBehindHead = false면 targetRotation.y += 180f를 통해 neck이 뒤가 아닌 앞으로 계속 향하도록 함
        /// 양 손이 머리 뒤 -> 앞으로 가면 handsBehindHead = true
        /// </summary>
        void detectHandsBehindHead(ref Vector3 targetRotation)
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
        /// 
        /// </summary>
        void clampHeadRotationDeltaUp(ref Vector3 targetRotation)
        {
            float headUpRotation = (avatarTrackingReferences.head.transform.eulerAngles.y + 360f) % 360f;
            float targetUpRotation = (targetRotation.y + 360f) % 360f;

            float delta = headUpRotation - targetUpRotation;

            if (delta > maxDeltaHeadRotation && delta < 180f || delta < -180f && delta >= -360f + maxDeltaHeadRotation)
            {
                targetRotation.y = headUpRotation - maxDeltaHeadRotation;
                clampingHeadRotation = true;
            }
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
        /// 양 손 위치에 대해 상체 forward vector의 y축에 대한 회전값 반환
        /// </summary>
        float getCombinedDirectionAngleUp()
        {
            Transform leftHand = avatarTrackingReferences.leftHand.transform, rightHand = avatarTrackingReferences.rightHand.transform;

            Vector3 distanceLeftHand = leftHand.position - transform.position,
                distanceRightHand = rightHand.position - transform.position;

            if (ignoreYPos)
            {
                distanceLeftHand.y = 0;
                distanceRightHand.y = 0;
            }

            Vector3 directionLeftHand = distanceLeftHand.normalized,
                directionRightHand = distanceRightHand.normalized;

            Vector3 combinedDirection = directionLeftHand + directionRightHand;

            return Mathf.Atan2(combinedDirection.x, combinedDirection.z) * 180f / Mathf.PI;
        }
    }
}
