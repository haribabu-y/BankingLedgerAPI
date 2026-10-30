using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace BankingLedgerAPI.Models;

public class Account
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [BsonElement("AccountType")]
    public string AccountType { get; set; } = "Savings";

    [BsonRepresentation(BsonType.Decimal128)]
    [BsonElement("balance")]
    public decimal Balance { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "INR";

    [BsonElement("status")]
    public string Status { get; set; } = "Active";

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
