# Projekt kryptoměnové směnárny

## Databázové systémy II

**Akademický rok:** 2025/2026  
**Autor:** Samuel Kratoš

# Specifikace programu

Primární úlohou programu je uživatelský portál kryptoměnové směnárny. Program umožuje uživateli zobrazit aktuální fiat zůstatek, zobrazit zůstatek vybrané kryptoměny, provádět vklady a výběry fiat měny, provádět vklady a výběry kryptoměny a nakoupit kryptoměnu za fiat měnu

Důležitou funkcí programu je také zobrazení historie transakcí. V historii se evidují informace o typu operace, použité měně, částce, kurzu a datu provedení operace.

Program také umožňuje pracovat s datovým modelem podporovaných měn a směnných kurzů, které jsou použity při nákupu kryptoměny.

# Datový model

<img src="images/datovyModel.png" alt="Datový model" width="600">



## Seznam funkcí

### F1 GetFiatBalance(p_user_id, p_fiat_code, p_balance)

Procedura vrátí aktuální fiat zůstatek uživatele pro zadanou fiat měnu.  
Ve formuláři se používá hlavně pro zobrazení CZK účtu.  
Výsledný zůstatek je vrácen pomocí výstupního parametru `p_balance`.

### F2 Deposit(p_user_id, p_currency_code, p_amount)

Transakce provede vklad zadané měny na peněženku uživatele.  
Procedura je společná pro fiat měny i kryptoměny. Navýší zůstatek odpovídající peněženky a zapíše záznam do tabulky `wallet_operations` s typem operace `DEPOSIT`.

