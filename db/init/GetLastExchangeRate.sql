CREATE OR REPLACE PROCEDURE GetLatestExchangeRate (
    p_pair_id     IN exchange_rates.pair_id%TYPE,
    p_rate_id     OUT exchange_rates.rate_id%TYPE,
    p_rate        OUT exchange_rates.rate%TYPE,
    p_valid_from  OUT exchange_rates.valid_from%TYPE,
    p_valid_to    OUT exchange_rates.valid_to%TYPE,
    p_is_valid    OUT NUMBER
)
IS
BEGIN
    p_rate_id := NULL;
    p_rate := NULL;
    p_valid_from := NULL;
    p_valid_to := NULL;
    p_is_valid := 0;

    -- Vrací poslední známý kurz; p_is_valid říká, jestli je ještě aktuální.
    SELECT
        rate_id,
        rate,
        valid_from,
        valid_to,
        CASE
            WHEN valid_from <= SYSTIMESTAMP
             AND (valid_to IS NULL OR valid_to > SYSTIMESTAMP)
            THEN 1
            ELSE 0
        END
    INTO
        p_rate_id,
        p_rate,
        p_valid_from,
        p_valid_to,
        p_is_valid
    FROM exchange_rates
    WHERE pair_id = p_pair_id
    ORDER BY valid_from DESC
    FETCH FIRST 1 ROW ONLY;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_rate_id := NULL;
        p_rate := NULL;
        p_valid_from := NULL;
        p_valid_to := NULL;
        p_is_valid := 0;

    WHEN OTHERS THEN
        p_rate_id := NULL;
        p_rate := NULL;
        p_valid_from := NULL;
        p_valid_to := NULL;
        p_is_valid := 0;
        RAISE;
END;
/
