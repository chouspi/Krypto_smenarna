namespace KryptoSmenarna.Wpf.Models;

public static class CurrencyAmountFormatter
{
    public static string Format(decimal amount, string currencyCode)
    {
        if (currencyCode == "EUR" || currencyCode == "USD")
            return amount.ToString("N2");

        return amount.ToString("N8");
    }
}
