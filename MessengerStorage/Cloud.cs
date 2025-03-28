using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using EncryptedMessaging;
using System.Text;

namespace MessengerStorage
{
    public class Cloud
    {
        /// <summary>
        /// Cloud Server will communicate to clients for making operations on cloud storage.
        /// </summary>
        /// <param name="privateKey">The device's private encryption key. If not set, one will be generated</param>
        /// <param name="entryPoint">The entry point of server, to access the network</param>
        /// <param name="path">The location where you want the cloud root to be</param>
        public Cloud(RemoteStorage remoteStorage, string privateKey, string entryPoint, string path = null)
        {
            CloudAppDataPath = path;
            Communication = new Communication(remoteStorage,CloudAppDataPath);
            EntryPoint = entryPoint;
            PrivateKey = privateKey;
            int platform = (int)Environment.OSVersion.Platform;
            Contact.RuntimePlatform runtimePlatform = (platform == 4) || (platform == 6) || (platform == 128) ? Contact.RuntimePlatform.Unix : Contact.RuntimePlatform.Windows;

            Context = new Context(EntryPoint, NetworkName, _multipleChatModes, PrivateKey, Modality.Server);
            Context.OnContactEvent += OnContactEvent;


            // Bind the event to change the connection when the connectivity changes
            NetworkChange.NetworkAvailabilityChanged += (object sender, NetworkAvailabilityEventArgs e) => Context.OnConnectivityChange(e.IsAvailable, CommunicationChannel.Channel.ConnectivityType.Internet);

            // Set the current connection status
            Context.OnConnectivityChange(NetworkInterface.GetIsNetworkAvailable(), CommunicationChannel.Channel.ConnectivityType.Internet);

        }
        public string CloudAppDataPath;
        public readonly Communication Communication;
        private static ushort CloudAppId = BitConverter.ToUInt16(Encoding.ASCII.GetBytes("cloud"), 0);
        const bool _multipleChatModes = true;
        private readonly string NetworkName = "mainnet";
        public Context Context;
        private readonly string EntryPoint;
        private readonly string PrivateKey;

        private void OnContactEvent(Message message)
        {
            if (message.Type == MessageFormat.MessageType.SubApplicationCommandWithParameters || message.Type == MessageFormat.MessageType.SubApplicationCommandWithData)
            {
                ushort appId = default;
                ushort command = default;
                List<byte[]> parameters = default;
                if (message.Type == MessageFormat.MessageType.SubApplicationCommandWithParameters)
                {
                    message.GetSubApplicationCommandWithParameters(out appId, out command, out parameters);
                }
                else if (message.Type == MessageFormat.MessageType.SubApplicationCommandWithData)
                {
                    message.GetSubApplicationCommandWithData(out appId, out command, out byte[] data);
                    parameters = new List<byte[]>(new byte[][] { data });
                }
                if (appId == CloudAppId)
                {
                    var answareToCommand = (Communication.Command)command;
                    Communication.OnCommand(message.Contact, answareToCommand, parameters);
                }
            }
        }
    }
}
