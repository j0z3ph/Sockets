using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client {
    internal class Program {
        static async Task Main(string[] args) {
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipEndPoint = new(ipAddress, 11_000);
            using Socket client = new(ipEndPoint.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);

            await client.ConnectAsync(ipEndPoint);
            while (true) {
                // Send message.
                var message = Console.ReadLine();
                message = message.ToLower().Equals("bye") ? "<|BYE|>" : string.Concat(message, "<|EOM|>");
                var messageBytes = Encoding.UTF8.GetBytes(message);
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
                //Console.WriteLine($"Socket client sent message: \"{message}\"");

                // Receive ack.
                var buffer = new byte[1_024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                if (response == "<|BYE|>") {
                    Console.WriteLine(
                        $"Socket client received bye: \"{response}\"");
                    break;
                }
            }

            client.Shutdown(SocketShutdown.Both);
        }
    }
}