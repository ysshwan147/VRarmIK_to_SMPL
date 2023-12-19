using UnityEngine;

namespace VRArmIKtoSMPL
{
    /// <summary>
    /// hmd와 controller의 위치를 tracking하는 오브젝트의 하위 오브젝트의 local position and rotation
    /// head -> position: (0, -0.6, -0.6)
    /// leftWrist -> position: (-0.028, -0.031, -0.109) rotation: (-90, 90, 0)
    /// rightWrist -> position: (0.028, -0.031, -0.109) rotation: (-90, -90, 0)
    /// </summary>
    public class LocalTransformAboutTrackingObject : MonoBehaviour
    {

        public Vector3 localPosition;   
        public Vector3 localEulerAngles;

        // Use this for initialization
        void Awake()
        {
            transform.localPosition = localPosition;
            transform.localEulerAngles = localEulerAngles;
        }
    }
}