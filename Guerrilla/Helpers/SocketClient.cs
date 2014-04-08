using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Guerrilla
{
    public class SocketMessageEventArgs : EventArgs
    {
        public SongMeta Meta { get; set; }
    }

    public class SocketClient
    {
        private const string serverName = "137.117.234.184";
        private const string serverPort = "1060";

        private StreamSocket socket;
        private HostName serverHost;
        private bool connected = false;

        Task lastTask;


        public event EventHandler<SocketMessageEventArgs> SocketMessageReceived;

        public SocketClient()
        {
        }

        public async void Connect()
        {
            socket = new StreamSocket();
            try
            {
                serverHost = new HostName(serverName);
                await socket.ConnectAsync(serverHost, serverPort);
                connected = true;

                lastTask = WaitForData();
            }
            catch (Exception ex)
            {
                connected = false;
                socket = null;
                lastTask = null;

                if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }
        }

        public void Disconnect()
        {
            if (!connected) return;
            if (socket != null)
            {
                connected = false;
                socket.Dispose();
                socket = null;
            }
        }

        public async Task WaitForData()
        {
            Debug.WriteLine("Wait for data was hit");
            try
            {
                if (socket == null) return;

                DataReader reader = new DataReader(socket.InputStream);
                //reader.InputStreamOptions = InputStreamOptions.Partial

                // Load the string length that's coming
                var header = await reader.LoadAsync(4);

                if (header == 0)
                {
                    // Disconnected
                    SendEmptySongEvent();
                    return;
                }

                int strLength = Convert.ToInt32(reader.ReadString(4));

                Debug.WriteLine("String Length read: " + strLength);
                if (strLength == 0)
                {
                    // There is no message afterwards; wait again 
                    Debug.WriteLine("Nothing to read - Guerrilla");
                    SendEmptySongEvent();
                    WaitForData();
                    return;
                }

                // Load the actual string
                uint strBytes = await reader.LoadAsync((uint)strLength);
                Debug.WriteLine("Read the string bytes");
                string metaString = reader.ReadString(strBytes);

                Debug.WriteLine(metaString);

                SongMeta meta = JsonConvert.DeserializeObject<SongMeta>(metaString);

                // Create the SocketMessageReceived event
                SocketMessageEventArgs args = new SocketMessageEventArgs();
                args.Meta = meta;
                if (SocketMessageReceived != null && socket != null)
                {
                    SocketMessageReceived(this, args);
                }

                reader.DetachStream();

                WaitForData();
                Debug.WriteLine("Exited wait for data");
            }
            catch (Exception ex)
            {
                //SocketErrorStatus status = SocketError.GetStatus(ex.HResult);
                Debug.WriteLine("Exception in Wait for Data: " + ex.Message);
                SendEmptySongEvent();
                socket.Dispose();
                connected = false;
                Connect();
            }
        }

        private void SendEmptySongEvent()
        {
            if (SocketMessageReceived != null && socket != null)
            {
                SocketMessageReceived(this, new SocketMessageEventArgs() { Meta = null });
            }
        }
    }
}
