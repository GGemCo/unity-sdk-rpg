using UnityEngine;

namespace GGemCo.Scripts.SaveData
{
    public class DefaultData
    {
        protected static void PlayerPrefsSave(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
        protected int PlayerPrefsLoadInt(string key, string defaultValue = "0")
        {
            return int.Parse(PlayerPrefs.GetString(key, defaultValue));
        }
        protected float PlayerPrefsLoadFloat(string key, string defaultValue = "0")
        {
            return float.Parse(PlayerPrefs.GetString(key, defaultValue));
        }
        protected long PlayerPrefsLoadLong(string key, string defaultValue = "0")
        {
            return long.Parse(PlayerPrefs.GetString(key, defaultValue));
        }
        protected string PlayerPrefsLoad(string key)
        {
            return PlayerPrefs.GetString(key);
        }
    }
}