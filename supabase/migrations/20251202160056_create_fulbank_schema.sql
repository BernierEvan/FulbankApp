/*
  # Fulbank Application Database Schema

  1. New Tables
    - `users`
      - `id` (uuid, primary key)
      - `username` (text, unique)
      - `email` (text, unique)
      - `password_hash` (text)
      - `first_name` (text)
      - `last_name` (text)
      - `birthdate` (date)
      - `profile_picture_url` (text, nullable)
      - `pin_hash` (text)
      - `current_skin` (text)
      - `created_at` (timestamptz)
      - `updated_at` (timestamptz)

    - `bank_accounts`
      - `id` (uuid, primary key)
      - `user_id` (uuid, foreign key)
      - `account_number` (text, unique)
      - `iban` (text, unique)
      - `account_type` (text)
      - `balance` (decimal)
      - `currency` (text)
      - `created_at` (timestamptz)

    - `crypto_wallets`
      - `id` (uuid, primary key)
      - `user_id` (uuid, foreign key)
      - `crypto_code` (text) -- BTC, ETH, USDT, etc.
      - `amount` (decimal)
      - `wallet_address` (text, nullable)
      - `created_at` (timestamptz)
      - `updated_at` (timestamptz)

    - `beneficiaries`
      - `id` (uuid, primary key)
      - `user_id` (uuid, foreign key)
      - `name` (text)
      - `iban` (text)
      - `note` (text, nullable)
      - `created_at` (timestamptz)

    - `transactions`
      - `id` (uuid, primary key)
      - `user_id` (uuid, foreign key)
      - `account_id` (uuid, foreign key, nullable)
      - `type` (text) -- transfer, payment, crypto_buy, crypto_sell, conversion
      - `amount` (decimal)
      - `currency` (text)
      - `recipient_name` (text, nullable)
      - `recipient_iban` (text, nullable)
      - `description` (text, nullable)
      - `status` (text)
      - `created_at` (timestamptz)

    - `settings`
      - `id` (uuid, primary key)
      - `user_id` (uuid, foreign key, unique)
      - `sms_notifications` (boolean)
      - `email_notifications` (boolean)
      - `connection_alerts` (boolean)
      - `two_factor_enabled` (boolean)
      - `updated_at` (timestamptz)

  2. Security
    - Enable RLS on all tables
    - Add policies for authenticated users to manage their own data
*/

-- Users table
CREATE TABLE IF NOT EXISTS users (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  username text UNIQUE NOT NULL,
  email text UNIQUE NOT NULL,
  password_hash text NOT NULL,
  first_name text NOT NULL,
  last_name text NOT NULL,
  birthdate date,
  profile_picture_url text,
  pin_hash text,
  current_skin text DEFAULT 'default_male',
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);

ALTER TABLE users ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can read own data"
  ON users FOR SELECT
  TO authenticated
  USING (auth.uid() = id);

CREATE POLICY "Users can update own data"
  ON users FOR UPDATE
  TO authenticated
  USING (auth.uid() = id)
  WITH CHECK (auth.uid() = id);

-- Bank accounts table
CREATE TABLE IF NOT EXISTS bank_accounts (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid REFERENCES users(id) ON DELETE CASCADE NOT NULL,
  account_number text UNIQUE NOT NULL,
  iban text UNIQUE NOT NULL,
  account_type text DEFAULT 'checking',
  balance decimal(15, 2) DEFAULT 0.00,
  currency text DEFAULT 'EUR',
  created_at timestamptz DEFAULT now()
);

ALTER TABLE bank_accounts ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view own accounts"
  ON bank_accounts FOR SELECT
  TO authenticated
  USING (
    user_id IN (SELECT id FROM users WHERE auth.uid() = id)
  );

CREATE POLICY "Users can update own accounts"
  ON bank_accounts FOR UPDATE
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id))
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can insert own accounts"
  ON bank_accounts FOR INSERT
  TO authenticated
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

-- Crypto wallets table
CREATE TABLE IF NOT EXISTS crypto_wallets (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid REFERENCES users(id) ON DELETE CASCADE NOT NULL,
  crypto_code text NOT NULL,
  amount decimal(20, 8) DEFAULT 0.00000000,
  wallet_address text,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  UNIQUE(user_id, crypto_code)
);

ALTER TABLE crypto_wallets ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view own crypto wallets"
  ON crypto_wallets FOR SELECT
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can update own crypto wallets"
  ON crypto_wallets FOR UPDATE
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id))
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can insert own crypto wallets"
  ON crypto_wallets FOR INSERT
  TO authenticated
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

-- Beneficiaries table
CREATE TABLE IF NOT EXISTS beneficiaries (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid REFERENCES users(id) ON DELETE CASCADE NOT NULL,
  name text NOT NULL,
  iban text NOT NULL,
  note text,
  created_at timestamptz DEFAULT now()
);

ALTER TABLE beneficiaries ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view own beneficiaries"
  ON beneficiaries FOR SELECT
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can insert own beneficiaries"
  ON beneficiaries FOR INSERT
  TO authenticated
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can update own beneficiaries"
  ON beneficiaries FOR UPDATE
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id))
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can delete own beneficiaries"
  ON beneficiaries FOR DELETE
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

-- Transactions table
CREATE TABLE IF NOT EXISTS transactions (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid REFERENCES users(id) ON DELETE CASCADE NOT NULL,
  account_id uuid REFERENCES bank_accounts(id) ON DELETE SET NULL,
  type text NOT NULL,
  amount decimal(15, 2) NOT NULL,
  currency text DEFAULT 'EUR',
  recipient_name text,
  recipient_iban text,
  description text,
  status text DEFAULT 'completed',
  created_at timestamptz DEFAULT now()
);

ALTER TABLE transactions ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view own transactions"
  ON transactions FOR SELECT
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can insert own transactions"
  ON transactions FOR INSERT
  TO authenticated
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

-- Settings table
CREATE TABLE IF NOT EXISTS settings (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid REFERENCES users(id) ON DELETE CASCADE UNIQUE NOT NULL,
  sms_notifications boolean DEFAULT false,
  email_notifications boolean DEFAULT false,
  connection_alerts boolean DEFAULT false,
  two_factor_enabled boolean DEFAULT false,
  updated_at timestamptz DEFAULT now()
);

ALTER TABLE settings ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view own settings"
  ON settings FOR SELECT
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can update own settings"
  ON settings FOR UPDATE
  TO authenticated
  USING (user_id IN (SELECT id FROM users WHERE auth.uid() = id))
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

CREATE POLICY "Users can insert own settings"
  ON settings FOR INSERT
  TO authenticated
  WITH CHECK (user_id IN (SELECT id FROM users WHERE auth.uid() = id));

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_bank_accounts_user_id ON bank_accounts(user_id);
CREATE INDEX IF NOT EXISTS idx_crypto_wallets_user_id ON crypto_wallets(user_id);
CREATE INDEX IF NOT EXISTS idx_beneficiaries_user_id ON beneficiaries(user_id);
CREATE INDEX IF NOT EXISTS idx_transactions_user_id ON transactions(user_id);
CREATE INDEX IF NOT EXISTS idx_transactions_created_at ON transactions(created_at DESC);
