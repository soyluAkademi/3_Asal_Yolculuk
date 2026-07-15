using System;
using UnityEngine;

[Serializable]
public class QuestionData
{
    public int id;
    public int target;
    public int[] items;
    public string solution;
}

[Serializable]
public class QuestionList
{
    public QuestionData[] questions;
}

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance { get; private set; }

    public static event Action OnQuestionChanged;

    [SerializeField] private int startIndex = -1;

    private const string LastQuestionIndexPrefKey = "LastQuestionIndex";

    public QuestionData CurrentQuestion { get; private set; }
    public int CurrentIndex => _currentIndex;

    private QuestionList _questionList;
    private int          _currentIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad kaldırıldı: QuestionManager yalnızca GamePlay sahnesinde yaşar.
        // Böylece her GamePlay girişinde Awake yeniden çalışıp SaveManager'daki aktif
        // bölüm/soruyu yükler (kalıcı instance eski seçimde takılı kalmaz).
        LoadQuestions();
    }

    private void LoadQuestions()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("question");
        if (jsonFile == null) return;

        _questionList = JsonUtility.FromJson<QuestionList>(jsonFile.text);

        if (_questionList == null || _questionList.questions == null || _questionList.questions.Length == 0) return;

        // Öncelik: Inspector startIndex (test) > SaveManager aktif bölüm/soru > eski PlayerPrefs.
        int saveIndex = (SaveManager.Instance != null && SaveManager.Instance.AktifBolumId >= 0)
            ? SaveManager.Instance.AktifGlobalIndex
            : -1;

        if (startIndex >= 0)      _currentIndex = startIndex;
        else if (saveIndex >= 0)  _currentIndex = saveIndex;
        else                      _currentIndex = PlayerPrefs.GetInt(LastQuestionIndexPrefKey, 0);

        _currentIndex   = Mathf.Clamp(_currentIndex, 0, _questionList.questions.Length - 1);
        CurrentQuestion = _questionList.questions[_currentIndex];
        OnQuestionChanged?.Invoke();
    }

    public void LoadQuestionByIndex(int index)
    {
        if (_questionList == null) return;
        _currentIndex   = Mathf.Clamp(index, 0, _questionList.questions.Length - 1);
        CurrentQuestion = _questionList.questions[_currentIndex];
        SaveCurrentIndex();
        OnQuestionChanged?.Invoke();
    }

    public void LoadNextQuestion()
    {
        if (_questionList == null) return;
        _currentIndex   = (_currentIndex + 1) % _questionList.questions.Length;
        CurrentQuestion = _questionList.questions[_currentIndex];
        SaveCurrentIndex();
        OnQuestionChanged?.Invoke();
    }

    private void SaveCurrentIndex()
    {
        PlayerPrefs.SetInt(LastQuestionIndexPrefKey, _currentIndex);
        PlayerPrefs.Save();
    }

    public int QuestionCount => _questionList?.questions?.Length ?? 0;
}
