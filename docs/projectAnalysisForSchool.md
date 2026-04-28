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

```sql
CREATE OR REPLACE PROCEDURE GetFiatBalance (
    p_user_id    IN  wallets.user_id%TYPE,
    p_fiat_code  IN  wallets.currency_code%TYPE,
    p_balance    OUT wallets.balance%TYPE
)
AS
BEGIN
    SELECT w.balance
    INTO p_balance
    FROM wallets w
    JOIN currencies c ON c.currency_code = w.currency_code
    WHERE w.user_id = p_user_id
      AND w.currency_code = p_fiat_code
      AND c.is_crypto = 0;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_balance := 0;

    WHEN TOO_MANY_ROWS THEN
        RAISE_APPLICATION_ERROR(-20001, 'Uživatel má více peněženek pro stejnou fiat měnu.');
END;
/

### F2 Deposit(p_user_id, p_currency_code, p_amount)

Transakce provede vklad zadané měny na peněženku uživatele.  
Procedura je společná pro fiat měny i kryptoměny. Navýší zůstatek odpovídající peněženky a zapíše záznam do tabulky `wallet_operations` s typem operace `DEPOSIT`.

```sql
CREATE OR REPLACE PROCEDURE Deposit (
    p_user_id        IN wallets.user_id%TYPE,
    p_currency_code  IN wallets.currency_code%TYPE,
    p_amount         IN wallets.balance%TYPE
)
AS
    v_wallet_id wallets.wallet_id%TYPE;
BEGIN
    IF p_amount <= 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Částka vkladu musí být větší než 0.');
    END IF;

    SELECT wallet_id
    INTO v_wallet_id
    FROM wallets
    WHERE user_id = p_user_id
      AND currency_code = p_currency_code
    FOR UPDATE;

    UPDATE wallets
    SET balance = balance + p_amount
    WHERE wallet_id = v_wallet_id;

    INSERT INTO wallet_operations (
        wallet_id,
        operation_type,
        amount,
        operation_time
    )
    VALUES (
        v_wallet_id,
        'DEPOSIT',
        p_amount,
        SYSTIMESTAMP
    );

    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK;
        RAISE_APPLICATION_ERROR(-20002, 'Peněženka pro zadanou měnu neexistuje.');

    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/
