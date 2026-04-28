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
        RAISE_APPLICATION_ERROR(-20001, 'Uživatel má více fiat peněženek pro stejnou měnu.');

    WHEN OTHERS THEN
        RAISE;
END;
/