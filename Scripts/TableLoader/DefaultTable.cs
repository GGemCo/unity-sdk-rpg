using System.Collections.Generic;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.TableLoader
{
    public class DefaultTable
    {
        private readonly Dictionary<int, Dictionary<string, string>> table = new Dictionary<int, Dictionary<string, string>>();

        public virtual void LoadData(string content)
        {
            string[] lines = content.Split('\n');
            string[] headers = lines[0].Trim().Split('\t');

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]) || lines[i].StartsWith("#")) continue;
                string[] values = lines[i].Split('\t');
                var data = new Dictionary<string, string>();

                for (int j = 0; j < headers.Length; j++)
                {
                    data[headers[j].Trim()] = values[j].Trim().Replace(@"\n", "\n");
                }

                int uid = int.Parse(values[0]);
                table[uid] = data;

                OnLoadedData(data);
            }
        }

        protected virtual void OnLoadedData(Dictionary<string, string> data)
        {
            
        }

        private string CheckNone(string value)
        {
            return value == "None" ? "" : value;
        }
        public Dictionary<int, Dictionary<string, string>> GetDatas() => table;
        protected Dictionary<string, string> GetData(int uid) => table.GetValueOrDefault(uid);
        protected string GetDataColumn(int uid, string columnName)
        {
            table.TryGetValue(uid, out var data);
            if (data == null)
            {
                return null;
            }

            data.TryGetValue(columnName, out var value);
            return value == null ? null : CheckNone(value);
        }

        protected Vector2 ConvertVector2(string value)
        {
            Vector2 position = new Vector2(0, 0);
            if (value != "")
            {
                var result2 = value.Split(",");
                position.x = float.Parse(result2[0]);
                position.y = float.Parse(result2[1]);
            }
            return position;
        }
        protected GameObject LoadPrefab(string prefabPath) {
            if (prefabPath == "") {
                GcLogger.LogError("prefab 경로가 없습니다. prefabPath: "+prefabPath+"");
                return null;
            }
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null) {
                GcLogger.LogError("prefab 오브젝트가 없습니다. prefabPath: "+prefabPath);
                return null;
            }
            return prefab;
        }

        protected bool ConvertBoolean(string value)
        {
            return value == "Y";
        }

        public int GetCount()
        {
            return table.Count;
        }
    }
}