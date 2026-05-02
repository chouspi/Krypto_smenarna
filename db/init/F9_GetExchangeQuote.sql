CREATE OR REPLACE PROCEDURE GetExchangeQuote (
    p_user_id             IN  users.user_id%TYPE,
    p_from_currency_code  IN  currencies.currency_code%TYPE,
    p_to_currency_code    IN  currencies.currency_code%TYPE,
    p_from_amount         IN  wallets.balance%TYPE,

    p_pair_id             OUT trading_pairs.pair_id%TYPE,
    p_rate_id             OUT exchange_rates.rate_id%TYPE,
    p_exchange_rate       OUT exchange_rates.rate%TYPE,
    p_is_reversed         OUT NUMBER,
    p_to_amount           OUT wallets.balance%TYPE,
    p_message             OUT VARCHAR2
)
AS
    v_max_amount      CONSTANT NUMBER(18, 8) := 9999999999.99999999;
    v_from_code       currencies.currency_code%TYPE;
    v_to_code         currencies.currency_code%TYPE;
    v_from_is_crypto  currencies.is_crypto%TYPE;
    v_to_is_crypto    currencies.is_crypto%TYPE;
    v_from_balance    wallets.balance%TYPE;
    v_to_balance      wallets.balance%TYPE;
    v_valid_from      exchange_rates.valid_from%TYPE;
    v_valid_to        exchange_rates.valid_to%TYPE;
BEGIN
    IF p_from_amount IS NULL OR p_from_amount <= 0 THEN
        RAISE_APPLICATION_ERROR(-20004, 'Částka musí být větší než 0.');
    END IF;

    v_from_code := UPPER(TRIM(p_from_currency_code));
    v_to_code := UPPER(TRIM(p_to_currency_code));

    SELECT c.is_crypto, w.balance
    INTO v_from_is_crypto, v_from_balance
    FROM wallets w
    JOIN currencies c ON c.currency_code = w.currency_code
    WHERE w.user_id = p_user_id
      AND w.currency_code = v_from_code;

    SELECT c.is_crypto, w.balance
    INTO v_to_is_crypto, v_to_balance
    FROM wallets w
    JOIN currencies c ON c.currency_code = w.currency_code
    WHERE w.user_id = p_user_id
      AND w.currency_code = v_to_code;

    IF v_from_is_crypto = v_to_is_crypto THEN
        RAISE_APPLICATION_ERROR(-20005, 'Směna musí být mezi fiat měnou a kryptoměnou.');
    END IF;

    IF v_from_balance < p_from_amount THEN
        RAISE_APPLICATION_ERROR(-20006, 'Nedostatečný zůstatek.');
    END IF;

    FindTradingPair(v_from_code, v_to_code, p_pair_id, p_is_reversed, p_message);

    IF p_pair_id IS NULL THEN
        RAISE_APPLICATION_ERROR(-20007, 'Trading pair neexistuje.');
    END IF;

    EnsureValidExchangeRate(p_pair_id, p_rate_id, p_exchange_rate, v_valid_from, v_valid_to);

    IF p_is_reversed = 0 THEN
        p_to_amount := ROUND(p_from_amount * p_exchange_rate, 8);
    ELSE
        p_to_amount := ROUND(p_from_amount / p_exchange_rate, 8);
    END IF;

    IF p_to_amount <= 0 OR p_to_amount > v_max_amount OR v_to_balance + p_to_amount > v_max_amount THEN
        RAISE_APPLICATION_ERROR(-20008, 'Výsledná částka je mimo povolený rozsah.');
    END IF;

    p_message := 'OK';
    COMMIT;
END;
/
