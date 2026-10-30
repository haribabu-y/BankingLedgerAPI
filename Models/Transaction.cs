using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace BankingLedgerAPI.Models;

public class Transaction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    [BsonElement("balanceAfter")]
    public decimal BalanceAfter { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
