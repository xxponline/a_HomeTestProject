using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace code
{
    public enum CharacterBallType
    {
        Player,
        AI
    }
    
    public interface IPointCheckInformationView
    {
        public int RewardBallCount { get; }
        public int EarnedPoints { get; }
        public int LostPoints { get; }
    }
    
    public class PointCheckInformation : IPointCheckInformationView
    {
        public int RewardBallCount { get; set; }
        public int EarnedPoints { get; set; }
        public int LostPoints { get; set; }

        public void Clear()
        {
            RewardBallCount = 0;
            EarnedPoints = 0;
            LostPoints = 0;
        }
    }
    
    
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
            _playerPointsInfo.Clear();
            _aiPointsInfo.Clear();
            
            var profs = GlobalProfs.GetInstance();
            var pawnCount = Random.Range(profs.normalBallRandomlyCountMin, profs.normalBallRandomlyCountMax);

            var gameYard = BallGameDirector.GetInstance().gameYard;
            var randomlySpawnXMax = gameYard.XMax - profs.characterBallRadius;
            var randomlySpawnXMin = gameYard.XMin + profs.characterBallRadius;
            
            var randomlySpawnZMax = gameYard.ZMax - profs.characterBallRadius;
            var randomlySpawnZMin = gameYard.ZMin + profs.characterBallRadius;

            DeckOfCards<ColorCard> colorCards = new DeckOfCards<ColorCard>();
            for (var i = 0; i < profs.characterRewardCardWeights; ++i)
            {
                colorCards.PutInCard(new ColorCard(ColorCardRewardType.Player, profs.playerBallColor));
            }
            if (_aiBall != null)
            {
                for (var i = 0; i < profs.characterRewardCardWeights; ++i)
                {
                    colorCards.PutInCard(new ColorCard(ColorCardRewardType.AI, profs.aiBallColor));
                }
            }
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
                switch (card.RewardType)
                {
                    case ColorCardRewardType.Player:
                        _playerPointsInfo.RewardBallCount++;
                        break;
                    case ColorCardRewardType.AI:
                        _aiPointsInfo.RewardBallCount++;
                        break;
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
        
        //Point Board
        private PointCheckInformation _playerPointsInfo = new();
        private PointCheckInformation _aiPointsInfo = new();

        public IPointCheckInformationView PlayerPointsInfo => _playerPointsInfo;
        public IPointCheckInformationView AIPointsInfo => _aiPointsInfo;
        
        
        //ball storage need refactoring to a QuadTree
        private readonly List<Ball> _allBalls = new(1002);
        private int _normalBallIndexStart = 1;
        private Ball _playerBall;
        private Ball _aiBall;

        public void PutCharacterBall(Ball playerBall, Ball aiBall)
        {
            _playerBall = playerBall;
            Debug.Assert(_playerBall);
            playerBall.BallId = _allBalls.Count;
            _allBalls.Add(playerBall);
            playerBall.BallRadius = GlobalProfs.GetInstance().characterBallRadius;
            playerBall.BallColor = GlobalProfs.GetInstance().playerBallColor;
            if (aiBall != null)
            {
                playerBall.transform.position = new Vector3(XMax / 2, 0, 0);
                
                _aiBall = aiBall;
                aiBall.BallId = _allBalls.Count;
                _allBalls.Add(aiBall);
                aiBall.transform.position = new Vector3(XMin / 2, 0, 0);
                aiBall.BallRadius = GlobalProfs.GetInstance().characterBallRadius;
                aiBall.BallColor = GlobalProfs.GetInstance().aiBallColor;

                _normalBallIndexStart = 2;

            }
            else
            {
                _normalBallIndexStart = 1;
                playerBall.transform.position = Vector3.zero;
            }
        }

        public void PointCheck(CharacterBallType characterBallType)
        {
            var characterBall = characterBallType == CharacterBallType.Player ? _playerBall : _aiBall;
            for (var i = _normalBallIndexStart; i < _allBalls.Count; ++i)
            {
                var ball = _allBalls[i];
                if (GamePhysicsUtils.CheckLogicIntersect(ball, characterBall))
                {
                    switch (characterBallType)
                    {
                        case CharacterBallType.Player:
                            if (ball.BallRewardType == ColorCardRewardType.Player)
                            {
                                _playerPointsInfo.EarnedPoints++;
                            }
                            else
                            {
                                _playerPointsInfo.LostPoints++;
                            }
                            break;
                        case CharacterBallType.AI:
                            if (ball.BallRewardType == ColorCardRewardType.AI)
                            {
                                _aiPointsInfo.EarnedPoints++;
                            }
                            else
                            {
                                _aiPointsInfo.LostPoints++;
                            }
                            break;
                    }
                    
                    ball.Dismiss();
                }
            }
        }

        public IEnumerator<Ball> GetBAllEnumerator()
        {
            return _allBalls.GetEnumerator();
        }

    }
}
