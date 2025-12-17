using System.Windows;
using System.Windows.Controls;

namespace FulbankApp.View.Skeletons
{
    public partial class SkeletonLoading : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(SkeletonLoading), new PropertyMetadata("Chargement..."));

        public SkeletonLoading()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
    }
}