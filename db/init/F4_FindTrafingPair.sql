CREATE OR REPLACE PROCEDURE FindTradingPair (
    p_base_currency_code  IN trading_pairs.base_currency_code%TYPE,
    p_quote_currency_code IN trading_pairs.quote_currency_code%TYPE,
    p_pair_id             OUT trading_pairs.pair_id%TYPE,
    p_is_reversed         OUT NUMBER,
    p_message             OUT VARCHAR2
)
IS
BEGIN
    p_pair_id := NULL;
    p_is_reversed := NULL;
    p_message := NULL;

    -- Pár se hledá v obou směrech, aby šel stejný kurz použít i inverzně.
    SELECT
        pair_id,
        CASE
            WHEN base_currency_code = UPPER(p_base_currency_code)
             AND quote_currency_code = UPPER(p_quote_currency_code)
            THEN 0
            ELSE 1
        END
    INTO p_pair_id, p_is_reversed
    FROM trading_pairs
    WHERE (
            base_currency_code = UPPER(p_base_currency_code)
        AND quote_currency_code = UPPER(p_quote_currency_code)
    )
    OR (
            base_currency_code = UPPER(p_quote_currency_code)
        AND quote_currency_code = UPPER(p_base_currency_code)
    );

    p_message := 'Trading pair nalezen.';

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_pair_id := NULL;
        p_is_reversed := NULL;
        p_message := 'Trading pair nebyl nalezen.';

    WHEN TOO_MANY_ROWS THEN
        p_pair_id := NULL;
        p_is_reversed := NULL;
        p_message := 'Nalezeno více odpovídajících trading pairs. Data jsou nejednoznačná.';

    WHEN OTHERS THEN
        p_pair_id := NULL;
        p_is_reversed := NULL;
        p_message := 'Chyba při hledání trading pair: ' || SQLERRM;
END;
/
