using BankingLedgerAPI.Models;

namespace BankingLedgerAPI.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByEmailAsync(string email);

    Task<Customer?> GetByIdAsync(string id);

    Task<List<Customer>> GetAllAsync();

    Task InsertAsync(Customer customer);

    Task<bool> DeleteByIdAsync(string id);
}
