using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ui {
    public class Timer : MonoBehaviour 
    {
        [SerializeField]
        private Text timeText = null;

        float lastTime = -1;
        float endTime = 0;

        int last_secs = 0;
        int last_mins = 0;

        public void Init(int timeLeft) 
        {
            endTime = GameApp.Instance.ClientTimeMs() + timeLeft;

            Update();
        }

        public void Update() 
        {
            float time = Mathf.Clamp(endTime - GameApp.Instance.ClientTimeMs(), 0, endTime);

            if (lastTime == time) return;
            lastTime = time;

            time /= 1000;
            int secs = (int)time % 60;
            int mins = (int)time / 60;
            if (last_secs != secs || last_mins != mins) {
                timeText.text = System.String.Format("{0:00}:{1:00}", mins, secs);
                last_secs = secs;
                last_mins = mins;
            }
        }
    }
}