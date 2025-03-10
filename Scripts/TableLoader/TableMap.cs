﻿using System.Collections.Generic;
using GGemCo.Scripts.Maps;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.TableLoader
{
    /// <summary>
    /// 맵 테이블 Structure
    /// </summary>
    public class StruckTableMap
    {
        public int Uid;
        public string Name;
        public MapConstants.Type Type;
        public MapConstants.SubType Subtype;
        public string FolderName;
        public Vector2 PlayerSpawnPosition;
        public int BgmUid;
    }
    /// <summary>
    /// 맵 테이블
    /// </summary>
    public class TableMap : DefaultTable
    {
        private static readonly Dictionary<string, MapConstants.Type> MapType;
        private static readonly Dictionary<string, MapConstants.SubType> MapSubType;

        static TableMap()
        {
            MapType = new Dictionary<string, MapConstants.Type>
            {
                { "Common", MapConstants.Type.Common },
            };
            MapSubType = new Dictionary<string, MapConstants.SubType>
            {
            };
        }
        private static MapConstants.Type ConvertType(string type) => MapType.GetValueOrDefault(type, MapConstants.Type.None);
        private static MapConstants.SubType ConvertTypeSub(string type) => MapSubType.GetValueOrDefault(type, MapConstants.SubType.None);

        
        public StruckTableMap GetDataByUid(int uid)
        {
            if (uid <= 0)
            {
                GcLogger.LogError("uid is 0.");
                return null;
            }
            var data = GetData(uid);
            if (data == null) return null;
            return new StruckTableMap
            {
                Uid = int.Parse(data["Uid"]),
                Name = data["Name"],
                Type = ConvertType(data["Type"]),
                Subtype = ConvertTypeSub(data["Subtype"]),
                FolderName = data["FolderName"],
                PlayerSpawnPosition = ConvertPlayerSpawnPosition(data["PlayerSpawnPosition"]),
                BgmUid = 0,
            };
        }

        private Vector2 ConvertPlayerSpawnPosition(string position)
        {
            Vector2 playerSpawnPosition = new Vector2(0, 0);
            if (position != "")
            {
                var result2 = position.Split(",");
                playerSpawnPosition.x = float.Parse(result2[0]);
                playerSpawnPosition.y = float.Parse(result2[1]);
            }
            return playerSpawnPosition;
        }
    }
}