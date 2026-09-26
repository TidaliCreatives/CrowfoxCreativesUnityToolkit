using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crowfox.Util
{
    public class MenuButtons : MonoBehaviour
    {
        public static event Action Action_OnGameReset;
        public static event Action Action_ResetVolumes;
        public static event Action Action_RestartLevel;
        public static event Action Action_NextLevel;
        public static event Action Action_ToMainMenu;
        public static event Action<int> Action_ChangeScene;

        public void ResetGame()
        {
            Action_OnGameReset?.Invoke();
        }

        public void ResetVolumes()
        {
            Action_ResetVolumes?.Invoke();
        }

        public void NextLevel()
        {
            Action_NextLevel?.Invoke();
        }

        public void RestartLevel()
        {
            Action_RestartLevel?.Invoke();
        }

        public void ToMainMenu()
        {
            Action_ToMainMenu?.Invoke();
        }

        public void ChangeScene(int index)
        {
            Action_ChangeScene?.Invoke(index);
        }
    }
}