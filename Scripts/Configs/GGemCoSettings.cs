using UnityEngine;

namespace GGemCo.Scripts.Configs
{
    [CreateAssetMenu(fileName = "GGemCoSettings", menuName = "GGemCo/Settings", order = 1)]
    public class GGemCoSettings : ScriptableObject
    {
        [Header("기본 설정")] [Tooltip("스파인2D 사용 여부. 사용할 경우 #define GGEMCO_USE_SPINE 이 추가됩니다. 스파인2D 패키지를 추가해주세요.")]
        public bool useSpine2d;

        // [Header("기본 설정")]
        // [Tooltip("이 설정은 첫 번째 기능의 기본 값을 정의합니다.")]
        // public string setting1 = "Default Value";
        //
        // [Tooltip("이 설정은 두 번째 기능의 값이며, 기본값은 10입니다.")]
        // public int setting2 = 10;
        //
        // [Header("고급 설정")]
        // [Tooltip("활성화하면 추가 기능이 동작합니다.")]
        // public bool enableFeature = false;
        //
        // [Tooltip("이 값은 0~100 범위에서 설정할 수 있습니다.")]
        // [Range(0, 100)]
        // public int advancedLevel = 50;
    }
}