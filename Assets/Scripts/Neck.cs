using UnityEngine;

namespace VRArmIKtoSMPL
{
    /// <summary>
    /// 논문에서는 neck joint의 회전이 목과 양 어깨가 모두 회전하는 것을 의미(목 관절의 하위 오브젝트가 어깨 관절)
    /// 즉, 상체가 회전하는 것이므로 smpl 모델에서 적절한 관절에게 회전값을 줘야 함
    /// 목과 양 어깨(정확히는 collar)의 상위 오브젝트들 중 하나에 회전값을 주거나, 각각에 나눠서 회전값을 주는 방법이 고려됨
    /// y축 회전은 spine3에서 수행하고
    /// x축 회전은 spine1에서 수행하는 방향으로 구현
    /// spine3, spine1 스크립트 따로 작성하였고 이 스크립트는 참고용
    /// </summary>
	public class Neck : MonoBehaviour
	{

		public AvatarVRTrackingReferences avatarTrackingReferences;

        public UpperBodyTransform upperBody;

        public float headNeckDistance = 0.03f;
        public Vector3 headNeckDirectionVector = new Vector3(0f, -0.9f, -.05f);
        public Vector3 neckShoulderDistance = new Vector3(0f, -.1f, -0.02f);

        // for pitch, beta_{n,0} and b
        public float weight1OfRotationAboutX = 135.3f;
        public float weight2OfRotationAboutX = 0.333f;
        public float rightRotationStartHeight = 0f;
        public float rightRotationHeadRotationOffset = -20f;

        public float maxDeltaHeadRotation = 80f;

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
            positionNeck();
            rotateNeckAboutY();
            rotateNeckAboutX();
		}


        void positionNeck()
        {
            Vector3 headNeckOffset = avatarTrackingReferences.hmd.transform.rotation * headNeckDirectionVector;
            Vector3 targetPosition = avatarTrackingReferences.head.transform.position + headNeckOffset * headNeckDistance;
            transform.localPosition = transform.parent.InverseTransformPoint(targetPosition) + neckShoulderDistance;
        }

        /// <summary>
        /// hmd의 x축 회전(pitch)에 대해서 
        /// </summary>
        // pitch of hmd(beta_H) 를 구할 때,
        // 새로 작성한 코드는 hmd의 transform.eulerAngles.x로 그대로 사용
        // 깃헙 코드는 상체 forward vector와 hmd forward vector의 차이를 사용
        // positionShoulderRelative()는 hmd의 위치와 rotation 정도를 고려하여 몸통을 앞으로 숙인 상태를 표현하는 것으로 보임
        // hmd -90~90 밖에서 오류
        void rotateNeckAboutX()
        {
            //float referencePlayerHeightHmd = upperBody.playerHeightHmd;
            //float playerHeightHmd = avatarTrackingReferences.hmd.transform.position.y;
            //float hmdRotationAboutX = avatarTrackingReferences.hmd.transform.eulerAngles.x;

            //// from 0~360 to -180~180
            //if (hmdRotationAboutX > 180.0f)
            //{
            //    hmdRotationAboutX -= 360.0f;
            //}

            //float ratio = (referencePlayerHeightHmd - playerHeightHmd) / referencePlayerHeightHmd;
            //ratio = Mathf.Clamp(ratio, 0.0f, 1.0f);

            //float rotationAngle = 0.0f;

            //rotationAngle = ratio * (weight1OfRotationAboutX + weight2OfRotationAboutX * hmdRotationAboutX);

            //Quaternion deltaRot = Quaternion.AngleAxis(rotationAngle, transform.right);
            //transform.rotation = deltaRot * transform.rotation;


            //------------------------------------
            //github code
            //------------------------------------
            float heightDiff = avatarTrackingReferences.hmd.transform.position.y;
            float relativeHeightDiff = (upperBody.playerHeightHmd - heightDiff) / upperBody.playerHeightHmd;

            float headRightRotation = VectorHelpers.getAngleBetween(transform.forward,
                                          avatarTrackingReferences.hmd.transform.forward,
                                          Vector3.up, transform.right) + rightRotationHeadRotationOffset;

            float heightFactor = Mathf.Clamp(relativeHeightDiff - rightRotationStartHeight, 0f, 1f);

            neckRightRotation = heightFactor * weight1OfRotationAboutX;
            neckRightRotation += Mathf.Clamp(headRightRotation * weight2OfRotationAboutX * heightFactor, 0f, 50f);

            Quaternion deltaRot = Quaternion.AngleAxis(neckRightRotation, transform.right);

            transform.rotation = deltaRot * transform.rotation;

            positionNeckRelative();
        }

        void positionNeckRelative()
        {
            Quaternion deltaRot = Quaternion.AngleAxis(neckRightRotation, transform.right);
            Vector3 shoulderHeadDiff = transform.position - avatarTrackingReferences.head.transform.position;
            transform.position = deltaRot * shoulderHeadDiff + avatarTrackingReferences.head.transform.position;
        }

        /// <summary>
        /// 상체를 y축에 대하여 회전(yaw)
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
        /// 머리로부터 양 손까지의 두 벡터가 이루는 각이 180도를 넘어가면 상체가 바라보아야 할 forward vector의 방향이 뒤쪽을 향하게 됨
        /// 이를 확인해서 뒤 쪽을 향하게 되었을 때 180도를 더해 계속 앞으로 향할 수 있도록 함
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
        /// head y축 rotation에 대해서 상체 y축 rotation을 maxDeltaHeadRotation만큼 clamp
        /// </summary>
        void clampHeadRotationDeltaUp(ref Vector3 targetRotation)
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
