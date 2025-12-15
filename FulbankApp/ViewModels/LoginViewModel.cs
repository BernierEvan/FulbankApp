using FulbankApp.Helpers;
using FulbankApp.Models;
using FulbankApp.Repositories;
using FulbankApp.Services;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace FulbankApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Fields

        private readonly AuthenticationService _authService;
        private string _currentUsername; // Pour stocker le username lors du setup du PIN

        #endregion

        #region Énumérations

        public enum AuthMode
        {
            Login,          // Mode connexion classique
            Register,       // Mode création de compte
            PinLogin,       // Mode connexion avec PIN
            PinSetup        // Mode configuration du PIN
        }

        private AuthMode _currentMode;

        #endregion

        #region Properties - Mode et Visibilité

        public AuthMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (SetProperty(ref _currentMode, value))
                {
                    UpdateVisibility();
                }
            }
        }

        private bool _isLoginFormVisible;
        public bool IsLoginFormVisible
        {
            get => _isLoginFormVisible;
            set => SetProperty(ref _isLoginFormVisible, value);
        }

        private bool _isRegisterFormVisible;
        public bool IsRegisterFormVisible
        {
            get => _isRegisterFormVisible;
            set => SetProperty(ref _isRegisterFormVisible, value);
        }

        private bool _isPinPadVisible;
        public bool IsPinPadVisible
        {
            get => _isPinPadVisible;
            set => SetProperty(ref _isPinPadVisible, value);
        }

        private bool _isHaloVisible;
        public bool IsHaloVisible
        {
            get => _isHaloVisible;
            set => SetProperty(ref _isHaloVisible, value);
        }

        private bool _areButtonsVisible;
        public bool AreButtonsVisible
        {
            get => _areButtonsVisible;
            set => SetProperty(ref _areButtonsVisible, value);
        }

        #endregion

        #region Properties - Formulaire de Login

        private string _loginUsername = string.Empty;
        public string LoginUsername
        {
            get => _loginUsername;
            set => SetProperty(ref _loginUsername, value);
        }

        private SecureString _loginPassword;
        public SecureString LoginPassword
        {
            get => _loginPassword;
            set => SetProperty(ref _loginPassword, value);
        }

        #endregion

        #region Properties - Formulaire d'Inscription

        private string _registerName = string.Empty;
        public string RegisterName
        {
            get => _registerName;
            set => SetProperty(ref _registerName, value);
        }

        private string _registerLastName = string.Empty;
        public string RegisterLastName
        {
            get => _registerLastName;
            set => SetProperty(ref _registerLastName, value);
        }

        private string _registerEmail = string.Empty;
        public string RegisterEmail
        {
            get => _registerEmail;
            set => SetProperty(ref _registerEmail, value);
        }

        private SecureString _registerPassword;
        public SecureString RegisterPassword
        {
            get => _registerPassword;
            set => SetProperty(ref _registerPassword, value);
        }

        private SecureString _registerPasswordConfirm;
        public SecureString RegisterPasswordConfirm
        {
            get => _registerPasswordConfirm;
            set => SetProperty(ref _registerPasswordConfirm, value);
        }

        #endregion

        #region Properties - PIN

        private string _currentPin = string.Empty;
        private string _pinDisplay = string.Empty;
        public string PinDisplay
        {
            get => _pinDisplay;
            set => SetProperty(ref _pinDisplay, value);
        }

        private string _pinInstructions = string.Empty;
        public string PinInstructions
        {
            get => _pinInstructions;
            set => SetProperty(ref _pinInstructions, value);
        }

        #endregion

        #region Properties - Messages et Erreurs

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string _successMessage = string.Empty;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        #endregion

        #region Properties - Skin

        private string _currentSkin;
        public string CurrentSkin
        {
            get => _currentSkin;
            set => SetProperty(ref _currentSkin, value);
        }

        private List<string> _availableSkins;
        public List<string> AvailableSkins
        {
            get => _availableSkins;
            set => SetProperty(ref _availableSkins, value);
        }

        #endregion

        #region Properties - Navigation

        private bool _isViewVisible = true;
        public bool IsViewVisible
        {
            get => _isViewVisible;
            set => SetProperty(ref _isViewVisible, value);
        }

        #endregion

        #region Commands

        public ICommand ShowLoginFormCommand { get; }
        public ICommand ShowRegisterFormCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand PinButtonCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand EnterSkinSelectionCommand { get; }
        public ICommand ChangeSkinCommand { get; }

        #endregion

        #region Events

        public event Action RequestResetZoom;
        public event Action<string> RequestSkinChangeAnimation;
        public event Action RequestSkinPanelDisplay;

        #endregion

        #region Constructor

        public LoginViewModel()
        {
            // Initialisation du service d'authentification
            _authService = new AuthenticationService(new UserRepository());

            // Initialisation des skins
            _availableSkins = new List<string>
            {
                Constants.DEFAULT_MALE_SKIN,
                Constants.DEFAULT_FEMALE_SKIN,
                Constants.THREE_PIECE_MAN_SKIN,
                Constants.SAD_EMPLOYEE_SKIN
            };
            _currentSkin = Constants.DEFAULT_MALE_SKIN;

            // Initialisation des commandes
            ShowLoginFormCommand = new RelayCommand(ShowLoginForm);
            ShowRegisterFormCommand = new RelayCommand(ShowRegisterForm);
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            PinButtonCommand = new RelayCommand<string>(HandlePinInput);
            BackCommand = new RelayCommand(GoBack);
            EnterSkinSelectionCommand = new RelayCommand(EnterSkinSelection);
            ChangeSkinCommand = new RelayCommand<string>(ChangeSkin);

            // Vérifier si l'utilisateur s'est déjà connecté (mode PIN)
            var localAuth = LocalAuth.Load();
            if (localAuth != null && localAuth.HasLoggedBefore)
            {
                LoginUsername = localAuth.Username;
                CurrentMode = AuthMode.PinLogin;
                PinInstructions = $"Bienvenue {LoginUsername} ! Entrez votre code PIN";
            }
            else
            {
                CurrentMode = AuthMode.Login;
            }

            // Charger le skin sauvegardé
            try
            {
                if (Application.Current?.Properties.Contains("SelectedSkin") == true)
                {
                    var stored = Application.Current.Properties["SelectedSkin"] as string;
                    if (!string.IsNullOrWhiteSpace(stored))
                    {
                        CurrentSkin = stored;
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Méthodes - Navigation entre modes

        private void ShowLoginForm()
        {
            CurrentMode = AuthMode.Login;
            ClearMessages();
        }

        private void ShowRegisterForm()
        {
            CurrentMode = AuthMode.Register;
            ClearMessages();
        }

        private void GoBack()
        {
            _currentPin = string.Empty;
            PinDisplay = string.Empty;
            RequestResetZoom?.Invoke();

            var localAuth = LocalAuth.Load();
            if (localAuth != null && localAuth.HasLoggedBefore)
            {
                CurrentMode = AuthMode.PinLogin;
            }
            else
            {
                CurrentMode = AuthMode.Login;
            }
            ClearMessages();
        }

        private void UpdateVisibility()
        {
            IsLoginFormVisible = CurrentMode == AuthMode.Login;
            IsRegisterFormVisible = CurrentMode == AuthMode.Register;
            IsPinPadVisible = CurrentMode == AuthMode.PinLogin || CurrentMode == AuthMode.PinSetup;
            IsHaloVisible = CurrentMode == AuthMode.Login || CurrentMode == AuthMode.Register;
            AreButtonsVisible = CurrentMode == AuthMode.Login || CurrentMode == AuthMode.Register;
        }

        #endregion

        #region Méthodes - Login

        private bool CanExecuteLogin()
        {
            return !string.IsNullOrWhiteSpace(LoginUsername) &&
                   LoginPassword != null &&
                   LoginPassword.Length >= 3;
        }

        private void ExecuteLogin()
        {
            ClearMessages();

            bool success = _authService.Login(
                LoginUsername,
                LoginPassword,
                out string errorMsg,
                out bool requiresPinSetup
            );

            if (success)
            {
                _currentUsername = LoginUsername;

                if (requiresPinSetup)
                {
                    // Première connexion : configurer le PIN
                    CurrentMode = AuthMode.PinSetup;
                    PinInstructions = "Première connexion ! Créez un code PIN à 8 chiffres";
                    SuccessMessage = "Connexion réussie !";
                }
                else
                {
                    // Utilisateur déjà enregistré avec PIN
                    CompleteLogin();
                }
            }
            else
            {
                ErrorMessage = errorMsg;
            }
        }

        #endregion

        #region Méthodes - Inscription

        private bool CanExecuteRegister()
        {
            return !string.IsNullOrWhiteSpace(RegisterName) &&
                   !string.IsNullOrWhiteSpace(RegisterLastName) &&
                   !string.IsNullOrWhiteSpace(RegisterEmail) &&
                   RegisterPassword != null &&
                   RegisterPassword.Length >= 8 &&
                   RegisterPasswordConfirm != null;
        }

        private void ExecuteRegister()
        {
            ClearMessages();

            // Vérifier que les mots de passe correspondent
            if (!SecureStringsEqual(RegisterPassword, RegisterPasswordConfirm))
            {
                ErrorMessage = "Les mots de passe ne correspondent pas";
                return;
            }

            bool success = _authService.CreateAccount(
                RegisterName,
                RegisterLastName,
                RegisterEmail,
                RegisterPassword,
                out string errorMsg
            );

            if (success)
            {
                // Générer le username pour affichage
                string username = $"{RegisterName}.{RegisterLastName}".ToLower();
                SuccessMessage = $"Compte créé avec succès ! Votre nom d'utilisateur est : {username}";

                // Basculer vers le login après 2 secondes
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    LoginUsername = username;
                    ShowLoginForm();
                };
                timer.Start();
            }
            else
            {
                ErrorMessage = errorMsg;
            }
        }

        /// <summary>
        /// Compare deux SecureString
        /// </summary>
        private bool SecureStringsEqual(SecureString ss1, SecureString ss2)
        {
            if (ss1 == null || ss2 == null)
                return false;

            if (ss1.Length != ss2.Length)
                return false;

            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;

            try
            {
                bstr1 = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(ss1);
                bstr2 = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(ss2);

                int length = System.Runtime.InteropServices.Marshal.ReadInt32(bstr1, -4);

                for (int i = 0; i < length; ++i)
                {
                    byte b1 = System.Runtime.InteropServices.Marshal.ReadByte(bstr1, i);
                    byte b2 = System.Runtime.InteropServices.Marshal.ReadByte(bstr2, i);
                    if (b1 != b2)
                        return false;
                }

                return true;
            }
            finally
            {
                if (bstr1 != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(bstr1);
                if (bstr2 != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(bstr2);
            }
        }

        #endregion

        #region Méthodes - Gestion du PIN

        public void HandlePinInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return;

            ClearMessages();

            switch (input)
            {
                case "←": // Backspace
                    if (_currentPin.Length > 0)
                    {
                        _currentPin = _currentPin[..^1];
                        PinDisplay = new string('●', _currentPin.Length);
                    }
                    break;

                case "X": // Clear
                    _currentPin = string.Empty;
                    PinDisplay = string.Empty;
                    break;

                default: // Chiffre
                    if (_currentPin.Length < Constants.PIN_CODE_LENGTH)
                    {
                        _currentPin += input;
                        PinDisplay = new string('●', _currentPin.Length);

                        // Valider automatiquement quand le PIN est complet
                        if (_currentPin.Length == Constants.PIN_CODE_LENGTH)
                        {
                            ValidatePin();
                        }
                    }
                    break;
            }
        }

        private void ValidatePin()
        {
            if (CurrentMode == AuthMode.PinSetup)
            {
                // Configuration d'un nouveau PIN
                bool success = _authService.SetupPin(_currentUsername, _currentPin, out string errorMsg);

                if (success)
                {
                    SuccessMessage = "Code PIN configuré avec succès !";

                    // Attendre un peu puis se connecter
                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        CompleteLogin();
                    };
                    timer.Start();
                }
                else
                {
                    ErrorMessage = errorMsg;
                    ResetPinInput();
                }
            }
            else if (CurrentMode == AuthMode.PinLogin)
            {
                // Connexion avec PIN existant
                bool success = _authService.LoginWithPin(_currentPin, out string errorMsg);

                if (success)
                {
                    var localAuth = LocalAuth.Load();
                    _currentUsername = localAuth?.Username;
                    CompleteLogin();
                }
                else
                {
                    ErrorMessage = errorMsg;
                    ResetPinInput();
                }
            }
        }

        private void ResetPinInput()
        {
            _currentPin = string.Empty;
            PinDisplay = string.Empty;
        }

        #endregion

        #region Méthodes - Finalisation de la connexion

        private void CompleteLogin()
        {
            // Définir le principal de sécurité
            Thread.CurrentPrincipal = new GenericPrincipal(
                new GenericIdentity(_currentUsername),
                null
            );

            // Masquer la vue de login
            IsViewVisible = false;
        }

        #endregion

        #region Méthodes - Gestion des skins

        private void EnterSkinSelection()
        {
            IsLoginFormVisible = false;
            IsRegisterFormVisible = false;
            IsHaloVisible = false;
            IsPinPadVisible = false;
            AreButtonsVisible = false;
            RequestSkinPanelDisplay?.Invoke();
        }

        private void ChangeSkin(string newSkinName)
        {
            if (string.IsNullOrWhiteSpace(newSkinName))
                return;

            if (newSkinName != CurrentSkin)
            {
                CurrentSkin = newSkinName;

                try
                {
                    if (Application.Current != null)
                        Application.Current.Properties["SelectedSkin"] = newSkinName;
                }
                catch { }

                RequestSkinChangeAnimation?.Invoke(newSkinName);
            }
        }

        #endregion

        #region Méthodes - Utilitaires

        private void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        #endregion
    }
}