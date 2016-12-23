using System;
using System.Collections.Generic;
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

                        const char StartOfHeading = '\x0001'; // SOH ascii code is 1 or 01//
                        ParseMessages(fullServerReply, StartOfHeading);

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


        static void ParseMessages(string fixString, char delimiter)
        {
            var parts = fixString.Split(delimiter);

            var list = new List<TagBase>();

            foreach (var element in parts)
            {
                Console.WriteLine(element);
                var tagParts = element.Split('=');

                var tagName = tagParts[0];
                //var tagWithoutSoh = tagName.Replace("\u0001", "");

                if (!string.IsNullOrEmpty(tagName))
                {
                    switch (Convert.ToInt32(tagName))
                    {
                        case (int)Tags.BeginString:
                            list.Add(new BeginTag() { Value = tagParts[1] });
                            break;
                        case (int)Tags.BodyLength:
                            list.Add(new BodyLength() { Value = tagParts[1] });
                            break;
                    }
                }
            }

            Console.WriteLine("Recognized Tags ....");

            foreach (var element in list)
            {
                Console.WriteLine(string.Format("Tag={0} TagDescr={1} Value={2} ValueDescr={3}",
                    element.Tag, element.Tag, element.Value, element.ValueDescription));
            }
        }
    }


    [Serializable]
    public abstract class TagBase
    {
        public T ShallowCopy<T>() where T : TagBase
        {
            return (T)(MemberwiseClone());
        }


        public abstract string Tag { get; }
        public abstract string TagDescription { get; }
        public abstract string Value { get; set; }
        public abstract string ValueDescription { get; }

    }

    public enum Tags
    {
        BeginString = 8,
        BodyLength = 9
    }

    [Serializable]
    public class BeginTag : TagBase
    {

        #region implemented abstract members of Expression

        public override string Tag
        {
            get
            {
                return Tags.BeginString.ToString();
            }
        }

        public override string TagDescription
        {
            get
            {
                return "BeginString";
            }

        }

        public override string Value { get; set; }

        public override string ValueDescription
        {
            get
            {
                return "Begin of message";
            }
        }

        #endregion
    }

    [Serializable]
    public class BodyLength : TagBase
    {

        #region implemented abstract members of Expression

        public override string Tag
        {
            get
            {
                return Tags.BodyLength.ToString();
            }
        }

        public override string TagDescription
        {
            get
            {
                return "BodyLength";
            }

        }

        public override string Value { get; set; }

        public override string ValueDescription
        {
            get
            {
                return "Body Length";
            }
        }

        #endregion
    }
}
