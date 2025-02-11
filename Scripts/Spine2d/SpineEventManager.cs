#if GGEMCO_USE_SPINE
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GGemCo.Scripts.Core;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Spine2d
{
    public class SpineEventManager : MonoBehaviour
    {
        public GameObject objectPrefab;
        public int minPoolSize;
        public int maxPoolSize;

        private readonly List<GameObject> objectPool = new List<GameObject>();
        private readonly Dictionary<GameObject, float> inactiveObjectTimes = new Dictionary<GameObject, float>();

        public const string EventNameAttack = "attack";
        public const string EventNameSound = "sound";
        public const string EventNameShake = "camera_shake";

        private void Start()
        {
            StartCoroutine(GCUnusedObjects());
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < minPoolSize; i++)
            {
                GameObject newObj = InstantiateObject();
                if (newObj == null) continue;

                newObj.SetActive(false);
                inactiveObjectTimes[newObj] = Time.time;
            }
        }

        private GameObject InstantiateObject()
        {
            if (objectPrefab == null) return null;

            GameObject newObj = Instantiate(objectPrefab);
            if (newObj == null) return null;

            //newObj.SetActive(false);
            objectPool.Add(newObj);
            return newObj;
        }

        public GameObject GetObjectFromPool()
        {
            foreach (var obj in objectPool.Where(obj => obj != null).Where(obj => !obj.activeSelf))
            {
                obj.SetActive(true);
                obj.GetComponent<SpineEventData>().FirstSetDisable();
                inactiveObjectTimes.Remove(obj); // 활성화되면 Dictionary에서 제거
                GcLogger.Log("re use");
                return obj;
            }

            if (objectPool.Count < maxPoolSize)
            {
                GameObject newObj = InstantiateObject();
                if (newObj == null) return null;

                //FG_Logger.Log("new newObj");
                newObj.SetActive(true);
                return newObj;
            }

            return null;
        }

        private IEnumerator GCUnusedObjects()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                List<GameObject> objectsToRemove = new List<GameObject>();

                foreach (var kvp in inactiveObjectTimes)
                {
                    GameObject obj = kvp.Key;
                    float inactiveTime = Time.time - kvp.Value;

                    if (inactiveTime > 1f && objectPool.Count > minPoolSize)
                    {
                        objectsToRemove.Add(obj);
                    }
                }

                foreach (var objToRemove in objectsToRemove)
                {
                    DestroyObject(objToRemove);
                    break;
                }
            }
        }

        private void DestroyObject(GameObject obj)
        {
            GcLogger.Log("DestroyObject ");
            if (obj == null) return;

            objectPool.Remove(obj);
            inactiveObjectTimes.Remove(obj);
            Destroy(obj);
        }

        public void OnDisableObject(GameObject obj)
        {
            if (obj == null) return;

            inactiveObjectTimes[obj] = Time.time;
        }
    }
}
#endif