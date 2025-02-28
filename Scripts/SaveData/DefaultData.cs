using UnityEngine;

namespace GGemCo.Scripts.SaveData
{
    public class DefaultData
    {
        protected static void PlayerPrefsSaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }
        protected static void PlayerPrefsSaveLong(string key, long value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }
        protected int PlayerPrefsLoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
        protected long PlayerPrefsLoadLong(string key, long defaultValue = 0)
        {
            return (long)PlayerPrefs.GetFloat(key, defaultValue);
        }
    }
}