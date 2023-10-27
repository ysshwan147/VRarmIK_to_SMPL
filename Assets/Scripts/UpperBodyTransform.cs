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

        //Quaternion shoulderStartRotation, elbowStartRotation, wristStartRotation;

        //Vector3 upperArmPos => arm.upperArm.position;
        //Vector3 lowerArmPos => arm.lowerArm.position;
        //Vector3 handPos => arm.hand.position;

        //Quaternion upperArmRotation => arm.upperArm.rotation * Quaternion.Inverse(upperArmStartRotation);
        //Quaternion lowerArmRotation => arm.lowerArm.rotation * Quaternion.Inverse(lowerArmStartRotation);
        //Quaternion handRotation => arm.hand.rotation * Quaternion.Inverse(handStartRotation);

        //void setShoulderRotation(Quaternion rotation) => shoulder.rotation = rotation * upperArmStartRotation;
        //void setElbowRotation(Quaternion rotation) => arm.lowerArm.rotation = rotation * lowerArmStartRotation;
        //void setElbowLocalRotation(Quaternion rotation) => arm.lowerArm.rotation = upperArmRotation * rotation * lowerArmStartRotation;
        //void setWristRotation(Quaternion rotation) => arm.wrist1.rotation = rotation * wristStartRotation;
        //void setWristLocalRotation(Quaternion rotation) => arm.wrist1.rotation = arm.lowerArm.rotation * rotation * wristStartRotation;


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
