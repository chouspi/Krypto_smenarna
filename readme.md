# Crypto Exchange

A desktop WPF application that simulates a simple cryptocurrency exchange. The user selects an account, manages fiat and crypto wallets, and the application calculates the selected cryptocurrency value using the latest exchange rate from the Oracle database.

## Features

- user selection on application startup
- fiat and crypto wallet overview
- fiat and crypto deposits through Oracle stored procedure `DepositToWallet`
- fiat and crypto withdrawals through Oracle stored procedure `WithdrawFromWallet`
- input validation for wallet operations before calling the database
- crypto value conversion to the selected fiat currency
- exchange rates loaded from an Oracle database
- automatic creation of a new simulated exchange rate after the previous one expires
- dashboard split into three user subwindows: wallets, transaction history, and exchange
- test data insert and delete actions through development buttons in the application

Transaction history and exchange forms are currently prepared in the UI as separate subwindows, but their full user workflows are not implemented yet.

## Requirements

- Windows
- .NET 9 SDK
- Docker Desktop
- Oracle database started through `compose.yaml`

## Setup

After cloning the repository, run:

```powershell
.\setup.ps1
```

The script creates `.env` from `.env.example` if it does not exist and starts Oracle through Docker Compose.

## Configuration

The repository does not include `.env`, because it contains local database credentials. If you do not use `setup.ps1`, create it from the provided example:

```powershell
Copy-Item .env.example .env
```

Default values in `.env.example`:

```env
ORACLE_PASSWORD=OracleTest123
APP_USER=APP
APP_USER_PASSWORD=AppTest123
```

On startup, the application searches for `.env` from the application output directory up to the project root.

## Database Setup

Start the database with Docker Compose manually:

```powershell
docker compose up -d
```

Oracle runs on port `1521`, and initialization SQL scripts are loaded from `db/init`.

If a Docker volume already exists and the initialization scripts need to run again, recreate the database:

```powershell
docker compose down -v
docker compose up -d
```

This deletes the local database data.

## Running The Application

Restore packages first:

```powershell
dotnet restore Krypto_smenarna.sln
```

Then run the WPF project:

```powershell
dotnet run --project src/KryptoSmenarna.Wpf/KryptoSmenarna.Wpf.csproj
```

After the first database startup, you may need to use the `Vlozit testovaci data` button in the application to insert users and wallets.

## Project Structure

```text
Krypto_smenarna/
|- db/
|  |- init/    database tables, procedures, and indexes
|  `- test/    scripts for inserting and deleting test data
|- docs/       notes and school project analysis
|- src/
|  `- KryptoSmenarna.Wpf/
|     |- Data/             Oracle repositories and database access
|     |- Models/           application data models
|     `- UserSubWindows/   wallet, transaction history, and exchange UI sections
|- compose.yaml
`- Krypto_smenarna.sln
```

## Main User Window

`UserWindow` is a dashboard shell. It hosts three WPF user controls from `UserSubWindows`:

- `WalletOperationsSubWindow` contains the implemented fiat/crypto deposit and withdrawal form.
- `TransactionHistorySubWindow` is prepared for transaction history.
- `ExchangeSubWindow` is prepared for fiat-to-crypto exchange.

The dashboard layout keeps wallets on the left side and places transaction history above exchange on the right side.

## Database Scripts

- `db/init/01_dbInit.sql` creates the base tables and indexes.
- `db/init/F1_GetFiatBalance.sql` returns a user's fiat balance.
- `db/init/F2_DepositToWallet.sql` deposits funds into a wallet.
- `db/init/F3_WithdrawFromWallet.sql` withdraws funds from a wallet.
- `db/init/F4_FindTrafingPair.sql` finds a trading pair in both currency directions.
- `db/init/GetLastExchangeRate.sql` returns the latest known exchange rate.
- `db/init/F6_GetTransactionFroXDays.sql` returns exchange transactions and wallet operations for a selected time range.
- `db/test/insertRestValues.sql` inserts test users, currencies, wallets, and exchange rates.
- `db/test/deleteAllData.sql` deletes test data.

## Wallet Operations

Wallet deposits and withdrawals are intentionally performed through database procedures instead of direct balance updates in the UI:

- `WalletsRepository.Deposit(...)` calls `DepositToWallet`.
- `WalletsRepository.TryWithdraw(...)` calls `WithdrawFromWallet`.

The procedures update wallet balances and insert records into `wallet_operations`. The UI reloads the affected wallet from the database after a successful operation.

## Notes

- `.env` is ignored by git and must exist locally.
- The application uses the Oracle connection target `localhost:1521/FREEPDB1`.
- The project uses `Oracle.ManagedDataAccess.Core`.

