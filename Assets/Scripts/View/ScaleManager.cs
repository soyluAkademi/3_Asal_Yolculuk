using DG.Tweening;
using UnityEngine;

public class ScaleManager : MonoBehaviour
{
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private Ease  ease     = Ease.InOutSine;

    private Tweener tween;

    private void OnEnable()
    {
        transform.localScale = Vector3.one * minScale;
        tween = transform.DOScale(maxScale, duration)
                         .SetEase(ease)
                         .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        tween?.Kill();
        tween = null;
        transform.localScale = Vector3.one * minScale;
    }
}
