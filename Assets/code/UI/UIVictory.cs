using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace code.UI
{
    public class UIVictory : MonoBehaviour, IPointerClickHandler
    {
        private TextMeshProUGUI _victoryText;
        private void Awake()
        {
            _victoryText = transform.Find("Content/Detail").GetComponent<TextMeshProUGUI>();
        }

        public void RefreshVictoryInfo(int earnedPoints, int round)
        {
            _victoryText.text = $"You earned {earnedPoints} points in {round} turns";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            gameObject.SetActive(false);
            UIManager.GetInstance().OpenPauseMenu();
        }
    }
}
