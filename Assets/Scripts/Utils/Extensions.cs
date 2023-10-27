using UnityEngine;

namespace VRArmIKtoSMPL
{

    public static class MonoBehaviourExtensions
    {
        public static T GetOrAddComponent<T>(this Component self) where T : Component
        {
            T component = self.GetComponent<T>();
            return component != null ? component : self.gameObject.AddComponent<T>();
        }

        public static T GetOrAddComponentInChildren<T>(this MonoBehaviour self) where T : MonoBehaviour
        {
            T component = self.GetComponentInChildren<T>();
            return component != null ? component : self.gameObject.AddComponent<T>();
        }
    }

    public static class VectorExtensions
    {
        public static Vector3 toVector3(this Vector2 self)
        {
            return new Vector3(self.x, self.y);
        }

        public static Vector2 xy(this Vector3 self)
        {
            return new Vector2(self.x, self.y);
        }

        public static Vector2 xz(this Vector3 self)
        {
            return new Vector2(self.x, self.z);
        }

        public static Vector2 yz(this Vector3 self)
        {
            return new Vector2(self.y, self.y);
        }
    }

    public static class FloatExtensions
    {
        public static float toSignedEulerAngle(this float self)
        {
            float result = self.toPositiveEulerAngle();
            if (result > 180f)
                result = result - 360f;
            return result;
        }

        public static float toPositiveEulerAngle(this float self)
        {
            float result = (self % 360f + 360f) % 360f;
            return result;
        }
    }
}