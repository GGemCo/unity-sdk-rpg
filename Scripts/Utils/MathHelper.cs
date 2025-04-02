using UnityEngine;

namespace GGemCo.Scripts.Utils
{
    public static class MathHelper
    {
        public static string FormatNumber(long number)
        {
            string numberStr = number.ToString();
            int length = numberStr.Length;

            if (length > 20) // 해 단위
            {
                return InsertDecimal(numberStr, length - 20) + "해";
            }
            else if (length > 16) // 경 단위
            {
                return InsertDecimal(numberStr, length - 16) + "경";
            }
            else if (length > 12) // 조 단위
            {
                return InsertDecimal(numberStr, length - 12) + "조";
            }
            else if (length > 8) // 억 단위
            {
                return InsertDecimal(numberStr, length - 8) + "억";
            }
            else if (length > 4) // 만 단위
            {
                return InsertDecimal(numberStr, length - 4) + "만";
            }
            else
            {
                return numberStr; // 만 단위 이하의 숫자는 그대로 반환
            }
        }
        
        private static string InsertDecimal(string numberStr, int position)
        {
            // 숫자의 자릿수에 맞게 소수점을 삽입 (최대 2자리 소수)
            if (position <= 0 || position >= numberStr.Length) return numberStr;

            string integerPart = numberStr.Substring(0, position); // 정수 부분
            string fractionalPart = numberStr.Substring(position); // 소수 부분

            // 소수점 뒤에 2자리까지만 표시
            if (fractionalPart.Length > 2)
            {
                fractionalPart = fractionalPart.Substring(0, 2);
            }

            return integerPart + "." + fractionalPart;
        }
        /// <summary>
        /// y 위치 값으로 sorting order 구하기
        /// </summary>
        /// <param name="maxY">게임 월드에서 캐릭터가 있을 수 있는 y 좌표의 범위를 정의합니다.</param>
        /// <param name="positionY"></param>
        /// <returns></returns>
        public static int GetSortingOrder(float maxY, float positionY)
        {
            float minY = 0;
            
            // SpriteRenderer의 정렬 순서 범위 설정
            int minOrder = 1;
            int maxOrder = 10000;
    
            // 현재 y 좌표를 0~1 범위로 정규화합니다.
            // (maxY에서 minY로 갈수록 값이 커지도록 InverseLerp 사용)
            float normalizedY = Mathf.InverseLerp(maxY, minY, positionY);

            // 정규화된 값을 minOrder ~ maxOrder 범위에 맞게 매핑합니다.
            return minOrder + Mathf.RoundToInt(normalizedY * (maxOrder - minOrder));
        }
    }
}