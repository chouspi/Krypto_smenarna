CREATE OR REPLACE FUNCTION GetWalletOperationsForDays (
    p_user_id IN users.user_id%TYPE,
    p_days    IN NUMBER
)
RETURN SYS_REFCURSOR
IS
    v_result SYS_REFCURSOR;
BEGIN
    IF p_user_id <= 0 THEN
        RAISE_APPLICATION_ERROR(-20006, 'ID uživatele musí být větší než 0.');
    END IF;

    IF p_days <= 0 THEN
        RAISE_APPLICATION_ERROR(-20007, 'Počet dní musí být větší než 0.');
    END IF;

    OPEN v_result FOR
        SELECT
            o.operation_id,
            o.wallet_id,
            w.user_id,
            w.currency_code,
            o.transaction_id,
            o.operation_type,
            o.amount,
            o.operation_time
        FROM wallet_operations o
        JOIN wallets w ON w.wallet_id = o.wallet_id
        WHERE w.user_id = p_user_id
          AND o.operation_time >= CAST(SYSTIMESTAMP AS TIMESTAMP) - NUMTODSINTERVAL(p_days, 'DAY')
        ORDER BY o.operation_time DESC;

    RETURN v_result;
END;
/
