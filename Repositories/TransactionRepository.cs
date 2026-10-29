using BankingLedgerAPI.Models;
using BankingLedgerAPI.Repositories.Interfaces;
using MongoDB.Driver;

namespace BankingLedgerAPI.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IMongoCollection<Transaction> _transactions;

    public TransactionRepository(IMongoDatabase database)
    {
        _transactions = database.GetCollection<Transaction>("Transactions");
    }

    public Task InsertAsync(Transaction transaction)
    {
        return _transactions.InsertOneAsync(transaction);
    }

    public Task<List<Transaction>> GetByAccountIdAsync(string accountId)
    {
        return _transactions.Find(transaction => transaction.AccountId == accountId)
            .SortByDescending(transaction => transaction.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(string id)
    {
        Transaction? transaction = await _transactions.Find(transaction => transaction.Id == id)
            .FirstOrDefaultAsync();

        return transaction;
    }
}
