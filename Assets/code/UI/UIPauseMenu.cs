using System;
using UnityEngine;
using UnityEngine.UI;

namespace code.UI
{
    public class UIPauseMenu : MonoBehaviour
    {
        private void Awake()
        {
            transform.Find("Content/GoGameBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                Time.timeScale = 1;
                if (!BallGameDirector.GetInstance().IsGameRunning)
                {
                    BallGameDirector.GetInstance().StartGame();
                }
                this.gameObject.SetActive(false);
            });
            
            transform.Find("Content/ExitGameBtn").GetComponent<Button>().onClick.AddListener(Application.Quit);
        }
    }
}
