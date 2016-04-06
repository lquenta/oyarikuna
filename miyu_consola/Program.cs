using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace miyu_consola
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //obtain DNS host name
                //string ip = GetPublicIP();
                Console.Write("Escribir IP a conectarse:");
                string ip = Console.ReadLine(); ;
                IPAddress IP_address = IPAddress.Parse(ip);


                //IPEndPoint end_point = new IPEndPoint(IP_address, 3306);
                //TcpClient client = new TcpClient(end_point);
                
                //TcpClient client = new TcpClient("localhost", 3307);
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                client.Connect(IP_address, 3307);

                try
                {
                    Stream s = client.GetStream();
                    StreamReader sr = new StreamReader(s);
                    StreamWriter sw = new StreamWriter(s);
                    sw.AutoFlush = true;
                    Console.WriteLine(sr.ReadLine());
                    while (true)
                    {
                        Console.Write("Comando: ");
                        string comando = Console.ReadLine();
                        sw.WriteLine(comando);
                        if (sr.ReadLine() == "file")
                        {
                            Console.Write("Receiving file");
                            string cmdFileSize = sr.ReadLine();
                            string cmdFileName = sr.ReadLine();
                            int length = Convert.ToInt32(cmdFileSize);
                            byte[] buffer = new byte[length];
                            int received = 0;
                            int read = 0;
                            int size = 1024;
                            int remaining = 0;
                            while (received < length)
                            {
                                remaining = length - received;
                                if (remaining < size)
                                {
                                    size = remaining;
                                }
                                read = client.GetStream().Read(buffer, received, size);
                                received += read;

                            }
                            using (FileStream fStream = new FileStream(Path.GetFileName(cmdFileName), FileMode.Create))
                            {
                                fStream.Write(buffer, 0, buffer.Length);
                                fStream.Flush();
                                fStream.Close();
                            }
                            Console.WriteLine("archivo recibido y guardado");
                        }
                        else
                        {
                            string respuesta = sr.ReadLine();
                            Console.WriteLine(sr.ReadLine());
                        }

                    }

                }
                finally
                {
                    // code in finally block is guranteed 
                    // to execute irrespective of 
                    // whether any exception occurs or does 
                    // not occur in the try block
                    client.Close();
                }
            }catch(Exception ex){
                Console.WriteLine(ex.Message);
            }

        }
    }
}
