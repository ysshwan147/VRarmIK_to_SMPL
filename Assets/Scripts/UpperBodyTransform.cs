using UnityEngine;

namespace VRArmIKtoSMPL
{
	/// <summary>
	/// 논문에서는 hmd의 높이를 1.7, 어깨넓이를 0.31로 정의하고 있음
	/// 상체 각 길이에 대해 smpl 아바타 모델 기준으로 계산해서 적용
	/// </summary>
	public class UpperBodyTransform : MonoBehaviour
	{
		public Transform head, neck;
		public Transform leftShoulder, rightShoulder;
		public Transform leftElbow, rightElbow;
        public Transform leftWrist, rightWrist;

		public float headHmdHeightOffset = 0.6f;
        public float playerHeightHmd = 1.70f;
		public float playerWidthShoulders = 0.31f;

        public float upperArmLength  = 0.0f;
		public float lowerArmLength = 0.0f;
        public float armLength = 0.0f;


        // Use this for initialization
        void Awake()
		{
			playerHeightHmd = head.position.y + headHmdHeightOffset;

			playerWidthShoulders = distance(leftShoulder, rightShoulder);

			upperArmLength = distance(leftShoulder, leftElbow);
			lowerArmLength = distance(leftWrist, leftElbow);
			armLength = upperArmLength + lowerArmLength;
		}

		private float distance(Transform a, Transform b)
		{
			return (a.position - b.position).magnitude;
		}
    }
}
