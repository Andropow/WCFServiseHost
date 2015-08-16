using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfClient.Vievs
{
    public partial class MessageControl : UserControl
    {
        public bool Flag { get; set; }
        public MessageControl(string login, string message, string imageUri, bool aligmentFlag)
        {
            InitializeComponent();
            // Border.Background = Resources[aligmentFlag ? "Brush1" : "Brush2"] as LinearGradientBrush;                     
            HorizontalAlignment = aligmentFlag ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            Image.Source = new BitmapImage(new Uri(imageUri));
            tbMessage.Text = message;
            lbLogin.Content = login;
            lbDate.Content = DateTime.Now;
        }

    }
}
