using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace BankingLedgerAPI.Models;

public class Customer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";

    public DateTime DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; }
}
