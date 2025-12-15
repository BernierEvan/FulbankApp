using FulbankApp.Helpers;
using FulbankApp.Models;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace FulbankApp.Services
{
    /// <summary>
    /// Service d'authentification centralisé
    /// </summary>
    public class AuthenticationService
    {
        private readonly IuserRepository _userRepository;

        public AuthenticationService(IuserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Crée un nouveau compte utilisateur
        /// </summary>
        public bool CreateAccount(string name, string lastName, string email, SecureString password, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(lastName))
            {
                errorMessage = "Le nom et le prénom sont requis";
                return false;
            }

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                errorMessage = "L'adresse email n'est pas valide";
                return false;
            }

            if (password == null || password.Length < 8)
            {
                errorMessage = "Le mot de passe doit contenir au moins 8 caractères";
                return false;
            }

            // Générer un username unique
            string username = GenerateUsername(name, lastName);

            // Vérifier si l'username existe déjà
            int attempt = 0;
            string baseUsername = username;
            while (_userRepository.GetByUsername(username) != null && attempt < 100)
            {
                attempt++;
                username = $"{baseUsername}{attempt}";
            }

            if (attempt >= 100)
            {
                errorMessage = "Impossible de générer un nom d'utilisateur unique";
                return false;
            }

            // Créer l'utilisateur
            try
            {
                var user = new UserModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    Password = HashPassword(SecureStringToString(password)),
                    Name = name,
                    LastName = lastName,
                    Email = email,
                    Pin = null // Le PIN sera défini à la première connexion
                };

                _userRepository.Add(user);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Erreur lors de la création du compte : {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Authentifie un utilisateur avec username et mot de passe
        /// </summary>
        public bool Login(string username, SecureString password, out string errorMessage, out bool requiresPinSetup)
        {
            errorMessage = string.Empty;
            requiresPinSetup = false;

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Le nom d'utilisateur est requis";
                return false;
            }

            if (password == null || password.Length == 0)
            {
                errorMessage = "Le mot de passe est requis";
                return false;
            }

            try
            {
                var credential = new System.Net.NetworkCredential(username, password);
                bool isValid = _userRepository.AuthenticateUser(credential);

                if (!isValid)
                {
                    errorMessage = "Nom d'utilisateur ou mot de passe incorrect";
                    return false;
                }

                // Vérifier si l'utilisateur a déjà configuré un PIN
                var user = _userRepository.GetByUsername(username);
                requiresPinSetup = string.IsNullOrEmpty(user?.Pin);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Erreur lors de la connexion : {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Configure le code PIN pour un utilisateur
        /// </summary>
        public bool SetupPin(string username, string pin, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (pin == null || pin.Length != Constants.PIN_CODE_LENGTH)
            {
                errorMessage = $"Le code PIN doit contenir exactement {Constants.PIN_CODE_LENGTH} chiffres";
                return false;
            }

            if (!IsNumeric(pin))
            {
                errorMessage = "Le code PIN ne doit contenir que des chiffres";
                return false;
            }

            try
            {
                var user = _userRepository.GetByUsername(username);
                if (user == null)
                {
                    errorMessage = "Utilisateur introuvable";
                    return false;
                }

                user.Pin = HashPassword(pin);
                _userRepository.Edit(user);

                // Sauvegarder localement pour la connexion rapide
                var localAuth = new LocalAuth
                {
                    Username = username,
                    PinHash = user.Pin,
                    HasLoggedBefore = true
                };
                localAuth.Save();

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Erreur lors de la configuration du PIN : {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Authentifie un utilisateur avec son code PIN
        /// </summary>
        public bool LoginWithPin(string pin, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (pin == null || pin.Length != Constants.PIN_CODE_LENGTH)
            {
                errorMessage = "Code PIN invalide";
                return false;
            }

            try
            {
                var localAuth = LocalAuth.Load();
                if (localAuth == null || !localAuth.HasLoggedBefore)
                {
                    errorMessage = "Aucune session locale trouvée";
                    return false;
                }

                string pinHash = HashPassword(pin);
                if (pinHash != localAuth.PinHash)
                {
                    errorMessage = "Code PIN incorrect";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Erreur lors de la connexion avec PIN : {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Génère un username à partir du nom et prénom
        /// Format: prenom.nom (en minuscules, sans accents)
        /// </summary>
        private string GenerateUsername(string name, string lastName)
        {
            string username = $"{name}.{lastName}".ToLower();
            username = RemoveAccents(username);
            username = username.Replace(" ", "");
            return username;
        }

        /// <summary>
        /// Hash un mot de passe ou PIN avec SHA256
        /// </summary>
        private string HashPassword(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Convertit une SecureString en string
        /// </summary>
        private string SecureStringToString(SecureString secureString)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        /// <summary>
        /// Valide une adresse email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Vérifie si une chaîne ne contient que des chiffres
        /// </summary>
        private bool IsNumeric(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Retire les accents d'une chaîne
        /// </summary>
        private string RemoveAccents(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            foreach (char c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                    System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    result.Append(c);
                }
            }

            return result.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}