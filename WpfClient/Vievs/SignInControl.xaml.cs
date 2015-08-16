using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfClient.Vievs
{
    /// <summary>
    /// Interaction logic for SignInControl.xaml
    /// </summary>
    public partial class SignInControl : UserControl
    {
        public IPAddress ServerIp { get; private set; }
        public string ClientName { get; private set; }
        public string AvatarPath { get; private set; }
        public SignInControl()
        {
            InitializeComponent();
            AvatarPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WcfWpfChat", "unknown.jpg"); ;            
            tbServerIp.Text = "192.168.1.2";
        }

        public static readonly RoutedEvent AddButtonClickEvent = EventManager.RegisterRoutedEvent(
           "AddButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SignInControl));

        /// <summary>
        /// Expose the AddButtonClick to external sources
        /// </summary>
        public event RoutedEventHandler AddButtonClick
        {
            add { AddHandler(AddButtonClickEvent, value); }
            remove { RemoveHandler(AddButtonClickEvent, value); }
        }

        private void Photo_Drop(object sender, DragEventArgs e)
        {
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            if (fileNames != null && fileNames.Length > 0)
            {
                AvatarPath = fileNames[0];
                photoSrc.Source = new BitmapImage(new Uri(AvatarPath));
            }         
                   
            // Mark the event as handled, so the control's native Drop handler is not called.
            e.Handled = true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {                      
            IPAddress adr;
            if (!string.IsNullOrEmpty(txtName.Text) &&
                IPAddress.TryParse(tbServerIp.Text, out adr))
               
            {
                
                ClientName = txtName.Text;
                ServerIp = IPAddress.Parse(tbServerIp.Text);
                RaiseEvent(new RoutedEventArgs(AddButtonClickEvent));
            }
            else
            {
                MessageBox.Show("You need to pick a screen Name and ServerIP ", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
    }   
}
