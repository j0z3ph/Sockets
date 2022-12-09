using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Server {
    internal class Program {
        public static List<Socket> list = new List<Socket>();
        public static List<Thread> threads = new List<Thread>();
        static async Task Main(string[] args) {
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipEndPoint = new(ipAddress, 6666);
            using Socket listener = new(ipEndPoint.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(100);



            while (true) {
                Socket l = await listener.AcceptAsync();
                list.Add(l);
                Thread t = new Thread(Wait4Msg);
                threads.Add(t);
                t.Start(l);
            }
        }

        private static async void Wait4Msg(object? obj) {
            Socket handler = (Socket)obj;
            while (true) {
                var buffer = new byte[1024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1) {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");
                    var aknMessage = "<|AKN|>";
                    //var echoBytes = Encoding.UTF8.GetBytes(aknMessage);
                    //await handler.SendAsync(echoBytes, 0);
                    await BroadcastMsg(response.Replace(eom, ""), handler);

                }
                if (response == "<|BYE|>") {
                    var byeMessage = "<|BYE|>";
                    var echoBytes = Encoding.UTF8.GetBytes(byeMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent bye message: \"{byeMessage}\"");

                    break;
                }
            }
        }

        private static async Task BroadcastMsg(string msg, Socket s) {
            int idx = list.IndexOf(s);
            foreach (var handler in list) {
                if (!handler.Equals(s)) {
                    var echoBytes = Encoding.UTF8.GetBytes($"Client {idx}: {msg}");
                    await handler.SendAsync(echoBytes, 0);
                }
            }
        }
    }
}