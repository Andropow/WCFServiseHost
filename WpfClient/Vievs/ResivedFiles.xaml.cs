using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using WpfClient.ServiceReference;

namespace WpfClient.Vievs
{
    public partial class ResivedFiles : UserControl
    {
        public ResivedFiles()
        {
            InitializeComponent();
            lblExit.MouseDown += LblExitOnMouseDown;
        }

        private void LblExitOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            RaiseEvent(new RoutedEventArgs(CloseClickEvent));
        }

        #region Routed Events

        /// <summary>
        /// CloseClickEvent event, occurs when the user clicks the close button
        /// </summary>
        public static readonly RoutedEvent CloseClickEvent = EventManager.RegisterRoutedEvent(
            "CloseClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ResivedFiles));

        /// <summary>
        /// Expose the CloseClickEvent to external sources
        /// </summary>
        public event RoutedEventHandler CloseClick
        {
            add { AddHandler(CloseClickEvent, value); }
            remove { RemoveHandler(CloseClickEvent, value); }
        }

        #endregion
        private void Dovnload_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var index = int.Parse((sender as Image).Tag.ToString());
            var item = (KeyValuePair<int, FileMessage>)FilesViev.Items[index];
            var file = item.Value;
            var sd = new SaveFileDialog {FileName = file.FileName};
            if (sd.ShowDialog() == false) return;
            File.WriteAllBytes(sd.FileName,file.Data);
        }
    }
}
