# Banking Ledger Management System API

## 1. Project Overview

The **Banking Ledger Management System API** is a backend application developed using C#, ASP.NET Core Web API, and MongoDB. It manages customers, bank accounts, and financial transactions while maintaining transaction history for deposits and withdrawals.

The application separates HTTP handling, business logic, persistence, and data contracts. Controllers call services; services apply business rules and use repositories for MongoDB access.

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
| ASP.NET Core OpenAPI | OpenAPI document generation in Development |
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
│   ├── CustomerService.cs
│   ├── AccountService.cs
│   ├── TransactionService.cs
│   ├── TokenService.cs
│   └── Interfaces/
│       ├── ICustomerService.cs
│       ├── IAccountService.cs
│       └── ITransactionService.cs
├── Repositories/
│   ├── CustomerRepository.cs
│   ├── AccountRepository.cs
│   ├── TransactionRepository.cs
│   └── Interfaces/
│       ├── ICustomerRepository.cs
│       ├── IAccountRepository.cs
│       └── ITransactionRepository.cs
├── DatabaseSetting/
│   └── MongoDbSettings.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── BankingLedgerAPI.http
├── BankingLedgerAPI.slnx
└── BankingLedgerAPI.csproj
```

### Responsibilities

- **Controllers:** Handle HTTP routes and authorization, call services, and translate results or known business exceptions into HTTP responses.
- **Services:** Apply business rules, validate operation-specific constraints, map models to response DTOs, and call repositories.
- **Repositories:** Encapsulate MongoDB collection queries and writes for customers, accounts, and transactions.
- **Service and repository interfaces:** Define layer contracts and support dependency injection.
- **Models:** Represent customers, bank accounts, and ledger transactions.
- **DTOs:** Define the data accepted by and returned from API endpoints.
- **TokenService:** Creates signed JWT access tokens after customer registration or successful login.
- **Program.cs:** Configure MongoDB connectivity, dependency injection, JWT authentication, authorization, OpenAPI, and middleware.

### Request and data flow

```text
HTTP request
  -> ASP.NET Core middleware (JWT authentication and authorization)
  -> Controller (route, DTO validation, HTTP status)
  -> Service (business rules and response mapping)
  -> Repository (MongoDB collection operations)
  -> Service and controller return the response
```

Registration and login are the exception to the protected-request path: they are anonymous, and the customer service hashes or verifies the password before `TokenService` issues a JWT. For account and transaction operations, the service uses repositories to verify records and apply business rules. A transaction changes the account balance first and then inserts its ledger record; these are separate database operations, not a single MongoDB transaction.

## 4. Installation and Setup

### Prerequisites

Install the following tools:

- .NET 10 SDK
- MongoDB Community Server or MongoDB Atlas
- Visual Studio, Visual Studio Code, or another compatible IDE
- Postman or another HTTP client for API testing

### Step 1: Clone the repository

```bash
git clone <your-repository-url>
cd BankingLedgerAPI
```

Replace `<your-repository-url>` with your Git repository URL.

### Step 2: Configure MongoDB

Configure the `MongoDbSettings` section and JWT settings. `Program.cs` requires a MongoDB connection string and database name, a JWT secret key of at least 32 UTF-8 bytes, and a JWT issuer and audience. Access-token lifetime is controlled by `Jwt:AccessTokenMinutes`.

Example:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017/?retryWrites=false",
    "DatabaseName": "BankingLedgerDb"
  },
  "Jwt": {
    "SecretKey": "<at-least-32-byte-secret>",
    "Issuer": "BankingLedgerAPI",
    "Audience": "BankingLedgerAPIClient",
    "AccessTokenMinutes": 15
  }
}
```

Do not commit real credentials or production secrets. Treat values in checked-in configuration as local-development settings and override the JWT secret and MongoDB connection details with secure values before deployment. For local development, store overrides in .NET user secrets or environment variables. Environment-variable names use double underscores, for example `MongoDbSettings__ConnectionString`, `MongoDbSettings__DatabaseName`, `Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`, and `Jwt__AccessTokenMinutes`.

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

Check the terminal output for the application URL. In Development, the application exposes its OpenAPI document at `/openapi/v1.json`. Swagger UI is not configured by the current application.

For example, if the application reports `https://localhost:7000`:

```text
https://localhost:7000/openapi/v1.json
```

Replace the example host and port with the URL reported by your application.

## 5. API Documentation

The API exposes customer, account, and transaction endpoints. Routes are case-insensitive in ASP.NET Core; the paths below match the controller route templates.

### Base URL

```text
https://localhost:<port>
```

Use the actual HTTP or HTTPS URL shown when the application starts.

### 5.1 Customer APIs

Customer registration and login are anonymous. Registration returns a bearer token immediately. The remaining customer endpoints require authentication, and listing or deleting customers requires the `Admin` role.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/customer/register` | Register a customer and return a JWT |
| POST | `/api/customer/login` | Authenticate a customer and return a JWT |
| GET | `/api/customer` | Retrieve all customers (`Admin`) |
| GET | `/api/customer/{id}` | Retrieve a customer by ID (authenticated) |
| DELETE | `/api/customer/{id}` | Delete a customer (`Admin`) |

Registration request:

```http
POST /api/customer/register
Content-Type: application/json
```

```json
{
  "name": "Sample Customer",
  "email": "customer@example.com",
  "password": "example-password",
  "phone": "9876543210",
  "dateOfBirth": "1990-01-01T00:00:00Z"
}
```

Login accepts `email` and `password`. Both registration and login return an access token, token type, expiration time, customer ID, email, and role. A duplicate registration returns `409 Conflict`; invalid credentials return `401 Unauthorized`.

For protected endpoints, send the token in the request header:

```http
Authorization: Bearer <access-token>
```

Customer and account IDs are MongoDB ObjectIds represented as strings.

### 5.2 Bank Account APIs

Account creation requires an authenticated user. Getting an individual account or a customer's accounts allows the `Customer` or `Admin` role. Listing all accounts and closing accounts requires the `Admin` role.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/account` | Create a bank account (authenticated) |
| GET | `/api/account` | Retrieve all accounts (`Admin`) |
| GET | `/api/account/{id}` | Retrieve an account by ID (`Customer` or `Admin`) |
| GET | `/api/account/customer/{customerId}` | Retrieve a customer's accounts (`Customer` or `Admin`) |
| DELETE | `/api/account/{id}` | Close an account (`Admin`) |

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

**Supported account types:** `Savings` and `Current`.

The API validates the request DTO; the service verifies that the customer ID is a valid ObjectId and that the customer exists. It generates an account number and creates an active account in INR with the requested initial balance. A negative initial deposit is rejected.

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

The endpoint changes an account's status to `Closed`; it does not delete the record. The balance must be zero. A non-zero balance returns `409 Conflict`; a missing, invalid, or already closed account returns `404 Not Found`.

### 5.3 Transaction APIs

Transaction endpoints support credits, debits, and transaction history. Credit and debit require authentication; history and transaction lookup allow the `Customer` or `Admin` role.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/transaction/credit/{accountId}` | Credit an account (authenticated) |
| POST | `/api/transaction/debit/{accountId}` | Debit an account (authenticated) |
| GET | `/api/transaction/account/{accountId}` | Retrieve an account's transaction history (`Customer` or `Admin`) |
| GET | `/api/transaction/{transactionId}` | Retrieve a transaction by ID (`Customer` or `Admin`) |

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

1. Validate the account ID and require a positive amount with no more than two decimal places.
2. Confirm that the account exists and is active.
3. Atomically increase the account balance if it is still active.
4. Insert a `Credit` ledger transaction containing the credit details and resulting balance.
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

1. Validate the account ID and require a positive amount with no more than two decimal places.
2. Confirm that the account exists and is active.
3. Verify that sufficient funds are available.
4. Atomically decrease the account balance only if it is still active and has sufficient funds.
5. Insert a `Debit` ledger transaction containing the withdrawal and resulting balance.
6. Return the transaction response.

Example: If the account balance is ₹1,500 before the withdrawal, withdrawing ₹200 results in a balance of ₹1,300.

A withdrawal must not be allowed to make the account balance negative.

#### Retrieve account transaction history

```http
GET /api/transaction/account/{accountId}
```

Returns the transactions associated with the specified account, sorted by creation time in descending order so that the newest transactions appear first. Amounts are positive; the `type` is `Credit` or `Debit`.

#### Retrieve a transaction by ID

```http
GET /api/transaction/{transactionId}
```

Returns the details of a specific transaction or a not-found response when the transaction does not exist.

### 5.4 Common HTTP Status Codes

| Status Code | Meaning |
|---|---|
| `200 OK` | Request completed successfully |
| `201 Created` | Account created |
| `400 Bad Request` | Invalid ID, invalid input, or failed request validation |
| `404 Not Found` | Customer, account, or transaction not found |
| `409 Conflict` | Business rule prevents the operation, such as insufficient funds |
| `401 Unauthorized` | Missing/invalid token or invalid login credentials |
| `403 Forbidden` | Authenticated user does not have the required role |
| `204 No Content` | Customer deleted or account closed |
| `500 Internal Server Error` | Unexpected server-side failure |

Invalid identifiers may produce `400 Bad Request` or `404 Not Found`, depending on the endpoint's implementation.

## 6. Database Design

The application uses MongoDB, a document-oriented NoSQL database. Data is organized into collections containing BSON documents.

The primary collections are:

1. `Customers`
2. `Accounts`
3. `Transactions`

The repositories access the `Customers`, `Accounts`, and `Transactions` collections.

### 6.1 Customers Collection

**Purpose:** Store customer information and identify customers who own bank accounts.

Example document:

```json
{
  "_id": "CUSTOMER_OBJECT_ID",
  "name": "Sample Customer",
  "email": "customer@example.com",
  "password": "<password-hash>",
  "phone": "9876543210",
  "role": "Customer",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "createdAt": "2026-10-28T09:00:00Z"
}
```

The password is stored as a hash; the customer response DTO does not include it.

| Field | Description |
|---|---|
| `_id` | MongoDB document identifier |
| `name`, `email`, `phone` | Customer profile details |
| `password` | Password hash (never returned by the customer response DTO) |
| `role` | Authorization role, set to `Customer` during registration |
| `dateOfBirth` | Customer date of birth |
| `createdAt` | Customer creation timestamp |

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
  "type": "Credit",
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
| `type` | Transaction type: `Credit` or `Debit` |
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

MongoDB does not enforce these references as relational foreign keys by default. The services validate referenced customer and account records when creating accounts and processing transactions; no database foreign-key constraint is configured.

### Relationship Summary

| Relationship | Type | Reference |
|---|---|---|
| Customer → Account | One-to-many | `accounts.customerId` |
| Account → Transaction | One-to-many | `transactions.accountId` |

## 8. Financial Transaction Workflow

### Credit workflow

1. The client submits a deposit request.
2. The API validates the request.
3. The service retrieves and validates the account.
4. The repository atomically increments the active account balance and returns the updated account.
5. The service inserts a `Credit` ledger record using that balance.
6. The response is returned.

### Debit workflow

1. The client submits a withdrawal request.
2. The API validates the request.
3. The service verifies the account status and available balance.
4. The repository atomically decrements the balance only if the account is active and has sufficient funds.
5. The service inserts a `Debit` ledger record using the updated balance.
6. The response is returned.

The balance update and ledger insertion are separate MongoDB operations; the current implementation does not use MongoDB sessions or multi-document transactions. Consequently, if the ledger insert fails after a successful balance update, the balance and transaction history can become inconsistent. Production financial use requires an atomic design, such as a MongoDB transaction on a deployment that supports transactions, plus appropriate retry and reconciliation handling.

## 9. Validation and Business Rules

The application enforces the following rules where implemented:

- MongoDB document IDs and customer/account references are ObjectIds exposed by the API as strings; malformed IDs may be rejected or treated as not found depending on the endpoint.
- An account must belong to an existing customer when created.
- Account types must be `Savings` or `Current`.
- Deposits and withdrawals must be greater than zero.
- Monetary amounts must not contain more than two decimal places.
- Transactions require a valid account.
- Deposits and withdrawals are allowed only on active accounts.
- Withdrawals cannot exceed the available balance.
- Accounts can be closed only when their balance is zero.
- Registration validates name, email, password, and phone format using request DTO annotations.
- Credit and debit descriptions are required and limited to 200 characters.
- Account creation and transaction endpoints require JWT authentication; other endpoints have role restrictions as listed above.
- Although endpoints restrict access by authentication and role, they do not currently verify that a `Customer` owns the requested customer record, account, or transaction.

## 10. API Testing

Use Postman or another HTTP client to verify the main workflows. In Development, `/openapi/v1.json` provides the OpenAPI document; the project does not currently configure Swagger UI.

Recommended test sequence:

1. Register a customer and save the returned access token and customer ID.
2. Send the bearer token with protected requests.
3. Create an account for the customer and save the returned account ID.
4. Credit ₹500, then debit ₹200.
5. Retrieve the account and transaction history; verify the balance and newest-first transaction order.
6. Attempt a debit greater than the available balance and verify that it fails without changing the balance.
7. To test administrator-only endpoints such as closing an account or listing all accounts, use a token whose role is `Admin`. The public registration endpoint always creates the `Customer` role; administrator provisioning is not provided by the API.

Example balance calculation:

| Operation | Amount | Expected Balance |
|---|---:|---:|
| Initial Credit | ₹1,000 | ₹1,000 |
| Credit | ₹500 | ₹1,500 |
| Debit | ₹200 | ₹1,300 |

## 11. Security and Production Considerations

The API currently issues JWTs on registration and login and applies role-based authorization to routes. Before using it for real banking or production financial operations, address the following limitations:

- Ownership checks on customer, account, and transaction endpoints.
- A controlled administrator provisioning process.
- Secure management of database credentials.
- Unique indexes for account numbers and customer email addresses.
- Idempotency keys to prevent duplicate deposits or withdrawals after client retries.
- Atomic handling of account balance updates and ledger inserts, with robust transaction retry and reconciliation.
- Centralized exception handling and structured logging.
- Audit trails and monitoring.
- Automated unit and integration tests.
- Rate limiting and production configuration management.

The current ledger design is suitable for a learning project or proof of concept. A production banking system requires additional accounting controls, reconciliation, and security review.

## 12. Future Enhancements

Potential improvements include:

- Fund transfers between accounts.
- Paginated transaction history with date filters.
- Account statements and downloadable reports.
- Multi-factor authentication and account recovery workflows.
- Search by account number and transaction type.
- Transaction reference numbers and idempotency.
- Unit and integration testing automation.
- Deployment using Docker and a cloud-hosted database.

## 13. License

Add your chosen license here, for example, the MIT License, if you intend to distribute the project under that license.
