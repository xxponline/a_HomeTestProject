using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace code
{
    public class BallAIController : MonoBehaviour
    {

        private bool _isActing = false;
        
        private GameObject _aimBase;
        private GameObject _aimArrow;
        private Transform _frontSight;
        

        private void Awake()
        {
            _aimBase = transform.Find("Display/Aim").gameObject;
            _aimArrow = transform.Find("Display/AimArrowAnchor").gameObject;
            _frontSight = _aimArrow.transform.Find("AimArrow");
            _aimBase.SetActive(false);
            _aimArrow.SetActive(false);
        }

        public IEnumerator DoAIAction()
        {
            _isActing = true;
            StartCoroutine(DoAIMove());
            return new WaitUntil(() => _isActing == false);
        }

        public IEnumerator DoAIMove()
        {
            var profs = GlobalProfs.GetInstance();
            Vector3 targetPos = ThinkActionTarget();
            Debug.Log($"Want move to {targetPos}");
            //calculate aim pos from displacement
            var displacement = targetPos - transform.position;
            var aMod = profs.deceleration;
            var dMod = displacement.magnitude;
            var t = Mathf.Sqrt(-2 * dMod / aMod);
            var initVelocityMod = -aMod * t;


            //var initialVelocity = GlobalProfs.GetInstance().velocityProportion * (controlPoint - transform.position);
            //var initialVelocityMod = initialVelocity.magnitude;
            
            var initialVelocity = initVelocityMod * Vector3.Normalize(displacement);
            var aimPos = transform.position + initialVelocity / profs.velocityProportion;
            
            //some performance for aim
            _aimBase.SetActive(true);
            float aimTime = 0f;
            while (aimTime < 0.8f)
            {
                PreviewMoveAction(Vector3.Lerp(transform.position, aimPos, aimTime / 0.8f));
                yield return null;
                aimTime += Time.deltaTime;
            }

            yield return new WaitForSeconds(0.5f);
            _aimBase.SetActive(false);
            _aimArrow.SetActive(false);

            var movement = GetComponent<BallMovement>();
            movement.DoMove(aimPos);

            yield return new WaitUntil(() => !movement.IsMoving);
            Debug.Log($"Finally moved to {transform.position}");
            _isActing = false;
        }
        
        public void PreviewMoveAction(Vector3 aimPoint)
        {
            var aimDir = aimPoint - transform.position;
            if (aimDir.magnitude > 0.1f)
            {
                _aimArrow.SetActive(true);
                _aimArrow.transform.rotation = Quaternion.FromToRotation(Vector3.forward, aimDir);
                _frontSight.position = aimPoint;
            }
            else
            {
                _aimArrow.SetActive(false);
            }
        }
        
        public Vector3 ThinkActionTarget()
        {

            var aiBallRadius = GetComponent<Ball>().BallRadius;
            var profs = GlobalProfs.GetInstance();
            
            var gameYard = BallGameDirector.GetInstance().gameYard;
            //XUnite
            var xSafeMax = gameYard.XMax - aiBallRadius;
            var xSafeMin = gameYard.XMin + aiBallRadius;
            var xSafeUniteCount = Mathf.CeilToInt((xSafeMax - xSafeMin) / (aiBallRadius * profs.aiThinkCoefficient)) + 1;
            var xUniteSize = (xSafeMax - xSafeMin) / xSafeUniteCount;
            //ZUnite
            var zSafeMax = gameYard.ZMax - aiBallRadius;
            var zSafeMin = gameYard.ZMin + aiBallRadius;
            var zSafeUniteCount = Mathf.CeilToInt((zSafeMax - zSafeMin) / (aiBallRadius * profs.aiThinkCoefficient)) + 1;
            var zUniteSize = (zSafeMax - zSafeMin) / zSafeUniteCount;

            var safeAreaBaseVector = new Vector3(xSafeMin, 0, zSafeMin);

            float[][] highField = new float[xSafeUniteCount][]; //[x,z]
            for (int index = 0; index < xSafeUniteCount; index++)
            {
                highField[index] = new float[zSafeUniteCount];
            }

            //iterate all normal ball
            var ballEnumerator = gameYard.GetBAllEnumerator();
            while (ballEnumerator.MoveNext())
            {
                var ball = ballEnumerator.Current;
                if (ball != null && ball.IsAlive && ball.type == BallType.BallTypeNormal)
                {
                    var coverRadius = aiBallRadius + ball.BallRadius;

                    int rectangleXMinIdx = Mathf.CeilToInt(
                        Mathf.Max(ball.LogicCentre.x - coverRadius - 1.01f * xSafeMin, 0) / xUniteSize);
                    
                    int rectangleXMaxIdx = Mathf.FloorToInt(
                        Mathf.Min(ball.LogicCentre.x + coverRadius - 1.01f * xSafeMin, 1.01f * xSafeMax) / xUniteSize);
                    
                    int rectangleZMinIdx = Mathf.CeilToInt(
                        Mathf.Max(ball.LogicCentre.z - coverRadius - 1.01f * zSafeMin, 0) / zUniteSize);
                    
                    int rectangleZMaxIdx = Mathf.FloorToInt(
                        Mathf.Min(ball.LogicCentre.z + coverRadius - 1.01f * zSafeMin, 1.01f * zSafeMax) / zUniteSize);
                    
                    //check and high-field value addition
                    for (var ix = rectangleXMinIdx; ix <= rectangleXMaxIdx ; ++ix)
                    {
                        for (var iz = rectangleZMinIdx; iz <= rectangleZMaxIdx; ++iz)
                        {
                            var checkPoint = new Vector3(ix * xUniteSize, 0, iz * zUniteSize) + safeAreaBaseVector;
                            if (GamePhysicsUtils.CheckLogicIntersect(ball.LogicCentre, 0, checkPoint, coverRadius))
                            {
                                highField[ix][iz] += ball.BallRewardType == ColorCardRewardType.AI ? 1.0f : -1f;
                            }
                        }
                    }
                }
            }
            
            // find the highest item in high-field
            int highestXIdxPos = 0, highestZIdxPos = 0;
            float highestValue = float.MinValue;

            for (var ix = 0; ix < xSafeUniteCount; ++ix)
            {
                for (var iz = 0; iz < zSafeUniteCount; ++iz)
                {
                    if (highField[ix][iz] > highestValue)
                    {
                        highestValue = highField[ix][iz];
                        highestXIdxPos = ix;
                        highestZIdxPos = iz;
                    }
                }
            }

            return new Vector3(highestXIdxPos * xUniteSize, 0, highestZIdxPos * xUniteSize) + safeAreaBaseVector;
        }
    }
}
