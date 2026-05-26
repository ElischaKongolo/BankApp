# BankApp - Online Banking Application

A full-featured online banking API built with ASP.NET Core 5.0, Entity Framework Core, and JWT authentication.

## Features

- **User Management**: Register, login, change password with secure password hashing (BCrypt)
- **Account Management**: Create checking/savings/business accounts, view balances, account status
- **Transactions**: Deposit, withdraw, transfer between accounts
- **Security**: JWT authentication, role-based authorization
- **API Documentation**: Swagger/OpenAPI integration

## Project Structure

```
BankApp/
├── BankApp.Core/           # Domain models and interfaces
│   ├── Models/             # User, Account, Transaction entities
│   └── Interfaces/         # Service contracts
├── BankApp.Infrastructure/ # Data access and services
│   ├── Data/               # DbContext
│   └── Services/           # Business logic implementation
└── BankApp.Api/            # Web API controllers
    ├── Controllers/        # Auth, Accounts, Transactions
    └── Models/             # DTOs
```

## Prerequisites

- .NET 5.0 SDK
- SQL Server (LocalDB or full instance)

## Getting Started

1. **Clone and navigate to the project**
   ```powershell
   cd C:\Users\User\BankApp
   ```

2. **Build the project**
   ```powershell
   dotnet build
   ```

3. **Run the application**
   ```powershell
   cd BankApp.Api
   dotnet run
   ```

4. **Access the API**
   - API Base URL: `https://localhost:5001/api`
   - Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and receive JWT token
- `POST /api/auth/change-password` - Change password (authenticated)

### Accounts
- `POST /api/accounts` - Create new account
- `GET /api/accounts` - Get user's accounts
- `GET /api/accounts/{id}` - Get specific account
- `GET /api/accounts/number/{accountNumber}` - Get account by number
- `GET /api/accounts/{id}/balance` - Get account balance

### Transactions
- `POST /api/transactions/deposit/{accountId}` - Deposit funds
- `POST /api/transactions/withdraw/{accountId}` - Withdraw funds
- `POST /api/transactions/transfer` - Transfer to another account
- `GET /api/transactions/account/{accountId}` - Get account transactions
- `GET /api/transactions/my-transactions` - Get all user's transactions

## Testing the API

### 1. Register a User
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "password": "SecurePass123!",
    "phoneNumber": "+1234567890",
    "dateOfBirth": "1990-01-01"
  }'
```

### 2. Login
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  }'
```
Response will include a JWT token - save it for subsequent requests.

### 3. Create an Account
```bash
curl -X POST "https://localhost:5001/api/accounts" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{ "accountType": 0 }'
```
Account types: 0 = Checking, 1 = Savings, 2 = Business

### 4. Make a Deposit
```bash
curl -X POST "https://localhost:5001/api/transactions/deposit/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "amount": 1000.00,
    "description": "Initial deposit"
  }'
```

### 5. Transfer Funds
```bash
curl -X POST "https://localhost:5001/api/transactions/transfer" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "fromAccountId": 1,
    "toAccountNumber": "ACC12345678",
    "amount": 500.00,
    "description": "Rent payment"
  }'
```

## Configuration

Edit `appsettings.json` to customize:

- **Database Connection**: Update `ConnectionStrings:DefaultConnection`
- **JWT Settings**: Update `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`

## Database

The application uses Entity Framework Core with SQL Server. The database is automatically created and migrated on startup using LocalDB by default.

To use a different SQL Server instance, update the connection string in `appsettings.json`.
