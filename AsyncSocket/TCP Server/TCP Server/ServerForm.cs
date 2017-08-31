using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace TCP_Server
{
    public partial class ServerForm : Form
    {
        private Socket _serverSocket;
        private Socket _clientSocket;
        private byte[] _buffer;

        public ServerForm()
        {
            string path = @"C:\Users\ELROIS\Documents\Visual Studio 2015\Projects\AsyncSocket\TCP Client\bin\Debug\TCP Client.exe";
            Process.Start(path);
            InitializeComponent();
            StartServer();
        }

        private void StartServer()
        {
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 3333));
                _serverSocket.Listen(0);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            try
            {
                _clientSocket = _serverSocket.EndAccept(AR);
                _buffer = new byte[_clientSocket.ReceiveBufferSize];

                _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                AppendToTextBox("Client has connected");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = _clientSocket.EndReceive(AR);

                if(received == 0)
                {
                    return;
                }

                Array.Resize(ref _buffer, received);
                string text = Encoding.ASCII.GetString(_buffer);

                if(text == "-exit") //-exit 샌드 시 꺼짐
                {
                    _clientSocket.Close();
                    _serverSocket.Close();
                    Application.Exit();
                }

                Array.Resize(ref _buffer, _clientSocket.ReceiveBufferSize);
                AppendToTextBox("Client says : " + text);
                
                _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AppendToTextBox(string text)
        {
            MethodInvoker invoker = new MethodInvoker(delegate
            {
                textBox.Text += text + "\r\n";
            });

            this.Invoke(invoker);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
