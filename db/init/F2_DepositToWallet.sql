CREATE OR REPLACE PROCEDURE DepositToWallet (
    p_user_id        IN wallets.user_id%TYPE,
    p_currency_code  IN wallets.currency_code%TYPE,
    p_amount         IN wallets.balance%TYPE
)
AS
    v_wallet_id wallets.wallet_id%TYPE;
    v_is_crypto currencies.is_crypto%TYPE;
BEGIN
    IF p_amount <= 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Částka vkladu musí být větší než 0.');
    END IF;

    SELECT w.wallet_id, c.is_crypto
    INTO v_wallet_id, v_is_crypto
    FROM wallets w
    JOIN currencies c ON c.currency_code = w.currency_code
    WHERE w.user_id = p_user_id
      AND w.currency_code = p_currency_code
    FOR UPDATE OF w.balance;

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
        CASE
            WHEN v_is_crypto = 1 THEN 'CRYPTO_DEPOSIT'
            ELSE 'FIAT_DEPOSIT'
        END,
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
