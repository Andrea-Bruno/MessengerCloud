using dotAPNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
namespace NotificationServer
{
    static public class Notification
    {
        // If you change the elements of this enum, then the elements in EncriptedMessaging Cloud.cs must also be changed
        public enum Keys : byte
        {
            ChatId,
            ContactNameOrigin,
            IsVideo,
            DeviceToken,
        }
        /// <summary>
        /// This event is triggered whenever the server receives a push notification
        /// </summary>
        /// <param name="keyValue">Data received from the server in the form of a key / value</param>
        public static async void OnPushNotification(Dictionary<Keys, byte[]> keyValue)
        {
            PushNotificationCounter++;
            ulong groupId = BitConverter.ToUInt64(keyValue[Keys.ChatId], 0); // the Chat ID
            string deviceToken = Encoding.ASCII.GetString(keyValue[Keys.DeviceToken]).Replace(" ", "");
            bool isVideo = BitConverter.ToUInt32(keyValue[Keys.IsVideo], 0) == 0 ? false : true;
            string contactNameOrigin = Encoding.Unicode.GetString(keyValue[Keys.ContactNameOrigin]);

            var cert = new X509Certificate2("Certificates.p12");
            var apns = ApnsClient.CreateUsingCert(cert);
            var push = new ApplePush(ApplePushType.Voip)
                .AddVoipToken(deviceToken)
                .AddCustomProperty("alert", contactNameOrigin + "?>@#" + groupId + "?>@#" + isVideo, true);
            //	.AddAlert("alert", contactNameOrigin + "?>@#" + groupId + "?>@#" + isVideo);
            //  apns.UseSandbox();
            try
            {
                ApnsResponse response = await apns.SendAsync(push);
                if (response.IsSuccessful)
                {
                    Debug.WriteLine("An alert push has been successfully sent!");
                }
                else
                {
                    PushNotificationErrors++;
                    LastPushNotificationError = response.ReasonString;
                    Debug.WriteLine("Failed to send a push, APNs reported an error: " + response.ReasonString);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debugger.Break();
            }

        }

        public static int PushNotificationCounter;
        public static int PushNotificationErrors;
        public static string LastPushNotificationError;
    }
}
