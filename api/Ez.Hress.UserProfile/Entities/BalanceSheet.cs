namespace Ez.Hress.UserProfile.Entities;

public class BalanceSheet
{
    public BalanceSheet()
    {
        Transactions = new List<Transaction>();
    }

    public int UserID { get; set; }
    public int Balance { get; set; }

    public IList<Transaction> Transactions { get; set; }
}
