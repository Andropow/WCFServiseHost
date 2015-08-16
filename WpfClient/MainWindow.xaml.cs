using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using WpfClient.ServiceReference;
using WpfClient.Vievs;

namespace WpfClient
{
    public partial class MainWindow : Window, IChatCallback
    {
#region Fields
        readonly string AvatarsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WcfWpfChat");
        private ChatClient _proxyClient;       
        private Client _localClient;
        private readonly ObservableCollection<KeyValuePair<int,FileMessage>> _files;
        private readonly ObservableCollection<Person> _clients;
        
        static readonly object LockObject= new object();
#endregion
        public MainWindow()
        {
            InitializeComponent();   
            _clients = new ObservableCollection<Person>();
            _files = new ObservableCollection<KeyValuePair<int, FileMessage>>();
            ResivedFiles.FilesViev.ItemsSource = _files;
            Loaded += InitializeGui;
            Closed += Window_Closed;
            SignInControl.AddButtonClick += SignInControl_AddButtonClick;
            ResivedFiles.CloseClick += ResivedFilesOnCloseClick;
            lvClients.ItemsSource = _clients;
            messagesViewer.ScrollToBottom();            
        }
     
#region UIEvents
        private void SignInControl_AddButtonClick(object sender, RoutedEventArgs e)
        {
            AvatarImage.Source = new BitmapImage(new Uri(SignInControl.AvatarPath));
            lbUserName.Content = SignInControl.ClientName;
            Conect();
        }
        private void Upload_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            var item = lvClients.Items[(int)img.Tag] as Person;
            SendFile(item.Name);
        }

        private void mnuConect_Click(object sender, RoutedEventArgs e)
        {
            Conect();
        }

        private void mnuDisConnect_Click(object sender, RoutedEventArgs e)
        {
            Disconect();
        }

        private void mnuResivedFiles_OnClick(object sender, RoutedEventArgs e)
        {
            mnuResivedFiles.Background = Brushes.Transparent;
            ResivedFiles.Visibility = Visibility.Visible;
            ((Storyboard)Resources["ResivedFilesShow"]).Begin(this);
            
            
        }
        private void ResivedFilesOnCloseClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ((Storyboard)Resources["ResivedFilesClose"]).Begin(this);           
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Disconect();
            Application.Current.Shutdown(0/*Exit code*/);
        }

        private void TbText_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Send();
        }

        private void SendFile_OnClick(object sender, RoutedEventArgs e)
        {
            SendFile(string.Empty);
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            Send();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Disconect();
            //var di = new DirectoryInfo(AvatarsPath);
            //foreach (var file in di.GetFiles())
            //{
            //    file.Delete();
            //}
            //di.Delete();            
            Application.Current.Shutdown(0/*Exit code*/);
        } 
#endregion    

#region PrivateMethods
        private void InitializeGui(object sender, RoutedEventArgs e)
        {          
            var dir = new DirectoryInfo(AvatarsPath);
            dir.Create();
            Properties.Resources.unknown.Save(Path.Combine(AvatarsPath, "unknown.jpg"));
            SignInControl.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
        }

        private string GetAvatarPath(string username)
        {
            return username.Equals(_localClient.Name) ? SignInControl.AvatarPath 
                : _clients.FirstOrDefault(c => c.Name.Equals(username)).ImageURL;
        }

        #endregion
 
#region Chat
        private void Conect()
        {
            var avatarPath = SignInControl.AvatarPath;
            if (avatarPath == null || !File.Exists(avatarPath)) return;
            var file = new FileInfo(avatarPath);
            _localClient = new Client
            {
                Name = SignInControl.ClientName,
                AvatarName = SignInControl.ClientName + file.Extension,
                AvatarFile = File.ReadAllBytes(avatarPath)
            };
                try
                {
                    var context = new InstanceContext(this);
                    _proxyClient = new ChatClient(context);
                    var servicePath = _proxyClient.Endpoint.ListenUri.AbsolutePath;
                    var serviceListenPort = _proxyClient.Endpoint.Address.Uri.Port.ToString();

                    _proxyClient.Endpoint.Address = new EndpointAddress("net.tcp://" + SignInControl.ServerIp + ":" + serviceListenPort + servicePath);
                    _proxyClient.Open();
                    //_proxyClient.InnerDuplexChannel.Faulted += (sender, args) => HandleProxy();
                    //_proxyClient.InnerDuplexChannel.Opened += (sender, args) => HandleProxy();
                    //_proxyClient.InnerDuplexChannel.Closed += (sender, args) => HandleProxy();
                    _proxyClient.ConnectAsync(_localClient);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
                   
                } 
        }

        private void Disconect()
        {
            if (_proxyClient != null)
            {
                _proxyClient.DisconnectAsync(_localClient);
                _proxyClient.Close();
                _proxyClient = null;
            }
            lock (LockObject)
            {
                _clients.Clear();
            }
            lbStatus.Foreground = Brushes.Red;
            lbStatus.Content = "Ofline";
        }

        private void Send()
        {           
            var item = lvClients.SelectedItem as Person;
            var msg = new Message { Sender = _localClient.Name, Content = tbText.Text, Time = DateTime.Now };
            if (item != null && (bool) cbWsiperMode.IsChecked)
            {
                _proxyClient.WhisperAsync(msg, item.Name);
            }
            else
            {
                _proxyClient.SayAsync(msg);
            }
            tbText.Text = string.Empty;            
        }

        private void SendFile(string reciver)
        {
            var fd = new OpenFileDialog { Multiselect = false };
            if (fd.ShowDialog() == DialogResult.HasValue) return;

            var fm = new FileMessage
            {
                Data = File.ReadAllBytes(fd.FileName),
                Sender = _localClient.Name,
                FileName = fd.SafeFileName,
                Reciver = reciver,
                Time = DateTime.Now
            };
            _proxyClient.SendFileAsync(fm);
        }
#endregion

#region IchatCalbacs
        public void Conected()
        {
            Action action = () =>
            {
                lbStatus.Foreground = Brushes.LimeGreen;
                lbStatus.Content ="Online";
                SignInControl.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;
                Header.Visibility = Visibility.Visible;
            };
            Dispatcher.BeginInvoke(action);
        }

        public void Error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Action showSigIn = () =>
            {
                SignInControl.Visibility = Visibility.Visible;
                MainGrid.Visibility = Visibility.Hidden;
            };
            Dispatcher.BeginInvoke(showSigIn);
        }

        public void RefreshClients(List<Client> clients)
        {
            foreach (var cl in clients.Where(c => c.Name != _localClient.Name))
            {
                UserJoin(cl);
            }
        }

        public void Receive(Message msg)
        {
            MessageContainer.ApendMessage(msg,GetAvatarPath(msg.Sender), msg.Sender.Equals(_localClient.Name));
            messagesViewer.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => messagesViewer.ScrollToBottom()));
        }

        public void ReceiveWhisper(Message msg, string receiver)
        {
            MessageContainer.ApendMessage(msg, GetAvatarPath(msg.Sender), msg.Sender.Equals(_localClient.Name));
            messagesViewer.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => messagesViewer.ScrollToBottom()));
        }

        public void IsWritingCallback(string client)
        {
            throw new NotImplementedException();
        }

        public void ReceivedFile(FileMessage fileMsg)
        {
            Action file = () =>
            {
                _files.Add(new KeyValuePair<int, FileMessage>(_files.Count,fileMsg));
                mnuResivedFiles.Background = Brushes.Lime;
            };
            Dispatcher.BeginInvoke(file);
        }

        public void UserJoin(Client client)
        {
            var localPath = Path.Combine(AvatarsPath, client.AvatarName);
            if (!File.Exists(localPath))
                using (var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(client.AvatarFile, 0, client.AvatarFile.Length);
                    fs.Close();
                }
            lock (LockObject)
            {
                Action apend = () => _clients.Add(new Person(client.Name, localPath));
                Dispatcher.BeginInvoke(apend);
            }     
        }

        public void UserLeave(Client client)
        {
            lock (LockObject)
            {
                Action remove =
                    () => _clients.Remove(_clients.FirstOrDefault(c => c.Name.Equals(client.Name)));
                Dispatcher.BeginInvoke(remove);
            }    
        }

        
#endregion

      
    }

    public static class StackPanelExtensions
    {
        public static void ApendMessage(this StackPanel panel, Message msg, string imageUri, bool aligmentFlag)
        {
            Action append = () => panel.Children.Add(new MessageControl(msg.Sender, msg.Content, imageUri, aligmentFlag));
            panel.Dispatcher.BeginInvoke(append);
        }
    }

    public class Person
    {
        private static int index;
        #region Instance Fields   
     
        public string ImageURL { get; private set; }
        public string Name { get; private set; }
        #endregion
        public int Index { get; private set; }       
        public Person(string name, string imgurl)
        {
            ImageURL = imgurl;
            Name = name;
            Index = index++;
        }
    }
}
