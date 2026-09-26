using UnityEngine;

namespace Crowfox.Util
{
    public class LookAtHelper : MonoBehaviour
    {
        [SerializeField] Transform tr_Target;
        [SerializeField] bool _doInvertDirection = false;

        private void Update()
        {
            if (tr_Target != null)
            {
                var direction = tr_Target.position - transform.position;
                transform.forward = _doInvertDirection ? -direction : direction;
            }
        }
    }
}
