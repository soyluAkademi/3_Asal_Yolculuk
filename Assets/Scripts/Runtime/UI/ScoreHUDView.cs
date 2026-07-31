using TMPro;
using UnityEngine;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Puanı "45 / 100" biçiminde gösterir; güncel puan kısmı vurgu rengiyle yazılır.
    /// </summary>
    public class ScoreHUDView : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private TMP_Text puanTxt;

        [Header("Renkler")]
        [Tooltip("Güncel puanın rengi")]
        [SerializeField] private Color currentScoreColor = new Color(1f, 0.84f, 0.2f, 1f);
        [Tooltip("\" / 100\" kısmının rengi")]
        [SerializeField] private Color targetScoreColor = Color.white;

        private string currentScoreHex;
        private string targetScoreHex;

        private void Awake()
        {
            currentScoreHex = ColorUtility.ToHtmlStringRGB(currentScoreColor);
            targetScoreHex = ColorUtility.ToHtmlStringRGB(targetScoreColor);
        }

        private void Start()
        {
            if (ScoreManager.Instance == null) return;

            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
            HandleScoreChanged(ScoreManager.Instance.CurrentScore, ScoreManager.Instance.TargetScore);
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance == null) return;

            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(int currentScore, int targetScore)
        {
            if (puanTxt == null) return;

            puanTxt.text = $"<color=#{currentScoreHex}>{currentScore}</color><color=#{targetScoreHex}> / {targetScore}</color>";
        }
    }
}
