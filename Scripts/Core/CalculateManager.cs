using GGemCo.Scripts.Characters.Player;
using GGemCo.Scripts.Scenes;
using UnityEngine;

namespace GGemCo.Scripts.Core
{
    public class CalculateManager : MonoBehaviour
    {
        private void Start()
        {
        }

        protected long GetPlayerWeaponAtk()
        {
            return 0;
        }
        public long GetPlayerTotalAtk()
        {
            return (long)SceneGame.Instance.player.GetComponent<Player>().StatAtk;
        }
    }
}