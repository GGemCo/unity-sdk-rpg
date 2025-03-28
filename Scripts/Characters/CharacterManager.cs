﻿using System.Collections.Generic;
using UnityEngine;

namespace GGemCo.Scripts.Characters
{
    /// <summary>
    /// 캐릭터 관리 매니저
    /// </summary>
    public class CharacterManager
    {
        public enum Type
        {
            None,
            Player,
            Monster,
            Npc
        }
        
         private readonly List<GameObject> characters = new List<GameObject>();

         public void Initialize() { }
         /// <summary>
         /// 캐릭터 만들기
         /// </summary>
         /// <param name="characterType"></param>
         /// <param name="prefab"></param>
         /// <param name="position"></param>
         /// <param name="parent"></param>
         /// <returns></returns>
         private GameObject CreateCharacter(Type characterType, GameObject prefab, Vector3 position, Transform parent = null)
         {
             GameObject characterObj = Object.Instantiate(prefab, position, Quaternion.identity, parent);
             switch (characterType)
             {
                 case Type.Player:
                     Player.Player player = characterObj.AddComponent<Player.Player>();
                     break;
                 case Type.Monster:
                     Monster.Monster monster = characterObj.AddComponent<Monster.Monster>();
                     break;
                 case Type.Npc:
                     Npc.Npc npc = characterObj.AddComponent<Npc.Npc>();
                     break;
             }
#if GGEMCO_USE_SPINE
             CharacterCharacterAnimationControllerSpine characterCharacterAnimationControllerSpine = characterObj.AddComponent<CharacterCharacterAnimationControllerSpine>();
             ICharacterAnimationController iCharacterAnimationController = characterCharacterAnimationControllerSpine.GetComponent<ICharacterAnimationController>();
#else
             SpriteAnimator spriteAnimator = characterObj.AddComponent<SpriteAnimator>();

#endif
             characterObj.GetComponent<CharacterBase>().CharacterAnimationController = iCharacterAnimationController;
             characters.Add(characterObj);
             return characterObj;
         }

         public GameObject CreatePlayer(GameObject prefab, Vector3 position, Transform parent = null)
         {
             return CreateCharacter(Type.Player, prefab, position, parent);
         }
         public GameObject CreateNpc(GameObject prefab, Vector3 position, Transform parent = null)
         {
             return CreateCharacter(Type.Npc, prefab, position, parent);
         }
         public GameObject CreateMonster(GameObject prefab, Vector3 position, Transform parent = null)
         {
             return CreateCharacter(Type.Monster, prefab, position, parent);
         }
         public void RemoveCharacter(GameObject character)
         {
             if (!characters.Contains(character)) return;
             characters.Remove(character);
             Object.Destroy(character.gameObject);
         }
    }
}