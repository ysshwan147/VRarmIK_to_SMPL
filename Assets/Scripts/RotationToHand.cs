using UnityEngine;

namespace VRArmIKtoSMPL
{
    public class RotationToHand : MonoBehaviour
    {
        public Transform targetHand;
        public bool isLeft = false;
        Vector3 armDirection;

        // Use this for initialization
        void Awake()
        {
            armDirection = isLeft? Vector3.left : Vector3.right;
        }

        // Update is called once per frame
        void Update()
        {
            rotateToHand();
        }

        private void rotateToHand()
        {
            Vector3 targetDirection = (targetHand.position - transform.position).normalized;

            transform.rotation = Quaternion.FromToRotation(armDirection, targetDirection);
        }
    }
}
