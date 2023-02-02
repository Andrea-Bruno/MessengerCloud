﻿using EncryptedMessaging;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using NotificationServer;
namespace MessengerStorage
{
    public class Communication
    {
        public Communication(RemoteStorage remoteStorage, string cloudAppDataPath)
        {
            RemoteStorage = remoteStorage;
            CloudAppDataPath = cloudAppDataPath;
        }
        private RemoteStorage RemoteStorage;
        readonly string CloudAppDataPath;
        public enum Command : ushort // 2 byte - the names must start with Get or Set
        {
            SaveData,
            LoadData,
            LoadAllData,
            DeleteData,
            PushNotification
        }
        public static Dictionary<Command, uint> Counter = new Dictionary<Command, uint>(); // used for statistical purposes(saves numbers of executed commands)
        private const string Extension = ".d";
        /// <summary>
        /// Method executes when new command is received from a cloud client. Depending on command it saves or loads data to/from the cloud.
        /// </summary>
        /// <param name="contact"> contact of the client which sent command </param>
        /// <param name="responseToCommand"> command sent by the client (SaveData, LoadData, LoadAllData, DeleteData, PushNotification)</param>
        /// <param name="parameters"> list of parameters needed for the command:
        /// parameter 0: type - group i.e. folder name;
        /// parameter 1: name - key i.e. file name;
        /// parameter 2: data - data(file itslef) which needs to be stored;
        /// parameter 3: shared - if data is private i.e. stored in a specific user folder or public i.e. stored in a common folder (boolean);</param>
        public void OnCommand(Contact contact, Command responseToCommand, List<byte[]> parameters)
        {
            if (!Counter.ContainsKey(responseToCommand))
                Counter[responseToCommand] = 0;
            Counter[responseToCommand] += 1;
            lock (Extension)
            {
                try
                {
                    if (contact?.UserId == null)
                        return;
                    var userId = contact.UserId;
                    string type; // group i.e. folder name
                    string name; // key i.e. file name
                    bool shared; // if data is private(stored in a specific user folder) or public( stored in common folder)
                    byte[] data; // data(file) which needs to be stored
                    string path; // path to the folder with file
                    string file; // path to the file itself
                    switch (responseToCommand)
                    {
                        case Command.SaveData:
                            type = Encoding.ASCII.GetString(parameters[0]);
                            name = Encoding.ASCII.GetString(parameters[1]);
                            data = parameters[2];
                            shared = BitConverter.ToBoolean(parameters[3], 0);
                            path = GetPath(shared ? null : userId, type, true);
                            File.WriteAllBytes(Path.Combine(path, name + Extension), data);
                            break;
                        case Command.LoadData:
                            type = Encoding.ASCII.GetString(parameters[0]);
                            name = Encoding.ASCII.GetString(parameters[1]);
                            int? ifSizeIsDifferent = null;// check if data has changed or not(comparing old and new data size)
                            if (parameters[2].Length > 0)
                                ifSizeIsDifferent = BitConverter.ToInt32(parameters[2], 0);
                            shared = BitConverter.ToBoolean(parameters[3], 0);
                            path = GetPath(shared ? null : userId, type);
                            file = Path.Combine(path, name + Extension);
                            if (File.Exists(file))
                            {
                                data = File.ReadAllBytes(file);
                                if (ifSizeIsDifferent == null || data.Length != ifSizeIsDifferent)
                                {
                                    SendCommand(contact, responseToCommand, new byte[][] { parameters[0], parameters[1], data, parameters[3] });
                                }
                            }
                            break;
                        case Command.LoadAllData:
                            type = Encoding.ASCII.GetString(parameters[0]);
                            shared = BitConverter.ToBoolean(parameters[1], 0);
                            path = GetPath(shared ? null : userId, type);
                            var files = Directory.GetFiles(path, "*" + Extension);
                            foreach (var filename in files)
                            {
                                name = filename.Substring(0, filename.Length - Extension.Length);
                                data = File.ReadAllBytes(filename);
                                SendCommand(contact, responseToCommand, new byte[][] { parameters[0], Encoding.ASCII.GetBytes(name), data, parameters[1] });
                            }
                            break;
                        case Command.DeleteData:
                            type = Encoding.ASCII.GetString(parameters[0]);
                            name = Encoding.ASCII.GetString(parameters[1]);
                            shared = BitConverter.ToBoolean(parameters[2], 0);
                            path = GetPath(shared ? null : userId, type);
                            file = Path.Combine(path, name + Extension);
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                            break;
                        case Command.PushNotification:
                            var keyValue = new Dictionary<Notification.Keys, byte[]>();
                            keyValue.Add(Notification.Keys.ChatId, parameters[0]);
                            keyValue.Add(Notification.Keys.DeviceToken, parameters[1]);
                            keyValue.Add(Notification.Keys.IsVideo, parameters[2]);
                            keyValue.Add(Notification.Keys.ContactNameOrigin, parameters[3]);
                            Notification.OnPushNotification(keyValue);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // There is probably an error with system dependent files (files open or access denied)
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        private void SendCommand(Contact contact, Command responseToCommand, byte[][] data)
        {
            var appId = BitConverter.ToUInt16(Encoding.ASCII.GetBytes("cloud"), 0);
            // Encryption is disabled because the data that must be encrypted by the client when it sends it is not saved in the clear, so it is not necessary to add additional encryption
            RemoteStorage.Cloud.Context.Messaging.SendCommandToSubApplication(contact, appId, (ushort)responseToCommand, true, false, data);
        }

        private string GetPath(ulong? forUserId, string type = null, bool createFolder = false)
        {
            var appData = CloudAppDataPath;
            var directory = forUserId == null ? "shared" : "id" + forUserId;
            var path = type == null ? Path.Combine(appData, directory) : Path.Combine(appData, directory, type);
            if (createFolder && !Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}