using UnityEngine;

namespace VRArmIKtoSMPL
{
    public class Head : MonoBehaviour
    {
        public Transform target;

        Quaternion startRotation;

        void setRotation(Quaternion rotation) => transform.rotation = rotation * startRotation;
        void setLocalRotation(Quaternion rotation) => transform.rotation = transform.parent.rotation * rotation * startRotation;

        // Start is called before the first frame update
        void Awake()
        {
            startRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            rotateHead();
        }


        public void rotateHead()
        {
            setRotation(target.rotation);
        }
    }
}
