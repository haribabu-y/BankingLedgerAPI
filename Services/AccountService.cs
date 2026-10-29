using BankingLedgerAPI.Models;
using BankingLedgerAPI.DTOs;
using MongoDB.Bson;
using BankingLedgerAPI.Repositories.Interfaces;

namespace BankingLedgerAPI.Services;

public class AccountService: IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;

    public AccountService(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
    }

    public async Task<AccountResponseDto> CreateAccount(CreateAccountRequestDto request)
    {
        if (!ObjectId.TryParse(request.CustomerId, out _))
        {
            throw new ArgumentException("Invalid customer ID.");
        }

        Customer? customer = await _customerRepository.GetByIdAsync(request.CustomerId);

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

        await _accountRepository.InsertAsync(account);

        return new AccountResponseDto(account);
    }

    public async Task<List<AccountResponseDto>> GetAllAccounts()
    {
        List<Account> accounts = await _accountRepository.GetAllAsync();

        return accounts.Select(a => new AccountResponseDto(a)).ToList();
    }

    public async Task<AccountResponseDto?> GetAccountById(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        Account? account = await _accountRepository.GetByIdAsync(id);

        return account is null ? null : new AccountResponseDto(account);
    }

    public async Task<List<AccountResponseDto>> GetAccountsByCustomerId(string customerId)
    {
        if (!ObjectId.TryParse(customerId, out _))
        {
            throw new ArgumentException("Invalid customer ID.");
        }

        List<Account> accounts = await _accountRepository.GetByCustomerIdAsync(customerId);

        return accounts.Select(a => new AccountResponseDto(a)).ToList();
    }

    public async Task<bool> CloseAccount(string id)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        Account? account = await _accountRepository.GetByIdAsync(id);

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

        return await _accountRepository.CloseIfActiveAndZeroBalanceAsync(id);
    }

    //Method to generate the bank account number
    private async Task<string> GenerateAccountNumber()
    {
        while (true)
        {
            string accountNumber = Random.Shared.NextInt64(100000000000, 999999999999).ToString();
            bool exists = await _accountRepository.AccountNumberExistsAsync(accountNumber);

            if (!exists)
            {
                return accountNumber;
            }
        }
    }
}
