using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//Aggiunta delle seguenti librerie
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace socket
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() 
        {
            InitializeComponent();
            
        }

        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            //Aggiungo il mio indirizzo IP personale, lo aggiunge ed inizia la ricezione con il thread
            btnInvia.IsEnabled = true;
            btnCreaSocket.IsEnabled = false;
            string persIP = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostAddresses(persIP);
            foreach (IPAddress ip in ips)
            {
                persIP = ip.ToString();
            }
            IPEndPoint sourceSocket = new IPEndPoint(IPAddress.Parse(persIP), 56000);
            Thread ricezione = new Thread(new ParameterizedThreadStart(SocketReceive));
            ricezione.Start(sourceSocket);

        }

        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            //invia il messaggio al destinatario con indirizzo IP e porta uguale a quelli delle texbox.
            string ipAddress;
            int port;
            IPAddress ip;
            if (IPAddress.TryParse(txtIP.Text,out ip) && int.TryParse(txtPort.Text, out port))
            {
               ipAddress = txtIP.Text;
               port = int.Parse(txtPort.Text);
               SocketSend(IPAddress.Parse(ipAddress), port, txtMsg.Text);
            }
            else
            {
                MessageBox.Show("IP o porta sbagliati", "errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }

           


        }

        public async void SocketReceive(object socksource)
        {
            //in questa parte riceviamo i messaggi e teniamo l'apparato in ascolto 
            IPEndPoint ipendp = (IPEndPoint)socksource;

            Socket t = new Socket(ipendp.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            t.Bind(ipendp);

            Byte[] bytesRicevuti = new Byte[256];

            string message;

            int contaCaratteri = 0;

            await Task.Run(() =>
            {
                while (true)
                {
                    if(t.Available >0)
                    {
                        message = "";

                        contaCaratteri = t.Receive(bytesRicevuti, bytesRicevuti.Length,0);
                        message = message + Encoding.ASCII.GetString(bytesRicevuti, 0, contaCaratteri);

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {

                            lstmessaggi.Items.Add(message);
                        }));

                    }

                }

            });

        }

        public void SocketSend(IPAddress dest, int destport, string message)
        {
            //in questa parte invece inviamo i messaggi
            Byte[] byteInviati = Encoding.ASCII.GetBytes(message);

            Socket s = new Socket(dest.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint remote_endpoint = new IPEndPoint(dest, destport);

            s.SendTo(byteInviati, remote_endpoint);
        }
    
    }
}
