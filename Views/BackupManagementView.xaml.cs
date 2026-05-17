using ElectricalPOS.ViewModels;
using System.Windows.Controls;

namespace ElectricalPOS.Views
{
    public partial class BackupManagementView : UserControl
    {
        public BackupManagementView()
        {
            InitializeComponent();
            DataContext = new BackupViewModel();
        }
    }
}
