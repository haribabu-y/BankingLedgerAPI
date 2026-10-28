using MongoDB.Driver;
using BankingLedgerAPI.Models;
using BankingLedgerAPI.DTOs;
using MongoDB.Bson;
using BankingLedgerAPI.Services;

namespace BankingLedgerAPI.Services;

public class AccountService: IAccountService
{
    private readonly IMongoCollection<Account> _accounts;
    private readonly IMongoCollection<Customer> _customers;

    public AccountService(IMongoDatabase database)
    {
        _accounts = database.GetCollection<Account>("Accounts");
        _customers = database.GetCollection<Customer>("Customers");
    }

    public async Task<AccountResponseDto> CreateAccount(CreateAccountRequestDto request)
    {
        if (!ObjectId.TryParse(request.CustomerId, out _))
        {
            throw new ArgumentException("Invalid customer ID.");
        }

        var customer = await _customers.Find(c => c.Id == request.CustomerId).FirstOrDefaultAsync();

        if (customer is null)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        if (request.InitialDeposit < 0)
        {
            throw new ArgumentException("Initial deposit cannot be negative.");
        }

        var account = new Account
        {
            CustomerId = request.CustomerId,
            AccountNumber = await GenerateAccountNumber(),
            AccountType = request.AccountType,
            Balance = request.InitialDeposit,
            Currency = "INR",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        await _accounts.InsertOneAsync(account);

        return new AccountResponseDto(account);
    }

    public async Task<List<AccountResponseDto>> GetAllAccounts()
    {
        var accounts = await _accounts.Find(Builders<Account>.Filter.Empty).ToListAsync();

        return accounts.Select(a => new AccountResponseDto(a)).ToList();
    }

    public async Task<AccountResponseDto?> GetAccountById(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        var account = await _accounts.Find(a => a.Id == id).FirstOrDefaultAsync();

        return account is null ? null : new AccountResponseDto(account);
    }

    public async Task<List<AccountResponseDto>> GetAccountsByCustomerId(string customerId)
    {
        if (!ObjectId.TryParse(customerId, out _))
        {
            throw new ArgumentException("Invalid customer ID.");
        }

        var accounts = await _accounts.Find(a => a.CustomerId == customerId).ToListAsync();

        return accounts.Select(a => new AccountResponseDto(a)).ToList();
    }

    public async Task<bool> CloseAccount(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        var account = await _accounts.Find(a => a.Id == id).FirstOrDefaultAsync();

        if (account is null)
        {
            return false;
        }

        if (account.Balance != 0)
        {
            throw new InvalidOperationException("An account must have a zero balance before closure.");
        }

        if (account.Status == "Closed")
        {
            return false;
        }

        var result = await _accounts.UpdateOneAsync(
            a => a.Id == id && a.Status == "Active",
            Builders<Account>.Update.Set(a => a.Status, "Closed"));

        return result.ModifiedCount > 0;
    }

    //Method to generate the bank account number
    private async Task<string> GenerateAccountNumber()
    {
        while (true)
        {
            var accountNumber = Random.Shared.NextInt64(100000000000, 999999999999).ToString();
            var exists = await _accounts.Find(a => a.AccountNumber == accountNumber).AnyAsync();

            if (!exists)
            {
                return accountNumber;
            }
        }
    }
}
