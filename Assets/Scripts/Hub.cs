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
        currencyText.text = "Currency: $" + currency;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
