using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Crowfox.Util;

namespace Crowfox.Audio
{
    public class UISounds : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public static event System.Action OnHovered;
        public static event System.Action OnDisabledHovered;
        public static event System.Action OnClicked;
        public static event System.Action OnDisabledClicked;

        [SerializeField] bool doIncreaseScale = false;
        [SerializeField] float scaleFactor = 1.1f;

        bool scaleIsActive = true;
        Vector3 initScale;
        InputSystem_Actions inputActions;

        private void OnEnable()
        {
            inputActions ??= new();
            inputActions.UI.Point.Enable();
        }

        private void OnDisable()
        {
            inputActions.UI.Point.Disable();
        }

        private void Awake()
        {
            initScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData pointerData)
        {
            if (!scaleIsActive) return;

            if (IsInteractableOrRaycastable())
            {
                OnHovered?.Invoke();
                if (doIncreaseScale)
                {
                    transform.localScale = initScale * scaleFactor;
                }
            }
            else
            {
                OnDisabledHovered?.Invoke();
            }
        }

        public void OnPointerExit(PointerEventData pointerData)
        {
            if (scaleIsActive) transform.localScale = initScale;
        }

        public void OnPointerClick(PointerEventData pointerData)
        {
            if (scaleIsActive)
            {
                OnClick(pointerData);
                transform.localScale = initScale;
            }
        }

        public void SetIncreaseScale(bool doScale)
        {
            scaleIsActive = doScale;
            transform.localScale = initScale;
        }

        bool IsInteractableOrRaycastable()
        {
            if (TryGetComponent(out Button button)) return button.interactable;
            if (TryGetComponent(out SmartButton smartButton)) return smartButton.interactable;
            if (TryGetComponent(out Slider slider)) return slider.interactable;
            if (TryGetComponent(out Toggle toggle)) return toggle.interactable;
            if (TryGetComponent(out InputField inputField)) return inputField.interactable;
            if (TryGetComponent(out Image image)) return image.raycastTarget;

            return true;
        }

        void OnClick(PointerEventData clickData)
        {
            bool? interactable = GetInteractableStatus(out SmartButton smartButton);

            if (smartButton != null)
            {
                if (IsClickEnabled(smartButton, clickData.button))
                {
                    OnClicked?.Invoke();
                }
                else
                {
                    OnDisabledClicked?.Invoke();
                }
                return;
            }

            if (interactable == null) return;
            else if (interactable == true) { OnClicked?.Invoke(); }
            else { OnDisabledClicked?.Invoke(); }
        }

        bool? GetInteractableStatus(out SmartButton smartButton)
        {
            if (TryGetComponent(out smartButton))
            {
                return smartButton.interactable;
            }

            if (TryGetComponent(out Button button)) return button.interactable;
            if (TryGetComponent(out Slider slider)) return slider.interactable;
            if (TryGetComponent(out Toggle toggle)) return toggle.interactable;
            if (TryGetComponent(out InputField inputField)) return inputField.interactable;
            if (TryGetComponent(out Image image)) return image.raycastTarget;

            return null;
        }

        bool IsClickEnabled(SmartButton smartButton, PointerEventData.InputButton button)
        {
            if (!smartButton.interactable) return false;

            switch (button)
            {
                case PointerEventData.InputButton.Left:
                    return smartButton.leftIsInteractable && smartButton.LeftClickDown != null;

                case PointerEventData.InputButton.Right:
                    return smartButton.rightIsInteractable && smartButton.RightClickDown != null;

                case PointerEventData.InputButton.Middle:
                    return smartButton.middleIsInteractable && smartButton.MiddleClickDown != null;

                default:
                    Debug.LogWarning($"Unsupported click button: {button}. Only Left, Right, and Middle clicks are supported.");
                    return false;
            }
        }
    }
}