using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRArmIKtoSMPL
{
	public class Collar : MonoBehaviour {

		public AvatarVRTrackingReferences avatarTrackingReferences;
		public UpperBodyTransform upperBody;
		public Transform shoulder;

		public bool isLeft = false;

		[LabelOverride("max forward rotation")]public float collarRotationLimitForward = 33f;
		[LabelOverride("max upward rotation")] public float collarRotationLimitUpward = 33f;
		public float collarRotationMultiplier = 30;


		private Vector3 shoulderStartLocalPosition;
		

		// Use this for initialization
		void Start () {
			shoulderStartLocalPosition = shoulder.transform.localPosition;
		}
		
		// Update is called once per frame
		void Update () {
			rotateCollarForward();
            rotateCollarUpward();
        }

		void rotateCollarForward()
		{
			Vector3 handShoulderOffset = calcHandShoulderOffset();
			Vector3 targetAngle = transform.localEulerAngles;
			float armLength = upperBody.armLength;

			float forwardDistanceRatio = Vector3.Dot(handShoulderOffset, upperBody.transform.forward) / armLength;

			targetAngle.y = Mathf.Clamp((forwardDistanceRatio - 0.5f) * collarRotationMultiplier, 0f, collarRotationLimitForward);
			targetAngle.y *= (isLeft ? 1.0f : -1.0f);

            transform.localEulerAngles = targetAngle;
		}

        void rotateCollarUpward()
        {
            Vector3 handShoulderOffset = calcHandShoulderOffset();
			Vector3 targetAngle = transform.localEulerAngles;
            float armLength = upperBody.armLength;

            float upwardDistanceRatio = Vector3.Dot(handShoulderOffset, upperBody.transform.up) / armLength;

            targetAngle.z = Mathf.Clamp((upwardDistanceRatio - 0.5f) * collarRotationMultiplier, 0f, collarRotationLimitUpward);
            targetAngle.z *= (isLeft ? -1.0f : 1.0f);

            transform.localEulerAngles = targetAngle;
        }

        private Vector3 calcHandShoulderOffset()
		{
            Vector3 targetHandPosition;

            if (isLeft)
            {
                targetHandPosition = avatarTrackingReferences.leftHand.transform.position;
            }
            else
            {
                targetHandPosition = avatarTrackingReferences.rightHand.transform.position;
            }

            Vector3 initialShoulderPos = shoulder.transform.TransformPoint(shoulderStartLocalPosition);
            return targetHandPosition - initialShoulderPos;
        }
	}
}