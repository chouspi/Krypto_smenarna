CREATE OR REPLACE PROCEDURE ExecuteExchange (
    p_user_id             IN  users.user_id%TYPE,
    p_from_currency_code  IN  currencies.currency_code%TYPE,
    p_to_currency_code    IN  currencies.currency_code%TYPE,
    p_from_amount         IN  wallets.balance%TYPE,
    p_expected_rate_id    IN  exchange_rates.rate_id%TYPE,

    p_transaction_id      OUT exchange_transactions.transaction_id%TYPE,
    p_exchange_rate       OUT exchange_transactions.exchange_rate%TYPE,
    p_to_amount           OUT exchange_transactions.to_amount%TYPE,
    p_message             OUT VARCHAR2
)
AS
    v_max_amount      CONSTANT NUMBER(18, 8) := 9999999999.99999999;
    v_from_code       currencies.currency_code%TYPE;
    v_to_code         currencies.currency_code%TYPE;
    v_from_wallet_id  wallets.wallet_id%TYPE;
    v_to_wallet_id    wallets.wallet_id%TYPE;
    v_from_balance    wallets.balance%TYPE;
    v_to_balance      wallets.balance%TYPE;
    v_from_is_crypto  currencies.is_crypto%TYPE;
    v_to_is_crypto    currencies.is_crypto%TYPE;
    v_pair_id         trading_pairs.pair_id%TYPE;
    v_rate_id         exchange_rates.rate_id%TYPE;
    v_is_reversed     NUMBER;
    v_valid_from      exchange_rates.valid_from%TYPE;
    v_valid_to        exchange_rates.valid_to%TYPE;
BEGIN
    IF p_from_amount IS NULL OR p_from_amount <= 0 THEN
        RAISE_APPLICATION_ERROR(-20009, 'Částka musí být větší než 0.');
    END IF;

    v_from_code := UPPER(TRIM(p_from_currency_code));
    v_to_code := UPPER(TRIM(p_to_currency_code));

    SELECT w.wallet_id, w.balance, c.is_crypto
    INTO v_from_wallet_id, v_from_balance, v_from_is_crypto
    FROM wallets w
    JOIN currencies c ON c.currency_code = w.currency_code
    WHERE w.user_id = p_user_id
      AND w.currency_code = v_from_code
    FOR UPDATE OF w.balance;

    SELECT w.wallet_id, w.balance, c.is_crypto
    INTO v_to_wallet_id, v_to_balance, v_to_is_crypto
    FROM wallets w
    JOIN currencies c ON c.currency_code = w.currency_code
    WHERE w.user_id = p_user_id
      AND w.currency_code = v_to_code
    FOR UPDATE OF w.balance;

    IF v_from_is_crypto = v_to_is_crypto THEN
        RAISE_APPLICATION_ERROR(-20010, 'Směna musí být mezi fiat měnou a kryptoměnou.');
    END IF;

    IF v_from_balance < p_from_amount THEN
        RAISE_APPLICATION_ERROR(-20011, 'Nedostatečný zůstatek.');
    END IF;

    FindTradingPair(v_from_code, v_to_code, v_pair_id, v_is_reversed, p_message);

    IF v_pair_id IS NULL THEN
        RAISE_APPLICATION_ERROR(-20012, 'Trading pair neexistuje.');
    END IF;

    EnsureValidExchangeRate(v_pair_id, v_rate_id, p_exchange_rate, v_valid_from, v_valid_to);

    IF v_is_reversed = 0 THEN
        p_to_amount := ROUND(p_from_amount * p_exchange_rate, 8);
    ELSE
        p_to_amount := ROUND(p_from_amount / p_exchange_rate, 8);
    END IF;

    IF p_to_amount <= 0 OR p_to_amount > v_max_amount OR v_to_balance + p_to_amount > v_max_amount THEN
        RAISE_APPLICATION_ERROR(-20014, 'Výsledná částka je mimo povolený rozsah.');
    END IF;

    INSERT INTO exchange_transactions (
        user_id, pair_id, from_currency_code, to_currency_code,
        from_amount, exchange_rate, to_amount, status
    )
    VALUES (
        p_user_id, v_pair_id, v_from_code, v_to_code,
        p_from_amount, p_exchange_rate, p_to_amount, 'DONE'
    )
    RETURNING transaction_id INTO p_transaction_id;

    UPDATE wallets SET balance = balance - p_from_amount WHERE wallet_id = v_from_wallet_id;
    UPDATE wallets SET balance = balance + p_to_amount WHERE wallet_id = v_to_wallet_id;

    INSERT INTO wallet_operations (wallet_id, transaction_id, operation_type, amount)
    VALUES (
        v_from_wallet_id,
        p_transaction_id,
        CASE WHEN v_from_is_crypto = 0 THEN 'BUY_FIAT_OUT' ELSE 'SELL_CRYPTO_OUT' END,
        p_from_amount
    );

    INSERT INTO wallet_operations (wallet_id, transaction_id, operation_type, amount)
    VALUES (
        v_to_wallet_id,
        p_transaction_id,
        CASE WHEN v_to_is_crypto = 1 THEN 'BUY_CRYPTO_IN' ELSE 'SELL_FIAT_IN' END,
        p_to_amount
    );

    p_message := 'OK';
    COMMIT;

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/
