using UnityEngine;

namespace Crowfox.Util
{
    public class MainCameraGetter : MonoBehaviour
    {
        private void Start()
        {
            if (TryGetComponent(out Canvas canvas)
                && canvas.worldCamera == null
                && canvas.renderMode == RenderMode.WorldSpace)
                canvas.worldCamera = Camera.main;
        }
    }
}