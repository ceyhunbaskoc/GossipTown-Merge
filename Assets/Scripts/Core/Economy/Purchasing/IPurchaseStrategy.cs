using System;

namespace Core.Economy.Purchasing
{
    public interface IPurchaseStrategy
    {
        void ProcessPurchase(int priceAmount, Action onSuccess, Action onFailure);
    }
}