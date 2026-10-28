using Microsoft.AspNetCore.Mvc;
using BankingLedgerAPI.Services;
using BankingLedgerAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
namespace BankingLedgerAPI.Controllers;

[ApiController]
[Route("api/transaction")]
public class TransactionController: ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [Authorize]
    [HttpPost("credit/{accountId}")]
    public async Task<IActionResult> Credit(string accountId, [FromBody] CreditRequestDto request)
    {
        try {
            var result = await _transactionService.Credit(accountId, request);
            return Ok(result);
        } catch (ArgumentException ex) {
            return BadRequest(new { message = ex.Message });
        } catch (KeyNotFoundException ex) {
            return NotFound(new { message = ex.Message });
        } catch (InvalidOperationException ex) {
            return Conflict(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("debit/{accountId}")]
    public async Task<IActionResult> Debit(string accountId, [FromBody] DebitRequestDto request)
    {
        try {
            var result = await _transactionService.Debit(accountId, request);

            return Ok(result);
        } catch (ArgumentException ex) {
            return BadRequest(new { message = ex.Message });
        } catch (KeyNotFoundException ex) {
            return NotFound(new { message = ex.Message });
        } catch (InvalidOperationException ex) {
            return Conflict(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("account/{accountId}")]
    public async Task<IActionResult> GetAccountTransactions(string accountId)
    {
        try {
            var transactions = await _transactionService.GetAccountTransactions(accountId);

            return Ok(transactions);
        } catch (ArgumentException ex) {
            return BadRequest(new { message = ex.Message });
        } catch (KeyNotFoundException ex) {
            return NotFound(new { message = ex.Message });
        }
    }
    
    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetTransactionById(string transactionId)
    {
        var transaction = await _transactionService.GetTransactionById(transactionId);

        if (transaction is null) {
            return NotFound(new { message = "Transaction not found." });
        }

        return Ok(transaction);
    }
}
