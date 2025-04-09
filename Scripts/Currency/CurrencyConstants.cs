﻿
using System.Collections.Generic;

namespace GGemCo.Scripts
{
    public abstract class CurrencyConstants
    {
        public enum Type
        {
            None,
            Gold,
            Silver,
        }

        public const int ItemUidGold = 301011001;
        public const int ItemUidSilver = 301021001;

        private static readonly Dictionary<Type, string> DictionaryNames = new Dictionary<Type, string>
        {
            { Type.Gold, "골드" },
            { Type.Silver, "실버" },
        };

        public static string GetNameByCurrencyType(Type type)
        {
            return DictionaryNames[type];
        }
    }
}