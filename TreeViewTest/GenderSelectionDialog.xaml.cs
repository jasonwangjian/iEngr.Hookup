using System.Windows;

namespace WpfTreeViewEditor
{
    public partial class GenderSelectionDialog : Window
    {
        public string SelectedGender { get; private set; } = "未知";

        public GenderSelectionDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (rbUnknown.IsChecked == true)
                SelectedGender = "未知";
            else if (rbMale.IsChecked == true)
                SelectedGender = "男性";
            else if (rbFemale.IsChecked == true)
                SelectedGender = "女性";

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}