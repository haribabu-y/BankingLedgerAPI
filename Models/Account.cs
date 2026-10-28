using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace BankingLedgerAPI.Models;

public class Account
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string CustomerId { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public string AccountType { get; set; } = "Savings";

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Balance { get; set; }

    public string Currency { get; set; } = "INR";

    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
