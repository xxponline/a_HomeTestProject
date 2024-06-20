using System;
using Unity.Mathematics;
using UnityEngine;

namespace code
{
    public class GlobalProfs : MonoBehaviour
    {
        private static GlobalProfs _instance = null;

        private void Awake()
        {
            Debug.Assert(_instance == null);
            _instance = this;
        }

        public static GlobalProfs GetInstance()
        {
            Debug.Assert(_instance);
            return _instance;
        }
        
        [Range(-20, -0.005f)] public float deceleration = 0.5f;

        [Range(0, 10)] public float velocityProportion = 0.5f;

        [Range(2, 20)] public int characterRewardCardWeights = 2;

        [Range(10, 800)] public int normalBallRandomlyCountMin = 20;
        [Range(10, 1000)] public int normalBallRandomlyCountMax = 800;
        
        [Range(0.01f, 2)] public float bollRandomlyRadiusMin = 0.2f;
        [Range(0.01f, 2)] public float bollRandomlyRadiusMax = 0.3f;

        [Range(1, 4)] public float characterBallRadius = 2;

        [Range(2,20)] public int playerTryTimes = 10;

        [Range(0.1f, 1.0f)] public float aiThinkCoefficient = 0.9f;
        
        public Color playerBallColor = Color.red;
        public Color aiBallColor = Color.blue;

        public Color[] ballColorsSet = new [] { Color.yellow, Color.green };
        
        
    }
}
