using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using FulbankApp.Models;

namespace FulbankApp.Services
{
    public class SupabaseService
    {
        private static SupabaseService _instance;
        private readonly Supabase.Client _client;
        private User _currentUser;

        public static SupabaseService Instance => _instance ??= new SupabaseService();

        public User CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

        public bool IsAuthenticated => CurrentUser != null;

        private SupabaseService()
        {
            var url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "YOUR_SUPABASE_URL";
            var key = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? "YOUR_SUPABASE_ANON_KEY";

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            _client = new Supabase.Client(url, key, options);
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                await _client.InitializeAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool success, string error)> SignUpAsync(string email, string password, string username, string firstName, string lastName)
        {
            try
            {
                var authResponse = await _client.Auth.SignUp(email, password);

                if (authResponse?.User == null)
                    return (false, "Failed to create account");

                var user = new User
                {
                    Id = Guid.Parse(authResponse.User.Id),
                    Username = username,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    CurrentSkin = "default_male",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _client.From<User>().Insert(user);

                var settings = new UserSettings
                {
                    UserId = user.Id,
                    SmsNotifications = false,
                    EmailNotifications = false,
                    ConnectionAlerts = false,
                    TwoFactorEnabled = false,
                    UpdatedAt = DateTime.UtcNow
                };

                await _client.From<UserSettings>().Insert(settings);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string error)> SignInAsync(string username, string password)
        {
            try
            {
                var userResponse = await _client.From<User>()
                    .Where(x => x.Username == username)
                    .Single();

                if (userResponse == null)
                    return (false, "Invalid username or password");

                var authResponse = await _client.Auth.SignIn(userResponse.Email, password);

                if (authResponse?.User == null)
                    return (false, "Invalid username or password");

                CurrentUser = userResponse;
                return (true, null);
            }
            catch
            {
                return (false, "Invalid username or password");
            }
        }

        public async Task SignOutAsync()
        {
            await _client.Auth.SignOut();
            CurrentUser = null;
        }

        public async Task<List<BankAccount>> GetBankAccountsAsync()
        {
            if (!IsAuthenticated) return new List<BankAccount>();

            var response = await _client.From<BankAccount>()
                .Where(x => x.UserId == CurrentUser.Id)
                .Get();

            return response.Models;
        }

        public async Task<List<CryptoWallet>> GetCryptoWalletsAsync()
        {
            if (!IsAuthenticated) return new List<CryptoWallet>();

            var response = await _client.From<CryptoWallet>()
                .Where(x => x.UserId == CurrentUser.Id)
                .Get();

            return response.Models;
        }

        public async Task<List<Beneficiary>> GetBeneficiariesAsync()
        {
            if (!IsAuthenticated) return new List<Beneficiary>();

            var response = await _client.From<Beneficiary>()
                .Where(x => x.UserId == CurrentUser.Id)
                .Get();

            return response.Models;
        }

        public async Task<List<Transaction>> GetTransactionsAsync(int limit = 50)
        {
            if (!IsAuthenticated) return new List<Transaction>();

            var response = await _client.From<Transaction>()
                .Where(x => x.UserId == CurrentUser.Id)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(limit)
                .Get();

            return response.Models;
        }

        public async Task<UserSettings> GetSettingsAsync()
        {
            if (!IsAuthenticated) return null;

            var response = await _client.From<UserSettings>()
                .Where(x => x.UserId == CurrentUser.Id)
                .Single();

            return response;
        }

        public async Task<bool> AddBeneficiaryAsync(string name, string iban, string note = null)
        {
            if (!IsAuthenticated) return false;

            try
            {
                var beneficiary = new Beneficiary
                {
                    UserId = CurrentUser.Id,
                    Name = name,
                    Iban = iban,
                    Note = note,
                    CreatedAt = DateTime.UtcNow
                };

                await _client.From<Beneficiary>().Insert(beneficiary);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBeneficiaryAsync(Guid id)
        {
            if (!IsAuthenticated) return false;

            try
            {
                await _client.From<Beneficiary>()
                    .Where(x => x.Id == id && x.UserId == CurrentUser.Id)
                    .Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateTransferAsync(Guid accountId, string recipientName, string recipientIban, decimal amount, string description = null)
        {
            if (!IsAuthenticated) return false;

            try
            {
                var transaction = new Transaction
                {
                    UserId = CurrentUser.Id,
                    AccountId = accountId,
                    Type = "transfer",
                    Amount = -amount,
                    Currency = "EUR",
                    RecipientName = recipientName,
                    RecipientIban = recipientIban,
                    Description = description,
                    Status = "completed",
                    CreatedAt = DateTime.UtcNow
                };

                await _client.From<Transaction>().Insert(transaction);

                var account = await _client.From<BankAccount>()
                    .Where(x => x.Id == accountId)
                    .Single();

                account.Balance -= amount;

                await _client.From<BankAccount>().Update(account);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSettingsAsync(UserSettings settings)
        {
            if (!IsAuthenticated) return false;

            try
            {
                settings.UpdatedAt = DateTime.UtcNow;
                await _client.From<UserSettings>().Update(settings);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserSkinAsync(string skinName)
        {
            if (!IsAuthenticated) return false;

            try
            {
                CurrentUser.CurrentSkin = skinName;
                CurrentUser.UpdatedAt = DateTime.UtcNow;
                await _client.From<User>().Update(CurrentUser);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
