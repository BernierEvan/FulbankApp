using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FulbankApp.ViewModels 
{
    // Cette classe implémente l'interface fondamentale de MVVM
    public class BaseViewModel : INotifyPropertyChanged
    {
        // 1. Événement requis par l'interface INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Méthode pour déclencher l'événement PropertyChanged.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Méthode générique pour définir une propriété, vérifier le changement de valeur, 
        /// et notifier la Vue si nécessaire.
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false; // La valeur n'a pas changé, on ne fait rien
            }

            storage = value; // Mise à jour de la valeur
            OnPropertyChanged(propertyName); // Notifie l'UI
            return true;
        }
    }
}