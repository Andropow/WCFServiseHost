using System;
using System.Runtime.Serialization;

namespace ServiceAssembly
{
    [DataContract]
    public class Client
    {
        private string _name;
        private string _avatarname;
        private byte[] _avatar;

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        [DataMember]
        public string AvatarName
        {
            get { return _avatarname; }
            set { _avatarname = value; }
        }
        [DataMember]
        public byte[] AvatarFile
        {
            get { return _avatar; }
            set { _avatar = value; }
        }
    }

    [DataContract]
    public class Message
    {
        private string _sender;
        private string _content;
        private DateTime _time = DateTime.Now;

        [DataMember]
        public string Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }
        [DataMember]
        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }
        [DataMember]
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }
    }

    [DataContract]
    public class FileMessage
    {
        private string sender;
        private string reciver;
        private string fileName;
        private byte[] data;
        private DateTime time = DateTime.Now;

        [DataMember]
        public string Sender
        {
            get { return sender; }
            set { sender = value; }
        }

        [DataMember]
        public string Reciver
        {
            get { return reciver; }
            set { reciver = value; }
        }

        [DataMember]
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        [DataMember]
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        [DataMember]
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }
    }
}