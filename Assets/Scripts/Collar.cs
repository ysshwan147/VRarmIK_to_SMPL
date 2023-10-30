using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRArmIKtoSMPL
{
	public class Collar : MonoBehaviour {

		public Transform target;
		public UpperBodyTransform upperBody;
		public Transform neck;
		public Transform shoulder;

		public bool isLeft = false;

		[LabelOverride("max forward rotation")]public float collarRotationLimitForward = 33f;
		[LabelOverride("max upward rotation")] public float collarRotationLimitUpward = 33f;
		public float collarRotationMultiplier = 30;
		public float noRotationThreshold = 0.5f;


		private Vector3 shoulderStartLocalPosition;
		

		// Use this for initialization
		void Start () {
			shoulderStartLocalPosition = neck.transform.InverseTransformPoint(shoulder.position);
        }
		
		// Update is called once per frame
		void Update () {
			rotateCollar();
        }

		/// <summary>
		/// 앞쪽과 위쪽 방향으로의 collar rotation
		/// 논문에선 shoulder joint의 yaw(y축 회전)와 roll(z축 회전)의 rotation으로 설명
		/// threshold: 0.5
		/// multiplier: 30
		/// clamp: 0~33
		/// </summary>
		void rotateCollar()
		{
			Vector3 targetHandPosition = target.position;
            Vector3 initialShoulderPos = neck.transform.TransformPoint(shoulderStartLocalPosition);
            Vector3 handShoulderOffset = targetHandPosition - initialShoulderPos;
			Vector3 targetAngle = transform.localEulerAngles;
			float armLength = upperBody.armLength;

			// rotation forward
			float forwardDistanceRatio = Vector3.Dot(handShoulderOffset, upperBody.transform.forward) / armLength;

			targetAngle.y = Mathf.Clamp((forwardDistanceRatio - noRotationThreshold) * collarRotationMultiplier, 0f, collarRotationLimitForward);
			targetAngle.y *= (isLeft ? 1.0f : -1.0f);

			// rotation upward
			float upwardDistanceRatio = Vector3.Dot(handShoulderOffset, upperBody.transform.up) / armLength;

            targetAngle.z = Mathf.Clamp((upwardDistanceRatio - noRotationThreshold) * collarRotationMultiplier, 0f, collarRotationLimitUpward);
            targetAngle.z *= (isLeft ? -1.0f : 1.0f);

            transform.localEulerAngles = targetAngle;
		}
	}
}