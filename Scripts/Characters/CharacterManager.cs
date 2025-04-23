using System.Collections.Generic;
using UnityEngine;

namespace GGemCo.Scripts
{
    /// <summary>
    /// 캐릭터 관리 매니저
    /// </summary>
    public class CharacterManager
    {
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
         public GameObject CreateCharacter(CharacterConstants.Type characterType, GameObject prefab, Vector3 position, Transform parent = null)
         {
             GameObject characterObj = Object.Instantiate(prefab, position, Quaternion.identity, parent);
             switch (characterType)
             {
                 case CharacterConstants.Type.Player:
                     Player player = characterObj.AddComponent<Player>();
                     player.type = CharacterConstants.Type.Player;
                     break;
                 case CharacterConstants.Type.Monster:
                     Monster monster = characterObj.AddComponent<Monster>();
                     monster.type = CharacterConstants.Type.Monster;
                     break;
                 case CharacterConstants.Type.Npc:
                     Npc npc = characterObj.AddComponent<Npc>();
                     npc.type = CharacterConstants.Type.Npc;
                     break;
             }
#if GGEMCO_USE_SPINE
             CharacterCharacterAnimationControllerSpine characterCharacterAnimationControllerSpine = characterObj.AddComponent<CharacterCharacterAnimationControllerSpine>();
             ICharacterAnimationController iCharacterAnimationController = characterCharacterAnimationControllerSpine.GetComponent<ICharacterAnimationController>();
#else
             CharacterAnimationControllerSprite characterAnimationControllerSprite = characterObj.AddComponent<CharacterAnimationControllerSprite>();
             ICharacterAnimationController iCharacterAnimationController = characterAnimationControllerSprite.GetComponent<ICharacterAnimationController>();
#endif
             characterObj.GetComponent<CharacterBase>().CharacterAnimationController = iCharacterAnimationController;
             characters.Add(characterObj);
             return characterObj;
         }

         public GameObject CreatePlayer(GameObject prefab, Vector3 position, Transform parent = null)
         {
             return CreateCharacter(CharacterConstants.Type.Player, prefab, position, parent);
         }
         public GameObject CreateNpc(GameObject prefab, Vector3 position, Transform parent = null)
         {
             return CreateCharacter(CharacterConstants.Type.Npc, prefab, position, parent);
         }
         public GameObject CreateMonster(GameObject prefab, Vector3 position, Transform parent = null)
         {
             return CreateCharacter(CharacterConstants.Type.Monster, prefab, position, parent);
         }

         public GameObject CreateCharacter(CharacterConstants.Type characterType, int characterUid, Vector3 position, Transform parent = null)
         {
             GameObject prefab = null;
             if (characterType == CharacterConstants.Type.Player)
             {
                 prefab = Resources.Load<GameObject>(ConfigCommon.PathPlayerPrefab);
             }
             else if (characterType == CharacterConstants.Type.Monster)
             {
                 prefab = TableLoaderManager.Instance.TableMonster.GetPrefab(characterUid);
             }
             else if (characterType == CharacterConstants.Type.Npc)
             {
                 prefab = TableLoaderManager.Instance.TableNpc.GetPrefab(characterUid);
             }

             if (prefab != null) return CreateCharacter(characterType, prefab, position, parent);
             GcLogger.LogError("캐릭터 프리팹이 없습니다. characterType: "+characterType+"/ uid: "+characterUid);
             return null;
         }
         public void RemoveCharacter(GameObject character)
         {
             if (!characters.Contains(character)) return;
             characters.Remove(character);
             Object.Destroy(character.gameObject);
         }
    }
}