using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Crowfox.Util
{
    public class SmartButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Interactivity")]
        public bool interactable = true;

        public bool leftIsInteractable = true;
        public bool rightIsInteractable = true;
        public bool middleIsInteractable = true;

        [Header("Double Click")]
        [SerializeField] private bool useDoubleClick = false;
        [SerializeField] private float doubleClickTime = 0.5f;

        [Header("Events")]
        public UnityEvent LeftClickDown = new();
        public UnityEvent LeftClickUp = new();
        public UnityEvent LeftClick = new();
        [Space]
        public UnityEvent MiddleClickDown = new();
        public UnityEvent MiddleClickUp = new();
        public UnityEvent MiddleClick = new();
        [Space]
        public UnityEvent RightClickDown = new();
        public UnityEvent RightClickUp = new();
        public UnityEvent RightClick = new();

        private float lastLeftClickTime = float.NegativeInfinity;
        private float lastMiddleClickTime = float.NegativeInfinity;
        private float lastRightClickTime = float.NegativeInfinity;

        private void OnEnable()
        {
            LeftClickDown ??= new UnityEvent();
            LeftClickUp ??= new UnityEvent();
            LeftClick ??= new UnityEvent();

            MiddleClickDown ??= new UnityEvent();
            MiddleClickUp ??= new UnityEvent();
            MiddleClick ??= new UnityEvent();

            RightClickDown ??= new UnityEvent();
            RightClickUp ??= new UnityEvent();
            RightClick ??= new UnityEvent();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;

            if (eventData.button == PointerEventData.InputButton.Left && leftIsInteractable) LeftClickDown.Invoke();
            if (eventData.button == PointerEventData.InputButton.Middle && middleIsInteractable) MiddleClickDown.Invoke();
            if (eventData.button == PointerEventData.InputButton.Right && rightIsInteractable) RightClickDown.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;

            if (eventData.button == PointerEventData.InputButton.Left && leftIsInteractable) LeftClickUp.Invoke();
            if (eventData.button == PointerEventData.InputButton.Middle && middleIsInteractable) MiddleClickUp.Invoke();
            if (eventData.button == PointerEventData.InputButton.Right && rightIsInteractable) RightClickUp.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;

            float now = Time.unscaledTime;

            if (eventData.button == PointerEventData.InputButton.Left && leftIsInteractable)
            {
                if (ShouldFireClick(now, ref lastLeftClickTime)) LeftClick.Invoke();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Middle && middleIsInteractable)
            {
                if (ShouldFireClick(now, ref lastMiddleClickTime)) MiddleClick.Invoke();
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Right && rightIsInteractable)
            {
                if (ShouldFireClick(now, ref lastRightClickTime)) RightClick.Invoke();
                return;
            }
        }

        private bool ShouldFireClick(float now, ref float lastClickTime)
        {
            if (!useDoubleClick)
                return true;

            if (now - lastClickTime <= doubleClickTime)
            {
                lastClickTime = float.NegativeInfinity;
                return true;
            }

            lastClickTime = now;
            return false;
        }

        // Manual triggers (optional, keeps your old style usable)
        public void OnLeftClickDown() { if (interactable && leftIsInteractable) LeftClickDown.Invoke(); }
        public void OnLeftClickUp() { if (interactable && leftIsInteractable) LeftClickUp.Invoke(); }
        public void OnLeftClick() { if (interactable && leftIsInteractable) LeftClick.Invoke(); }

        public void OnMiddleClickDown() { if (interactable && middleIsInteractable) MiddleClickDown.Invoke(); }
        public void OnMiddleClickUp() { if (interactable && middleIsInteractable) MiddleClickUp.Invoke(); }
        public void OnMiddleClick() { if (interactable && middleIsInteractable) MiddleClick.Invoke(); }

        public void OnRightClickDown() { if (interactable && rightIsInteractable) RightClickDown.Invoke(); }
        public void OnRightClickUp() { if (interactable && rightIsInteractable) RightClickUp.Invoke(); }
        public void OnRightClick() { if (interactable && rightIsInteractable) RightClick.Invoke(); }
    }
}
