using DG.Tweening;
using TMPro;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private Transform gridPanel;
    [SerializeField] private TextMeshProUGUI targetTxt;
    [SerializeField] private float animDuration = 0.5f;

    public float AnimDuration => animDuration;

    private TextMeshProUGUI[] _cells;
    private RectTransform[]   _rects;
    private CanvasGroup[]     _canvasGroups;
    private Vector2[]         _originalAnchoredPositions;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildCellRefs();
        PopulateGrid();
        PlayIntroAnimation();
    }

    private void BuildCellRefs()
    {
        int count = gridPanel.childCount;
        _cells                     = new TextMeshProUGUI[count];
        _rects                     = new RectTransform[count];
        _canvasGroups              = new CanvasGroup[count];
        _originalAnchoredPositions = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            Transform child = gridPanel.GetChild(i);
            _cells[i]                     = child.GetComponentInChildren<TextMeshProUGUI>();
            _rects[i]                     = child.GetComponent<RectTransform>();
            _canvasGroups[i]              = child.GetComponent<CanvasGroup>();
            _originalAnchoredPositions[i] = _rects[i].anchoredPosition;
        }
    }

    private void PopulateGrid()
    {
        QuestionData q = QuestionManager.Instance.CurrentQuestion;
        if (q == null) return;

        if (targetTxt != null) targetTxt.text = q.target.ToString();

        for (int i = 0; i < _cells.Length; i++)
        {
            if (_cells[i] == null) continue;
            _cells[i].text = i < q.items.Length ? q.items[i].ToString() : string.Empty;
        }
    }

    public void RefreshForNewQuestion()
    {
        for (int i = 0; i < _rects.Length; i++)
        {
            _rects[i].DOKill();
            if (_canvasGroups[i] != null) _canvasGroups[i].DOKill();

            _rects[i].gameObject.SetActive(true);
            _rects[i].anchoredPosition = _originalAnchoredPositions[i];
            _rects[i].localScale       = Vector3.one;
            _rects[i].localRotation    = Quaternion.identity;

            if (_canvasGroups[i] != null) _canvasGroups[i].alpha = 1f;
        }

        PopulateGrid();
        PlayIntroAnimation();
    }

    private void PlayIntroAnimation()
    {
        int count = _rects.Length;

        for (int i = 0; i < count; i++)
        {
            if (_canvasGroups[i] != null) _canvasGroups[i].alpha = 0f;
            _rects[i].anchoredPosition = _originalAnchoredPositions[(i + count - 1) % count];
            _rects[i].localScale       = Vector3.zero;

            ButtonHoverEffect hover = _rects[i].GetComponent<ButtonHoverEffect>();
            if (hover != null) hover.enabled = false;
        }

        for (int i = 0; i < count; i++)
        {
            _rects[i].DOAnchorPos(_originalAnchoredPositions[i], animDuration).SetEase(Ease.OutCubic);
            _rects[i].DOScale(Vector3.one, animDuration).SetEase(Ease.OutCubic);

            if (_canvasGroups[i] != null)
                _canvasGroups[i].DOFade(1f, animDuration);
        }

        DOVirtual.DelayedCall(animDuration, EnableHoverEffects);
    }

    private void EnableHoverEffects()
    {
        foreach (RectTransform r in _rects)
        {
            ButtonHoverEffect hover = r.GetComponent<ButtonHoverEffect>();
            if (hover != null) hover.enabled = true;
        }
    }
}
