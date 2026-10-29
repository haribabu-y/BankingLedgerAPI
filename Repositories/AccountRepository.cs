using BankingLedgerAPI.Models;
using BankingLedgerAPI.Repositories.Interfaces;
using MongoDB.Driver;

namespace BankingLedgerAPI.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IMongoCollection<Account> _accounts;

    public AccountRepository(IMongoDatabase database)
    {
        _accounts = database.GetCollection<Account>("Accounts");
    }

    public async Task<Account?> GetByIdAsync(string id)
    {
        Account? account = await _accounts.Find(account => account.Id == id)
            .FirstOrDefaultAsync();

        return account;
    }

    public Task<List<Account>> GetAllAsync()
    {
        return _accounts.Find(Builders<Account>.Filter.Empty).ToListAsync();
    }

    public Task<List<Account>> GetByCustomerIdAsync(string customerId)
    {
        return _accounts.Find(account => account.CustomerId == customerId)
            .ToListAsync();
    }

    public Task InsertAsync(Account account)
    {
        return _accounts.InsertOneAsync(account);
    }

    public Task<bool> AccountNumberExistsAsync(string accountNumber)
    {
        return _accounts.Find(account => account.AccountNumber == accountNumber)
            .AnyAsync();
    }

    public async Task<bool> CloseIfActiveAndZeroBalanceAsync(string id)
    {
        UpdateResult result = await _accounts.UpdateOneAsync(
            account => account.Id == id &&
                       account.Status == "Active" &&
                       account.Balance == 0,
            Builders<Account>.Update.Set(account => account.Status, "Closed"));

        return result.ModifiedCount > 0;
    }

    public async Task<Account?> CreditBalanceIfActiveAsync(string id, decimal amount)
    {
        FilterDefinition<Account> filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(account => account.Id, id),
            Builders<Account>.Filter.Eq(account => account.Status, "Active"));
        UpdateDefinition<Account> update = Builders<Account>.Update
            .Inc(account => account.Balance, amount);

        Account? account = await _accounts.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<Account>
            {
                ReturnDocument = ReturnDocument.After
            });

        return account;
    }

    public async Task<Account?> DebitBalanceIfActiveAndSufficientAsync(string id, decimal amount)
    {
        FilterDefinition<Account> filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(account => account.Id, id),
            Builders<Account>.Filter.Eq(account => account.Status, "Active"),
            Builders<Account>.Filter.Gte(account => account.Balance, amount));
        UpdateDefinition<Account> update = Builders<Account>.Update
            .Inc(account => account.Balance, -amount);

        Account? account = await _accounts.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<Account>
            {
                ReturnDocument = ReturnDocument.After
            });

        return account;
    }
}
