using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI.Icon;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.UI
{
    /// <summary>
    /// 윈도우 관리 매니저
    /// </summary>
    public class UIWindowManager : MonoBehaviour
    {
        // 윈도우 고유번호 
        public enum WindowUid 
        {
            None,
            Hud,
            Inventory,
            ItemInfo,
            Equip,
            PlayerInfo,
        }
        [Header("기본속성")]
        [Tooltip("윈도우 리스트")]
        [SerializeField] private UIWindow[] uiWindows;
        // 아이템 아이콘용 프리팹
        private GameObject prefabIconItem;

        private void Awake()
        {
            if (AddressableSettingsLoader.Instance == null) return;
            prefabIconItem = AddressablePrefabLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyPrefabIconItem);
        }
        private void Start()
        {
            InitializationShowDisable();
        }
        /// <summary>
        /// 초기화시 안보여야 하는 윈도우 처리 
        /// </summary>
        private void InitializationShowDisable()
        {
            TableWindow tableWindow = TableLoaderManager.Instance.TableWindow;
            var tables = tableWindow.GetDatas();
            if (tables == null) return;
            foreach (var table in tables)
            {
                int uid = table.Key;
                if (uid == 0) continue;
                var info = tableWindow.GetDataByUid(uid);
                if (info == null || info.Uid <= 0 || info.DefaultActive) continue;
                GameObject window = uiWindows[uid].gameObject;
                if (window == null) continue;
                window.SetActive(false);
            }
        }
        /// <summary>
        /// 윈도우 보임/안보임 처리 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="show"></param>
        public void ShowWindow(WindowUid uid, bool show)
        {
            UIWindow uiWindow = GetUIWindowByUid<UIWindow>(uid);
            if (uiWindow == null) {
                GcLogger.LogError("UIWindow 컴포넌트가 없습니다. uid:"+uid);
                return;
            }

            uiWindow.Show(show);
        }
        /// <summary>
        /// 특정 윈도우에서 아이콘 가져오기
        /// </summary>
        /// <param name="srcWindowUid"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        private UIIcon GetIconByWindowUid(WindowUid srcWindowUid, int srcIndex)
        {
            UIWindow uiWindow = GetUIWindowByUid<UIWindow>(srcWindowUid);
            if (uiWindow == null)
            {
                GcLogger.LogError("UIWindow 컴포넌트가 없습니다. uid:"+srcWindowUid);
                return null;
            }
            return uiWindow.GetIconByIndex(srcIndex);
        }
        /// <summary>
        /// UIWindow 찾기 
        /// </summary>
        /// <param name="windowUid"></param>
        /// <returns></returns>
        public T GetUIWindowByUid<T>(WindowUid windowUid) where T : UIWindow
        {
            UIWindow uiWindow = uiWindows[(int)windowUid];
            if (uiWindow == null)
            {
                GcLogger.LogError("UIWindow 컴포넌트가 없습니다. uid:"+windowUid);
                return null;
            }

            return uiWindow as T;
        }
        /// <summary>
        /// 특정 윈도우에서 아이콘 지우기
        /// </summary>
        /// <param name="windowUid"></param>
        /// <param name="slotIndex"></param>
        public void RemoveIcon(WindowUid windowUid, int slotIndex)
        {
            UIWindow uiWindow = GetUIWindowByUid<UIWindow>(windowUid);
            if (uiWindow == null)
            {
                GcLogger.LogError("UIWindow 컴포넌트가 없습니다. uid:"+windowUid);
                return;
            }
            uiWindow.DetachIcon(slotIndex);
        }
        /// <summary>
        /// 윈도우가 활성화 되어있는지 체크
        /// </summary>
        /// <param name="windowUid"></param>
        /// <returns></returns>
        public bool IsShowByWindowUid(WindowUid windowUid)
        {
            UIWindow uiWindow = GetUIWindowByUid<UIWindow>(windowUid);
            if (uiWindow == null) return false;
            return uiWindow.gameObject.activeSelf;
        }
    }
}
