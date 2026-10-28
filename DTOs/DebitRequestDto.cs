using System.ComponentModel.DataAnnotations;
namespace BankingLedgerAPI.DTOs;

public class DebitRequestDto
{
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
}
