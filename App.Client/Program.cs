using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace App.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = null;

            try
            {
                tcpClient = new TcpClient();
                Console.WriteLine("Connecting ...");
                tcpClient.Connect("192.168.1.107", 8001);
                Console.WriteLine("Connected ...");

                var asciiEncoding = new ASCIIEncoding();

                // Send login to service
                var login = asciiEncoding.GetBytes("Rahman/Mahmoodi");
                var networkStream = tcpClient.GetStream();
                networkStream.Write(login, 0, login.Length);
                // End sending message to server

                #region Read All Server Message

                while (true)
                {
                    if (networkStream.CanRead)
                    {
                        ////Buffer to store the response bytes.
                        byte[] readBuffer = new byte[tcpClient.ReceiveBufferSize];

                        // Initiate a read of zero bytes. Note: without this it won't work!
                        //var bytes = new byte[0];
                        //networkStream.Read(bytes, 0, 0);
                        var fullServerReply = string.Empty;

                        using (var writer = new MemoryStream())
                        {
                            while (networkStream.DataAvailable)
                            {
                                int numberOfBytesRead = networkStream.Read(readBuffer, 0, readBuffer.Length);

                                if (numberOfBytesRead <= 0)
                                {
                                    break;
                                }
                                writer.Write(readBuffer, 0, numberOfBytesRead);
                            }

                            fullServerReply = Encoding.UTF8.GetString(writer.ToArray());
                        }


                        Console.WriteLine(fullServerReply);

                        Thread.Sleep(2000); // Sleep for demo purpuses!!!
                        //Console.WriteLine("Press enter to read next message...");
                        //Console.ReadLine();

                    }
                }

                #endregion

                //// Note: this is fixed buffer size
                #region Fixed Buffer

                //StringBuilder totalString = new StringBuilder();

                //// Read responses from server. Note: Working
                //while (true)
                //{
                //    var bytes = new byte[4000];
                //    var bufferSize = networkStream.Read(bytes, 0, 4000);

                //    for (int i = 0; i < bufferSize; i++)
                //    {
                //        Console.WriteLine(Convert.ToChar(bytes[i]));
                //        totalString.Append(Convert.ToChar(bytes[i]));
                //    }

                //    Console.WriteLine(totalString.ToString());
                //    totalString.Clear();
                //    //Console.ReadLine();
                //}

                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (tcpClient != null) tcpClient.Close();
                Console.ReadLine();
            }
        }
    }
}
