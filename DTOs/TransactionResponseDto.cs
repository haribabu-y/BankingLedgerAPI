
using BankingLedgerAPI.Models;

namespace BankingLedgerAPI.DTOs;

public class TransactionResponseDto
{
    public string? Id { get; set; }

    public string AccountId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public TransactionResponseDto(Transaction transaction)
    {
        Id = transaction.Id;
        AccountId = transaction.AccountId;
        Type = transaction.Type;
        Amount = transaction.Amount;
        BalanceAfter = transaction.BalanceAfter;
        Description = transaction.Description;
        CreatedAt = transaction.CreatedAt;
    }
}
