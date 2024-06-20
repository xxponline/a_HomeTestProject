using System;
using UnityEngine;

namespace code
{
    public static class GamePhysicsUtils
    {
        public static bool CheckIntersect(Ray ray, Ball ball)
        {
            Vector3 oc = ball.Centre - ray.origin;
            float cosTheta = Vector3.Dot(ray.direction, Vector3.Normalize(oc));
            if (cosTheta < 0)
            {
                return false;
            }

            var mod_oc = oc.magnitude;
            var mod_oa = cosTheta * mod_oc;
            // oa ^ 2 + ac ^ 2 = oc ^ 2 (ac is distance)
            var distance = MathF.Sqrt(mod_oc * mod_oc - mod_oa * mod_oa);
            return distance <= ball.BallRadius;
        }

        public static bool CheckLogicIntersect(Ball lhs, Ball rhs)
        {
            return Vector3.Distance(lhs.LogicCentre, rhs.LogicCentre) <= (lhs.BallRadius + rhs.BallRadius);
        }
        
        public static bool CheckLogicIntersect(Ball ball, Vector3 bollPos, float ballRadius)
        {
            return Vector3.Distance(ball.LogicCentre, bollPos) <= (ball.BallRadius + ballRadius);
        }
        
        public static bool CheckLogicIntersect(Vector3 lhsCentre, float lhsRadius, Vector3 rhsCentre, float rhsRadius)
        {
            return Vector3.Distance(lhsCentre, rhsCentre) <= (lhsRadius + rhsRadius);
        }
        
        // just calculate the position of ray cross the z-x plane
        // now, it only for check control line.
        public static Vector3 RayCastGameYard(Ray ray, GameYard gameYard)
        {
            var cosTheta = Vector3.Dot(Vector3.down, ray.direction);
            Debug.Assert(cosTheta >= 0);
            var distance = ray.origin.y / cosTheta;
            return ray.origin + ray.direction * distance;
        }
        
    }
}