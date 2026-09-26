using UnityEngine;

public class DisableInBuild : MonoBehaviour
{
    void Awake()
    {
#if !UNITY_EDITOR
        gameObject.SetActive(false);
        return;
#endif
        if (!Application.isEditor)
            gameObject.SetActive(false);
    }
}
