using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace P2P_File_Sharing
{
    public class StatusMessage
    {
        private static readonly MainWindow _mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        public static void PostToActivityBox(string message, MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.ERROR:
                    _mainWindow.tbAppActivity.Text = "ERROR: " + message;
                    break;
                case MessageType.INFORMATION:
                    _mainWindow.tbAppActivity.Text = "INFO: " + message;
                    break;
                case MessageType.WARNING:
                    _mainWindow.tbAppActivity.Text = "WARNING: " + message;
                    break;
                case MessageType.NONE:
                    _mainWindow.tbAppActivity.Text = message;
                    break;
                default:
                    _mainWindow.tbAppActivity.Text = "MESSAGE: " + message;
                    break;
            }
        }

        public void Log(string message)
        {
            StringBuilder builder = new StringBuilder();
            var timestamp = DateTime.Now.ToString();
            builder.Append(timestamp);
            builder.Append(" :=> ");
            builder.Append(message);
            using (StreamWriter writer = new StreamWriter("errors.log", true))
            {
                writer.WriteLine(builder);
            }
        }
    }
}
