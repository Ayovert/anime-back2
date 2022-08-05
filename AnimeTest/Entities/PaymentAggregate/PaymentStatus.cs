namespace AnimeBack.Entities.PaymentAggregate
{
    public enum PaymentStatus
    {
        initiated, 
        successful,
        failed,
        error,
        pending
    }
}