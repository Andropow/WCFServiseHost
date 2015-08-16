using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace ServiceAssembly
{
    [ServiceContract(CallbackContract = typeof(IChatCallback), SessionMode = SessionMode.Required)]
    public interface IChat
    {
        [OperationContract(IsInitiating = true)]
        bool Connect(Client client);

        [OperationContract(IsOneWay = true)]
        void Say(Message msg);

        [OperationContract(IsOneWay = true)]
        void Whisper(Message msg, string receiver);

        [OperationContract(IsOneWay = true)]
        void IsWriting(string client);

        [OperationContract(IsOneWay = false)]
        bool SendFile(FileMessage fileMsg);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(Client client);  
    }
    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void Conected();

        [OperationContract(IsOneWay = true)]
        void Error(string message);

        [OperationContract(IsOneWay = true)]
        void RefreshClients(List<Client> clients);

        [OperationContract(IsOneWay = true)]
        void Receive(Message msg);

        [OperationContract(IsOneWay = true)]
        void ReceiveWhisper(Message msg, string receiver);

        [OperationContract(IsOneWay = true)]
        void IsWritingCallback(string client);

        [OperationContract(IsOneWay = true)]
        void ReceivedFile(FileMessage filemsg);

        [OperationContract(IsOneWay = true)]
        void UserJoin(Client client);

        [OperationContract(IsOneWay = true)]
        void UserLeave(Client client);
    }


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ChatService : IChat
    {
        readonly Dictionary<Client, IChatCallback> _clients = new Dictionary<Client, IChatCallback>();
        List<FileMessage> _files = new List<FileMessage>();
        private IChatCallback CurrentCallback
        {
            get { return OperationContext.Current.GetCallbackChannel<IChatCallback>(); }
        }

        static readonly object syncObj = new object();

        private bool SearchClientsByName(string name)
        {
            return _clients.Keys.Any(c => c.Name == name);
        }

        private KeyValuePair<Client, IChatCallback> GetClientByName(string name)
        {
            KeyValuePair<Client, IChatCallback> client;
            lock (syncObj)
            {
                client = _clients.FirstOrDefault(c => c.Key.Name.Equals(name));
            }
            return client;
        }

        public bool Connect(Client client)
        {
            lock (syncObj)
            {
                var calback = CurrentCallback;
                if (_clients.ContainsValue(CurrentCallback) || SearchClientsByName(client.Name))
                {
                    calback.Error("Currently login is busy, enter new login");
                    return false;
                }
                _clients.Add(client, CurrentCallback);

                foreach (var key in _clients.Keys)
                {
                    var callback = _clients[key];
                    try
                    {
                        if (key.Name.Equals(client.Name))
                        {
                            callback.Conected();
                            callback.RefreshClients(_clients.Keys.ToList());
                        }
                        else
                        {
                            callback.UserJoin(client);
                        }
                    }
                    catch
                    {
                        _clients.Remove(key);
                        return false;
                    }
                }
                return true;
            }
        }

        public void Say(Message msg)
        {
            lock (syncObj)
            {
                foreach (var callback in _clients.Values)
                {
                    callback.Receive(msg);
                }
            }
        }

        public void Whisper(Message msg, string receiver)
        {
            var sender = GetClientByName(msg.Sender);
            var recive = GetClientByName(receiver);
            if (sender.Key == null || recive.Key == null) return;
            sender.Value.ReceiveWhisper(msg, receiver);
            recive.Value.ReceiveWhisper(msg, receiver);
        }

        public void IsWriting(string client)
        {
            lock (syncObj)
            {
                foreach (var callback in _clients.Values)
                {
                    callback.IsWritingCallback(client);
                }
            }
        }

        public bool SendFile(FileMessage fileMsg)
        {

            var rec = GetClientByName(fileMsg.Reciver);
            if (rec.Key == null)
            {
                lock (syncObj)
                {
                    _files.Add(fileMsg);
                    foreach (var client in _clients.Where(c => !c.Key.Name.Equals(fileMsg.Sender)))
                    {
                        client.Value.ReceivedFile(fileMsg);
                    }
                    return true;
                }
            }
            lock (syncObj)
            {
                rec.Value.ReceivedFile(fileMsg);
            }

            return true;
        }

        public void Disconnect(Client client)
        {
            lock (syncObj)
            {
                var cl = GetClientByName(client.Name);
                if (cl.Key == null) return;
                _clients.Remove(cl.Key);
                foreach (var callback in _clients.Values)
                {
                    callback.UserLeave(client);
                }
            }
        }  
    }
}


