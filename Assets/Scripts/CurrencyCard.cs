using UnityEngine;
using TMPro;

public class CurrencyCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyCodeText;
    [SerializeField] private TextMeshProUGUI rateText;

    public void Initialize(string currencyCode, string rate)
    {
        currencyCodeText.text = currencyCode;

        rateText.text = rate;
    }

    public void UpdateRate(string newRate)
    {
        rateText.text = newRate;
    }
}