﻿using UnityEngine;

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

        /// <summary>
        /// 실질적으로 구하는 각은
        /// 축: 팔꿈치를 지나고 어깨, 팔꿈치, 손목 3개 point에 대한 평면의 법선
        /// 어깨-팔꿈치 라인을 연장한 직선에 대해 lower arm의 시계방향 회전각
        /// </summary>
        float calcInnerAngle()
        {
            float targetShoulderDistance = (target.position - shoulder.position).magnitude;
            float angle = 0.0f; // inner angle

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