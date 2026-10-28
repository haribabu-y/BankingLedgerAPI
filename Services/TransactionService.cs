using BankingLedgerAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using BankingLedgerAPI.DTOs;
namespace BankingLedgerAPI.Services;

public class TransactionService: ITransactionService
{
    private readonly IMongoClient _client;
    private readonly IMongoCollection<Account> _accounts;
    private readonly IMongoCollection<Transaction> _transactions;

    public TransactionService(IMongoClient client, IMongoDatabase database)
    {
        _client = client;
        _accounts = database.GetCollection<Account>("Accounts");
        _transactions = database.GetCollection<Transaction>("Transactions");
    }

    public async Task<TransactionResponseDto> Credit(string accountId, CreditRequestDto request)
    {
        ValidateAmount(request.Amount);

        if(!ObjectId.TryParse(accountId, out _)) {
            throw new ArgumentException("Invalid account ID.");
        }

        var account = await _accounts.Find(a => a.Id == accountId).FirstOrDefaultAsync();

        if(account is null) {
            throw new KeyNotFoundException("Account not found.");
        }

        if(account.Status != "Active") {
            throw new InvalidOperationException("Cannot deposit into a closed account.");
        }

        // Update only an active account.
        var filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(a => a.Id, accountId),
            Builders<Account>.Filter.Eq(a => a.Status, "Active"));

        var update = Builders<Account>.Update
            .Inc(a => a.Balance, request.Amount);

        var result = await _accounts.UpdateOneAsync(filter, update);

        if(result.ModifiedCount != 1) {
            throw new InvalidOperationException("Account balance could not be updated.");
        }

        var updatedAccount = await _accounts.Find(a => a.Id == accountId).FirstAsync();

        var transaction = new Transaction {
            AccountId = accountId,
            Type = "Credit",
            Amount = request.Amount,
            BalanceAfter = updatedAccount.Balance,
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _transactions.InsertOneAsync(transaction);

        return new TransactionResponseDto(transaction);
    }

    public async Task<TransactionResponseDto> Debit(string accountId, DebitRequestDto request)
    {
        ValidateAmount(request.Amount);

        if(!ObjectId.TryParse(accountId, out _)) {
            throw new ArgumentException("Invalid account ID.");
        }

        var account = await _accounts.Find(a => a.Id == accountId).FirstOrDefaultAsync();

        if(account is null) {
            throw new KeyNotFoundException("Account not found.");
        }

        if(account.Status != "Active") {
            throw new InvalidOperationException("Cannot withdraw from a closed account.");
        }

        // Atomically check the balance and deduct the amount.
        var filter = Builders<Account>.Filter.And(
            Builders<Account>.Filter.Eq(a => a.Id, accountId),
            Builders<Account>.Filter.Eq(a => a.Status, "Active"),
            Builders<Account>.Filter.Gte(a => a.Balance, request.Amount));

        var update = Builders<Account>.Update
            .Inc(a => a.Balance, -request.Amount);

        var result = await _accounts.UpdateOneAsync(filter, update);

        if(result.ModifiedCount != 1) {
            throw new InvalidOperationException("Balance changed or funds are insufficient. Retry the request.");
        }

        var updatedAccount = await _accounts.Find(a => a.Id == accountId).FirstAsync();

        var transaction = new Transaction {
            AccountId = accountId,
            Type = "Debit",
            Amount = request.Amount,
            BalanceAfter = updatedAccount.Balance,
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _transactions.InsertOneAsync(transaction);

        return new TransactionResponseDto(transaction);
    }

    public async Task<List<TransactionResponseDto>> GetAccountTransactions(string accountId)
    {
        if(!ObjectId.TryParse(accountId, out _)) {
            throw new ArgumentException("Invalid account ID.");
        }

        var accountExists = await _accounts.Find(a => a.Id == accountId).AnyAsync();

        if(!accountExists) {
            throw new KeyNotFoundException("Account not found.");
        }

        var transactions = await _transactions
            .Find(t => t.AccountId == accountId)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(t => new TransactionResponseDto(t)).ToList();
    }
    
    public async Task<TransactionResponseDto?> GetTransactionById(string transactionId)
    {
        if(!ObjectId.TryParse(transactionId, out _)) {
            return null;
        }
        var transaction = await _transactions.Find(t => t.Id == transactionId).FirstOrDefaultAsync();
        return transaction is null ? null : new TransactionResponseDto(transaction);
    }
    
    private static void ValidateAmount(decimal amount) {
        if(amount <= 0) {
            throw new ArgumentException("Transaction amount must be greater than zero.");
        }

        if(decimal.Round(amount, 2) != amount) {
            throw new ArgumentException("Transaction amount cannot have more than two decimal places.");
        }
    }
}
