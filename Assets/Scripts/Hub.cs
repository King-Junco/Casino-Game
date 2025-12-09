using System;
using TMPro;
using UnityEngine;

public class Hub : MonoBehaviour
{
    private int currency;
    [SerializeField] private ExternalFileManager universalCurrency;
    [SerializeField] TextMeshProUGUI currencyText;
    void Start()
    {
        currency = universalCurrency.ReadFromExternalFile();
    }

    // Update is called once per frame
    void Update()
    {
        currencyText.text = "Currency: $" + currency;
    }

    public void randomCurrency()
    {
        currency = UnityEngine.Random.Range(0, 10000);
        universalCurrency.WriteToExternalFile(currency);
    }
}
