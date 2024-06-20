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

        private int _maxRound;
        
        private void Awake()
        {
            transform.Find("HUD/MenuBtn").GetComponent<Button>().onClick.AddListener(() => {UIManager.GetInstance().OpenPauseMenu();});
            _roundTextMesh = transform.Find("HUD/RoundInfo").GetComponent<TextMeshProUGUI>();
            _pointsTextMesh = transform.Find("HUD/CurrentPoint").GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            _maxRound = GlobalProfs.GetInstance().playerTryTimes;
        }

        public void RefreshRound(int round)
        {
            _roundTextMesh.text = $"Round {round}/{_maxRound}";
        }

        public void RefreshPoints(int earnPoints, int lostPoints)
        {
            _pointsTextMesh.text = $"Earned Point: {earnPoints} Lost Point: {lostPoints}";
        }
    }
}
