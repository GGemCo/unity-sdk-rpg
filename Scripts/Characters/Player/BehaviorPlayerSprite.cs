using UnityEngine;

namespace GGemCo.Scripts.Characters.Player
{
    public class BehaviorPlayerSprite : DefaultCharacterBehavior
    {
        private Player myPlayer;
        private Transform playerTransform;
        private float playerStep;
        private float playerSpeed;
        private float originalScaleX;

        private Vector3 direction;
        private Vector3 directionPrev;

        protected override void Start()
        {
            base.Start();
            myPlayer = GetComponent<Player>();
            // 플레이어 transform
            playerTransform = myPlayer.transform;
        }
        private void HandleInput()
        {
            direction = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) direction += Vector3.up;
            if (Input.GetKey(KeyCode.S)) direction += Vector3.down;
            if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
            if (Input.GetKey(KeyCode.D)) direction += Vector3.right;
        
            direction.Normalize();
        }
        private void Update()
        {
            HandleInput();
            
            if (Input.GetKey(KeyCode.A))
            {
                playerTransform.localScale = new Vector3(myPlayer.OriginalScaleX, playerTransform.localScale.y, playerTransform.localScale.z);
            }
            
            if (Input.GetKey(KeyCode.D))
            {
                playerTransform.localScale = new Vector3(myPlayer.OriginalScaleX * -1, playerTransform.localScale.y, playerTransform.localScale.z);
            }
            
            playerTransform.Translate(direction * (myPlayer.CurrentMoveStep * myPlayer.GetCurrentMoveSpeed() * Time.deltaTime));
        }

        public override float GetCharacterHeight()
        {
            float height = 0;
            return height;
        }
    }
}