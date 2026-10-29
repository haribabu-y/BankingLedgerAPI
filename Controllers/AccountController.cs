using Microsoft.AspNetCore.Mvc;
using BankingLedgerAPI.Services;
using BankingLedgerAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
namespace BankingLedgerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController: ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService) {
        _accountService = accountService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountRequestDto request)
    {
        try {
            AccountResponseDto account = await _accountService.CreateAccount(request);

            return CreatedAtAction(
                nameof(GetAccountById),
                new { id = account.Id },
                account);
        } catch (ArgumentException ex) {
            return BadRequest(new { message = ex.Message });
        } catch (KeyNotFoundException ex) {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        List<AccountResponseDto> accounts = await _accountService.GetAllAccounts();

        return Ok(accounts);
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(string id)
    {
        AccountResponseDto? account = await _accountService.GetAccountById(id);

        if (account is null) {
            return NotFound(new { message = "Account not found." });
        }

        return Ok(account);
    }
    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetAccountsByCustomerId(string customerId)
    {
        try {
            List<AccountResponseDto> accounts = await _accountService.GetAccountsByCustomerId(customerId);

            return Ok(accounts);
        } catch (ArgumentException ex) {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> CloseAccount(string id) {
        try {
            bool closed = await _accountService.CloseAccount(id);

            if (!closed) {
                return NotFound(new {
                    message = "Account not found or already closed."
                });
            }

            return NoContent();
        } catch (InvalidOperationException ex) {
            return Conflict(new { message = ex.Message });
        }
    }

}
