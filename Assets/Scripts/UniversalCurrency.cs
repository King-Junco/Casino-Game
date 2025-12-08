using UnityEngine;

public class UniversalCurrency : MonoBehaviour
{
    private int universalBalance = 0;

    public int addCurrency(int amount)
    {
        universalBalance += amount;
        return universalBalance;
    }

    public int getCurrency()
    {
        return universalBalance;
    }   
    
}
