using System;
using System.Collections.Generic;

namespace GGemCo.Scripts
{
    public static class DialogueTextFormatter
    {
        public static List<string> SplitMessage(string message, int maxLineCount)
        {
            var result = new List<string>();
            string[] lines = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i += maxLineCount)
            {
                string chunk = string.Join("\n", lines, i, Math.Min(maxLineCount, lines.Length - i));
                result.Add(chunk);
            }

            return result;
        }
    }
}