using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using SimpleFileSortingWPF.models;
using System.Windows.Controls.Primitives;

namespace SimpleFileSortingWPF.interfaces
{
    /// <summary>
    /// Interaction logic for UserSetting.xaml
    /// </summary>
    public partial class UserSetting : Window
    {
        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
        PrimaryViewModel FormatData;
        c_format selectedformat;

        public UserSetting()
        {
            FormatData = mainWindow.FormatData;
            FormatData.Formats.LoadFormatsFromJson();


            InitializeComponent();
            this.Title = "User Settings";
            DataContext = mainWindow.FormatData;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            foreach(c_format Data in FormatData.Formats.mFormats)
            {
                if(Data.mTypes.Count == 0)
                {
                    MessageBox.Show("A Category cannot be empty");
                    e.Cancel = true;
                    return;
                }
            }

            lstboxFileFormatsTypes.SelectedIndex = -1;
            mainWindow.FormatData.AppSetting.Save();
            mainWindow.Focus();
        }

        public void FormatSelection_Changed(object sender, EventArgs e)
        {
            mainWindow.FormatData.AppSetting.SelectedDateFormat = FormatSelection.SelectedItem.ToString();
        }

        public void btnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(txtbCategoryName.Text)) 
            {
                try
                {
                    var newdata = new c_format
                    {
                        mName = txtbCategoryName.Text
                    };

                    foreach(c_format Data in FormatData.Formats.mFormats)
                    {
                        if(Data.mName.Contains(newdata.mName))
                        {
                            MessageBox.Show("That Category already exists, please name it differently");
                            return;
                        }
                    }

                    FormatData.Formats.mFormats.Add(newdata);
                    FormatData.Formats.SaveFormatsToJson();
                    txtbCategoryName.Text = string.Empty;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "btnRemoveFormat":
                    c_format formats = (c_format)button.DataContext;
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the category?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        FormatData.Formats.mFormats.Remove(formats);
                        FormatData.Formats.SaveFormatsToJson();
                    }
                    break;
                case "btnRemoveFormatType":
                    lstboxFileFormatsTypes.SelectedItem = button.DataContext;
                    string selectedType = lstboxFileFormatsTypes.SelectedItem as string;
                    if (!string.IsNullOrEmpty(selectedType))
                    {
                        selectedformat.mTypes.Remove(selectedType);
                        FormatData.Formats.SaveFormatsToJson();
                    }
                    break;
                case "btnEditName":
                    Button btnEditName = sender as Button;
                    Grid parentGrid = btnEditName.Parent as Grid;
                    TextBox txtbEditName = parentGrid.FindName("txtbEditName") as TextBox;
                    c_format selecteddata = txtbEditName.DataContext as c_format;

                    if (txtbEditName != null)
                    {
                        txtbEditName.Visibility = Visibility.Visible;
                        txtbEditName.Text = selecteddata.mName;
                        txtbEditName.Focus();
                        btnEditName.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }


        public void btnAddType_Click(object sender, RoutedEventArgs e)
        {
            string newType = txtbTypeName.Text.ToLower();

            if (!string.IsNullOrEmpty(newType))
            {
                foreach (c_format format in FormatData.Formats.mFormats)
                {
                    if (format.mTypes.Contains(newType))
                    {
                        if(format == selectedformat)
                        {
                            MessageBox.Show("File type is already included");
                            return;
                        }
                      
                        MessageBoxResult result = MessageBox.Show("File Type already exists on another category. Do you want to transfer it to this category?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            format.mTypes.Remove(newType);
                            selectedformat.mTypes.Add(newType);
                            FormatData.Formats.SaveFormatsToJson();
                            txtbTypeName.Text = string.Empty;
                            return;
                        }
                       else
                        {
                            return;
                        }
                    }
                }
                selectedformat.mTypes.Add(newType);
                FormatData.Formats.SaveFormatsToJson();
                txtbTypeName.Text = string.Empty;
            }
        }

        public void lstboxFileFormats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstboxFileFormats.SelectedItem != null)
            {
                selectedformat = (c_format)lstboxFileFormats.SelectedItem;
                lstboxFileFormatsTypes.ItemsSource = selectedformat.mTypes;
                stkpanelFormatType.IsEnabled = true;
            }
        }

        private void txtbCategoryName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RoutedEventArgs routedEventArgs = new RoutedEventArgs();
                btnAddCategory_Click(sender, routedEventArgs);
            }
        }

        private void txtbTypeName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RoutedEventArgs routedEventArgs = new RoutedEventArgs();
                btnAddType_Click(sender, routedEventArgs);
            }
        }

        public void txtbEditName_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            Grid parentGrid = textbox.Parent as Grid;
            Button button = parentGrid.FindName("btnEditName") as Button;

            if (e.Key == Key.Enter)
            {
                if(!string.IsNullOrEmpty(textbox.Text))
                {
                    try
                    {
                        Grid grid = textbox.Parent as Grid;
                        c_format selecteddata = textbox.DataContext as c_format;

                        selecteddata.mName = textbox.Text;
                        textbox.Text = string.Empty;
                        textbox.Visibility = Visibility.Collapsed;
                        button.Visibility = Visibility.Visible;

                        FormatData.Formats.SaveFormatsToJson();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    textbox.Visibility = Visibility.Collapsed;
                    button.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
