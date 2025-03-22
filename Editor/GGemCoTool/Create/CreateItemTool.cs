using System.Collections.Generic;
using GGemCo.Editor.GGemCoTool.MapEditor;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Maps;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.UI;
using GGemCo.Scripts.UI.Window;
using UnityEditor;
using UnityEngine;
using TableLoaderManager = GGemCo.Editor.GGemCoTool.TableLoader.TableLoaderManager;

namespace GGemCo.Editor.GGemCoTool.Create
{
    public class CreateItemTool : EditorWindow
    {
        private const string Title = "아이템 생성툴";
        private TableItem tableItem;
        private int selectedItemIndex;
        private int makeItemCount;
        private List<string> itemNames;
        private List<int> itemUids;
        private Dictionary<int, Dictionary<string, string>> itemDictionary;
        
        [MenuItem("GGemCoTool/생성툴/"+Title, false, 3)]
        public static void ShowWindow()
        {
            GetWindow<CreateItemTool>(Title);
        }
        private void OnEnable()
        {
            selectedItemIndex = 0;
            TableLoaderManager tableLoaderManager = new TableLoaderManager();
            tableItem = tableLoaderManager.LoadItemTable();
            itemDictionary = tableItem.GetDatas();
            
            LoadItemInfoData();
        }

        private void OnGUI()
        {
            selectedItemIndex = EditorGUILayout.Popup("아이템 선택", selectedItemIndex, itemNames.ToArray());
            makeItemCount = EditorGUILayout.IntField("추가할 개수", makeItemCount);
            if (GUILayout.Button("인벤토리에 아이템 추가"))
            {
                AddItem();
            }
            
            GUILayout.Space(20);

            if (GUILayout.Button("인벤토리 모든 아이템 삭제"))
            {
                RemoveAllInventoryItem();
            }
        }

        private void RemoveAllInventoryItem()
        {
            SceneGame.Instance.saveDataManager.Inventory.RemoveAllItems();
            var inventory = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowInventory>(UIWindowManager.WindowUid
                .Inventory);
            if (inventory != null)
            {
                inventory.LoadIcons();
            }
        }
        private void AddItem()
        {
            if (SceneGame.Instance == null)
            {
                EditorUtility.DisplayDialog(Title, "게임을 실행해주세요.", "OK");
                return;
            }

            if (makeItemCount <= 0)
            {
                EditorUtility.DisplayDialog(Title, "생성할 아이템 개수를 입력해주세요.", "OK");
                return;
            }

            int itemUid = itemUids[selectedItemIndex];
            if (itemUid <= 0)
            {
                EditorUtility.DisplayDialog(Title, "생성할 아이템을 선택해주세요.", "OK");
                return;
            }
            var result = SceneGame.Instance.saveDataManager.Inventory.AddItem(itemUid, makeItemCount);
            var inventory = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowInventory>(UIWindowManager.WindowUid
                .Inventory);
            if (inventory != null)
            {
                inventory.SetIcons(result);
            }
        }
        /// <summary>
        ///  몬스터 정보 불러오기
        /// </summary>
        private void LoadItemInfoData()
        {
             
            itemNames = new List<string>();
            itemUids = new List<int>();
            // foreach 문을 사용하여 딕셔너리 내용을 출력
            foreach (KeyValuePair<int, Dictionary<string, string>> outerPair in itemDictionary)
            {
                var info = tableItem.GetDataByUid(outerPair.Key);
                if (info.Uid <= 0) continue;
                itemNames.Add($"{info.Uid} - {info.Name}");
                itemUids.Add(info.Uid);
            }
        }
    }
}