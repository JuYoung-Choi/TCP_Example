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

namespace TCP_Client
{
    public partial class ClientForm : Form
    {
        private Socket _clientSocket;

        public ClientForm()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if(_clientSocket != null && _clientSocket.Connected)
            {
                _clientSocket.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(txtInput.Text), 3333), new AsyncCallback(ConnectCallback), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                _clientSocket.EndConnect(AR);
                UpdateControlStates(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateControlStates(bool toggle)
        {
            MethodInvoker invoker = new MethodInvoker(delegate
            {
                btnSend.Enabled = toggle;
                btnConnect.Enabled = !toggle;
                lblIP.Visible = txtInput.Visible = !toggle;
            });
            this.Invoke(invoker);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(textBox.Text);
                _clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateControlStates(false);
            }
        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                _clientSocket.EndSend(AR);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
