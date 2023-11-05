using UnityEngine;

namespace VRArmIKtoSMPL
{
    /// <summary>
    /// neck의 x축 회전(pitch)를 upper body 전체의 회전으로 변경
    /// spine1은 pelvis의 하위인 upper body의 최상위 오브젝트
    /// </summary>
    public class Spine1 : MonoBehaviour
    {
        public AvatarVRTrackingReferences avatarTrackingReferences;

        public UpperBodyTransform upperBody;

        // for pitch, beta_{n,0} and b
        public float weight1OfRotationAboutX = 135.3f;
        public float weight2OfRotationAboutX = 0.333f;
        public float rightRotationHeadRotationOffset = -20f;

        public float upperBodyRightRotation;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            rotateUpperBodyAboutX();
        }


        /// <summary>
        /// hmd의 x축 회전(pitch)에 대해서 
        /// 상체 forward vector와 hmd forward vector의 차이를 사용
        /// rightRotationHeadRotationOffset은 논문에 언급되지 않았으나 기본 hmd의 방향이 살짝 위로 향한 것을 고려한 값으로 보임
        /// </summary>
        void rotateUpperBodyAboutX()
        {
            float hmdHeight = avatarTrackingReferences.hmd.position.y;
            float heightRatio = Mathf.Clamp((upperBody.playerHeightHmd - hmdHeight) / upperBody.playerHeightHmd, 0f, 1f);

            float headRightRotation = VectorHelpers.getAngleBetween(transform.forward,
                                          avatarTrackingReferences.hmd.forward,
                                          Vector3.up, transform.right) + rightRotationHeadRotationOffset;

            upperBodyRightRotation = heightRatio * weight1OfRotationAboutX;
            upperBodyRightRotation += Mathf.Clamp(headRightRotation * weight2OfRotationAboutX * heightRatio, 0f, 50f);

            //Quaternion deltaRot = Quaternion.AngleAxis(neckRightRotation, transform.right);
            //transform.rotation = deltaRot * transform.rotation;

            Vector3 targetRotation = new Vector3(upperBodyRightRotation, 0f, 0f);
            transform.eulerAngles = targetRotation;

            //positionNeckRelative();
        }

        /// <summary>
        /// neck rotation을 upper body로 변경함으로 인해 사용하지 않는 함수
        /// 참고용
        /// hmd의 위치와 rotation 정도를 고려하여 몸통을 앞으로 숙인 상태를 표현하는 것으로 보임
        /// </summary>
        void positionNeckRelative()
        {
            Quaternion deltaRot = Quaternion.AngleAxis(upperBodyRightRotation, transform.right);
            Vector3 shoulderHeadDiff = transform.position - avatarTrackingReferences.head.position;
            transform.position = deltaRot * shoulderHeadDiff + avatarTrackingReferences.head.position;
        }
    }
}
