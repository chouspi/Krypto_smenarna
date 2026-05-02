CREATE OR REPLACE FUNCTION GetTransactionsForXDays (
    p_user_id IN users.user_id%TYPE,
    p_days    IN NUMBER
)
RETURN SYS_REFCURSOR
IS
    v_result SYS_REFCURSOR;
BEGIN
    OPEN v_result FOR
        SELECT
            'EXCHANGE' AS source_type,
            transaction_time AS event_time,
            status,
            from_currency_code,
            to_currency_code,
            from_amount,
            to_amount,
            exchange_rate
        FROM exchange_transactions
        WHERE user_id = p_user_id
          AND transaction_time >= CAST(SYSTIMESTAMP AS TIMESTAMP) - NUMTODSINTERVAL(p_days, 'DAY')

        UNION ALL

        SELECT
            o.operation_type,
            o.operation_time,
            'DONE',
            w.currency_code,
            CAST(NULL AS VARCHAR2(10)),
            o.amount,
            CAST(NULL AS NUMBER),
            CAST(NULL AS NUMBER)
        FROM wallet_operations o
        JOIN wallets w ON w.wallet_id = o.wallet_id
        WHERE w.user_id = p_user_id
          AND o.transaction_id IS NULL
          AND o.operation_time >= CAST(SYSTIMESTAMP AS TIMESTAMP) - NUMTODSINTERVAL(p_days, 'DAY')

        ORDER BY event_time DESC;

    RETURN v_result;
END;
/
