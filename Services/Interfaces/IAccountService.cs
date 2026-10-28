using BankingLedgerAPI.DTOs;
namespace BankingLedgerAPI.Services;

public interface IAccountService
{
    Task<AccountResponseDto> CreateAccount(CreateAccountRequestDto request);

    Task<List<AccountResponseDto>> GetAllAccounts();

    Task<AccountResponseDto?> GetAccountById(string id);

    Task<List<AccountResponseDto>> GetAccountsByCustomerId(
        string customerId);

    Task<bool> CloseAccount(string id);
}
