using TMPro;
using UnityEngine;

public class NumberWriteManager : MonoBehaviour
{
    [SerializeField] private string sayiTxtName = "sayiTxt";
    [SerializeField] private int startNumber = 1;

    private void Start()
    {
        WriteNumbers();
    }

    private void WriteNumbers()
    {
        int number = startNumber;

        foreach (Transform child in transform)
        {
            TMP_Text sayiTxt = FindSayiTxt(child);
            if (sayiTxt != null)
            {
                sayiTxt.text = number.ToString();
                number++;
            }
        }
    }

    private TMP_Text FindSayiTxt(Transform parent)
    {
        foreach (TMP_Text txt in parent.GetComponentsInChildren<TMP_Text>(true))
        {
            if (txt.name == sayiTxtName)
                return txt;
        }
        return null;
    }
}
