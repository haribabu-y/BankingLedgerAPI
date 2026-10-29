using BankingLedgerAPI.Models;

namespace BankingLedgerAPI.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task InsertAsync(Transaction transaction);

    Task<List<Transaction>> GetByAccountIdAsync(string accountId);

    Task<Transaction?> GetByIdAsync(string id);
}
