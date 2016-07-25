using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ui 
{
    public class ArenaUIController : MonoBehaviour 
    {
        [System.Serializable]
        struct PlayerLevelData
        {
            public AnimatedProgress progressBar;
            public Text progressText;
        }

        [SerializeField]
        private PlayerLevelData plrLevelData;

        [SerializeField]
        private ui.Timer timer = null;

        [SerializeField]
        private StatsPanel upgradePanel = null;

        public StatsPanel StatsPanel
        { get { return upgradePanel; }}

        public void Init(int timerTime, Player player)
        {
            timer.Init(timerTime);

            plrLevelData.progressBar.ShowSmooth();
            plrLevelData.progressBar.Progress = 0.0f;
            plrLevelData.progressText.text = "1";

            upgradePanel.Reset();
            upgradePanel.Player = player;
        }

        public void ShowUpgradePanel(int points)
        {
            upgradePanel.AddPoints(points);
        }

        public void DrawLevelData(PlayerExperience exp)
        {
            plrLevelData.progressBar.Progress = exp.ExpProgress;
            plrLevelData.progressText.text = exp.Level.ToString();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}