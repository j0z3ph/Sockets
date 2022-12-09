using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client {
    internal class Program {
        private static bool running = true;
        static async Task Main(string[] args) {
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipEndPoint = new(ipAddress, 6666);
            using Socket client = new(ipEndPoint.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);

            await client.ConnectAsync(ipEndPoint);
            // Send message.
            Thread t1 = new Thread(ReadLine);
            t1.Start(client);

            // Receive ack.
            Thread t2 = new Thread(ReceiveMsg);
            t2.Start(client);
            while (running) {
                
            }

            client.Shutdown(SocketShutdown.Both);
        }

        private static async void ReadLine(object? obj) {
            Socket s = (Socket)obj;
            while (running) {
                var message = Console.ReadLine();
                message = message.ToLower().Equals("bye") ? "<|BYE|>" : string.Concat(message, "<|EOM|>");
                var messageBytes = Encoding.UTF8.GetBytes(message);
                _ = await s.SendAsync(messageBytes, SocketFlags.None);
            }
        }

        private static async void ReceiveMsg(object? obj) {
            Socket s = (Socket)obj;
            while(running) {
                var buffer = new byte[1024];
                var received = await s.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                if (response == "<|BYE|>") {
                    Console.WriteLine(
                        $"Socket client received bye: \"{response}\"");
                    running = false;
                    break;
                } else {
                    Console.WriteLine(response);
                }
            }
        }
    }
}