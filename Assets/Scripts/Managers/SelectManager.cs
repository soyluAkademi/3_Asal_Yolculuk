using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    public static SelectManager Instance { get; private set; }

    [SerializeField] private Transform gridPanel;
    [SerializeField] private Transform iconsPanel;
    [SerializeField] private TextMeshProUGUI resultTxt;
    [SerializeField] private Button backBtn;
    [SerializeField] private GameObject confettiPrefab;
    [SerializeField] private float moveDuration   = 0.55f;
    [SerializeField] private float shrinkDuration = 0.2f;
    [SerializeField] private float swingAngle     = 15f;
    [SerializeField] private float questionDelay  = 1.2f;
    [SerializeField] private float winPause       = 0.3f;

    [Header("Başarısızlık (son sayı kaldı, hedefe ulaşılamadı)")]
    [SerializeField] private RectTransform failShakeTarget;
    [SerializeField] private float failShakeDuration  = 0.5f;
    [SerializeField] private float failShakeStrength  = 20f;
    [SerializeField] private int   failShakeVibrato   = 12;
    [SerializeField] private AudioSource failAudio;

    private GameObject _selectedGridButton;
    private GameObject _selectedOperatorButton;

    public GameObject SelectedGridButton     => _selectedGridButton;
    public GameObject SelectedOperatorButton => _selectedOperatorButton;

    // ── Geçmiş ──────────────────────────────────────────────────────────────

    private struct ButtonSnapshot
    {
        public GameObject button;
        public bool       isActive;
        public Vector2    anchoredPosition;
        public string     numberText;
    }

    private struct HistoryEntry
    {
        public List<ButtonSnapshot> snapshots;
        public string               resultText;
    }

    private readonly Stack<HistoryEntry> _history = new Stack<HistoryEntry>();

    private AudioSource _mergeAudio;

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _mergeAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        WirePanel(gridPanel,  OnGridButtonClicked);
        WirePanel(iconsPanel, OnIconButtonClicked);

        if (backBtn != null)
            backBtn.onClick.AddListener(OnBackClicked);
    }

    private void WirePanel(Transform panel, System.Action<GameObject> callback)
    {
        if (panel == null) return;
        foreach (Transform child in panel)
        {
            Button btn = child.GetComponent<Button>();
            if (btn == null) continue;
            GameObject go = child.gameObject;
            btn.onClick.AddListener(() => callback(go));
        }
    }

    // ── Seçim ───────────────────────────────────────────────────────────────

    private void OnGridButtonClicked(GameObject button)
    {
        PlayClick(button);

        if (_selectedGridButton != null && _selectedOperatorButton != null && button != _selectedGridButton)
        {
            PlayMergeAnimation(button);
            return;
        }

        if (_selectedGridButton == button) return;

        SetBorder(_selectedGridButton, false);
        SetBorder(_selectedOperatorButton, false);
        _selectedOperatorButton = null;

        _selectedGridButton = button;
        SetBorder(_selectedGridButton, true);
        UpdateResultTxt();
    }

    private void OnIconButtonClicked(GameObject button)
    {
        PlayClick(button);

        if (_selectedOperatorButton == button) return;

        SetBorder(_selectedOperatorButton, false);
        _selectedOperatorButton = button;
        SetBorder(_selectedOperatorButton, true);
        UpdateResultTxt();
    }

    private void PlayClick(GameObject go)
    {
        if (SoundManager.Instance == null || !SoundManager.Instance.SfxOn) return;
        AudioSource src = go.GetComponent<AudioSource>();
        if (src == null || src.clip == null) return;
        src.PlayOneShot(src.clip);
    }

    // ── Geri al ─────────────────────────────────────────────────────────────

    private void OnBackClicked()
    {
        if (_history.Count == 0) return;
        PlayClick(backBtn.gameObject);

        SetBorder(_selectedGridButton, false);
        SetBorder(_selectedOperatorButton, false);
        _selectedGridButton     = null;
        _selectedOperatorButton = null;

        HistoryEntry entry = _history.Pop();

        foreach (ButtonSnapshot s in entry.snapshots)
        {
            s.button.SetActive(s.isActive);
            RectTransform rect = s.button.GetComponent<RectTransform>();
            rect.anchoredPosition = s.anchoredPosition;
            rect.localScale       = Vector3.one;
            rect.localRotation    = Quaternion.identity;
            TextMeshProUGUI txt   = s.button.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = s.numberText;
        }

        SetResultTxt(string.Empty);
    }

    // ── Merge animasyonu ─────────────────────────────────────────────────────

    private void PlayMergeAnimation(GameObject secondButton)
    {
        RectTransform firstRect  = _selectedGridButton.GetComponent<RectTransform>();
        RectTransform secondRect = secondButton.GetComponent<RectTransform>();

        int  firstNum  = GetNumber(_selectedGridButton);
        int  secondNum = GetNumber(secondButton);
        bool valid     = TryCalculate(firstNum, secondNum, _selectedOperatorButton.name, out int result);

        GameObject firstBtn    = _selectedGridButton;
        GameObject operatorBtn = _selectedOperatorButton;
        string     opSymbol    = GetOperatorSymbol(operatorBtn.name);
        Vector2    originalPos = firstRect.anchoredPosition;

        SetBorder(_selectedGridButton, false);
        SetBorder(_selectedOperatorButton, false);
        _selectedGridButton     = null;
        _selectedOperatorButton = null;

        firstRect.SetAsLastSibling();
        SetPanelInteractable(false);
        SetResultTxt($"{firstNum}{opSymbol}{secondNum}");

        if (valid)
        {
            if (SoundManager.Instance != null && SoundManager.Instance.SfxOn
                && _mergeAudio != null && _mergeAudio.clip != null)
                _mergeAudio.PlayOneShot(_mergeAudio.clip);

            // Geçmişi kaydet (animasyon başlamadan, değişmeden önce)
            SaveSnapshot($"{firstNum}{opSymbol}");

            SetBorder(secondButton, true);

            Sequence seq = DOTween.Sequence();
            seq.Join(firstRect.DOAnchorPos(secondRect.anchoredPosition, moveDuration)
                .SetEase(Ease.InOutCubic));
            seq.Join(firstRect.DOLocalRotate(new Vector3(0f, 0f, swingAngle), moveDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo));
            seq.OnComplete(() =>
            {
                // Birinci ikinciye ulaştığında prefabı o pozisyonda üret
                Transform mergeParticle = secondButton.transform.Find("mergeParticle");
                if (mergeParticle != null) mergeParticle.gameObject.SetActive(true);

                firstRect.localRotation = Quaternion.identity;
                firstRect.DOScale(Vector3.zero, shrinkDuration)
                    .OnComplete(() =>
                    {
                        firstBtn.SetActive(false);
                        firstRect.localScale = Vector3.one;

                        SetNumber(secondButton, result);

                        _selectedGridButton = secondButton;
                        SetBorder(_selectedGridButton, true);
                        SetResultTxt(result.ToString());

                        if (CheckWinCondition(result))
                            return;

                        int activeCount = 0;
                        foreach (Transform child in gridPanel)
                            if (child.gameObject.activeSelf) activeCount++;
                        if (activeCount == 1)
                            PlayFailFeedback();

                        SetPanelInteractable(true);
                    });
            });
        }
        else
        {
            SoundManager.Instance.YanlisSesiCikar();

            Sequence seq = DOTween.Sequence();
            seq.Append(firstRect.DOAnchorPos(secondRect.anchoredPosition, moveDuration * 0.5f)
                .SetEase(Ease.OutCubic));
            seq.Append(firstRect.DOAnchorPos(originalPos, moveDuration * 0.5f)
                .SetEase(Ease.InCubic));
            seq.OnComplete(() =>
            {
                _selectedGridButton     = firstBtn;
                _selectedOperatorButton = operatorBtn;
                SetBorder(_selectedGridButton, true);
                SetBorder(_selectedOperatorButton, true);
                SetPanelInteractable(true);
                SetResultTxt($"{firstNum}{opSymbol}");
            });
        }
    }

    // ── Geçmiş kaydetme ──────────────────────────────────────────────────────

    private void SaveSnapshot(string currentResultText)
    {
        var entry = new HistoryEntry
        {
            snapshots  = new List<ButtonSnapshot>(),
            resultText = currentResultText
        };

        foreach (Transform child in gridPanel)
        {
            RectTransform rect = child.GetComponent<RectTransform>();
            TextMeshProUGUI txt = child.GetComponentInChildren<TextMeshProUGUI>();
            entry.snapshots.Add(new ButtonSnapshot
            {
                button           = child.gameObject,
                isActive         = child.gameObject.activeSelf,
                anchoredPosition = rect.anchoredPosition,
                numberText       = txt != null ? txt.text : string.Empty
            });
        }

        _history.Push(entry);
    }

    // ── Kazanma kontrolü ────────────────────────────────────────────────────

    private bool CheckWinCondition(int result)
    {
        if (result != QuestionManager.Instance.CurrentQuestion.target) return false;

        int activeCount = 0;
        foreach (Transform child in gridPanel)
            if (child.gameObject.activeSelf) activeCount++;

        if (activeCount != 1) return false;

        SetPanelInteractable(false);
        SetBorder(_selectedGridButton, false);

        RectTransform lastRect = _selectedGridButton.GetComponent<RectTransform>();
        CanvasGroup   lastCg   = _selectedGridButton.GetComponent<CanvasGroup>();

        Sequence winSeq = DOTween.Sequence();
        winSeq.AppendInterval(winPause);
        winSeq.Append(lastRect.DOScale(Vector3.zero, shrinkDuration));
        if (lastCg != null)
            winSeq.Join(lastCg.DOFade(0f, shrinkDuration));
        winSeq.OnComplete(() =>
        {
            SoundManager.Instance.DogruSesiCikar();
            if (confettiPrefab != null)
                Instantiate(confettiPrefab);

            // Effect patlarken puanı ver ve göstergeyi güncelle.
            // (Index ilerletme de burada olur; sonraki soru ancak questionDelay
            //  sonra GoToNextQuestion'da yüklendiği için güvenli.)
            SaveManager sm = SaveManager.Instance;
            if (sm != null && sm.AktifBolumId >= 0)
                sm.SoruCozuldu();   // çözüldü işaretle + puan + kalinanSoru ilerlet
            if (PuanManager.Instance != null)
                PuanManager.Instance.PuanGoster();

            // Bu soruyla bölüm tamamlandıysa sonraki soru yerine geçiş panelini aç.
            if (sm != null && sm.AktifBolumId >= 0 && sm.BolumTamamlandi(sm.AktifBolumId))
                DOVirtual.DelayedCall(questionDelay, ShowBolumGecis);
            else
                DOVirtual.DelayedCall(questionDelay, GoToNextQuestion);
        });

        return true;
    }

    // ── Başarısızlık geri bildirimi (titreme + yanlış ses) ───────────────────

    private void PlayFailFeedback()
    {
        if (SoundManager.Instance != null && SoundManager.Instance.SfxOn
            && failAudio != null && failAudio.clip != null)
            failAudio.PlayOneShot(failAudio.clip);

        if (failShakeTarget == null) return;

        failShakeTarget.DOKill();
        failShakeTarget.DOShakeAnchorPos(failShakeDuration, failShakeStrength, failShakeVibrato)
            .SetEase(Ease.OutQuad);
    }

    // ── Bölüm tamamlandı: geçiş panelini tetikle ─────────────────────────────

    private void ShowBolumGecis()
    {
        SetPanelInteractable(false);

        // Panel + yıldız animasyonu BolumGecisManager'da. Yoksa eski davranışa düş.
        if (BolumGecisManager.Instance != null && BolumGecisManager.Instance.Ac())
            return;

        GoToNextQuestion();
    }

    // Geçiş paneli "Sonraki Bölüm" -> panel kapandıktan sonra dışarıdan çağrılır.
    // SaveManager zaten yeni bölümü aktif etmiş olur; burada o bölümün sorusu yüklenir.
    public void YeniBolumeBaslat() => GoToNextQuestion();

    private void GoToNextQuestion()
    {
        DOTween.Kill(gridPanel);
        foreach (Transform child in gridPanel)
            child.GetComponent<RectTransform>()?.DOKill();

        SetBorder(_selectedGridButton, false);
        _selectedGridButton     = null;
        _selectedOperatorButton = null;
        _history.Clear();
        SetResultTxt(string.Empty);

        // Puan/ilerleme zaten effect anında (CheckWinCondition) işlendi.
        // Burada yalnızca bir sonraki soruya geçilir.
        // SaveManager yoksa (örn. GamePlay'i doğrudan test ederken) eski davranışa düş.
        SaveManager sm = SaveManager.Instance;
        if (sm != null && sm.AktifBolumId >= 0)
            QuestionManager.Instance.LoadQuestionByIndex(sm.AktifGlobalIndex);
        else
            QuestionManager.Instance.LoadNextQuestion();

        GridManager.Instance.RefreshForNewQuestion();

        DOVirtual.DelayedCall(GridManager.Instance.AnimDuration, () => SetPanelInteractable(true));
    }

    // ── Yardımcılar ─────────────────────────────────────────────────────────

    private void UpdateResultTxt()
    {
        if (_selectedGridButton == null) { SetResultTxt(string.Empty); return; }

        string txt = GetNumber(_selectedGridButton).ToString();
        if (_selectedOperatorButton != null)
            txt += GetOperatorSymbol(_selectedOperatorButton.name);

        SetResultTxt(txt);
    }

    private void SetResultTxt(string value)
    {
        if (resultTxt != null) resultTxt.text = value;
    }

    private string GetOperatorSymbol(string opName)
    {
        switch (opName)
        {
            case "toplamaBtn": return "+";
            case "cikarmaBtn": return "-";
            case "carpmaBtn":  return "×";
            case "bolmeBtn":   return "÷";
            default:           return "?";
        }
    }

    private bool TryCalculate(int a, int b, string opName, out int result)
    {
        result = 0;
        switch (opName)
        {
            case "toplamaBtn": result = a + b; return true;
            case "cikarmaBtn": result = a - b; return true;
            case "carpmaBtn":  result = a * b; return true;
            case "bolmeBtn":
                if (b == 0 || a % b != 0) return false;
                result = a / b;
                return true;
            default: return false;
        }
    }

    private int GetNumber(GameObject button)
    {
        TextMeshProUGUI txt = button.GetComponentInChildren<TextMeshProUGUI>();
        if (txt == null) return 0;
        return int.TryParse(txt.text, out int n) ? n : 0;
    }

    private void SetNumber(GameObject button, int value)
    {
        TextMeshProUGUI txt = button.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.text = value.ToString();
    }

    private void SetPanelInteractable(bool interactable)
    {
        SetGroupInteractable(gridPanel,  interactable);
        SetGroupInteractable(iconsPanel, interactable);
        if (backBtn != null) backBtn.interactable = interactable;
    }

    private void SetGroupInteractable(Transform panel, bool interactable)
    {
        if (panel == null) return;
        foreach (Transform child in panel)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;

            ButtonHoverEffect hover = child.GetComponent<ButtonHoverEffect>();
            if (hover != null) hover.enabled = interactable;
        }
    }

    private void SetBorder(GameObject button, bool active)
    {
        if (button == null) return;
        Transform border = button.transform.Find("borderImg");
        if (border != null) border.gameObject.SetActive(active);
    }
}
