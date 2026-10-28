using BankingLedgerAPI.DTOs;
using BankingLedgerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BankingLedgerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController: ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateCustomerDto dto)
    {
        var result = await _customerService.RegisterCustomer(dto);
        if (result is null) {
            return Conflict(new {
                message = "An account with this email already exists."
            });
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _customerService.LoginCustomer(dto);

        if (result is null) {
            return Unauthorized(new {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(string id)
    {
        var customer = await _customerService.GetCustomerById(id);

        if (customer is null) {
            return NotFound(new {
                message = "Customer not found."
            });
        }

        return Ok(customer);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomers();

        return Ok(customers);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(string id)
    {
        var deleted = await _customerService.DeleteCustomer(id);

        if (!deleted) {
            return NotFound(new { message = "Customer not found." });
        }

        return NoContent();
    }
}
