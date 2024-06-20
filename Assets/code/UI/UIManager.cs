using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace code.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance = null;

        public static UIManager GetInstance() => _instance;
        
        private UIMain _mainUI;
        private UIPauseMenu _pauseMenu;
        private UIVictory _victoryUI;
        
        

        private void Awake()
        {
            Debug.Assert(_instance == null);
            _instance = this;
            
            _mainUI = transform.Find("MainUI").GetComponent<UIMain>();
            Debug.Assert(_mainUI);
            _pauseMenu = transform.Find("PauseMenu").GetComponent<UIPauseMenu>();
            Debug.Assert(_pauseMenu);
            _victoryUI = transform.Find("Victory").GetComponent<UIVictory>();
            Debug.Assert(_victoryUI);
            
            _pauseMenu.gameObject.SetActive(false);
            _victoryUI.gameObject.SetActive(false);
        }

        private void Start()
        {   
            OpenPauseMenu();
        }

        public void RefreshPlayerPoints(int earnedPoints, int lostPoints)
        {
            _mainUI.RefreshPlayerPoints(earnedPoints, lostPoints);
        }

        public void RefreshAIPoints(int earnedPoints, int lostPoints)
        {
            _mainUI.RefreshAIPoints(earnedPoints, lostPoints);
        }

        public void RefreshRound(int round)
        {
            _mainUI.RefreshRound(round);
        }

        public void OpenPauseMenu()
        {
            Time.timeScale = 0;
            _pauseMenu.gameObject.SetActive(true);
        }

        public void OpenVictory(int points, int maxPoints, int turns)
        {
            _victoryUI.gameObject.SetActive(true);
            _victoryUI.RefreshVictoryInfo(points, turns);
        }
    }
}
