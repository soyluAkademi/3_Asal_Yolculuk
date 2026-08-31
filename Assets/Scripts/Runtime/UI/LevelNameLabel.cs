using PayeGames.AsalYolculuk.Core;
using TMPro;
using UnityEngine;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Oynanış ekranındaki bölüm adı yazısını ("2. Asal Krallığı") doldurur.
    /// Ad, <see cref="Data.PlanetDataSO.DisplayName"/> üzerinden okunur; sahnede
    /// elle yazılmış metin bölüm değiştiğinde eskimez.
    ///
    /// Sahne başında bir kez yazılır, ayrıca bölüm ilerlediğinde
    /// (<see cref="GameState.Gameplay"/>'e her dönüşte) tazelenir — yeni gezegen
    /// sahne yeniden yüklenmeden başladığı için tek seferlik yazmak yetmez.
    /// </summary>
    public class LevelNameLabel : MonoBehaviour
    {
        [Header("Hedef Yazı")]
        [SerializeField] private TMP_Text bolumTxt;

        private void Awake()
        {
            if (bolumTxt == null)
            {
                TryGetComponent(out bolumTxt);
            }
        }

        private void Start()
        {
            RefreshLabel();

            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.Gameplay) return;

            RefreshLabel();
        }

        public void RefreshLabel()
        {
            if (bolumTxt == null) return;
            if (LevelManager.Instance == null) return;

            Data.PlanetDataSO planetData = LevelManager.Instance.CurrentPlanetData;
            if (planetData == null) return;

            bolumTxt.text = planetData.DisplayName;
        }
    }
}
