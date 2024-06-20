using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace code
{
    /// <summary>
    /// Game Yard
    /// Pay attention: Now The GameYard Always on the X-Z plane (y = 0);
    /// </summary>
    public class GameYard : MonoBehaviour
    {
        [Range(1,30)]
        public float yardXLength;
        [Range(1,30)]
        public float yardZLength;
        
        
        public float XMax { get; private set; }
        public float XMin { get; private set; }
        public float ZMax { get; private set; }
        public float ZMin { get; private set; }

        private void Awake()
        {
            Debug.Assert(transform.rotation == Quaternion.identity);
            Debug.Assert(transform.position.y == 0);

            XMax = transform.position.x + yardXLength / 2;
            XMin = transform.position.x - yardXLength / 2;

            ZMax = transform.position.z + yardZLength / 2;
            ZMin = transform.position.z - yardZLength / 2;
        }


#if UNITY_EDITOR
        public bool showGameYardRange = false;

        private void OnDrawGizmos()
        {
            if (showGameYardRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    transform.position + new Vector3(yardXLength / 2,0f, -yardZLength / 2),
                    transform.position + new Vector3(yardXLength / 2,0f, yardZLength / 2)
                    );
                
                Gizmos.DrawLine(
                    transform.position + new Vector3(-yardXLength / 2,0f, -yardZLength / 2),
                    transform.position + new Vector3(-yardXLength / 2,0f, yardZLength / 2)
                );
                
                Gizmos.DrawLine(
                    transform.position + new Vector3(-yardXLength / 2,0f, yardZLength / 2),
                    transform.position + new Vector3(yardXLength / 2,0f, yardZLength / 2)
                );
                
                Gizmos.DrawLine(
                    transform.position + new Vector3(-yardXLength / 2,0f, -yardZLength / 2),
                    transform.position + new Vector3(yardXLength / 2,0f, -yardZLength / 2)
                );
            }
        }
#endif
        
        
        public void CleanYard()
        {
            Destroy(_playerBall?.gameObject);   
            Destroy(_aiBall?.gameObject);
            foreach (var ball in _allBalls)
            {
                if (ball != null)
                {
                    Destroy(ball?.gameObject);
                }
            }

            _allBalls.Clear();
        }

        public void CreateNormalBall()
        {
            RewardBallCount = EarnedPoints = 0;
            LostPoints = 0;
            
            var profs = GlobalProfs.GetInstance();
            var pawnCount = Random.Range(profs.normalBallRandomlyCountMin, profs.normalBallRandomlyCountMax);

            var gameYard = BallGameDirector.GetInstance().gameYard;
            var randomlySpawnXMax = gameYard.XMax - profs.characterBallRadius;
            var randomlySpawnXMin = gameYard.XMin + profs.characterBallRadius;
            
            var randomlySpawnZMax = gameYard.ZMax - profs.characterBallRadius;
            var randomlySpawnZMin = gameYard.ZMin + profs.characterBallRadius;

            DeckOfCards<ColorCard> colorCards = new DeckOfCards<ColorCard>();
            colorCards.PutInCard(new ColorCard(ColorCardRewardType.Player, profs.playerBallColor));
            colorCards.PutInCard(new ColorCard(ColorCardRewardType.AI, profs.aiBallColor));
            colorCards.PutInCard(profs.ballColorsSet.Select((m) => new ColorCard(ColorCardRewardType.None, m)));
            
            
            
            for (var i = 0; i < pawnCount; ++i)
            {
                var ball = Instantiate(BallGameDirector.GetInstance().normalBallAsset).GetComponent<Ball>();
                Debug.Assert(ball);
                
                //To heavy need optimization (QTree)
                Vector3 spawnPos;
                float spawnRadius;
                for(;;)
                {
                    spawnPos = new Vector3(
                        Random.Range(randomlySpawnXMin,randomlySpawnXMax), 
                        0,
                        Random.Range(randomlySpawnZMin,randomlySpawnZMax)
                    );
                    spawnRadius = Random.Range(profs.bollRandomlyRadiusMin, profs.bollRandomlyRadiusMax);

                    bool acceptableSpawn = true;
                    foreach (var existBall in _allBalls)
                    {
                        if (existBall.type == BallType.BallTypeCharacter)
                        {
                            if (GamePhysicsUtils.CheckLogicIntersect(existBall, spawnPos, profs.bollRandomlyRadiusMax))
                            {
                                acceptableSpawn = false;
                                break;
                            }
                        }
                        //check intersect by max radius to avoid recheck when breathing
                        else if (GamePhysicsUtils.CheckLogicIntersect(existBall.LogicCentre, profs.bollRandomlyRadiusMax, spawnPos, profs.bollRandomlyRadiusMax))
                        {
                            acceptableSpawn = false;
                            break;
                        };
                    }

                    if (acceptableSpawn)
                    {
                        break;
                    }
                }
                //
                ball.transform.position = spawnPos;
                ball.BallRadius = spawnRadius;
                var card = colorCards.Deal();
                if (card.RewardType == ColorCardRewardType.Player)
                {
                    RewardBallCount++;
                }
                ball.BallRewardType = card.RewardType;
                ball.BallColor = card.BallColor;
                
                ball.BallId = _allBalls.Count;
                _allBalls.Add(ball);
            }
        }

        public void NormalBallBreathing()
        {
            var profs = GlobalProfs.GetInstance();
            for (var i = _normalBallIndexStart; i < _allBalls.Count; ++i) //ignore character ball
            {
                var ball = _allBalls[i];
                ball.BallRadius = Random.Range(profs.bollRandomlyRadiusMin, profs.bollRandomlyRadiusMax);
            }
        }
        
        //ball storage need refactoring to a QuadTree
        private readonly List<Ball> _allBalls = new(1002);
        private int _normalBallIndexStart = 1;
        public int RewardBallCount { get; private set; } = 0;
        public int EarnedPoints { get; private set; } = 0;
        public int LostPoints { get; private set; } = 0;
        private Ball _playerBall;
        private Ball _aiBall;

        public void PutCharacterBall(Ball playerBall, Ball aiBall)
        {
            _playerBall = playerBall;
            Debug.Assert(_playerBall);
            playerBall.BallId = _allBalls.Count;
            _allBalls.Add(playerBall);
            playerBall.BallRadius = GlobalProfs.GetInstance().characterBallRadius;
            if (aiBall != null)
            {
                playerBall.transform.position = new Vector3(XMax / 2, 0, 0);
                
                _aiBall = aiBall;
                aiBall.BallId = _allBalls.Count;
                _allBalls.Add(aiBall);
                aiBall.transform.position = new Vector3(XMin / 2, 0, 0);
                aiBall.BallRadius = GlobalProfs.GetInstance().characterBallRadius;

                _normalBallIndexStart = 2;

            }
            else
            {
                _normalBallIndexStart = 1;
                playerBall.transform.position = Vector3.zero;
            }
        }

        public void PointCheck()
        {
            for (var i = _normalBallIndexStart; i < _allBalls.Count; ++i)
            {
                var ball = _allBalls[i];
                if (GamePhysicsUtils.CheckLogicIntersect(ball, _playerBall))
                {
                    if (ball.BallRewardType == ColorCardRewardType.Player)
                    {
                        EarnedPoints++;
                    }
                    else
                    {
                        LostPoints++;
                    }
                    ball.Dismiss();
                }
            }
        }
        
    }
}
