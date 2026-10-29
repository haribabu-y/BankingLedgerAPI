using Microsoft.AspNetCore.Identity;
using BankingLedgerAPI.Models;
using BankingLedgerAPI.DTOs;
using BankingLedgerAPI.Repositories.Interfaces;
using System.Linq;

namespace BankingLedgerAPI.Services;


public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly PasswordHasher<Customer> _passwordHasher;
    private readonly TokenService _tokenService;

    public CustomerService(
        ICustomerRepository customerRepository,
        TokenService tokenService)
    {
        _customerRepository = customerRepository;
        _tokenService = tokenService;
        _passwordHasher = new PasswordHasher<Customer>();
    }

    public async Task<LoginResponseDto?> RegisterCustomer(CreateCustomerDto dto)
    {
        string email = dto.Email.Trim().ToLowerInvariant();
        Customer? existingCustomer = await _customerRepository.GetByEmailAsync(email);

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

        await _customerRepository.InsertAsync(customer);

        return _tokenService.CreateToken(customer);
    }

    public async Task<LoginResponseDto?> LoginCustomer(LoginRequestDto dto)
    {
        string email = dto.Email.Trim().ToLowerInvariant();

        Customer? customer = await _customerRepository.GetByEmailAsync(email);

        if (customer is null)
        {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
            customer, customer.Password, dto.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return _tokenService.CreateToken(customer);
    }

    public async Task<CustomerResponseDto?> GetCustomerById(string id)
    {
        Customer? customer = await _customerRepository.GetByIdAsync(id);

        if (customer is null)
        {
            return null;
        }

        return new CustomerResponseDto(customer);
    }

    public async Task<List<CustomerResponseDto>> GetAllCustomers()
    {
        List<Customer> customers = await _customerRepository.GetAllAsync();

        return customers.Select(customer => new CustomerResponseDto(customer)).ToList();
    }

    public async Task<bool> DeleteCustomer(string id)
    {
        Customer? customer = await _customerRepository.GetByIdAsync(id);

        if (customer is null)
        {
            return false;
        }

        return await _customerRepository.DeleteByIdAsync(id);
    }
}
