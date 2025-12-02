using FulbankApp.Helpers;
using FulbankApp.Models;
using FulbankApp.Repositories;
using FulbankApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace FulbankApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Evan

        //Title
        public string PageTitle => "Login";

        //Visibility
        private bool _isLoginVisible = true;
        private bool _isRegisterVisible = true;
        private bool _isHaloVisible = true;
        private bool _isPinPadVisible = false;

        //Pin variables
        private string _currentPin = "";

        private string _pinTextDisplay = string.Empty;
        public string PinTextDisplay
        {
            get { return _pinTextDisplay; }
            set
            {
                _pinTextDisplay = value;
                OnPropertyChanged(nameof(PinTextDisplay));
            }
        }

        //Skin variables
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

        //Visibility Changers
        public bool IsLoginVisible
        {
            get => _isLoginVisible;
            set { _isLoginVisible = value; OnPropertyChanged(nameof(IsLoginVisible)); }
        }

        public bool IsRegisterVisible
        {
            get => _isRegisterVisible;
            set { _isRegisterVisible = value; OnPropertyChanged(nameof(IsRegisterVisible)); }
        }

        public bool IsHaloVisible
        {
            get => _isHaloVisible;
            set { _isHaloVisible = value; OnPropertyChanged(nameof(IsHaloVisible)); }
        }

        public bool IsPinPadVisible
        {
            get => _isPinPadVisible;
            set { _isPinPadVisible = value; OnPropertyChanged(nameof(IsPinPadVisible)); }
        }

        // Commands
        public ICommand HideBothCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand EnterSkinSelection { get; }
        public ICommand ExitSkinSelection { get; }
        public ICommand ExitPinSelection { get; }
        public ICommand ChangeSkinCommand { get; }

        // Events (View can s'abonner)
        public event Action RequestResetZoom;
        // Fournit le nom du skin à la View pour l'animation
        public event Action<string> RequestSkinChangeAnimation;
        public event Action RequestSkinPanelDisplay;

        #endregion

        #region Maxime

        private string _username = string.Empty;
        private SecureString _password;
        private string _errorMessage = string.Empty;
        private bool _isViewVisible = true;
        private IuserRepository userRepository;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public SecureString Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public bool IsViewVisible
        {
            get => _isViewVisible;
            set { _isViewVisible = value; OnPropertyChanged(nameof(IsViewVisible)); }
        }

        // Commands pour auth
        public ICommand LoginCommand { get; set; }
        public ICommand RecoverPasswordCommand { get; set; }
        public ICommand ShowPasswordCommand { get; set; }
        public ICommand RememberPasswordCommand { get; set; }

        private bool _isPinMode;
        public bool IsPinMode
        {
            get => _isPinMode;
            set { _isPinMode = value; OnPropertyChanged(nameof(IsPinMode)); }
        }

        public string Pin { get; set; } = "";
        public ICommand AjouterChiffreCommand { get; set; }

        #endregion

        #region Constructor

        public LoginViewModel()
        {
            // Initialisations pour éviter les null refs/warnings
            _availableSkins = new List<string>();
            _currentSkin = Constants.DEFAULT_MALE_SKIN;

            LoadAvailableSkins();

            HideBothCommand = new RelayCommand(() =>
            {
                IsLoginVisible = false;
                IsRegisterVisible = false;
                IsHaloVisible = false;
                IsPinPadVisible = true;
            });

            EnterSkinSelection = new RelayCommand(() =>
            {
                IsLoginVisible = false;
                IsRegisterVisible = false;
                IsHaloVisible = false;
                IsPinPadVisible = false;
                RequestSkinPanelDisplay?.Invoke();
            });

            BackCommand = new RelayCommand(() =>
            {
                IsPinPadVisible = false;
                IsLoginVisible = true;
                IsRegisterVisible = true;
                IsHaloVisible = true;
                RequestResetZoom?.Invoke();
            });

            ChangeSkinCommand = new RelayCommand<string>(ChangeSkin);

            userRepository = new UserRepository();
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));

            var local = LocalAuth.Load();
            if (local != null && local.HasLoggedBefore)
            {
                Username = local.Username;
                IsPinMode = true;
            }

            // Clavier PIN
            AjouterChiffreCommand = new ViewModelCommand(p => AddDigit(p?.ToString() ?? ""));
        }

        #endregion

        #region PIN Management

        public void HandlePinInput(string input)
        {
            if (input == null) return;

            switch (input)
            {
                case "←":
                    if (_currentPin.Length > 0)
                    {
                        _currentPin = _currentPin[..^1];
                        PinTextDisplay = PinTextDisplay.Length > 0 ? PinTextDisplay[..^1] : "";
                    }
                    break;
                case "X":
                    _currentPin = "";
                    PinTextDisplay = "";
                    break;
                default:
                    _currentPin += input;
                    PinTextDisplay = new string('*', _currentPin.Length);
                    break;
            }

            if (_currentPin.Length == Constants.PIN_CODE_LENGTH)
            {
                ValidatePin(_currentPin);
                _currentPin = "";
                PinTextDisplay = "";
            }
        }

        private void ValidatePin(string pin)
        {
            MessageBox.Show($"Code PIN saisi : {pin}");
        }

        #endregion

        #region SkinManagement

        private void LoadAvailableSkins()
        {
            _availableSkins.Clear();
            _availableSkins.Add(Constants.DEFAULT_MALE_SKIN);
            _availableSkins.Add(Constants.DEFAULT_FEMALE_SKIN);
            _availableSkins.Add(Constants.THREE_PIECE_MAN_SKIN);
            _availableSkins.Add(Constants.SAD_EMPLOYEE_SKIN);
        }

        private void ChangeSkin(string newSkinName)
        {
            if (string.IsNullOrWhiteSpace(newSkinName)) return;

            if (newSkinName != CurrentSkin)
            {
                CurrentSkin = newSkinName;
                RequestSkinChangeAnimation?.Invoke(newSkinName);
            }       
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged(string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }

        #endregion

        #region Auth (Maxime)

        private bool CanExecuteLoginCommand(object obj)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 ||
                Password == null || Password.Length < 3)
            {
                validData = false;
            }
            else
            {
                validData = true;
            }
            return validData;
        }

        private void ExecuteLoginCommand(object obj)
        {
            var isValidUser = userRepository.AuthenticateUser(
                new System.Net.NetworkCredential(Username, Password)
            );

            if (isValidUser)
            {
                if (!IsPinMode)
                {
                    string pin = "1234";
                    var local = new LocalAuth()
                    {
                        Username = Username,
                        PinHash = Hash(pin),
                        HasLoggedBefore = true
                    };
                    local.Save();
                }

                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Username), null);
                IsViewVisible = false;
            }
            else
            {
                ErrorMessage = "* Invalid username or password *";
            }
        }

        private string Hash(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        private void AddDigit(string digit)
        {
            if (digit == null) return;
            Pin += digit;
            OnPropertyChanged(nameof(Pin));
            if (Pin.Length == 4) VerifyPin();
        }

        private void ExecuteRecoverPassCommand(string username, string email)
        {
            throw new NotImplementedException();
        }

        private void VerifyPin()
        {
            var local = LocalAuth.Load();
            if (local == null) return;
            string hash = Hash(Pin);
            if (hash == local.PinHash)
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(local.Username), null);
                IsViewVisible = false;
            }
            else
            {
                ErrorMessage = "Invalid PIN";
                Pin = "";
                OnPropertyChanged(nameof(Pin));
            }
        }

        #endregion
    }
}
