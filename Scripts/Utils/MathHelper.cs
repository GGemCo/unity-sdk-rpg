namespace GGemCo.Scripts.Utils
{
    public class MathHelper
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
    }
}