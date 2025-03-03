namespace GGemCo.Scripts.Items
{
    public static class ItemConstants
    {
        public enum Type
        {
            None,
            Equip, // 장비
            Consumable, //소모품
        }

        public enum Category
        {
            None,
            Weapon, // 무기
            Armor, // 방어구
            Potion, // 물약
        }

        public enum SubCategory
        {
            None,
            Sword, // 칼
            Top, // 상의
            RecoverHp, // hp 물약
            RecoverMp, // mp 물약
            IncreaseAttackSpeed, // 공격속도 증가
            IncreaseMoveSpeed, // 이동속도 증가
        }

        public enum Class
        {
            None,
            Normal, // 일반
        }
    }
}