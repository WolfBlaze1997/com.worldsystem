using System.Collections.Generic;
using UnityEngine;

namespace WorldSystem.Runtime
{
    [ExecuteAlways]
    public class LightningData : MonoBehaviour
    {
        public static readonly HashSet<LightningData> LightningDataHashList = new HashSet<LightningData>();
        public const int MaxLightningDataCount = 2;
        public float intensity = 1000000;
        public float Intensity => intensity;

        public Vector3 Position => transform.position;

        private void OnEnable()
        {
            intensity = Random.Range(500000, 1000000);
            LightningDataHashList.Add(this);
            
        }

        private void OnDisable()
        {
            LightningDataHashList.Remove(this);
        }
    }
}
