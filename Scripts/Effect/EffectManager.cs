﻿using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Effect
{
    public abstract class EffectManager
    {
        public static DefaultEffect CreateEffect(int effectUid)
        {
            var info = TableLoaderManager.Instance.TableEffect.GetDataByUid(effectUid);
            if (info == null)
            {
                GcLogger.LogError("effect 테이블에 없는 이펙트 입니다. effect Uid: "+effectUid);
                return null;
            }
            GameObject prefab = TableLoaderManager.Instance.TableEffect.GetPrefab(effectUid);
            if (prefab == null) return null;
            GameObject effect = Object.Instantiate(prefab);
            DefaultEffect defaultEffect = effect.AddComponent<DefaultEffect>();
            defaultEffect.Initialize(info);
            // defaultEffect.Initialize();
            return defaultEffect;
        }
    }
}