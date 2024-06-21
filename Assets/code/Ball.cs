using System;
using System.Collections.Generic;
using code.QuadTree;
using UnityEngine;

namespace code
{
    public class Ball : MonoBehaviour
    {
        [NonSerialized] public int BallId;

        public BallType type;

        private Transform _displayTransform;
        
        public Transform DisplayTransform
        {
            get
            {
                if (_displayTransform == null)
                {
                    _displayTransform = transform.Find("Display");
                }
                return _displayTransform;
            }
        }

        private Color _ballColor;

        public Color BallColor
        {
            get => _ballColor;
            set
            {
                _ballColor = value;
                transform.Find("Display/Sphere").GetComponent<MeshRenderer>().material.color = _ballColor;
            }
        }

        public float BallRadius
        {
            get => DisplayTransform.localScale.x / 2;
            set => DisplayTransform.localScale = new Vector3(value * 2, value * 2, value * 2);
        }

        public ColorCardRewardType BallRewardType
        {
            get;
            set;
        } = ColorCardRewardType.None;

        public Vector3 LogicCentre => transform.position;
        public Vector3 Centre => DisplayTransform.position;

        public bool IsAlive { get; private set; } = true;

        public void Dismiss()
        {
            IsAlive = false;
            transform.gameObject.SetActive(false);
        }


    }
}
