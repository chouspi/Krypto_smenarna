
------------------------------------------------------------
-- USERS
------------------------------------------------------------

INSERT INTO users (user_id, email, hash_of_password, full_name)
VALUES (1, 'alice.novakova@example.com', 'hash_alice_123', 'Alice Nováková');

INSERT INTO users (user_id, email, hash_of_password, full_name)
VALUES (2, 'petr.svoboda@example.com', 'hash_petr_123', 'Petr Svoboda');

INSERT INTO users (user_id, email, hash_of_password, full_name)
VALUES (3, 'jana.dvorakova@example.com', 'hash_jana_123', 'Jana Dvořáková');

INSERT INTO users (user_id, email, hash_of_password, full_name)
VALUES (4, 'tomas.prochazka@example.com', 'hash_tomas_123', 'Tomáš Procházka');

INSERT INTO users (user_id, email, hash_of_password, full_name)
VALUES (5, 'eva.cerna@example.com', 'hash_eva_123', 'Eva Černá');


------------------------------------------------------------
-- CURRENCIES
-- 4 crypto + 2 fiat
------------------------------------------------------------

INSERT INTO currencies (currency_code, name, is_crypto)
VALUES ('BTC', 'Bitcoin', 1);

INSERT INTO currencies (currency_code, name, is_crypto)
VALUES ('ETH', 'Ethereum', 1);

INSERT INTO currencies (currency_code, name, is_crypto)
VALUES ('SOL', 'Solana', 1);

INSERT INTO currencies (currency_code, name, is_crypto)
VALUES ('ADA', 'Cardano', 1);

INSERT INTO currencies (currency_code, name, is_crypto)
VALUES ('USD', 'US Dollar', 0);

INSERT INTO currencies (currency_code, name, is_crypto)
VALUES ('EUR', 'Euro', 0);


------------------------------------------------------------
-- TRADING PAIRS
-- crypto/fiat pairs
------------------------------------------------------------

INSERT INTO trading_pairs (pair_id, base_currency_code, quote_currency_code)
VALUES (1, 'BTC', 'USD');

INSERT INTO trading_pairs (pair_id, base_currency_code, quote_currency_code)
VALUES (2, 'ETH', 'USD');

INSERT INTO trading_pairs (pair_id, base_currency_code, quote_currency_code)
VALUES (3, 'SOL', 'USD');

INSERT INTO trading_pairs (pair_id, base_currency_code, quote_currency_code)
VALUES (4, 'ADA', 'USD');

INSERT INTO trading_pairs (pair_id, base_currency_code, quote_currency_code)
VALUES (5, 'BTC', 'EUR');

INSERT INTO trading_pairs (pair_id, base_currency_code, quote_currency_code)
VALUES (6, 'ETH', 'EUR');

INSERT INTO trading_pairs (pair_id, base_currency_code, quote_currency_code)
VALUES (7, 'SOL', 'EUR');

INSERT INTO trading_pairs (pair_id, base_currency_code, quote_currency_code)
VALUES (8, 'ADA', 'EUR');


------------------------------------------------------------
-- WALLETS
-- každý user má 4 crypto wallets + 2 fiat wallets
-- wallet_operations zůstane prázdné
------------------------------------------------------------

INSERT ALL
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (1, 1, 'BTC', 0.12500000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (2, 1, 'ETH', 1.50000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (3, 1, 'SOL', 25.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (4, 1, 'ADA', 1000.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (5, 1, 'USD', 5000.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (6, 1, 'EUR', 2500.00000000)

  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (7, 2, 'BTC', 0.05000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (8, 2, 'ETH', 0.75000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (9, 2, 'SOL', 10.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (10, 2, 'ADA', 500.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (11, 2, 'USD', 1500.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (12, 2, 'EUR', 900.00000000)

  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (13, 3, 'BTC', 0.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (14, 3, 'ETH', 2.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (15, 3, 'SOL', 50.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (16, 3, 'ADA', 2500.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (17, 3, 'USD', 8000.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (18, 3, 'EUR', 3000.00000000)

  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (19, 4, 'BTC', 0.30000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (20, 4, 'ETH', 0.10000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (21, 4, 'SOL', 5.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (22, 4, 'ADA', 100.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (23, 4, 'USD', 300.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (24, 4, 'EUR', 700.00000000)

  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (25, 5, 'BTC', 0.01000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (26, 5, 'ETH', 0.25000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (27, 5, 'SOL', 100.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (28, 5, 'ADA', 750.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (29, 5, 'USD', 12000.00000000)
  INTO wallets (wallet_id, user_id, currency_code, balance) VALUES (30, 5, 'EUR', 6000.00000000)
SELECT 1 FROM dual;


------------------------------------------------------------
-- EXCHANGE RATES
-- jeden kurz pro každý trading pair
-- valid_from = teď
-- valid_to   = teď + 2 minuty
------------------------------------------------------------

INSERT INTO exchange_rates (rate_id, pair_id, rate, valid_from, valid_to)
VALUES (1, 1, 95000.00000000, SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '2' MINUTE);

INSERT INTO exchange_rates (rate_id, pair_id, rate, valid_from, valid_to)
VALUES (2, 2, 3300.00000000, SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '2' MINUTE);

INSERT INTO exchange_rates (rate_id, pair_id, rate, valid_from, valid_to)
VALUES (3, 3, 180.00000000, SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '2' MINUTE);

INSERT INTO exchange_rates (rate_id, pair_id, rate, valid_from, valid_to)
VALUES (4, 4, 0.75000000, SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '2' MINUTE);

INSERT INTO exchange_rates (rate_id, pair_id, rate, valid_from, valid_to)
VALUES (5, 5, 88000.00000000, SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '2' MINUTE);

INSERT INTO exchange_rates (rate_id, pair_id, rate, valid_from, valid_to)
VALUES (6, 6, 3050.00000000, SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '2' MINUTE);

INSERT INTO exchange_rates (rate_id, pair_id, rate, valid_from, valid_to)
VALUES (7, 7, 167.00000000, SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '2' MINUTE);

INSERT INTO exchange_rates (rate_id, pair_id, rate, valid_from, valid_to)
VALUES (8, 8, 0.69000000, SYSTIMESTAMP, SYSTIMESTAMP + INTERVAL '2' MINUTE);

------------------------------------------------------------
-- wallet_operations a exchange_transactions záměrně prázdné
------------------------------------------------------------

COMMIT;