# Banking Ledger Management System API

## 1. Project Overview

The **Banking Ledger Management System API** is a backend application developed using C#, ASP.NET Core Web API, and MongoDB. It manages customers, bank accounts, and financial transactions while maintaining transaction history for deposits and withdrawals.

The application follows a service-based architecture that separates HTTP requests, business logic, data models, and database operations.

### Key Features

- Customer management
- Bank account creation and management
- Deposit money into bank accounts
- Withdraw money from bank accounts
- Balance validation and insufficient-funds protection
- Transaction history and transaction details
- MongoDB database integration
- Request validation and structured HTTP responses
- Asynchronous database operations

## 2. Technologies Used

| Technology | Purpose |
|---|---|
| C# | Backend programming language |
| ASP.NET Core Web API | REST API development |
| .NET 10 | Application runtime and framework |
| MongoDB | NoSQL database |
| MongoDB C# Driver | MongoDB database connectivity |
| Swagger / OpenAPI | API documentation and testing |
| Data Annotations | Request validation |

## 3. Project Architecture

The application is organized into the following logical layers:

```text
BankingLedgerAPI/
├── Controllers/
│   ├── CustomerController.cs
│   ├── AccountController.cs
│   └── TransactionController.cs
├── DTOs/
│   ├── AccountResponseDto.cs
│   ├── CreateAccountRequestDto.cs
│   ├── CreateCustomerDto.cs
│   ├── CreditRequestDto.cs
│   ├── CustomerResponseDto.cs
│   ├── DebitRequestDto.cs
│   ├── LoginRequestDto.cs
│   ├── LoginResponseDto.cs
│   └── TransactionResponseDto.cs
├── Models/
│   ├── Customer.cs
│   ├── Account.cs
│   └── Transaction.cs
├── Services/
│   ├── ICustomerService.cs
│   ├── IAccountService.cs
│   ├── ITransactionService.cs
│   ├── CustomerService.cs
│   ├── AccountService.cs
│   └── TransactionService.cs
├── DatabaseSettings/
│   └── MongoDbSettings.cs
├── Program.cs
├── appsettings.json
└── BankingLedgerAPI.csproj
```

*Note: The directory and file names above illustrate the intended organization. Adjust them to match the actual project structure.*

### Responsibilities

- **Controllers:** Handle HTTP requests, invoke services, and return HTTP responses.
- **Services:** Implement business rules, validation, and database operations.
- **Interfaces:** Define service contracts and support dependency injection.
- **Models:** Represent customers, bank accounts, and ledger transactions.
- **DTOs:** Define the data accepted by and returned from API endpoints.
- **Program.cs:** Configure application services, MongoDB connectivity, and middleware.

## 4. Installation and Setup

### Prerequisites

Install the following tools:

- .NET 10 SDK
- MongoDB Community Server or MongoDB Atlas
- Visual Studio, Visual Studio Code, or another compatible IDE
- Postman or Swagger UI for API testing

### Step 1: Clone the repository

```bash
git clone <your-repository-url>
cd BankingLedgerAPI
```

Replace `<your-repository-url>` with your Git repository URL.

### Step 2: Configure MongoDB

Update the MongoDB settings in `appsettings.json` to match your environment.

Example:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017/?retryWrites=false",
    "DatabaseName": "BankingLedgerDB"
  }
}
```

Use the configuration keys expected by your `Program.cs`. Do not commit real database credentials, passwords, or production connection strings to source control.

### Step 3: Restore dependencies

```bash
dotnet restore
```

### Step 4: Build the application

```bash
dotnet build
```

### Step 5: Run the application

```bash
dotnet run
```

Check the terminal output for the application URL. Open the Swagger UI at the corresponding `/swagger` path if Swagger is enabled.

For example:

```text
https://localhost:<port>/swagger
```

Replace `<port>` with the actual port reported by your application.

## 5. API Documentation

The API exposes endpoints for customer management, bank accounts, and financial transactions.

### Base URL

```text
https://localhost:<port>
```

Use the actual HTTP or HTTPS URL shown when the application starts.

### 5.1 Customer APIs

These endpoints manage customer records. Verify the route prefixes and request DTO fields against your `CustomersController` and DTO classes.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/customer/rigister` | Create a customer |
| POST | `/api/customer/login` | login a customer |
| GET | `/api/customer` | Retrieve all customers |
| GET | `/api/customer/{id}` | Retrieve a customer by ID |
| DELETE | `/api/customer/{id}` | Delete a customer |

**Example: Retrieve all customers**

```http
GET /api/customer
```

**Example: Retrieve a customer by ID**

```http
GET /api/customer/{id}
```

A customer ID should be a valid MongoDB ObjectId represented as a string when that is how the model is configured.

### 5.2 Bank Account APIs

These endpoints create and manage accounts associated with customers.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/account` | Create a bank account |
| GET | `/api/account` | Retrieve all accounts |
| GET | `/api/account/{id}` | Retrieve an account by ID |
| GET | `/api/account/customer/{customerId}` | Retrieve accounts belonging to a customer |
| DELETE | `/api/account/{id}` | Close an account |

#### Create a bank account

```http
POST /api/account
Content-Type: application/json
```

Example request body:

```json
{
  "customerId": "YOUR_CUSTOMER_OBJECT_ID",
  "accountType": "Savings",
  "initialDeposit": 1000
}
```

Replace the customer ID with an existing customer's actual MongoDB ObjectId.

**Supported account types:** `Savings` and `Current`, subject to request validation.

The service validates the customer, creates the account, generates an account number, and initializes the account balance from the initial deposit.

#### Retrieve all accounts

```http
GET /api/account
```

Returns the bank accounts available in the database.

#### Retrieve an account by ID

```http
GET /api/account/{id}
```

#### Retrieve accounts by customer

```http
GET /api/account/customer/{customerId}
```

Returns the accounts associated with the specified customer.

#### Close a bank account

```http
DELETE /api/account/{id}
```

The endpoint closes an account according to the application's business rules. In the current implementation, an account can be closed only when its balance is zero. Closing an account changes its status rather than physically deleting its record.

### 5.3 Transaction APIs

Transaction endpoints support deposits, withdrawals, and transaction history.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/transaction/credit/{accountId}` | Deposit money |
| POST | `/api/transaction/debit/{accountId}` | Withdraw money |
| GET | `/api/transaction/account/{accountId}` | Retrieve an account's transaction history |
| GET | `/api/transaction/{transactionId}` | Retrieve a transaction by ID |

#### Deposit money

```http
POST /api/transaction/credit/{accountId}
Content-Type: application/json
```

Example request body:

```json
{
  "amount": 500,
  "description": "Cash deposit"
}
```

**Expected behavior:**

1. Validate the account ID and deposit amount.
2. Confirm that the account exists and is active.
3. Increase the account balance by the deposit amount.
4. Create a ledger transaction containing the deposit details and resulting balance.
5. Return the transaction response.

Example: If the account balance is ₹1,000 before the deposit, depositing ₹500 results in a balance of ₹1,500.

#### Withdraw money

```http
POST /api/transaction/debit/{accountId}
Content-Type: application/json
```

Example request body:

```json
{
  "amount": 200,
  "description": "ATM withdrawal"
}
```

**Expected behavior:**

1. Validate the account ID and withdrawal amount.
2. Confirm that the account exists and is active.
3. Verify that sufficient funds are available.
4. Decrease the account balance by the withdrawal amount.
5. Record the withdrawal and resulting balance in the ledger.
6. Return the transaction response.

Example: If the account balance is ₹1,500 before the withdrawal, withdrawing ₹200 results in a balance of ₹1,300.

A withdrawal must not be allowed to make the account balance negative.

#### Retrieve account transaction history

```http
GET /api/transaction/account/{accountId}
```

Returns the transactions associated with the specified account, sorted by creation time in descending order so that the newest transactions appear first.

#### Retrieve a transaction by ID

```http
GET /api/transaction/{transactionId}
```

Returns the details of a specific transaction or a not-found response when the transaction does not exist.

### 5.4 Common HTTP Status Codes

| Status Code | Meaning |
|---|---|
| `200 OK` | Request completed successfully |
| `400 Bad Request` | Invalid ID, invalid input, or failed request validation |
| `404 Not Found` | Customer, account, or transaction not found |
| `409 Conflict` | Business rule prevents the operation, such as insufficient funds |
| `500 Internal Server Error` | Unexpected server-side failure |

The exact status code depends on the endpoint's implementation and validation behavior.

## 6. Database Design

The application uses MongoDB, a document-oriented NoSQL database. Data is organized into collections containing BSON documents.

The primary collections are:

1. `Customers`
2. `Accounts`
3. `Transactions`

The actual collection names must match those configured in the service classes.

### 6.1 Customers Collection

**Purpose:** Store customer information and identify customers who own bank accounts.

Example document:

```json
{
  "_id": "CUSTOMER_OBJECT_ID",
  "name": "Sample Customer",
  "email": "customer@example.com",
  "phone": "9876543210",
  "createdAt": "2026-10-28T09:00:00Z",
  "updatedAt": "2026-10-28T09:00:00Z"
}
```

The field names are illustrative; use the exact fields defined in `Customer.cs`.

| Field | Description |
|---|---|
| `_id` | MongoDB document identifier |
| Customer details | Name, email, phone, and other configured fields |
| `createdAt` | Customer creation timestamp |
| `updatedAt` | Timestamp of the last update, if maintained |

### 6.2 Accounts Collection

**Purpose:** Store bank account information, ownership, current balance, and account status.

Example document:

```json
{
  "_id": "ACCOUNT_OBJECT_ID",
  "customerId": "CUSTOMER_OBJECT_ID",
  "accountNumber": "GENERATED_ACCOUNT_NUMBER",
  "accountType": "Savings",
  "balance": {
    "$numberDecimal": "1000.00"
  },
  "currency": "INR",
  "status": "Active",
  "createdAt": "2026-10-28T09:00:00Z"
}
```

| Field | Description |
|---|---|
| `_id` | Unique MongoDB account identifier |
| `customerId` | ID of the customer who owns the account |
| `accountNumber` | Generated bank account number |
| `accountType` | Type of account, such as Savings or Current |
| `balance` | Current account balance, stored as Decimal128 |
| `currency` | Account currency, defaulting to INR |
| `status` | Account status, such as Active or Closed |
| `createdAt` | Account creation timestamp |

**Relationship:** Each account references its owner through `customerId`. A customer can own multiple accounts.

### 6.3 Transactions Collection

**Purpose:** Maintain the financial ledger of deposits and withdrawals.

Example deposit document:

```json
{
  "_id": "TRANSACTION_OBJECT_ID",
  "accountId": "ACCOUNT_OBJECT_ID",
  "type": "Deposit",
  "amount": {
    "$numberDecimal": "500.00"
  },
  "balanceAfter": {
    "$numberDecimal": "1500.00"
  },
  "description": "Cash deposit",
  "createdAt": "2026-10-28T09:30:00Z"
}
```

| Field | Description |
|---|---|
| `_id` | Unique transaction identifier |
| `accountId` | ID of the account affected by the transaction |
| `type` | Transaction type: Deposit or Withdrawal |
| `amount` | Positive monetary amount of the transaction |
| `balanceAfter` | Account balance after the transaction |
| `description` | Explanation of the transaction |
| `createdAt` | Transaction creation timestamp |

**Important design rules:**

- Store the transaction amount as a positive value; use `type` to indicate whether the transaction adds or subtracts money.
- Store monetary values using MongoDB Decimal128 to avoid binary floating-point rounding issues.
- Treat completed ledger records as immutable. Corrections should be recorded as new transactions rather than silently editing historical entries.
- The `balanceAfter` field provides a record of the balance immediately after the transaction.

## 7. Database Relationships

The logical relationships between collections are:

```text
customers
   |
   | One customer can own multiple accounts
   v
accounts
   |
   | One account can have multiple transactions
   v
transactions
```

MongoDB does not enforce these references as relational foreign keys by default. The application services validate referenced customer and account records where required.

### Relationship Summary

| Relationship | Type | Reference |
|---|---|---|
| Customer → Account | One-to-many | `accounts.customerId` |
| Account → Transaction | One-to-many | `transactions.accountId` |

## 8. Financial Transaction Workflow

### Credir workflow

1. The client submits a deposit request.
2. The API validates the request.
3. The service retrieves and validates the account.
4. The balance is increased.
5. A deposit ledger record is inserted.
6. The transaction is committed and the response is returned.

### Debit workflow

1. The client submits a withdrawal request.
2. The API validates the request.
3. The service verifies the account status and available balance.
4. The balance is decreased only when sufficient funds are available.
5. A withdrawal ledger record is inserted.
6. The transaction is committed and the response is returned.

The implementation uses MongoDB sessions and transactions for the account-balance update and ledger insertion. **This requires a MongoDB deployment that supports multi-document transactions**, such as MongoDB Atlas or a properly configured replica set. A standalone MongoDB server does not support these transactions.

## 9. Validation and Business Rules

The application enforces the following rules where implemented:

- Customer and account identifiers must be valid MongoDB ObjectIds.
- An account must belong to an existing customer when created.
- Account types must be supported values.
- Deposits and withdrawals must be greater than zero.
- Monetary amounts must not contain more than two decimal places.
- Transactions require a valid account.
- Deposits and withdrawals are allowed only on active accounts.
- Withdrawals cannot exceed the available balance.
- Accounts can be closed only when their balance is zero.
- Required fields and description lengths are validated using request DTOs.

## 10. API Testing

Use Swagger UI or Postman to verify the main workflows.

Recommended test sequence:

1. Create a customer.
2. Retrieve the customer and note its ID.
3. Create an account for that customer.
4. Retrieve the account and note its ID.
5. Deposit ₹500.
6. Withdraw ₹200.
7. Retrieve the account and verify its balance.
8. Retrieve the account's transaction history.
9. Attempt a withdrawal greater than the available balance and verify that it fails without changing the balance.
10. Close the account after the balance reaches zero.

Example balance calculation:

| Operation | Amount | Expected Balance |
|---|---:|---:|
| Initial Credit | ₹1,000 | ₹1,000 |
| Credit | ₹500 | ₹1,500 |
| Debit | ₹200 | ₹1,300 |

## 11. Security and Production Considerations

This project documents the API's current functionality. Before using it for real banking or production financial operations, consider adding:

- Authentication and authorization for customers and administrators.
- Account ownership checks on every account and transaction endpoint.
- Secure management of database credentials.
- Unique indexes for account numbers and appropriate customer identifiers.
- Idempotency keys to prevent duplicate deposits or withdrawals after client retries.
- Robust handling of transaction retries and concurrent balance updates.
- Centralized exception handling and structured logging.
- Audit trails and monitoring.
- Automated unit and integration tests.
- Rate limiting, HTTPS, and production configuration management.

The current ledger design is suitable for a learning project or proof of concept. A production banking system requires additional accounting controls, reconciliation, and security review.

## 12. Future Enhancements

Potential improvements include:

- Fund transfers between accounts.
- Paginated transaction history with date filters.
- Account statements and downloadable reports.
- Customer authentication and role-based access control.
- Search by account number and transaction type.
- Transaction reference numbers and idempotency.
- Unit and integration testing automation.
- Deployment using Docker and a cloud-hosted database.

## 13. License

Add your chosen license here, for example, the MIT License, if you intend to distribute the project under that license.
