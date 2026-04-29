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

### F2 DepositToWallet(p_user_id, p_currency_code, p_amount)

Transakce provede vklad zadané měny na peněženku uživatele.  
Procedura je společná pro fiat měny i kryptoměny. Navýší zůstatek odpovídající peněženky a zapíše záznam do tabulky `wallet_operations` s typem operace `DEPOSIT`.

### T2 Withdraw(p_user_id, p_currency_code, p_amount)

Transakční procedura provede výběr zadané měny z peněženky uživatele.  
Je společná pro fiat měny i kryptoměny.

Procedura nejprve ověří, že částka výběru je větší než 0. Poté najde odpovídající peněženku uživatele a zkontroluje, zda má uživatel dostatečný zůstatek. Pokud je zůstatek dostatečný, sníží zůstatek peněženky a zapíše záznam do tabulky `wallet_operations` s typem operace `WITHDRAWAL`.

### F3 FindTradingPair(p_base_currency_code, p_quote_currency_code, p_pair_id, p_is_reversed, p_message)

Procedura vyhledá obchodní pár v tabulce `trading_pairs` podle zadaných kódů měn.  
Na vstupu přijímá kód základní měny a kód kotační měny. Pořadí zadaných měn není důležité, protože procedura kontroluje obě možné kombinace.

Procedura se používá před provedením směny kryptoměny za fiat měnu nebo fiat měny za kryptoměnu. Jejím cílem je zjistit, zda pro zadanou dvojici měn existuje podporovaný obchodní pár, a vrátit jeho `pair_id`.

Vstupní parametry:

- `p_base_currency_code` – kód první měny, například `BTC`
- `p_quote_currency_code` – kód druhé měny, například `USD`

Výstupní parametry:

- `p_pair_id` – ID nalezeného obchodního páru
- `p_is_reversed` – určuje, zda byl pár nalezen v opačném pořadí
  - `0` znamená, že pořadí odpovídá záznamu v tabulce `trading_pairs`
  - `1` znamená, že uživatel zadal měny v opačném pořadí
- `p_message` – textová zpráva s výsledkem operace

