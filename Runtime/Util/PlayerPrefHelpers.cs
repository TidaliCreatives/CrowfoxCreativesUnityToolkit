using System;
using UnityEngine;

namespace Crowfox.Util
{
    /// <summary>
    /// Small, practical PlayerPrefs helpers.
    /// - Fluent "SaveToPrefs" extensions (keep your original style)
    /// - Safe getters with defaults
    /// - Bool support
    /// - JSON support for simple objects
    /// - Optional immediate disk flush (PlayerPrefs.Save)
    /// </summary>
    public static class PlayerPrefHelpers
    {
        // =========================
        // Save (fluent extensions)
        // =========================

        public static string SaveToPrefs(this string value, string key, bool saveImmediately = false)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("PlayerPrefHelpers: Key is null/empty. Nothing was saved.");
                return value;
            }

            PlayerPrefs.SetString(key, value ?? string.Empty);

            if (saveImmediately)
                PlayerPrefs.Save();

            return value;
        }

        public static int SaveToPrefs(this int value, string key, bool saveImmediately = false)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("PlayerPrefHelpers: Key is null/empty. Nothing was saved.");
                return value;
            }

            PlayerPrefs.SetInt(key, value);

            if (saveImmediately)
                PlayerPrefs.Save();

            return value;
        }

        public static float SaveToPrefs(this float value, string key, bool saveImmediately = false)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("PlayerPrefHelpers: Key is null/empty. Nothing was saved.");
                return value;
            }

            PlayerPrefs.SetFloat(key, value);

            if (saveImmediately)
                PlayerPrefs.Save();

            return value;
        }

        public static bool SaveToPrefs(this bool value, string key, bool saveImmediately = false)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("PlayerPrefHelpers: Key is null/empty. Nothing was saved.");
                return value;
            }

            PlayerPrefs.SetInt(key, value ? 1 : 0);

            if (saveImmediately)
                PlayerPrefs.Save();

            return value;
        }

        public static Color SaveToPrefs(this Color value, string key, bool saveImmediately = false)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("PlayerPrefHelpers: Key is null/empty. Nothing was saved.");
                return value;
            }

            // Store as HTML RGBA (#RRGGBBAA)
            string html = ColorUtility.ToHtmlStringRGBA(value);
            PlayerPrefs.SetString(key, html);

            if (saveImmediately)
                PlayerPrefs.Save();

            return value;
        }

        // =========================
        // Get (safe getters)
        // =========================

        public static string GetString(string key, string defaultValue = "")
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            return PlayerPrefs.GetString(key, defaultValue);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        public static Color GetColor(string key, Color defaultValue)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            string html = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(html))
                return defaultValue;

            // Unity expects #RRGGBB or #RRGGBBAA
            if (!html.StartsWith("#")) html = "#" + html;

            if (ColorUtility.TryParseHtmlString(html, out Color c))
                return c;

            return defaultValue;
        }

        // =========================
        // JSON (simple objects)
        // =========================

        public static T SaveJsonToPrefs<T>(this T value, string key, bool saveImmediately = false)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("PlayerPrefHelpers: Key is null/empty. Nothing was saved.");
                return value;
            }

            try
            {
                string json = JsonUtility.ToJson(value);
                PlayerPrefs.SetString(key, json);

                if (saveImmediately)
                    PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"PlayerPrefHelpers: Failed to save JSON for key '{key}'. {e.Message}");
            }

            return value;
        }

        public static bool TryGetJson<T>(string key, out T value, T fallback = default)
        {
            value = fallback;

            // Validate input
            if (string.IsNullOrWhiteSpace(key))
                return false;

            string json = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                value = JsonUtility.FromJson<T>(json);
                return value != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"PlayerPrefHelpers: Failed to read JSON for key '{key}'. {e.Message}");
                value = fallback;
                return false;
            }
        }

        // =========================
        // Convenience / maintenance
        // =========================

        public static bool HasKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return PlayerPrefs.HasKey(key);
        }

        public static void DeleteKey(string key, bool saveImmediately = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            PlayerPrefs.DeleteKey(key);

            if (saveImmediately)
                PlayerPrefs.Save();
        }

        public static void DeleteAll(bool saveImmediately = false)
        {
            PlayerPrefs.DeleteAll();

            if (saveImmediately)
                PlayerPrefs.Save();
        }
    }
}
