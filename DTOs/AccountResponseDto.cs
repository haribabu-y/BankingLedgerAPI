using System;
using BankingLedgerAPI.Models;

namespace BankingLedgerAPI.DTOs;

public class AccountResponseDto
{
    public string? Id { get; set; }

    public string CustomerId { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public string AccountType { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public AccountResponseDto(Account account)
    {
        Id = account.Id;
        CustomerId = account.CustomerId;
        AccountNumber = account.AccountNumber;
        AccountType = account.AccountType;
        Balance = account.Balance;
        Currency = account.Currency;
        Status = account.Status;
        CreatedAt = account.CreatedAt;
    }
}
