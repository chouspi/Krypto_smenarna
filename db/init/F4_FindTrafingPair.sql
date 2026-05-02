CREATE OR REPLACE PROCEDURE FindTradingPair (
    p_base_currency_code  IN trading_pairs.base_currency_code%TYPE,
    p_quote_currency_code IN trading_pairs.quote_currency_code%TYPE,
    p_pair_id             OUT trading_pairs.pair_id%TYPE,
    p_is_reversed         OUT NUMBER,
    p_message             OUT VARCHAR2
)
AS
BEGIN
    SELECT
        pair_id,
        CASE
            WHEN base_currency_code = UPPER(TRIM(p_base_currency_code)) THEN 0
            ELSE 1
        END
    INTO p_pair_id, p_is_reversed
    FROM trading_pairs
    WHERE (base_currency_code = UPPER(TRIM(p_base_currency_code))
       AND quote_currency_code = UPPER(TRIM(p_quote_currency_code)))
       OR (base_currency_code = UPPER(TRIM(p_quote_currency_code))
       AND quote_currency_code = UPPER(TRIM(p_base_currency_code)))
    FETCH FIRST 1 ROW ONLY;

    p_message := 'OK';

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_pair_id := NULL;
        p_is_reversed := NULL;
        p_message := NULL;
END;
/
