using UnityEngine;

namespace GGemCo.Scripts.FgPlayerPrefs
{
    public static class LocaleSettings
    {
        public const string localeIndexKey = "Locale_index_Int";

        public static void SetLocaleIndex(int index)
        {
            PlayerPrefs.SetInt(localeIndexKey, index);
            PlayerPrefs.Save();
        }

        public static int GetLocaleIndex()
        {
            return PlayerPrefs.GetInt(localeIndexKey, 0);
        }
    }
}