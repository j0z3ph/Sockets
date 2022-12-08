using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Server {
    internal class Program {
        static async Task Main(string[] args) {
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipEndPoint = new(ipAddress, 11_000);
            using Socket listener = new(ipEndPoint.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(100);

            var handler = await listener.AcceptAsync();
            while (true) {
                // Receive message.
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1) {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");
                    var aknMessage = "<|AKN|>";
                    var echoBytes = Encoding.UTF8.GetBytes(aknMessage);
                    await handler.SendAsync(echoBytes, 0);
                }
                if(response == "<|BYE|>") {
                    var byeMessage = "<|BYE|>";
                    var echoBytes = Encoding.UTF8.GetBytes(byeMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent bye message: \"{byeMessage}\"");

                    break;
                }
            }
        }
    }
}