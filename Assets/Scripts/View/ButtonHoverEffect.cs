using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverOffsetX  = 0f;
    [SerializeField] private float hoverOffsetY  = 12f;
    [SerializeField] private float hoverDuration = 0.18f;
    [SerializeField] private Ease  hoverEase     = Ease.OutQuad;

    private RectTransform _rect;
    private Vector2       _originalPos;
    private Tweener       _tween;

    private void Awake()
    {
        _rect        = GetComponent<RectTransform>();
        _originalPos = _rect.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enabled) return;
        _tween?.Kill();
        _tween = _rect.DOAnchorPos(_originalPos + new Vector2(hoverOffsetX, hoverOffsetY), hoverDuration)
                      .SetEase(hoverEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enabled) return;
        _tween?.Kill();
        _tween = _rect.DOAnchorPos(_originalPos, hoverDuration)
                      .SetEase(hoverEase);
    }
}
