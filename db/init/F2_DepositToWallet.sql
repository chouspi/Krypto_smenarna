CREATE OR REPLACE PROCEDURE DepositToWallet (
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