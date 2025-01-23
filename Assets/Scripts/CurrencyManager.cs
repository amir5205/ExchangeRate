using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking; 
using Newtonsoft.Json; 
using Cysharp.Threading.Tasks;
using DG.Tweening; 

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform parent;

    [SerializeField] private float cardSpacing = 200f;
    [SerializeField] private float animationDuration = 0.5f; 

    private readonly List<string> currencyCodes = new List<string> { "USD", "JPY", "KZT" };

    private Dictionary<string, string> currentRates = new Dictionary<string, string>();

    private const string ApiUrl = "https://api.exchangerate-api.com/v4/latest/RUB";

    private async void Start()
    {
        var rates = await FetchCurrencyRates();

        if (rates != null)
        {
            currentRates = rates;
            SpawnCards(rates);

            UpdateCurrencyRatesLoop().Forget();
        }
        else
        {
            Debug.LogError("Не удалось получить данные о курсах валют");
        }
    }

    private async UniTask<Dictionary<string, string>> FetchCurrencyRates()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(ApiUrl))
        {
            try
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;

                    var jsonResponse = JsonConvert.DeserializeObject<CurrencyResponse>(response);

                    Dictionary<string, string> rates = new Dictionary<string, string>();

                    foreach (var code in currencyCodes)
                    {
                        if (jsonResponse.rates.ContainsKey(code))
                        {
                            rates[code] = jsonResponse.rates[code].ToString();
                        }
                    }

                    return rates;
                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Ошибка получении данных: {ex.Message}");
                return null;
            }
        }
    }

    private void SpawnCards(Dictionary<string, string> rates)
    {
        Vector3 centerPosition = parent.position;
        float cardWidth = cardPrefab.GetComponent<RectTransform>().rect.width;

        List<Vector3> cardPositions = new List<Vector3>
        {
            centerPosition,
            centerPosition - Vector3.right * (cardWidth + cardSpacing) / 2,
            centerPosition + Vector3.right * (cardWidth + cardSpacing) / 2
        };

        for (int i = 0; i < currencyCodes.Count; i++)
        {
            GameObject cardObject = Instantiate(cardPrefab, parent);
            cardObject.transform.position = cardPositions[i];
            cardObject.transform.localScale = Vector3.zero;

            CurrencyCard card = cardObject.GetComponent<CurrencyCard>();
            if (card != null)
            {
                card.Initialize(currencyCodes[i], rates[currencyCodes[i]]);

                cardObject.transform.DOScale(Vector3.one, 1f).SetDelay(i + animationDuration);
            }
        }
    }

    private async UniTaskVoid UpdateCurrencyRatesLoop()
    {
        while (true)
        {
            await UniTask.Delay(30000);

            var rates = await FetchCurrencyRates();
            if (rates != null)
            {
                currentRates = rates;

                for (int i = 0; i < currencyCodes.Count; i++)
                {
                    GameObject cardObject = parent.GetChild(i).gameObject;
                    CurrencyCard card = cardObject.GetComponent<CurrencyCard>();
                    if (card != null)
                    {
                        card.UpdateRate(rates[currencyCodes[i]]);
                    }
                }
            }
        }
    }
}

public class CurrencyResponse
{
    public Dictionary<string, float> rates { get; set; }
}