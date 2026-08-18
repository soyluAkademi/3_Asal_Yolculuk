using UnityEngine;
using UnityEngine.EventSystems;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Standart Button sadece "tıklama" verir; yön kontrolü için basılı tutma
    /// bilgisi gerektiğinden basılı olma durumunu bu bileşen takip eder.
    /// </summary>
    [DisallowMultipleComponent]
    public class HoldableButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
        }

        // Parmak/imleç butondan kayarsa basılı kalmaması gerekir.
        public void OnPointerExit(PointerEventData eventData)
        {
            IsPressed = false;
        }

        private void OnDisable()
        {
            IsPressed = false;
        }
    }
}
