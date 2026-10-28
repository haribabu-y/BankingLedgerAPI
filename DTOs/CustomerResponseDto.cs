using BankingLedgerAPI.Models;
namespace BankingLedgerAPI.DTOs;

public class CustomerResponseDto
{
    public string? Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }

    public CustomerResponseDto(Customer customer)
    {
        Id = customer.Id;
        Name = customer.Name;
        Email = customer.Email;
        Phone = customer.Phone;
        Role = customer.Role;
        DateOfBirth = customer.DateOfBirth;
        CreatedAt = customer.CreatedAt;
    }
}
