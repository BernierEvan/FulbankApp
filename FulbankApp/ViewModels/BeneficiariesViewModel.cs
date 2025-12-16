using FulbankApp.Models.BeneficiariesModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FulbankApp.Data;  // Assure-toi d'avoir ton DbContext ici

namespace FulbankApp.ViewModels
{
    public class BeneficiariesViewModel
    {
        public ObservableCollection<BeneficiaryClass> Beneficiaries { get; set; }

        public BeneficiariesViewModel() 
        {

            Beneficiaries = new ObservableCollection<BeneficiaryClass>();

            // 2. On charge les données tout de suite
            LoadData();
        }

        private void LoadData()
        {
            // On reprend ta logique EF Core exactement comme avant
            using (var context = new FulbankAppContext())
            {
                try
                {
                    // Récupération des données
                    var list = context.Beneficiaries.ToList();

                    // On remplit l'ObservableCollection
                    // (On ne peut pas faire "Beneficiaries = list" directement avec une ObservableCollection, il faut ajouter les items)
                    foreach (var beneficiary in list)
                    {
                        Beneficiaries.Add(beneficiary);
                    }
                }
                catch (System.Exception ex)
                {
                    // Optionnel : Gérer l'erreur (ex: Log ou propriété ErrorMessage)
                    System.Diagnostics.Debug.WriteLine($"Erreur : {ex.Message}");
                }
            }
        }
    }
}
