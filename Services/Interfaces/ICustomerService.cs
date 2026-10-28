using BankingLedgerAPI.DTOs;

namespace BankingLedgerAPI.Services;

public interface ICustomerService
{
    Task<LoginResponseDto?> RegisterCustomer(CreateCustomerDto dto);

    Task<LoginResponseDto?> LoginCustomer(LoginRequestDto dto);

    Task<CustomerResponseDto?> GetCustomerById(string id);

    Task<List<CustomerResponseDto>> GetAllCustomers();

    Task<bool> DeleteCustomer(string id);
}
