using GGemCo.Scripts.Core;
using GGemCo.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo.Scripts.UI
{
    public class UIWindowManager : MonoBehaviour
    {
        public enum WindowUid 
        {
        }

        public UIWindow[] uiWindows;
        
        public GameObject prefabFloatingText;
        public VerticalLayoutGroup debugMessageVerticalLayoutGroup;
        public GameObject prefabDebugMessage;

        // Start is called before the first frame update
        private void Start()
        {
            InitializationShowDisable();
        }
        /// <summary>
        /// 초기화시 안보여야 하는 윈도우 처리 
        /// </summary>
        private void InitializationShowDisable()
        {
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
            if (uiWindow.gameObject.activeSelf == show) return;
            UIWindowFade uiWindowFade = uiWindow.GetComponent<UIWindowFade>();
            if (uiWindowFade == null) return;
            if (show) {
                uiWindowFade.ShowPanel();
            }
            else {
                uiWindowFade.HidePanel();
            }

            // uiWindow.OnShow(show);
            // window.SetActive(show);
        }
        /// <summary>
        /// 윈도우 간에 아이콘 이동시키기 
        /// </summary>
        /// <param name="fromWindowUid"></param>
        /// <param name="fromIndex"></param>
        /// <param name="toWindowUid"></param>
        /// <param name="toIndex"></param>
        public void MoveIcon(WindowUid fromWindowUid, int fromIndex, WindowUid toWindowUid, int toIndex)
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
            
                fromWindow.SetIcon(toIcon, fromIndex);
                toWindow.SetIcon(fromIcon, toIndex);
            }
            else
            {
                // fromWindow.DetachIcon(fromIndex);
                toWindow.SetIcon(fromIcon, toIndex);
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
            GameObject srcIcon = this.GetIconByWindowUid(srcWindowUid, srcIndex);
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
            GameObject registerIcon = this.GetIconByWindowUid(toWindowUid, toIndex);
            if (registerIcon == null)
            {
                registerIcon = AddIcon(toWindowUid, toIndex, UIIcon.Type.Skill, uiIcon.uid, 0);
            }
            else
            {
                registerIcon.GetComponent<UIIcon>().ChangeInfoByUid(uiIcon.uid);
                UIWindow uiWindow = GetUIWindowByUid<UIWindow>(toWindowUid);
                if (uiWindow != null)
                {
                    uiWindow.SetIcon(registerIcon, toIndex);
                }
            }
            return registerIcon;
        }
        /// <summary>
        /// 아이콘 추가하기. 보류
        /// </summary>
        /// <param name="toWindowUid"></param>
        /// <param name="toIndex"></param>
        /// <param name="type"></param>
        /// <param name="uid"></param>
        /// <param name="vid"></param>
        /// <returns></returns>
        private GameObject AddIcon(WindowUid toWindowUid, int toIndex, UIIcon.Type type, int uid, int vid)
        {
            // let icon = this.createIcon(type, uid, dragging, click, byClient, item_class, authority);
            // if(!icon) return null;
            //
            // icon.vid = vid;
            // if (byClient === true)
            //     icon.vid = uid;
            //
            // if(wnd === Def.WINDOW.SKILLTIER) {
            //     icon.createName();
            // }
            //
            // if (wnd === Def.WINDOW.QUICKSLOT_ITEM) {
            //     wnd = Def.WINDOW.HUDMANAGER;
            //     index = index + Def.QSLOT_SKILL_COUNT;
            // }
            //
            // if(wnd === Def.WINDOW.QUICKSLOT) {
            //     wnd = Def.WINDOW.HUDMANAGER;
            //     // index = index % Def.QSLOT_SKILL_COUNT;
            // }
            //
            // if(wnd == Def.WINDOW.REPURCHASE_ITEM){
            //     wnd = Def.WINDOW.SHOP;
            // }
            //
            // let wndObj = this.getWindow(wnd);
            // if(wndObj == undefined) {
            //     if(DEBUG) {
            //         if (Config.debug_icons) {
            //             console.log('ADD_ICON: UNDEFINED %d', wnd);
            //         }
            //     }
            //     return null;
            // }
            //
            //
            // wndObj.setIcon(icon, index);
            // return icon;
            return null;
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
        public void ShowFloatingText(string text, Vector3 position)
        {
            ShowFloatingText(text, position, Color.yellow);
        }
        public void ShowFloatingText(string text, Vector3 position, float fontSize)
        {
            ShowFloatingText(text, position, Color.yellow);
        }
        public void ShowFloatingText(string text, Vector3 position, Color textColor, float fontSize = 55)
        {
            // UIFloatingText uiFloatingText = Instantiate(prefabFloatingText, MySceneGame.Instance.uIWindowManager.transform).GetComponent<UIFloatingText>();
            // uiFloatingText.SetText(text);
            // uiFloatingText.SetColor(textColor);
            // uiFloatingText.SetFontSize(fontSize);
            // uiFloatingText.transform.position = position;
        }
        public void AddDebugMessage(string text)
        {
            if (debugMessageVerticalLayoutGroup == null || prefabDebugMessage == null || GameObject.Find("ScrollViewDebugMessage") == null) return;
            Instantiate(prefabDebugMessage, debugMessageVerticalLayoutGroup.transform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(debugMessageVerticalLayoutGroup.GetComponent<RectTransform >());
            GameObject.Find("ScrollViewDebugMessage").GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
        }
        /// <summary>
        /// 썸네일, 칭호 아이콘 이미지 변경하기
        /// </summary>
        /// <param name="index"></param>
        public void ChangeHudCharacterInfo(string index)
        {
        }

        public bool IsShowByWindowUid(WindowUid windowUid)
        {
            UIWindow uiWindow = GetUIWindowByUid<UIWindow>(windowUid);
            if (uiWindow == null) return false;
            return uiWindow.gameObject.activeSelf;
        }
    }
}
