using UnityEngine;

namespace VRArmIKtoSMPL
{
    public class Spine1 : MonoBehaviour
    {
        public AvatarVRTrackingReferences avatarTrackingReferences;

        public UpperBodyTransform upperBody;

        // for pitch, beta_{n,0} and b
        public float weight1OfRotationAboutX = 135.3f;
        public float weight2OfRotationAboutX = 0.333f;
        public float rightRotationStartHeight = 0f;
        public float rightRotationHeadRotationOffset = -20f;

        public float neckRightRotation;

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
        /// </summary>
        // pitch of hmd(beta_H) 를 구할 때,
        // 새로 작성한 코드는 hmd의 transform.eulerAngles.x로 그대로 사용
        // 깃헙 코드는 상체 forward vector와 hmd forward vector의 차이를 사용
        // positionShoulderRelative()는 hmd의 위치와 rotation 정도를 고려하여 몸통을 앞으로 숙인 상태를 표현하는 것으로 보임
        // hmd -90~90 밖에서 오류
        void rotateUpperBodyAboutX()
        {
            //float referencePlayerHeightHmd = upperBody.playerHeightHmd;
            //float playerHeightHmd = avatarTrackingReferences.hmd.transform.position.y;
            //float hmdRotationAboutX = avatarTrackingReferences.hmd.transform.eulerAngles.x;

            //// from 0~360 to - 180~180
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

            //Quaternion deltaRot = Quaternion.AngleAxis(neckRightRotation, transform.right);

            //transform.rotation = deltaRot * transform.rotation;

            Vector3 targetRotation = new Vector3(neckRightRotation, 0f, 0f);
            transform.eulerAngles = targetRotation;

            //positionNeckRelative();
        }

        void positionNeckRelative()
        {
            Quaternion deltaRot = Quaternion.AngleAxis(neckRightRotation, transform.right);
            Vector3 shoulderHeadDiff = transform.position - avatarTrackingReferences.head.transform.position;
            transform.position = deltaRot * shoulderHeadDiff + avatarTrackingReferences.head.transform.position;
        }
    }
}
