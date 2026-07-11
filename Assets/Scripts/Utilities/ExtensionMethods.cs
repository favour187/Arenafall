using System.Collections.Generic;
using UnityEngine;

namespace ArenaFall.Utilities
{
    /// <summary>
    /// Extension methods for common Unity types.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Get or add a component to a GameObject.
        /// </summary>
        public static T GetOrAdd<T>(this GameObject obj) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        /// <summary>
        /// Set layer on this GameObject and all children.
        /// </summary>
        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Check if a layer is in a layer mask.
        /// </summary>
        public static bool ContainsLayer(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        /// <summary>
        /// Get a random element from a list.
        /// </summary>
        public static T RandomElement<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Shuffle a list in-place.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Destroy all children of a transform.
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Get closest point on a line segment.
        /// </summary>
        public static Vector3 ClosestPointOnLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 line = lineEnd - lineStart;
            float t = Vector3.Dot(point - lineStart, line) / line.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return lineStart + line * t;
        }

        /// <summary>
        /// Convert a Vector3 to a flat Vector3 (ignoring Y).
        /// </summary>
        public static Vector3 Flatten(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }

        /// <summary>
        /// Get the direction to a target, ignoring height.
        /// </summary>
        public static Vector3 DirectionTo(this Vector3 source, Vector3 target)
        {
            return (target - source).Flatten().normalized;
        }

        /// <summary>
        /// Format time as mm:ss.
        /// </summary>
        public static string FormatTime(float seconds)
        {
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{mins}:{secs:D2}";
        }

        /// <summary>
        /// Format large numbers with K/M suffix.
        /// </summary>
        public static string FormatNumber(int number)
        {
            if (number >= 1000000)
                return $"{number / 1000000f:F1}M";
            if (number >= 1000)
                return $"{number / 1000f:F1}K";
            return number.ToString();
        }
    }
}
