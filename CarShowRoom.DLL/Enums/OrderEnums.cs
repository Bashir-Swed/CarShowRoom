namespace CarShowRoom.DAL.Enums
{
    public enum OrderType
    {
        Rent = 1,
        Installment = 2,
        Buy = 3,
        Sell = 4
    }

    public enum OrderStatus
    {
        Pending = 1,
        Approved = 2,  
        Rejected = 3, 
        Completed = 4,
        Canceled = 5
    }
}
