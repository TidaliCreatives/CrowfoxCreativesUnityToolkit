using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Crowfox.Util
{
        
    [RequireComponent(typeof(Button))]
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField] bool _isOn = false;
        [SerializeField] Image img_Icon;
        [SerializeField] Sprite sprite_On;
        [SerializeField] Sprite sprite_Off;
        public UnityEvent OnToggle;

        public void Toggle()
        {
            _isOn = !_isOn;
            UpdateVisuals();
            OnToggle?.Invoke();
        }

        void UpdateVisuals()
        {
            if (!img_Icon)
                return;

            img_Icon.sprite = _isOn ? sprite_On : sprite_Off;
        }
    }
}
