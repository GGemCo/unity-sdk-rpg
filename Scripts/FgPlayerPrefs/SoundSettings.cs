using UnityEngine;

namespace GGemCo.Scripts.FgPlayerPrefs
{
    public static class SoundSettings
    {
        public const string bgmVolumeKey = "Sound_BGMVolume_Float";
        public const string sfxVolumeKey = "Sound_SFXVolume_Float";

        public static void SetBGMVolume(float volume)
        {
            PlayerPrefs.SetFloat(bgmVolumeKey, volume);
            PlayerPrefs.Save();
        }

        public static float GetBGMVolume()
        {
            return PlayerPrefs.GetFloat(bgmVolumeKey, 1.0f);
        }

        public static void SetSfxVolume(float volume)
        {
            PlayerPrefs.SetFloat(sfxVolumeKey, volume);
            PlayerPrefs.Save();
        }

        public static float GetSfxVolume()
        {
            return PlayerPrefs.GetFloat(sfxVolumeKey, 1.0f);
        }
    }
}