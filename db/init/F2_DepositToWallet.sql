CREATE OR REPLACE PROCEDURE DepositToWallet (
    p_user_id        IN wallets.user_id%TYPE,
    p_currency_code  IN wallets.currency_code%TYPE,
    p_amount         IN wallets.balance%TYPE
)
AS
    v_wallet_id       wallets.wallet_id%TYPE;
    v_operation_type  wallet_operations.operation_type%TYPE;
BEGIN
    IF p_amount <= 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Částka musí být větší než 0.');
    END IF;

    SELECT
        w.wallet_id,
        CASE WHEN c.is_crypto = 1 THEN 'CRYPTO_DEPOSIT' ELSE 'FIAT_DEPOSIT' END
    INTO v_wallet_id, v_operation_type
    FROM wallets w
    JOIN currencies c ON c.currency_code = w.currency_code
    WHERE w.user_id = p_user_id
      AND w.currency_code = UPPER(TRIM(p_currency_code))
    FOR UPDATE OF w.balance;

    UPDATE wallets
    SET balance = balance + p_amount
    WHERE wallet_id = v_wallet_id;

    INSERT INTO wallet_operations (wallet_id, operation_type, amount)
    VALUES (v_wallet_id, v_operation_type, p_amount);

    COMMIT;

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
/
