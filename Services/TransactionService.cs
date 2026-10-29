using BankingLedgerAPI.Models;
using MongoDB.Bson;
using BankingLedgerAPI.DTOs;
using BankingLedgerAPI.Repositories.Interfaces;
namespace BankingLedgerAPI.Services;

public class TransactionService: ITransactionService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<TransactionResponseDto> Credit(string accountId, CreditRequestDto request)
    {
        ValidateAmount(request.Amount);

        if(!ObjectId.TryParse(accountId, out _)) {
            throw new ArgumentException("Invalid account ID.");
        }

        Account? account = await _accountRepository.GetByIdAsync(accountId);

        if(account is null) {
            throw new KeyNotFoundException("Account not found.");
        }

        if(account.Status != "Active") {
            throw new InvalidOperationException("Cannot deposit into a closed account.");
        }

        Account? updatedAccount = await _accountRepository
            .CreditBalanceIfActiveAsync(accountId, request.Amount);

        if(updatedAccount is null) {
            throw new InvalidOperationException("Account balance could not be updated.");
        }

        var transaction = new Transaction
        {
            AccountId = accountId,
            Type = "Credit",
            Amount = request.Amount,
            BalanceAfter = updatedAccount.Balance,
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _transactionRepository.InsertAsync(transaction);
        }
        catch (Exception error)
        {
            Console.WriteLine(
                $"Transaction insert failed: {error}");

            try
            {
                var rollbackSucceeded =
                    await _accountRepository.DebitBalanceIfActiveAndSufficientAsync(accountId, request.Amount);

                if (rollbackSucceeded is null)
                {
                    throw new InvalidOperationException("Could not roll back the account credit.");
                }
            }
            catch (Exception rollbackError)
            {
                Console.WriteLine($"CRITICAL: Account rollback failed: {rollbackError}");

                throw new InvalidOperationException(
                    "Transaction failed and account rollback could not be confirmed.",
                    new AggregateException(error, rollbackError));
            }

            throw;
        }
        return new TransactionResponseDto(transaction);
    }

    public async Task<TransactionResponseDto> Debit(string accountId, DebitRequestDto request)
    {
        ValidateAmount(request.Amount);

        if(!ObjectId.TryParse(accountId, out _)) {
            throw new ArgumentException("Invalid account ID.");
        }

        Account? account = await _accountRepository.GetByIdAsync(accountId);

        if(account is null) {
            throw new KeyNotFoundException("Account not found.");
        }

        if(account.Status != "Active") {
            throw new InvalidOperationException("Cannot withdraw from a closed account.");
        }

        Account? updatedAccount = await _accountRepository
            .DebitBalanceIfActiveAndSufficientAsync(accountId, request.Amount);

        if(updatedAccount is null) {
            throw new InvalidOperationException("Balance changed or funds are insufficient. Retry the request.");
        }

        var transaction = new Transaction
        {
            AccountId = accountId,
            Type = "Debit",
            Amount = request.Amount,
            BalanceAfter = updatedAccount.Balance,
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            await _transactionRepository.InsertAsync(transaction);
        }
        catch (Exception error)
        {
            Console.WriteLine(
                $"Transaction insert failed: {error}");

            try
            {
                Account? rollbackSucceeded =
                    await _accountRepository.CreditBalanceIfActiveAsync(accountId, request.Amount);

                if (rollbackSucceeded is null)
                {
                    throw new InvalidOperationException("Could not roll back the account credit.");
                }
            }
            catch (Exception rollbackError)
            {
                Console.WriteLine($"CRITICAL: Account rollback failed: {rollbackError}");

                throw new InvalidOperationException(
                    "Transaction failed and account rollback could not be confirmed.",
                    new AggregateException(error, rollbackError));
            }

            throw;
        }

        return new TransactionResponseDto(transaction);
    }

    public async Task<List<TransactionResponseDto>> GetAccountTransactions(string accountId)
    {
        if(!ObjectId.TryParse(accountId, out _)) {
            throw new ArgumentException("Invalid account ID.");
        }

        Account? account = await _accountRepository.GetByIdAsync(accountId);

        if(account is null) {
            throw new KeyNotFoundException("Account not found.");
        }

        List<Transaction> transactions = await _transactionRepository.GetByAccountIdAsync(accountId);

        return transactions.Select(t => new TransactionResponseDto(t)).ToList();
    }
    
    public async Task<TransactionResponseDto?> GetTransactionById(string transactionId)
    {
        if(!ObjectId.TryParse(transactionId, out _)) {
            return null;
        }
        Transaction? transaction = await _transactionRepository.GetByIdAsync(transactionId);
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
