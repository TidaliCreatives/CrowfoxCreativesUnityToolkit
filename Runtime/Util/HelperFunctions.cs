using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Crowfox.Util
{
    public static class HelperFunctions
    {
        // -------------------------------------
        // TryGetComponentInChildren (iterative)
        // -------------------------------------

        // Iterative search for a component in children (no recursion)
        public static bool TryGetComponentInChildren<T>(this Transform current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            // Validate input
            if (current == null)
            {
                component = null;
                return false;
            }

            // Check current
            if (doIncludeCurrent && current.TryGetComponent(out component))
                return true;

            // Iterative DFS using a stack
            var stack = new Stack<Transform>(32);
            for (int i = 0; i < current.childCount; i++)
                stack.Push(current.GetChild(i));

            while (stack.Count > 0)
            {
                var t = stack.Pop();

                if (t.TryGetComponent(out component))
                    return true;

                for (int i = 0; i < t.childCount; i++)
                    stack.Push(t.GetChild(i));
            }

            component = null;
            return false;
        }

        public static bool TryGetComponentInChildren<T>(this GameObject current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            component = null;
            return current != null && current.transform.TryGetComponentInChildren(out component, doIncludeCurrent);
        }

        public static bool TryGetComponentInChildren<T>(this Component current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            component = null;
            return current != null && current.transform.TryGetComponentInChildren(out component, doIncludeCurrent);
        }


        // ------------------------
        // TryGetComponentInParents
        // ------------------------

        // Search for a component in parents
        public static bool TryGetComponentInParents<T>(this Transform current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            // Validate input
            if (current == null)
            {
                component = null;
                return false;
            }

            // Decide where to start
            var t = doIncludeCurrent ? current : current.parent;

            while (t != null)
            {
                if (t.TryGetComponent(out component))
                    return true;

                t = t.parent;
            }

            component = null;
            return false;
        }

        public static bool TryGetComponentInParents<T>(this GameObject current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            component = null;
            return current != null && current.transform.TryGetComponentInParents(out component, doIncludeCurrent);
        }

        public static bool TryGetComponentInParents<T>(this Component current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            component = null;
            return current != null && current.transform.TryGetComponentInParents(out component, doIncludeCurrent);
        }


        // -------------------------
        // TryGetComponentInSiblings
        // -------------------------

        // Search for component in siblings
        public static bool TryGetComponentInSiblings<T>(this Transform current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            // Validate input
            if (current == null)
            {
                component = null;
                return false;
            }

            // Check current
            if (doIncludeCurrent && current.TryGetComponent(out component))
                return true;

            Transform parent = current.parent;
            if (parent == null)
            {
                component = null;
                return false;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                var sibling = parent.GetChild(i);

                if (!doIncludeCurrent && sibling == current)
                    continue;

                if (sibling.TryGetComponent(out component))
                    return true;
            }

            component = null;
            return false;
        }

        public static bool TryGetComponentInSiblings<T>(this GameObject current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            component = null;
            return current != null && current.transform.TryGetComponentInSiblings(out component, doIncludeCurrent);
        }

        public static bool TryGetComponentInSiblings<T>(this Component current, out T component, bool doIncludeCurrent = true) where T : Component
        {
            component = null;
            return current != null && current.transform.TryGetComponentInSiblings(out component, doIncludeCurrent);
        }


        // --------------------------------------
        // TryGetComponentsInChildren (non-alloc)
        // --------------------------------------

        // Non-alloc search for all components on object and in children
        // Caller provides results list and (optionally) a working stack to avoid allocations.
        public static bool TryGetComponentsInChildrenNonAlloc<T>(this Transform current, List<T> results, bool doIncludeCurrent = true, Stack<Transform> workStack = null) where T : Component
        {
            // Validate input
            if (results == null)
                return false;

            results.Clear();

            if (current == null)
                return false;

            // Use provided stack or allocate a local one (allocates once per call)
            bool ownsStack = workStack == null;
            if (ownsStack) workStack = new Stack<Transform>(32);
            else workStack.Clear();

            // Check current
            if (doIncludeCurrent && current.TryGetComponent(out T foundOnCurrent))
                results.Add(foundOnCurrent);

            // Push children
            for (int i = 0; i < current.childCount; i++)
                workStack.Push(current.GetChild(i));

            // DFS
            while (workStack.Count > 0)
            {
                var t = workStack.Pop();

                if (t.TryGetComponent(out T c))
                    results.Add(c);

                for (int i = 0; i < t.childCount; i++)
                    workStack.Push(t.GetChild(i));
            }

            // If caller didn't provide stack, we just let it go out of scope
            return results.Count > 0;
        }

        public static bool TryGetComponentsInChildrenNonAlloc<T>(this GameObject current, List<T> results, bool doIncludeCurrent = true, Stack<Transform> workStack = null) where T : Component
        {
            if (current == null)
            {
                if (results != null) results.Clear();
                return false;
            }

            return current.transform.TryGetComponentsInChildrenNonAlloc(results, doIncludeCurrent, workStack);
        }

        public static bool TryGetComponentsInChildrenNonAlloc<T>(this Component current, List<T> results, bool doIncludeCurrent = true, Stack<Transform> workStack = null) where T : Component
        {
            if (current == null)
            {
                if (results != null) results.Clear();
                return false;
            }

            return current.transform.TryGetComponentsInChildrenNonAlloc(results, doIncludeCurrent, workStack);
        }


        // --------------------------------------
        // TryGetComponentsInSiblings (non-alloc)
        // --------------------------------------

        // Non-alloc search for all components in siblings
        public static bool TryGetComponentsInSiblingsNonAlloc<T>(this Transform current, List<T> results, bool doIncludeCurrent = true) where T : Component
        {
            // Validate input
            if (results == null)
                return false;

            results.Clear();

            if (current == null)
                return false;

            Transform parent = current.parent;
            if (parent == null)
                return false;

            for (int i = 0; i < parent.childCount; i++)
            {
                var sibling = parent.GetChild(i);

                if (!doIncludeCurrent && sibling == current)
                    continue;

                if (sibling.TryGetComponent(out T component))
                    results.Add(component);
            }

            return results.Count > 0;
        }

        public static bool TryGetComponentsInSiblingsNonAlloc<T>(this GameObject current, List<T> results, bool doIncludeCurrent = true) where T : Component
        {
            if (current == null)
            {
                if (results != null) results.Clear();
                return false;
            }

            return current.transform.TryGetComponentsInSiblingsNonAlloc(results, doIncludeCurrent);
        }

        public static bool TryGetComponentsInSiblingsNonAlloc<T>(this Component current, List<T> results, bool doIncludeCurrent = true) where T : Component
        {
            if (current == null)
            {
                if (results != null) results.Clear();
                return false;
            }

            return current.transform.TryGetComponentsInSiblingsNonAlloc(results, doIncludeCurrent);
        }


        // --------------------------------
        // Safe destroy with optional delay
        // --------------------------------

        public static void SafeDestroy(this GameObject obj, float delay = 0f)
        {
            if (obj != null)
                Object.Destroy(obj, delay);
        }


        // -------------------------------------
        // Screen bounds / RectTransform fitting
        // -------------------------------------

        public static Rect GetScreenBoundsRect(Renderer renderer, Camera cam)
        {
            // Validate input
            if (renderer == null || cam == null)
                return default;

            Bounds bounds = renderer.bounds;

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            Vector3[] pts = new Vector3[8] {
                center + new Vector3( extents.x,  extents.y,  extents.z),
                center + new Vector3( extents.x,  extents.y, -extents.z),
                center + new Vector3( extents.x, -extents.y,  extents.z),
                center + new Vector3( extents.x, -extents.y, -extents.z),
                center + new Vector3(-extents.x,  extents.y,  extents.z),
                center + new Vector3(-extents.x,  extents.y, -extents.z),
                center + new Vector3(-extents.x, -extents.y,  extents.z),
                center + new Vector3(-extents.x, -extents.y, -extents.z)
            };

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            bool anyInFront = false;

            for (int i = 0; i < pts.Length; i++)
            {
                Vector3 sp = cam.WorldToScreenPoint(pts[i]);

                // If behind the camera: skip
                if (sp.z < 0) continue;

                anyInFront = true;

                minX = Mathf.Min(minX, sp.x);
                maxX = Mathf.Max(maxX, sp.x);
                minY = Mathf.Min(minY, sp.y);
                maxY = Mathf.Max(maxY, sp.y);
            }

            if (!anyInFront)
                return default;

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public static void FitRectTransformToScreenRect(RectTransform rt, Rect screenRect, float minWidth = 0f, float minHeight = 0f)
        {
            // Validate input
            if (rt == null)
                return;

            Vector2 size = new(
                Mathf.Max(minWidth, screenRect.width),
                Mathf.Max(minHeight, screenRect.height)
            );

            Vector2 pos = new(
                screenRect.x + screenRect.width * 0.5f,
                screenRect.y + screenRect.height * 0.5f
            );

            // Convert screen space → canvas local space
            Canvas canvas = rt.GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            RectTransform canvasRT = canvas.GetComponent<RectTransform>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT,
                pos,
                cam,
                out Vector2 localPos
            );

            rt.anchoredPosition = localPos;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }


        // ----------------------------------------
        // Layers
        // ----------------------------------------

        // Sets layer of this and all children objects to the specified layer
        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            // Validate input
            if (obj == null)
                return;

            obj.layer = layer;

            foreach (Transform child in obj.transform)
                child.gameObject.SetLayerRecursively(layer);
        }

        public static void SetLayerRecursively(this Transform obj, int layer)
        {
            // Validate input
            if (obj == null)
                return;

            obj.gameObject.layer = layer;

            foreach (Transform child in obj)
                child.SetLayerRecursively(layer);
        }

        public static void SetLayerRecursively(this Component obj, int layer)
        {
            // Validate input
            if (obj == null)
                return;

            obj.gameObject.layer = layer;

            foreach (Transform child in obj.transform)
                child.SetLayerRecursively(layer);
        }


        // ----------------------------------------
        // Tasks
        // ----------------------------------------
        public static IEnumerator WaitForTask(Task task, float timeoutSeconds = -1f)
        {
            float start = Time.realtimeSinceStartup;

            while (!task.IsCompleted)
            {
                if (timeoutSeconds > 0f && (Time.realtimeSinceStartup - start) >= timeoutSeconds)
                {
                    Debug.LogWarning("WaitForTask: timed out.");
                    yield break;
                }

                yield return null;
            }

            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
                yield break;
            }

            if (task.IsCanceled)
            {
                Debug.LogWarning("WaitForTask: task was canceled.");
                yield break;
            }
        }

        public static IEnumerator WaitForTask<T>(Task<T> task, System.Action<T> onSuccess, float timeoutSeconds = -1f)
        {
            float start = Time.realtimeSinceStartup;

            while (!task.IsCompleted)
            {
                if (timeoutSeconds > 0f && (Time.realtimeSinceStartup - start) >= timeoutSeconds)
                {
                    Debug.LogWarning("WaitForTask<T>: timed out.");
                    yield break;
                }

                yield return null;
            }

            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
                yield break;
            }

            if (task.IsCanceled)
            {
                Debug.LogWarning("WaitForTask<T>: task was canceled.");
                yield break;
            }

            onSuccess?.Invoke(task.Result);
        }

        public static bool TryGetLocalIPAddress(out string localIP)
        {
            localIP = "";
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            return false;
        }

        public static float NormalizeAngle(this float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }

        public static float ClampAngle(this float angle, Vector2 limits)
        {
            angle = NormalizeAngle(angle);
            return Mathf.Clamp(angle, limits.x, limits.y);
        }
        public class RaycastHitDistanceComparer : IComparer<RaycastHit>
        {
            public static readonly RaycastHitDistanceComparer Instance = new();
            public int Compare(RaycastHit a, RaycastHit b)
            {
                return a.distance.CompareTo(b.distance);
            }
        }

        [System.Serializable]
        public struct SerializableCurveKey
        {
            public int time;
            public float value;
        }

        [System.Serializable]
        public struct SerializableCurve
        {
            public List<SerializableCurveKey> Keys;
        }

        public static SerializableCurve SerializeCurve(this AnimationCurve curve)
        {
            SerializableCurve sc = new() { Keys = new List<SerializableCurveKey>() };

            foreach (var key in curve.keys)
            {
                sc.Keys.Add(new SerializableCurveKey
                {
                    time = Mathf.RoundToInt(key.time),
                    value = key.value
                });
            }
            sc.Keys.Sort((a, b) => a.time.CompareTo(b.time));

            return sc;
        }
    }
}