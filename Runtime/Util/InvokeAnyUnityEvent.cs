using UnityEngine;
using UnityEngine.Events;

namespace Crowfox.Util
{
    public class InvokeAnyUnityEvent : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent unityEvent = new();

        /// <summary>
        /// Invokes the assigned UnityEvent.
        /// Can be called from UI, animation events, timeline, etc.
        /// </summary>
        public void InvokeEvent()
        {
            unityEvent.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("Invoke Event")]
        private void InvokeEventEditor()
        {
            unityEvent.Invoke();    
        }
#endif
    }
}
