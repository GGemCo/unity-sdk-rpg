using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GGemCo.Scripts.Characters
{
    public class CharacterManager
    {
        public enum CharacterType
        {
            None,
            Player,
            Npc,
            Monster
        }
        
        public DefaultCharacter CreateCharacter(CharacterType type, CharacterData data)
        {
            DefaultCharacter newCharacter;
            switch (type)
            {
                case CharacterType.Player:
                    // return new Player { Uid = data.Uid };
                    newCharacter = CreatePlayerCharacter(data);
                    break;
                // case CharacterType.NPC:
                //     return new NPC { Uid = data.Uid };
                // case CharacterType.Monster:
                //     return new Monster { Uid = data.Uid };
                default:
                    throw new ArgumentException("Invalid character type");
            }

            return newCharacter;
        }
        /// <summary>
        /// 플레이어 캐릭터 생성
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private GGemCo.Scripts.Characters.Player.Player CreatePlayerCharacter(CharacterData data)
        {
            // 플레이어 캐릭터 프리팹을 스폰합니다. (여기서는 프리팹 로드를 가정)
            GameObject playerPrefab = Resources.Load<GameObject>("Characters/Player/Player");
            if (playerPrefab ==null)
            {
                // GcLogger.LogError("플레이어 프리팹이 없습니다.");
                return null;
            }
            GameObject playerObject = Object.Instantiate(playerPrefab);
        
            // PlayerCharacter 설정
            GGemCo.Scripts.Characters.Player.Player player = playerObject.GetComponent<GGemCo.Scripts.Characters.Player.Player>();
            if (player == null)
            {
                // GcLogger.LogError("플레이어 스크립트가 프리팹에 없습니다.");
                return null;
            }

            return player;
        }
    }
}