using System.ComponentModel.DataAnnotations;
namespace BankingLedgerAPI.DTOs;

public class CreateAccountRequestDto
{
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Savings|Current)$", ErrorMessage = "Account type must be Savings or Current.")]
    public string AccountType { get; set; } = "Savings";

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal InitialDeposit { get; set; } = 0;
}
