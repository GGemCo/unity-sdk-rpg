using System.Collections.Generic;
using GGemCo.Scripts.TableLoader;

namespace GGemCo.Scripts.Characters.Player
{
    /// <summary>
    /// 장비 착용, 해제 관리
    /// </summary>
    public class EquipController
    {
        public enum PartsType
        {
            Body,
            Leg,
            Weapon
        }

        public static readonly Dictionary<PartsType, List<string>> SlotNameByPartsType = new Dictionary<PartsType, List<string>>
        {
            { PartsType.Body, new List<string> { "body" } },
            { PartsType.Leg, new List<string> { "leg_l", "leg_r" } },
            { PartsType.Weapon, new List<string> { "knife" } },
        };
        public static readonly Dictionary<string, string> AttachmentNameBySlotName = new Dictionary<string, string>
        {
            { "body", "body" },
            { "leg_l", "leg_l" },
            { "leg_r", "leg_r" },
            { "knife", "knife" },
            { "knife2", "knife" },
        };
        
        private Player player;
        private PlayerStat playerStat;
        // 현재 장착 중인 아이템
        private readonly Dictionary<int, StruckTableItem> equippedItems = new Dictionary<int, StruckTableItem>();
        
        private delegate void DelegateOnEquiped();
        private event DelegateOnEquiped OnEquiped;
        private delegate void DelegateOnUnEquiped();
        private event DelegateOnUnEquiped OnUnEquiped;
        
        TableItem tableItem;
        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="pplayer"></param>
        public void Initialize(Player pplayer)
        {
            tableItem = TableLoaderManager.Instance.TableItem;
            equippedItems.Clear();
            if (pplayer == null) return;
            player = pplayer;
            if (player.PlayerStat != null)
            {
                playerStat = player.PlayerStat;
                OnEquiped += playerStat.UpdateStatCache;
                OnUnEquiped += playerStat.UpdateStatCache;
            }
        }
        /// <summary>
        /// 장비 착용
        /// </summary>
        /// <param name="partIndex">착용 부위</param>
        /// <param name="itemUid"></param>
        public bool EquipItem(int partIndex, int itemUid)
        {
            if (player == null) return false;
            StruckTableItem item = tableItem.GetDataByUid(itemUid);
            if (equippedItems.TryAdd(partIndex, item))
            {
                OnEquiped?.Invoke();
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
                OnUnEquiped?.Invoke();
            }

            return true;
        }

        public Dictionary<int, StruckTableItem> GetEquippedItems()
        {
            return equippedItems;
        }
    }
}