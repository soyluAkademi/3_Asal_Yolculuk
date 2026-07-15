using TMPro;
using UnityEngine;

public class NameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nameTxt;

    private void OnEnable()
    {
        QuestionManager.OnQuestionChanged += UpdateName;
    }

    private void OnDisable()
    {
        QuestionManager.OnQuestionChanged -= UpdateName;
    }

    private void Start()
    {
        UpdateName();
    }

    private void UpdateName()
    {
        if (nameTxt == null || QuestionManager.Instance == null) return;

        QuestionData q = QuestionManager.Instance.CurrentQuestion;
        if (q == null) return;

        nameTxt.text = BuildName(q);
    }

    // "Hedef 12 - 3" gibi: bölüm adı + bölüm İÇİNDEKİ soru sırası (1-based).
    // Soru global id'si 303 bile olsa bölüm içi sıra gösterilir.
    private string BuildName(QuestionData q)
    {
        SaveManager sm    = SaveManager.Instance;
        BolumData   bolum = sm != null ? sm.AktifBolum : null;

        if (bolum != null)
        {
            int goreli = QuestionManager.Instance.CurrentIndex - bolum.startIndex + 1;
            goreli = Mathf.Clamp(goreli, 1, bolum.soruSayisi);
            return $"{bolum.bolumAdi}\n{goreli}/{bolum.soruSayisi}";
        }

        // Aktif bölüm yoksa (ör. Inspector startIndex ile test): global id'den çıkar.
        string ad = ((q.id - 1) / 100) switch
        {
            0 => "Hedef 12",
            1 => "Hedef 24",
            2 => "Hedef 36",
            3 => "Asal",
            _ => "Rastgele"
        };
        return $"{ad}\n{(q.id - 1) % 100 + 1}/100";
    }
}
