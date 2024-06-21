using System;
using System.Collections.Generic;
using UnityEngine;

namespace code.QuadTree
{
    //      Z      max
    //   UL | UR
    //   --------- X
    //   DL | DR
    //min
    public enum QuadTreeNodeDirection
    {
        // ReSharper disable once InconsistentNaming
        UR = 0,
        // ReSharper disable once InconsistentNaming
        UL = 1,
        // ReSharper disable once InconsistentNaming
        DL = 2,
        // ReSharper disable once InconsistentNaming
        DR = 3
    }

    public interface IBallQuadTreeNodeView
    {
        public int Depth { get; }
        
        public Vector3 BorderMin { get; }
        public Vector3 BorderMax { get; }
        public bool IsLeaf { get; }
        
        public IBallQuadTreeNodeView Parent { get; }
        public IBallQuadTreeNodeView GetChild(QuadTreeNodeDirection direction);

        public bool IntersectWithBall(Ball ball);
        public bool EntirelyInside(Ball ball);

        public void AddBall(Ball ball);
    }
    
    public class BallQuadTreeNode : IBallQuadTreeNodeView
    {
        private readonly int _maxDepth;
        private readonly int _recommendCapacity;
        public int Depth { get; private set; }
        
        public Vector3 BorderMin { get; private set; }
        public Vector3 BorderMax { get; private set; }

        private BallQuadTreeNode _parent;
        private BallQuadTreeNode[] _children = null;

        private List<Ball> _balls = new();
        public bool IsLeaf => _children == null;
        public IBallQuadTreeNodeView Parent => _parent;

        public BallQuadTreeNode(BallQuadTreeNode parent, Vector3 borderMin, Vector3 borderMax, int maxDepth, int recommendCapacity)
        {
            _maxDepth = maxDepth;
            _recommendCapacity = recommendCapacity;
            _parent = parent;
            Depth = _parent != null ? _parent.Depth + 1 : 1;

            BorderMin = borderMin;
            BorderMax = borderMax;
        }

        public IBallQuadTreeNodeView GetChild(QuadTreeNodeDirection direction)
        {
            if (IsLeaf)
            {
                return _children[(int)direction];
            }

            return null;
        }

        public bool IntersectWithBall(Ball ball)
        {
            return IntersectWithBall(ball.LogicCentre, ball.BallRadius);
        }

        public bool IntersectWithBall(Vector3 ballCentre, float radius)
        {
            float closestX = Mathf.Clamp(ballCentre.x, BorderMin.x, BorderMax.x);
            float closestZ = Mathf.Clamp(ballCentre.z, BorderMin.z, BorderMax.z);

            return (new Vector3(closestX, 0, closestZ) - ballCentre).magnitude <= radius;
        }
        
        public bool EntirelyInside(Ball ball)
        {
            return EntirelyInside(ball.LogicCentre, ball.BallRadius);
        }

        public bool EntirelyInside(Vector3 ballCentre, float radius)
        {
            bool isInside = 
                ballCentre.x - radius >= BorderMin.x &&
                ballCentre.x + radius <= BorderMax.x &&
                ballCentre.z - radius >= BorderMin.z &&
                ballCentre.z + radius <= BorderMax.z;
            return isInside;
        }

        private void BreakDown()
        {
            if (!IsLeaf)
            {
                throw new Exception("Only Leaf Can Be Break Down");
            }
            
            Debug.Assert(Depth < _maxDepth);
            
            //      Z      max
            //   UL | UR
            //   --------- X
            //   DL | DR
            //min
            _children = new[]
            {
                //UR
                new BallQuadTreeNode(this, (BorderMin + BorderMax) / 2, BorderMax, _maxDepth, _recommendCapacity),
                //UL
                new BallQuadTreeNode(this,
                    new Vector3(BorderMin.x, 0, (BorderMin.z + BorderMax.z) / 2),
                    new Vector3((BorderMin.x + BorderMax.x) / 2, 0, BorderMax.z)
                    , _maxDepth, _recommendCapacity
                ),
                //DL
                new BallQuadTreeNode(this, BorderMin, (BorderMin + BorderMax) / 2,  _maxDepth, _recommendCapacity),
                //DR
                new BallQuadTreeNode(this,
                    new Vector3((BorderMin.x + BorderMax.x) / 2, 0, BorderMin.z),
                    new Vector3(BorderMax.x, 0, (BorderMin.z + BorderMax.z) / 2)
                    , _maxDepth, _recommendCapacity
                )
            };
            
            foreach(var ball in _balls)
            {
                bool noOverflowHappened = true;
                foreach (var child in _children)
                {
                    if (child.IntersectWithBall(ball))
                    {
                        child.InnerAddBall(ball);
                        if (noOverflowHappened)
                        {
                            if (child.EntirelyInside(ball))
                            {
                                break;
                            }
                            noOverflowHappened = false;
                        }
                    }
                }
            }
            _balls.Clear();
            
        }
        
        public void AddBall(Ball ball)
        {
            if (IsLeaf)
            {
                InnerAddBall(ball);
                if (_balls.Count > _recommendCapacity &&
                    Depth < _maxDepth)
                {
                    BreakDown();
                }
            }
            else
            {
                bool noOverflowHappened = true;
                foreach (var child in _children)
                {
                    if (child.IntersectWithBall(ball))
                    {
                        child.AddBall(ball);
                        if (noOverflowHappened)
                        {
                            if (child.EntirelyInside(ball))
                            {
                                break;
                            }
                            noOverflowHappened = false;
                        }
                    }
                }
            }
        }

        private void InnerAddBall(Ball ball)
        {
            Debug.Assert(IsLeaf);
            _balls.Add(ball);
        }

        public IEnumerator<Ball> GetBallEnumerator()
        {
            return _balls.GetEnumerator();
        }

        public bool CheckIntersectingExist(Vector3 pos, float radius)
        {
            if (IsLeaf)
            {
                if (_balls.Count >= 1)
                {
                    for (var i = _balls.Count - 1; i >= 0; --i)
                    {
                        var ball = _balls[i];
                        if (ball.IsAlive)
                        {
                            if (GamePhysicsUtils.CheckLogicIntersect(ball,pos,radius))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            _balls.RemoveAt(i);
                        }
                    }
                }
            }
            else
            {
                foreach (var child in _children)
                {
                    if (child.IntersectWithBall(pos, radius))
                    {
                        if (child.CheckIntersectingExist(pos, radius))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        
        public void QueryIntersecting(Vector3 pos, float radius,List<Ball> result)
        {
            if (IsLeaf)
            {
                if (_balls.Count >= 1)
                {
                    for (var i = _balls.Count - 1; i >= 0; ++i)
                    {
                        var ball = _balls[i];
                        if (ball.IsAlive)
                        {
                            if (GamePhysicsUtils.CheckLogicIntersect(ball, pos, radius))
                            {
                                result.Add(ball);
                            }
                        }
                        else
                        {
                            _balls.RemoveAt(i);
                        }
                    }
                }
            }
            else
            {
                bool noOverflowHappened = true;
                foreach (var child in _children)
                {
                    if (child.IntersectWithBall(pos, radius))
                    {
                        child.QueryIntersecting(pos, radius, result);
                        if (noOverflowHappened)
                        {
                            if (child.EntirelyInside(pos, radius))
                            {
                                break;
                            }
                            noOverflowHappened = false;
                        }
                    }
                }
            }
        }
    }
}