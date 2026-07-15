using DG.Tweening;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The vertical distance the image will move up and down.")]
    [SerializeField] private float moveDistance = 20f;

    [Tooltip("The duration of one direction of the movement (in seconds).")]
    [SerializeField] private float duration = 1.5f;

    [Tooltip("The ease curve style for the movement (InOutSine is recommended for smooth floating).")]
    [SerializeField] private Ease easeType = Ease.InOutSine;

    [Tooltip("If checked, the animation starts automatically when the object is initialized.")]
    [SerializeField] private bool autoStart = true;

    private RectTransform _rectTransform;
    private Tween _moveTween;
    private Vector2 _startPosition;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_rectTransform == null)
        {
            Debug.LogError($"[AnimationManager] RectTransform component not found on {gameObject.name}. " +
                           "This script is designed for UI elements like Canvas Images.", gameObject);
            return;
        }

        _startPosition = _rectTransform.anchoredPosition;

        if (autoStart)
        {
            StartFloatingAnimation();
        }
    }

    /// <summary>
    /// Starts the continuous floating up and down animation.
    /// </summary>
    public void StartFloatingAnimation()
    {
        // Stop any active tween on this object to prevent overlaps
        StopFloatingAnimation();

        float targetY = _startPosition.y + moveDistance;

        // Use DOTween to animate the anchored Y position continuously
        _moveTween = _rectTransform.DOAnchorPosY(targetY, duration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true); // Continues to animate even if the game is paused (timeScale = 0)
    }

    /// <summary>
    /// Stops the animation and resets the image to its starting position.
    /// </summary>
    public void StopFloatingAnimation()
    {
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
        }

        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = _startPosition;
        }
    }

    private void OnDisable()
    {
        // Pause or kill the tween if the object is disabled
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Pause();
        }
    }

    private void OnEnable()
    {
        // Resume the tween if it exists when the object is enabled
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Play();
        }
        else if (autoStart && _rectTransform != null)
        {
            StartFloatingAnimation();
        }
    }

    private void OnDestroy()
    {
        // Clean up the tween to avoid memory leaks when the object is destroyed
        if (_moveTween != null)
        {
            _moveTween.Kill();
        }
    }
}
