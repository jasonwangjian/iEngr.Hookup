using iEngr.Hookup.Models;
using iEngr.Hookup.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace iEngr.Hookup.Views
{
    public partial class PropertyEditorDialog : Window
    {
        public PropertyEditorDialog(HkTreeItem treeItem)
        {
            InitializeComponent();
            var viewModel = new PropertyEditorViewModel(treeItem);
            this.DataContext = viewModel;
            viewModel.CloseRequested += (sender, result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
        }

        private void AvailablePropertiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is PropertyEditorViewModel viewModel && sender is ListBox listBox)
            {
                viewModel.UpdateSelectedAvailableProperties(listBox.SelectedItems);
            }
        }
    }
}