using iEngr.Hookup.Models;
using iEngr.Hookup.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace iEngr.Hookup.Views
{
    public partial class PropertyEditorDialog : Window
    {
        public PropertyEditorViewModel ViewModel => DataContext as PropertyEditorViewModel;
        public PropertyEditorDialog(HkTreeItem treeItem)
        {
            InitializeComponent();
            DataContext = new PropertyEditorViewModel(treeItem);
            ViewModel.CloseRequested += (sender, result) =>
            {
                DialogResult = result;
                Close();
            };
        }

        private void AvailablePropertiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel != null && sender is ListBox listBox)
            {
                ViewModel.UpdateSelectedAvailableItems(listBox.SelectedItems);
            }
        }
    }
}