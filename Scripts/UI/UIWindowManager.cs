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
        /// 윈도우 간에 아이콘 바꾸기 
        /// </summary>
        /// <param name="fromWindowUid"></param>
        /// <param name="fromIndex"></param>
        /// <param name="toWindowUid"></param>
        /// <param name="toIndex"></param>
        public void SwitchIcon(WindowUid fromWindowUid, int fromIndex, WindowUid toWindowUid, int toIndex)
        {
            UIWindow fromWindow = GetUIWindowByUid<UIWindow>(fromWindowUid);
            GameObject fromIcon = null;
            if (fromWindow != null)
            {
                fromIcon = fromWindow.GetIconByIndex(fromIndex);
            }

            UIWindow toWindow = GetUIWindowByUid<UIWindow>(toWindowUid);
            GameObject toIcon = null;
            if (toWindow != null)
            {
                toIcon = toWindow.GetIconByIndex(toIndex);
            }

            if (fromWindow == null || toWindow == null)
            {
                GcLogger.LogError("fromUIWindow 또는 toUIWindow 컴포넌트가 없습니다. fromWindow: "+fromWindowUid+" / toWindow: "+toWindow);
                return;
            }
            if (fromIcon == null)
            {
                GcLogger.LogError("이동할 아이콘이 없습니다. fromWindow: "+fromWindowUid+" / fromIndex: "+fromIndex);
                return;
            }

            if (toIcon != null)
            {
                // fromWindow.DetachIcon(fromIndex);
                // toWindow.DetachIcon(toIndex);
            
                fromWindow.SetIcon(fromIndex, toIcon);
                toWindow.SetIcon(toIndex, fromIcon);
            }
            else
            {
                // fromWindow.DetachIcon(fromIndex);
                toWindow.SetIcon(toIndex, fromIcon);
            }
        }
        /// <summary>
        /// 특정 윈도우에서 아이콘 가져오기
        /// </summary>
        /// <param name="srcWindowUid"></param>
        /// <param name="srcIndex"></param>
        /// <returns></returns>
        private GameObject GetIconByWindowUid(WindowUid srcWindowUid, int srcIndex)
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
        /// 윈도우에 아이콘 등록하기
        /// </summary>
        /// <param name="srcWindowUid"></param>
        /// <param name="srcIndex"></param>
        /// <param name="toWindowUid"></param>
        /// <param name="toIndex"></param>
        /// <returns></returns>
        public GameObject RegisterIcon(WindowUid srcWindowUid, int srcIndex, WindowUid toWindowUid, int toIndex)
        {
            GameObject srcIcon = GetIconByWindowUid(srcWindowUid, srcIndex);
            if(srcIcon == null) 
            {
                GcLogger.LogError("원본 아이콘이 없습니다. window uid: "+srcWindowUid+ " / srcIndex: "+srcIndex);
                return null;
            }

            UIIcon uiIcon = srcIcon.GetComponent<UIIcon>();
            if(uiIcon == null) 
            {
                GcLogger.LogError("원본 아이콘에 UIIcon 컴포넌트가 없습니다. window uid: "+srcWindowUid+ " / srcIndex: "+srcIndex);
                return null;
            }
            GameObject registerIcon = GetIconByWindowUid(toWindowUid, toIndex);
            if (registerIcon == null)
            {
                registerIcon = AddIcon(toWindowUid, toIndex, uiIcon.GetIconType(), uiIcon.uid);
            }
            else
            {
                registerIcon.GetComponent<UIIcon>().ChangeInfoByUid(uiIcon.uid);
                UIWindow uiWindow = GetUIWindowByUid<UIWindow>(toWindowUid);
                if (uiWindow != null)
                {
                    uiWindow.SetIcon(toIndex, registerIcon);
                }
            }
            return registerIcon;
        }
        /// <summary>
        /// 아이콘 만들기
        /// </summary>
        /// <param name="toWindowUid"></param>
        /// <param name="type"></param>
        /// <param name="itemUid"></param>
        /// <returns></returns>
        GameObject CreateIcon(WindowUid toWindowUid, IIcon.Type type, int itemUid)
        {
            if (prefabIconItem == null) return null;
            UIWindow uiWindow = GetUIWindowByUid<UIWindow>(toWindowUid);
            if(uiWindow == null) {
                GcLogger.LogError("등록된 윈도우가 아닙니다. windowUid: "+toWindowUid);
                return null;
            }
            GameObject icon = Instantiate(prefabIconItem, uiWindow.gameObject.transform);
            if (icon == null)
            {
                GcLogger.LogError("아이콘을 만들지 못했습니다.");
                return null;
            }
            UIIcon uiIcon = icon.GetComponent<UIIcon>();
            if (uiIcon == null)
            {
                GcLogger.LogError("UIIcon 스크립트 컴포넌트가 없습니다.");
                return null;
            }

            int uid = 0;
            switch (type)
            {
                case IIcon.Type.Item:
                    var table = TableLoaderManager.Instance.TableItem.GetDataByUid(itemUid);
                    if (table == null || table.Uid <= 0)
                    {
                        GcLogger.LogError("아이템 테이블에 정보가 없습니다. uid:"+itemUid);
                        return null;
                    }
                    uid = table.Uid;
                    break;
                case IIcon.Type.Skill:
                case IIcon.Type.None:
                default:
                        break;
            }
            uiIcon.uid = uid;
            return icon;
        }
        /// <summary>
        /// 아이콘 추가하기
        /// </summary>
        /// <param name="toWindowUid"></param>
        /// <param name="toIndex"></param>
        /// <param name="type"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        private GameObject AddIcon(WindowUid toWindowUid, int toIndex, IIcon.Type type, int uid)
        {
            var icon = CreateIcon(toWindowUid, type, uid);
            if(!icon) return null;
            
            UIWindow uiWindow = GetUIWindowByUid<UIWindow>(toWindowUid);
            if(uiWindow == null) {
                return null;
            }
            uiWindow.SetIcon(toIndex, icon);
            return icon;
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

        public GameObject GetPrefabIconItem()
        {
            return prefabIconItem;
        }
    }
}
