CREATE OR REPLACE FUNCTION GetTransactionsForXDays (
    p_user_id IN users.user_id%TYPE,
    p_days    IN NUMBER
)
RETURN SYS_REFCURSOR
IS
    v_result SYS_REFCURSOR;
BEGIN
    IF p_days <= 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Počet dní musí být větší než 0.');
    END IF;

    OPEN v_result FOR
        SELECT
            'EXCHANGE' AS source_type,
            t.transaction_time AS event_time,
            t.status AS status,
            t.from_currency_code AS from_currency_code,
            t.to_currency_code AS to_currency_code,
            t.from_amount AS from_amount,
            t.to_amount AS to_amount,
            t.exchange_rate AS exchange_rate
        FROM exchange_transactions t
        WHERE t.user_id = p_user_id
          AND t.transaction_time >= CAST(SYSTIMESTAMP AS TIMESTAMP) - NUMTODSINTERVAL(p_days, 'DAY')

        UNION ALL

        SELECT
            o.operation_type AS source_type,
            o.operation_time AS event_time,
            'DONE' AS status,
            w.currency_code AS from_currency_code,
            NULL AS to_currency_code,
            o.amount AS from_amount,
            NULL AS to_amount,
            NULL AS exchange_rate
        FROM wallet_operations o
        JOIN wallets w ON w.wallet_id = o.wallet_id
        WHERE w.user_id = p_user_id
          AND o.transaction_id IS NULL
          AND o.operation_time >= CAST(SYSTIMESTAMP AS TIMESTAMP) - NUMTODSINTERVAL(p_days, 'DAY')

        ORDER BY event_time DESC;

    RETURN v_result;
END;
/