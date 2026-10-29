using BankingLedgerAPI.Models;
using BankingLedgerAPI.Repositories.Interfaces;
using MongoDB.Driver;

namespace BankingLedgerAPI.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IMongoCollection<Customer> _customers;

    public CustomerRepository(IMongoDatabase database)
    {
        _customers = database.GetCollection<Customer>("Customers");
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        Customer? customer = await _customers.Find(customer => customer.Email == email)
            .FirstOrDefaultAsync();

        return customer;
    }

    public async Task<Customer?> GetByIdAsync(string id)
    {
        Customer? customer = await _customers.Find(customer => customer.Id == id)
            .FirstOrDefaultAsync();
        return customer;
    }

    public Task<List<Customer>> GetAllAsync()
    {
        return _customers.Find(Builders<Customer>.Filter.Empty).ToListAsync();
    }

    public Task InsertAsync(Customer customer)
    {
        return _customers.InsertOneAsync(customer);
    }

    public async Task<bool> DeleteByIdAsync(string id)
    {
        DeleteResult result = await _customers.DeleteOneAsync(customer => customer.Id == id);
        return result.DeletedCount > 0;
    }
}
