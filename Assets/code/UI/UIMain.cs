using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace code.UI
{
    public class UIMain : MonoBehaviour
    {
        private TextMeshProUGUI _roundTextMesh;
        private TextMeshProUGUI _pointsTextMesh;
        private TextMeshProUGUI _aiPointsTextMesh;

        private int _maxRound;
        
        private void Awake()
        {
            transform.Find("HUD/MenuBtn").GetComponent<Button>().onClick.AddListener(() => {UIManager.GetInstance().OpenPauseMenu();});
            _roundTextMesh = transform.Find("HUD/RoundInfo").GetComponent<TextMeshProUGUI>();
            _pointsTextMesh = transform.Find("HUD/CurrentPoint").GetComponent<TextMeshProUGUI>();
            _aiPointsTextMesh = transform.Find("HUD/CurrentAIPoint").GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            _maxRound = GlobalProfs.GetInstance().playerTryTimes;
        }

        public void RefreshRound(int round)
        {
            _roundTextMesh.text = $"Round {round}/{_maxRound}";
        }

        public void RefreshPlayerPoints(int earnPoints, int lostPoints)
        {
            _pointsTextMesh.text = $"Player Earned: {earnPoints} Lost: {lostPoints}";
        }

        public void RefreshAIPoints(int earnPoints, int lostPoints)
        {
            _aiPointsTextMesh.text = $"AI Earned: {earnPoints} Lost: {lostPoints}";
        }
    }
}
