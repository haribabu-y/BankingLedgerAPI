using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using BankingLedgerAPI.Models;
using BankingLedgerAPI.DTOs;
using System.Linq;

namespace BankingLedgerAPI.Services;


public class CustomerService : ICustomerService
{
    private readonly IMongoCollection<Customer> _customer;
    private readonly PasswordHasher<Customer> _passwordHasher;
    private readonly TokenService _tokenService;

    public CustomerService(IMongoDatabase database, TokenService tokenService)
    {
        _customer = database.GetCollection<Customer>("Customers");
        _tokenService = tokenService;
        _passwordHasher = new PasswordHasher<Customer>();
    }

    public async Task<LoginResponseDto?> RegisterCustomer(CreateCustomerDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var existingCustomer = await _customer
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (existingCustomer is not null)
        {
            return null;
        }

        var customer = new Customer
        {
            Name = dto.Name,
            Email = email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        };

        customer.Password = _passwordHasher.HashPassword(customer, dto.Password);

        await _customer.InsertOneAsync(customer);

        return _tokenService.CreateToken(customer);
    }

    public async Task<LoginResponseDto?> LoginCustomer(LoginRequestDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var customer = await _customer.Find(u => u.Email == email).FirstOrDefaultAsync();

        if (customer is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            customer, customer.Password, dto.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return _tokenService.CreateToken(customer);
    }

    public async Task<CustomerResponseDto?> GetCustomerById(string id)
    {
        var customer = await _customer.Find(u => u.Id == id).FirstOrDefaultAsync();

        if (customer is null)
        {
            return null;
        }

        return new CustomerResponseDto(customer);
    }

    public async Task<List<CustomerResponseDto>> GetAllCustomers()
    {
        var customers = await _customer.Find(Builders<Customer>.Filter.Empty).ToListAsync();

        return customers.Select(customer => new CustomerResponseDto(customer)).ToList();
    }

    public async Task<bool> DeleteCustomer(string id)
    {
        var customer = await _customer.Find(u => u.Id == id).FirstOrDefaultAsync();

        if (customer is null)
        {
            return false;
        }

        var result = await _customer.DeleteOneAsync(c => c.Id == id); ;

        return result.DeletedCount > 0;
    }
}
