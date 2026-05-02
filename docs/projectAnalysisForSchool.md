# Projekt kryptoměnové směnárny

## Databázové systémy II

**Akademický rok:** 2025/2026  
**Autor:** Samuel Kratoš

# Specifikace programu

Primární úlohou programu je uživatelský portál kryptoměnové směnárny. Program umožňuje vybrat uživatele, zobrazit jeho fiat a kryptoměnové peněženky, provádět vklady a výběry fiat měn i kryptoměn a zobrazit orientační hodnotu vybrané kryptoměny ve vybrané fiat měně podle aktuálního kurzu z databáze.

Hlavní uživatelské okno je rozděleno na tři samostatné části:

- peněženky, vklady a výběry
- transakční historie
- směna fiat měny za kryptoměnu podle kurzu

Aktuálně je plně implementována část peněženek. Část transakční historie a část směny jsou v uživatelském rozhraní připravené jako samostatné formuláře, ale jejich kompletní workflow bude doplněno následně.

Program pracuje s databází Oracle. Vklady a výběry nejsou prováděny přímou změnou zůstatku v aplikaci, ale přes databázové procedury. Databáze tím zajišťuje konzistentní úpravu zůstatku a zároveň zapisuje historii operací.

# Datový model

<img src="images/datovyModel.png" alt="Datový model" width="600">

Základní tabulky datového modelu:

- `users` ukládá uživatele aplikace.
- `currencies` ukládá podporované fiat a kryptoměny.
- `wallets` ukládá peněženky uživatelů a jejich zůstatky.
- `wallet_operations` ukládá vklady a výběry z peněženek.
- `trading_pairs` ukládá podporované směnné páry.
- `exchange_rates` ukládá směnné kurzy a jejich časovou platnost.
- `exchange_transactions` ukládá provedené směny mezi měnami.

# Architektura aplikace

Aplikace je vytvořena jako desktopová WPF aplikace v .NET 9. Přístup do databáze je oddělen do repository tříd ve složce `Data`. Uživatelské rozhraní pro hlavní okno je rozdělené do složky `UserSubWindows`.

Hlavní části UI:

- `UserWindow` slouží jako dashboard a pouze skládá jednotlivé části obrazovky.
- `WalletOperationsSubWindow` obsahuje formulář pro fiat a crypto vklady a výběry.
- `TransactionHistorySubWindow` je připravená část pro historii transakcí.
- `ExchangeSubWindow` je připravená část pro směnu fiat měny za kryptoměnu.

Rozložení hlavního okna odpovídá pracovní ploše, kde jsou peněženky vlevo přes celou výšku a pravá část je rozdělena na transakční historii nahoře a směnu dole.

# Validace vstupů

Formulář pro peněženky ověřuje vstupy ještě před voláním databáze:

- částka musí být platné desetinné číslo,
- částka musí být větší než 0,
- částka může mít maximálně 8 desetinných míst,
- částka nesmí překročit rozsah databázového typu `NUMBER(18,8)`,
- při výběru musí mít peněženka dostatečný zůstatek,
- po vkladu nesmí zůstatek překročit maximální povolenou hodnotu.

Po úspěšném vkladu nebo výběru aplikace znovu načte konkrétní peněženku z databáze a aktualizuje hodnoty v UI.

# Seznam databázových funkcí a procedur

## F1 GetFiatBalance(p_user_id, p_fiat_code, p_balance)

Procedura vrátí aktuální fiat zůstatek uživatele pro zadanou fiat měnu. Výsledný zůstatek je vrácen pomocí výstupního parametru `p_balance`. Pokud peněženka není nalezena, vrací se hodnota `0`.

## F2 DepositToWallet(p_user_id, p_currency_code, p_amount)

Transakční procedura provede vklad zadané měny na peněženku uživatele. Procedura je společná pro fiat měny i kryptoměny.

Postup procedury:

- ověří, že částka vkladu je větší než 0,
- najde peněženku uživatele pro zadanou měnu,
- uzamkne řádek peněženky pomocí `FOR UPDATE`,
- navýší zůstatek peněženky,
- vloží záznam do `wallet_operations`,
- provede `COMMIT`.

Typ operace se zapisuje podle typu měny:

- `FIAT_DEPOSIT` pro fiat měnu,
- `CRYPTO_DEPOSIT` pro kryptoměnu.

Při chybě se provede `ROLLBACK`.

## F3 WithdrawFromWallet(p_user_id, p_currency_code, p_amount)

Transakční procedura provede výběr zadané měny z peněženky uživatele. Je společná pro fiat měny i kryptoměny.

Postup procedury:

- ověří, že částka výběru je větší než 0,
- najde peněženku uživatele pro zadanou měnu,
- uzamkne řádek peněženky pomocí `FOR UPDATE`,
- zkontroluje dostatečný zůstatek,
- sníží zůstatek peněženky,
- vloží záznam do `wallet_operations`,
- provede `COMMIT`.

Typ operace se zapisuje podle typu měny:

- `FIAT_WITHDRAWAL` pro fiat měnu,
- `CRYPTO_WITHDRAWAL` pro kryptoměnu.

Při nedostatečném zůstatku nebo jiné chybě se provede `ROLLBACK`.

## F4 FindTradingPair(p_base_currency_code, p_quote_currency_code, p_pair_id, p_is_reversed, p_message)

Procedura vyhledá obchodní pár v tabulce `trading_pairs` podle zadaných kódů měn. Pořadí zadaných měn není důležité, protože procedura kontroluje obě možné kombinace.

Výstupní parametry:

- `p_pair_id` je ID nalezeného obchodního páru,
- `p_is_reversed` určuje, zda byl pár nalezen v opačném pořadí,
- `p_message` obsahuje textovou zprávu s výsledkem.

Pokud je pár nalezen v opačném pořadí, aplikace při přepočtu používá převrácený kurz.

## GetLatestExchangeRate(p_pair_id, p_rate_id, p_rate, p_valid_from, p_valid_to, p_is_valid)

Procedura vrátí poslední známý kurz pro zadaný obchodní pár. Zároveň vrací informaci, zda je kurz aktuálně platný.

Výstupní parametry:

- `p_rate_id` je ID kurzu,
- `p_rate` je hodnota kurzu,
- `p_valid_from` je začátek platnosti,
- `p_valid_to` je konec platnosti,
- `p_is_valid` určuje, zda kurz platí v aktuálním čase.

Pokud aplikace dostane neplatný kurz, vytvoří nový simulovaný kurz v repository vrstvě metodou `MakeNewValid`.

## F6 GetTransactionsForDays(p_user_id, p_days)

Funkce vrací `SYS_REFCURSOR` s transakcemi a peněženkovými operacemi za zadaný počet dní. Výsledek sjednocuje:

- záznamy z `exchange_transactions`,
- záznamy z `wallet_operations`, které nejsou navázané na směnnou transakci.

Výsledek je řazen sestupně podle času události. Funkce ověřuje, že počet dní je větší než 0.

# Repository vrstvy

## WalletsRepository

Třída zajišťuje práci s peněženkami. Obsahuje metody pro načtení seznamu peněženek, načtení konkrétní peněženky, vklad a výběr.

Důležité metody:

- `GetAllWallets(userId, isCrypto)` načte fiat nebo crypto peněženky uživatele,
- `GetWallet(userId, currencyCode)` načte konkrétní peněženku,
- `Deposit(amount, currencyCode, userId)` volá proceduru `DepositToWallet`,
- `TryWithdraw(amount, currencyCode, userId)` volá proceduru `WithdrawFromWallet`.

U částek se při volání Oracle nastavuje `Precision = 18` a `Scale = 8`, aby odpovídaly databázovému typu `NUMBER(18,8)`.

## TradingPairsRepository

Třída volá proceduru `FindTradingPair` a převádí výsledek do C# hodnot. Aplikace díky tomu ví, zda lze vybranou kryptoměnu přepočítat do vybrané fiat měny a zda má použít přímý nebo převrácený kurz.

## ExchangeRateRepository

Třída získává poslední kurz pomocí procedury `GetLatestExchangeRate`. Pokud kurz již není platný, metoda `MakeNewValid` vytvoří nový simulovaný kurz s malou náhodnou změnou a novou časovou platností.

## ExchangeTransactionsRepository

Třída obsahuje metody pro práci se směnnými transakcemi. Aktuálně umí načíst transakce za zadaný počet dní a vložit novou směnnou transakci. Uživatelská část směny zatím není dokončena.

# Aktuální stav implementace

Implementováno:

- výběr uživatele při spuštění aplikace,
- zobrazení fiat a crypto peněženek,
- přepočet hodnoty vybrané kryptoměny do vybrané fiat měny,
- fiat a crypto vklady přes databázovou proceduru,
- fiat a crypto výběry přes databázovou proceduru,
- automatické obnovení neplatného simulovaného kurzu,
- rozdělení hlavního okna na tři samostatné části.

Připraveno pro další doplnění:

- zobrazení transakční historie v UI,
- formulář pro směnu fiat měny za kryptoměnu,
- plné propojení směny s tabulkou `exchange_transactions`.
