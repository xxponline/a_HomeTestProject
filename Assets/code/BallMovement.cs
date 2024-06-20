using System.Collections;
using UnityEngine;

namespace code
{
    public class BallMovement : MonoBehaviour
    {
        public bool IsMoving { get; private set; }
        
        public void DoMove(Vector3 controlPoint)
        {
            IsMoving = true;
            
            // gnore edge of map
            var initialVelocity = GlobalProfs.GetInstance().velocityProportion * (controlPoint - transform.position);
            var initialVelocityMod = initialVelocity.magnitude;
            var deceleration =  GlobalProfs.GetInstance().deceleration;
            Debug.Assert(deceleration < 0);
            float timeMax = -(initialVelocityMod / deceleration);
            var displacementMod = initialVelocityMod * timeMax + 0.5f * deceleration * timeMax * timeMax;
            var simpleFinalPosition = Vector3.Normalize(initialVelocity) * displacementMod + transform.position;

            ConsiderMapEdge(initialVelocity, deceleration, 
                simpleFinalPosition,timeMax,
                out var finalTime, out var finalPos);
            
            
            StartCoroutine(Inner_DoMove(initialVelocity, deceleration, finalTime, finalPos ));
        }

        public void ConsiderMapEdge(Vector3 initialVelocity, float deceleration, Vector3 preConsiderFinalPos, 
            float preConsiderFinalTime, out float finalTime, out Vector3 finalPos)
        {
            var gameYard = BallGameDirector.GetInstance().gameYard;
            Vector3 velocityDirection = Vector3.Normalize(initialVelocity);
            Vector3 rollbackDirection = -velocityDirection;

            float radius = GetComponent<Ball>().BallRadius;

            float rollbackDisplacement = 0f;

            if (velocityDirection.x > 0)
            {
                var overDistance = preConsiderFinalPos.x + radius - gameYard.XMax;
                if (overDistance > 0)
                {
                    var cosTheta = Vector3.Dot(velocityDirection, Vector3.right);
                    rollbackDisplacement = Mathf.Max(rollbackDisplacement, overDistance / cosTheta);
                }
            }
            else if (velocityDirection.x < 0)
            {
                var overDistance = -(preConsiderFinalPos.x - radius - gameYard.XMin);
                if (overDistance > 0)
                {
                    var cosTheta = Vector3.Dot(velocityDirection, Vector3.left);
                    rollbackDisplacement = Mathf.Max(rollbackDisplacement, overDistance / cosTheta);
                }
            }

            if (velocityDirection.z > 0)
            {
                var overDistance = preConsiderFinalPos.z + radius - gameYard.ZMax;
                if (overDistance > 0)
                {
                    var cosTheta = Vector3.Dot(velocityDirection, Vector3.forward);
                    rollbackDisplacement = Mathf.Max(rollbackDisplacement, overDistance / cosTheta);
                }
            }
            else if (velocityDirection.z < 0)
            {
                var overDistance = -(preConsiderFinalPos.z - radius - gameYard.ZMin);
                if (overDistance > 0)
                {
                    var cosTheta = Vector3.Dot(velocityDirection, Vector3.back);
                    rollbackDisplacement = Mathf.Max(rollbackDisplacement, overDistance / cosTheta);
                }
            }


            if (rollbackDisplacement > 0)
            {
                finalPos = preConsiderFinalPos + rollbackDirection * rollbackDisplacement;

                var initialVelocityMod = initialVelocity.magnitude;
                var realDisplacement = Vector3.Distance(transform.position, finalPos);

                var aStuff = Mathf.Sqrt(initialVelocityMod * initialVelocityMod + 2f * deceleration * realDisplacement);
                var t1 = (-initialVelocityMod + aStuff) / deceleration;
                var t2 = (initialVelocityMod + aStuff) / deceleration;
                finalTime = Mathf.Max(t1, t2);  
            }
            else
            {
                finalPos = preConsiderFinalPos;
                finalTime = preConsiderFinalTime;   
            }
        }

        private IEnumerator Inner_DoMove(Vector3 initialVelocity, float deceleration, float moveTime, Vector3 finalPos)
        {
            float time = 0f;
            Vector3 originalPos = transform.position;
            var initialVelocityMod = initialVelocity.magnitude;
            var velocityDir = Vector3.Normalize(initialVelocity);
            do
            {
                var displacementNow = initialVelocityMod * time + 0.5f * deceleration * time * time;
                transform.position = originalPos + velocityDir * displacementNow;
                yield return null; // next frame
                time += Time.deltaTime;
            } while (moveTime >= time);

            transform.position = finalPos;
            IsMoving = false;
        }
    }
}
