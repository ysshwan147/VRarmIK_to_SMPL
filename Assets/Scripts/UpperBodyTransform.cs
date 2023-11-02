using UnityEngine;

namespace VRArmIKtoSMPL
{
	public class UpperBodyTransform : MonoBehaviour
	{
		public Transform head, neck;
		public Transform leftShoulder, rightShoulder;
		public Transform leftElbow, rightElbow;
        public Transform leftWrist, rightWrist;

        public float playerHeightHmd = 1.70f;
		public float playerWidthWrist = 1.39f;
		public float playerWidthShoulders = 0.31f;

        public float upperArmLength  = 0.0f;
		public float lowerArmLength = 0.0f;
        public float armLength = 0.0f;


        // Use this for initialization
        void Awake()
		{
			playerHeightHmd = head.position.y;

			playerWidthShoulders = distance(leftShoulder, rightShoulder);

			upperArmLength = distance(leftShoulder, leftElbow);
			lowerArmLength = distance(leftWrist, leftElbow);
			armLength = upperArmLength + lowerArmLength;
		}

		// Update is called once per frame
		void Update()
		{

		}

		private float distance(Transform a, Transform b)
		{
			return (a.position - b.position).magnitude;
		}
    }
}
