using BankingLedgerAPI.Models;

namespace BankingLedgerAPI.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(string id);

    Task<List<Account>> GetAllAsync();

    Task<List<Account>> GetByCustomerIdAsync(string customerId);

    Task InsertAsync(Account account);

    Task<bool> AccountNumberExistsAsync(string accountNumber);

    Task<bool> CloseIfActiveAndZeroBalanceAsync(string id);

    Task<Account?> CreditBalanceIfActiveAsync(string id, decimal amount);

    Task<Account?> DebitBalanceIfActiveAndSufficientAsync(string id, decimal amount);
}
