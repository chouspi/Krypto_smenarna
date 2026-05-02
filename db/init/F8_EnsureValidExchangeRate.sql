CREATE OR REPLACE PROCEDURE EnsureValidExchangeRate (
    p_pair_id     IN  exchange_rates.pair_id%TYPE,
    p_rate_id     OUT exchange_rates.rate_id%TYPE,
    p_rate        OUT exchange_rates.rate%TYPE,
    p_valid_from  OUT exchange_rates.valid_from%TYPE,
    p_valid_to    OUT exchange_rates.valid_to%TYPE
)
AS
    v_now  exchange_rates.valid_from%TYPE;
BEGIN
    v_now := CAST(SYSTIMESTAMP AS TIMESTAMP);

    BEGIN
        SELECT rate_id, rate, valid_from, valid_to
        INTO p_rate_id, p_rate, p_valid_from, p_valid_to
        FROM exchange_rates
        WHERE pair_id = p_pair_id
          AND valid_from <= v_now
          AND (valid_to IS NULL OR valid_to > v_now)
        ORDER BY valid_from DESC
        FETCH FIRST 1 ROW ONLY;

        RETURN;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            NULL;
    END;

    SELECT rate_id, rate, valid_from, valid_to
    INTO p_rate_id, p_rate, p_valid_from, p_valid_to
    FROM exchange_rates
    WHERE pair_id = p_pair_id
      AND valid_from <= v_now
    ORDER BY valid_from DESC
    FETCH FIRST 1 ROW ONLY;

    UPDATE exchange_rates
    SET valid_to = v_now
    WHERE pair_id = p_pair_id
      AND valid_from < v_now
      AND (valid_to IS NULL OR valid_to > v_now);

    p_rate := ROUND(GREATEST(p_rate * (1 + DBMS_RANDOM.VALUE(-0.05, 0.05)), 0.00000001), 8);
    p_valid_from := v_now;
    p_valid_to := v_now + INTERVAL '2' MINUTE;

    INSERT INTO exchange_rates (pair_id, rate, valid_from, valid_to)
    VALUES (p_pair_id, p_rate, p_valid_from, p_valid_to)
    RETURNING rate_id INTO p_rate_id;
END;
/
