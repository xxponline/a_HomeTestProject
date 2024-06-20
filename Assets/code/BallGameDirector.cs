using System;
using System.Collections;
using code.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace code
{
    public class BallGameDirector : MonoBehaviour
    {
        private static BallGameDirector _instance = null;
        
        public GameObject normalBallAsset;
        public GameObject playerCharacterAsset;
        public GameObject aiCharacterAsset;

        public GameYard gameYard;

        [NonSerialized] public BallPlayerController BallPlayerController;
        [NonSerialized] public BallAIController BallAIController;

        private bool _isVictory = false;
        private int _maxRound = 10;
        private int _round = 0;

        public bool IsGameRunning { get; private set; } = false;


        public static BallGameDirector GetInstance()
        {
            return _instance;
        }

        private void Awake()
        {
            Debug.Assert(_instance == null);
            _instance = this;
        }

        private void Start()
        {
            _maxRound = GlobalProfs.GetInstance().playerTryTimes;
            
            Debug.Assert(gameYard != null);
            Debug.Assert(normalBallAsset != null);
            Debug.Assert(playerCharacterAsset != null);
            Debug.Assert(aiCharacterAsset != null);
        }


        public void StartGame()
        {
            Debug.Assert(IsGameRunning == false);
            CleanGame();
            IsGameRunning = true;
            StartCoroutine(MainGameplayMechanicalFlow());
        }

        void CleanGame()
        {
            StopAllCoroutines();
            gameYard.CleanYard();
        }

        IEnumerator MainGameplayMechanicalFlow()
        {
            UIManager.GetInstance().RefreshRound(0);
            yield return null;
            //Init Game

            _isVictory = false;
            
            //spawn Player
            var playerCharacter = Instantiate(playerCharacterAsset);
            BallPlayerController = playerCharacter.GetComponent<BallPlayerController>();
            Debug.Assert(BallPlayerController);;
            
            //spawn AI
            // if (aiCharacterAsset)
            // {
            //     var aiCharacter = Instantiate(aiCharacterAsset);
            //     BallAIController = aiCharacter.GetComponent<BallAIController>();
            //     Debug.Assert(BallAIController);
            // }
            
            gameYard.PutCharacterBall(BallPlayerController.GetComponent<Ball>(), BallAIController?.GetComponent<Ball>());
            
            //spawn normalBall
            gameYard.CreateNormalBall();

            do
            {
                _round++;
                UIManager.GetInstance().RefreshRound(_round);

                //Ball Brething
                gameYard.NormalBallBreathing();
                //
                yield return BallPlayerController.DoPlayerAction();
                //
                gameYard.PointCheck();
                UIManager.GetInstance().RefreshPoint(gameYard.EarnedPoints, gameYard.LostPoints);
            } while (_round < _maxRound && gameYard.EarnedPoints < gameYard.RewardBallCount);
            
            //Vectory Show
            UIManager.GetInstance().OpenVictory(gameYard.EarnedPoints, gameYard.RewardBallCount, _round);
            IsGameRunning = false;
        } 
    }
}
