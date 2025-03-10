using UnityEngine;

namespace GGemCo.Scripts.Core
{
    public class PlayerPrefsManager
    {
        private const string KEY_SAVE_DATA_SLOT_INDEX = "GGEMCO_KEY_SAVE_DATA_SLOT_INDEX";

        private static void PlayerPrefsSave(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
        private int PlayerPrefsLoadInt(string key, string defaultValue = "0")
        {
            return int.Parse(PlayerPrefs.GetString(key, defaultValue));
        }
        private float PlayerPrefsLoadFloat(string key, string defaultValue = "0")
        {
            return float.Parse(PlayerPrefs.GetString(key, defaultValue));
        }
        private long PlayerPrefsLoadLong(string key, string defaultValue = "0")
        {
            return long.Parse(PlayerPrefs.GetString(key, defaultValue));
        }
        private string PlayerPrefsLoad(string key)
        {
            return PlayerPrefs.GetString(key);
        }
        
        public void SaveSaveDataSlotIndex(int gameLoadSlotIndex)
        {
            PlayerPrefsSave(KEY_SAVE_DATA_SLOT_INDEX, gameLoadSlotIndex.ToString());
        }
        public int LoadSaveDataSlotIndex()
        {
            return PlayerPrefsLoadInt(KEY_SAVE_DATA_SLOT_INDEX);
        }
    }
}