using System.Collections.Generic;
using UnityEngine;

namespace code.QuadTree
{
    public class BallQuadTree
    {
        public int MaxDepth { get; }
        public int RecommendNodeCapacity { get; }
        
        public Vector3 BorderMin { get; }
        public Vector3 BorderMax { get; }
        
        private BallQuadTreeNode _root;

        public BallQuadTree(int maxDepth, int recommendNodeCapacity, Vector3 borderMin, Vector3 borderMax)
        {
            MaxDepth = maxDepth;
            RecommendNodeCapacity = recommendNodeCapacity;
            BorderMin = borderMin;
            BorderMax = borderMax;
            Clean();
        }

        public void Clean()
        {
            _root = new BallQuadTreeNode(null, BorderMin, BorderMax, MaxDepth, RecommendNodeCapacity);
        }

        public void AddBall(Ball ball)
        {
            _root.AddBall(ball);
        }
        
        public bool CheckIntersectingExist(Ball ball)
        {
            return _root.CheckIntersectingExist(ball.LogicCentre, ball.BallRadius);
        }

        public bool CheckIntersectingExist(Vector3 pos, float radius)
        {
            return _root.CheckIntersectingExist(pos, radius);
        }

        public void QueryIntersecting(Vector3 pos, float radius, List<Ball> result)
        {
            _root.QueryIntersecting(pos, radius, result);
        }
        
        public void QueryIntersecting(Ball ball, List<Ball> result)
        {
            _root.QueryIntersecting(ball.LogicCentre, ball.BallRadius, result);
        }
    }
}