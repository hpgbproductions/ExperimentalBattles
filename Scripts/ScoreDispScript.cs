namespace Assets.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ScoreDispScript : MonoBehaviour
    {
        // Values to take
        public bool display = true;
        public int tgt;
        public int tgt_max;
        public int score;

        private string ScoreText;

        [SerializeField]
        private GUISkin ScoreStyleSkin;

        private GUIStyle ScoreStyle;

        void Start()
        {
            ScoreStyle = ScoreStyleSkin.label;
        }

        void OnGUI()
        {
            ScoreText = string.Format("TGT {0:D2}/{1:D2}\nPTS {2:D5}", tgt, tgt_max, score);
            GUI.Label(new Rect(Screen.width - 280, 30, 250, 65), ScoreText, ScoreStyle);
        }
    }
}