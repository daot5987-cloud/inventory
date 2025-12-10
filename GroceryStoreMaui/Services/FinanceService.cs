using GroceryStoreMaui.Models;
using SQLite;

namespace GroceryStoreMaui.Services;

public class FinanceService
{
    private readonly SQLiteAsyncConnection _db;

    public FinanceService(DatabaseService database)
    {
        _db = database.Connection;
    }

    public Task<List<CashTransaction>> GetCashTransactionsAsync(
        DateTime? from = null,
        DateTime? to = null)
    {
        var query = _db.Table<CashTransaction>();

        if (from != null)
            query = query.Where(c => c.Date >= from.Value);

        if (to != null)
            query = query.Where(c => c.Date <= to.Value);

        return query.OrderByDescending(c => c.Date).ToListAsync();
    }
}
