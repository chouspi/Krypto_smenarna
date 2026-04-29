# Crypto Exchange

A desktop WPF application that simulates a simple cryptocurrency exchange. The user selects an account, views fiat and crypto wallets, and the application calculates the selected cryptocurrency value using the latest exchange rate from the database.

## Features

- user selection on application startup
- fiat and crypto wallet overview
- crypto value conversion to the selected fiat currency
- exchange rates loaded from an Oracle database
- automatic creation of a new simulated exchange rate after the previous one expires
- test data insert and delete actions through development buttons in the application

## Requirements

- Windows
- .NET 9 SDK
- Docker Desktop
- Oracle database started through `compose.yaml`

## Configuration

The repository does not include `.env`, because it contains local database credentials. Create it from the provided example:

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

Start the database with Docker Compose:

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
|- src/        WPF application source code
|- compose.yaml
`- Krypto_smenarna.sln
```

## Database Scripts

- `db/init/01_dbInit.sql` creates the base tables and indexes.
- `db/init/F1_GetFiatBalance.sql` returns a user's fiat balance.
- `db/init/F2_DepositToWallet.sql` deposits funds into a wallet.
- `db/init/F3_WithdrawFromWallet.sql` withdraws funds from a wallet.
- `db/init/F4_FindTrafingPair.sql` finds a trading pair in both currency directions.
- `db/init/GetLastExchangeRate.sql` returns the latest known exchange rate.
- `db/test/insertRestValues.sql` inserts test users, currencies, wallets, and exchange rates.
- `db/test/deleteAllData.sql` deletes test data.

## Notes

- `.env` is ignored by git and must exist locally.
- The application uses the Oracle connection target `localhost:1521/FREEPDB1`.
- The project uses `Oracle.ManagedDataAccess.Core`.

