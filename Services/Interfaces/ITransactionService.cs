using BankingLedgerAPI.DTOs;
namespace BankingLedgerAPI.Services;

public interface ITransactionService
{
    Task<TransactionResponseDto> Credit(string accountId, CreditRequestDto request);

    Task<TransactionResponseDto> Debit(string accountId, DebitRequestDto request);

    Task<List<TransactionResponseDto>> GetAccountTransactions(string accountId);

    Task<TransactionResponseDto?> GetTransactionById(string transactionId);
}
