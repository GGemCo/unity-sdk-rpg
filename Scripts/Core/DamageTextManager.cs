using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Addressable;
using GGemCo.Scripts.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GGemCo.Scripts.Core
{
    /// <summary>
    /// 데미지 텍스트 매니저
    /// </summary>
    public class DamageTextManager : MonoBehaviour
    {
        private Transform canvasTransform;
        [SerializeField] private int poolSize = 20;
        [SerializeField] private float moveUpTime = 0.3f;
        [SerializeField] private float fadeOutTime = 0.1f;
        [SerializeField] private float moveUpDistance = 50.0f; // 추가된 이동 거리 설정
        [SerializeField] private float randomXRange = 10.0f; // X 좌표 랜덤 범위 추가

        private readonly Queue<TextMeshProUGUI> textPool = new Queue<TextMeshProUGUI>();
        private void Awake()
        {
            CreateTextDamageCanvas();
            InitializePool();
        }
        /// <summary>
        /// 데미지 텍스트가 들어갈 canvas 만들기
        /// </summary>
        private void CreateTextDamageCanvas()
        {
            GameObject gameObjectCanvas = new GameObject("CanvasTextDamage");
            Canvas canvas = gameObjectCanvas.gameObject.AddComponent<Canvas>();
            gameObjectCanvas.gameObject.AddComponent<CanvasScaler>();
            gameObjectCanvas.gameObject.AddComponent<GraphicRaycaster>();
            
            canvas.sortingLayerName = ConfigSortingLayer.GetValue(ConfigSortingLayer.Keys.UI);
            canvas.sortingOrder = 999;
            canvas.renderMode = RenderMode.WorldSpace;
            
            canvasTransform = gameObjectCanvas.transform;
        }
        /// <summary>
        /// Addressable 에 등록된 damageText 를 불러와서 pool 을 만든다 
        /// </summary>
        private void InitializePool()
        {
            if (AddressableSettingsLoader.Instance == null) return;
            textPool.Clear();
            if (poolSize <= 0) return;
            GameObject textFloatingDamage = AddressableSettingsLoader.Instance.GetPreLoadGamePrefabByName(ConfigAddressables.KeyTextFloatingDamage);
            if (textFloatingDamage == null) return;
            for (int i = 0; i < poolSize; i++)
            {
                GameObject gameObjectText = Instantiate(textFloatingDamage, canvasTransform);
                TextMeshProUGUI text = gameObjectText.GetComponent<TextMeshProUGUI>();
                text.gameObject.SetActive(false);
                textPool.Enqueue(text);
            }
        }
        /// <summary>
        /// 데미지 텍스트 보여주기
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="damage"></param>
        /// <param name="color"></param>
        public void ShowDamageText(Vector3 worldPosition, float damage, Color color)
        {
            if (textPool.Count == 0)
                return;

            TextMeshProUGUI text = textPool.Dequeue();
            text.text = $"{damage}";
            text.color = color;

            // X 좌표를 -10 ~ +10 범위에서 랜덤 설정
            worldPosition.x += Random.Range(-randomXRange, randomXRange);
        
            text.transform.position = worldPosition;
            
            text.gameObject.SetActive(true);

            StartCoroutine(AnimateDamageText(text));
        }
        /// <summary>
        /// 데미지 floating 애니메이션
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private IEnumerator AnimateDamageText(TextMeshProUGUI text)
        {
            if (text == null) yield break;
            
            Vector3 startPos = text.transform.position;
            Vector3 endPos = startPos + new Vector3(0, moveUpDistance, 0); // 이동 거리 적
            float elapsedTime = 0f;
            Color originalColor = text.color;

            // Move Up
            while (elapsedTime < moveUpTime)
            {
                text.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / moveUpTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Fade Out
            elapsedTime = 0f;
            while (elapsedTime < fadeOutTime)
            {
                text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1 - (elapsedTime / fadeOutTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            text.gameObject.SetActive(false);
            text.color = originalColor;
            textPool.Enqueue(text);
        }
    }
}
