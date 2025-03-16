using System.Collections.Generic;
using GGemCo.Scripts.TableLoader;
using UnityEngine;

namespace GGemCo.Scripts.Characters.Player
{
    /// <summary>
    /// 장비 착용, 해제 관리
    /// </summary>
    public class EquipController : MonoBehaviour
    {
        public enum PartsType
        {
            Body,
            Leg,
            Weapon
        }
        /// <summary>
        /// 부위별 리소스 폴더 이름
        /// </summary>
        public static readonly Dictionary<PartsType, string> FolderNameByPartsType = new Dictionary<PartsType, string>
        {
            { PartsType.Body, "Top" },
            { PartsType.Leg, "Leg" },
            { PartsType.Weapon, "Weapon" },
        };
        /// <summary>
        /// 부위별 스파인 슬롯 이름
        /// </summary>
        public static readonly Dictionary<PartsType, List<string>> SlotNameByPartsType = new Dictionary<PartsType, List<string>>
        {
            { PartsType.Body, new List<string> { "body" } },
            { PartsType.Leg, new List<string> { "leg_l", "leg_r" } },
            { PartsType.Weapon, new List<string> { "knife" } },
        };
        /// <summary>
        /// 슬롯별 어태치먼트 이름
        /// </summary>
        public static readonly Dictionary<string, string> AttachmentNameBySlotName = new Dictionary<string, string>
        {
            { "body", "body" },
            { "leg_l", "leg_l" },
            { "leg_r", "leg_r" },
            { "knife", "knife" },
            { "knife2", "knife" },
        };
        
        private Player player;
        // 현재 장착 중인 아이템
        private readonly Dictionary<int, StruckTableItem> equippedItems = new Dictionary<int, StruckTableItem>();
        
        private delegate void DelegateOnPlayerEquiped(Dictionary<int, StruckTableItem> equippedItems);
        private event DelegateOnPlayerEquiped OnPlayerEquiped;
        private delegate void DelegateOnPlayerUnEquiped(Dictionary<int, StruckTableItem> equippedItems);
        private event DelegateOnPlayerUnEquiped OnPlayerUnEquiped;
        
        TableItem tableItem;

        private void Awake()
        {
            tableItem = TableLoaderManager.Instance.TableItem;
            player = GetComponent<Player>();
            if (player != null)
            {
                OnPlayerEquiped += player.UpdateStatCache;
                OnPlayerUnEquiped += player.UpdateStatCache;
            }
        }

        private void Start()
        {
            equippedItems.Clear();
        }
        /// <summary>
        /// 장비 착용
        /// </summary>
        /// <param name="partIndex">착용 부위</param>
        /// <param name="itemUid"></param>
        public bool EquipItem(int partIndex, int itemUid)
        {
            if (player == null) return false;
            if (itemUid <= 0)
            {
                UnEquipItem(partIndex);
                return true;
            }
            StruckTableItem item = tableItem.GetDataByUid(itemUid);
            if (equippedItems.TryAdd(partIndex, item))
            {
                OnPlayerEquiped?.Invoke(equippedItems);
            }

            return true;
        }
        /// <summary>
        /// 장비 해제 
        /// </summary>
        /// <param name="partIndex"></param>
        public bool UnEquipItem(int partIndex)
        {
            if (player == null) return false;
            if (equippedItems.Remove(partIndex))
            {
                OnPlayerUnEquiped?.Invoke(equippedItems);
            }

            return true;
        }
        /// <summary>
        /// 장착된 모든 아이템 정보 가져오기
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, StruckTableItem> GetEquippedItems()
        {
            return equippedItems;
        }
    }
}