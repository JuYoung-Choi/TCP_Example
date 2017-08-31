using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class TCPSocket : MonoBehaviour {

    private string m_address = "";
    private const int m_port = 3333;

    private Socket m_listener = null;
    private Socket m_socket = null;

    private State m_state;

    enum State
    {
        SelectHost = 0,
        StartListener,
        AcceptClient,
        ServerCommunication,
        StopListener,
        ClientCommunication,
        Endcommunication,
    }

	void Start () {
        m_state = State.SelectHost;

        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress hostAddress = hostEntry.AddressList[0];
        m_address = hostAddress.ToString();
        Debug.Log(m_address);
	}
	
	void Update () {
        switch (m_state)
        {
            case State.StartListener: //서버 생성(서버 대기)
                StartListener();
                break;

            case State.AcceptClient: //클라이언트의 접속 대기
                AcceptClient();
                break;

            case State.ServerCommunication: //클라이언트의 메시지 수신
                ServerCommunication();
                break;

            case State.StopListener: //대기 종료
                StopListener();
                break;

            case State.ClientCommunication: //클라이언트의 접속, 메시지 송신, 접속 해제
                ClientProcess();
                break;

            default:
                break;
        }	
	}

    /*
    * Server
    */

    void StartListener()
    {
        Debug.Log("Start Server Communication.");

        //Server : Socket() -> Bind() -> Listen() [소켓 생성 -> 포트번호 할당(이어주는 역할) -> 대기]
        m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Socket()
        m_listener.Bind(new IPEndPoint(IPAddress.Any, m_port)); //Bind()
        m_listener.Listen(1); //Listen()

        m_state = State.AcceptClient; //클라이언트의 접속 대기
    }

    void AcceptClient()
    {
        //Socket.Poll(응답을 기다릴 시간, 폴링 모드 값에 따른 소켓의 상태) : 소켓의 상태 확인
        if(m_listener != null && m_listener.Poll(0, SelectMode.SelectRead)) 
        {
            m_socket = m_listener.Accept(); //Accept()
            Debug.Log("[TCP]Connected form Client.");

            m_state = State.ServerCommunication; //클라이언트의 메시지 수신
        }
    }

    //서버에서 클라이언트의 메시지 수신
    void ServerCommunication()
    {
        byte[] buffer = new byte[1400];
        int recvSize = m_socket.Receive(buffer, buffer.Length, SocketFlags.None); //Receive()
        if(recvSize > 0)
        {
            string message = System.Text.Encoding.UTF8.GetString(buffer);
            Debug.Log(message);
            m_state = State.StopListener; //대기 종료
        }
    }

    //서버 대기 종료
    void StopListener()
    {
        if(m_listener != null)
        {
            m_listener.Close(); //Close()
            m_listener = null;
        }

        m_state = State.Endcommunication;

        Debug.Log("[TCP]ENd server communication.");
    }

    /*
    * Client
    */

    void ClientProcess()
    {
        Debug.Log("[TCP]Start client communication.");

        //Clinet : Socket() -> Connect() [소켓 생성 -> 연결]
        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Socket()
        m_socket.NoDelay = true;
        m_socket.SendBufferSize = 0;
        m_socket.Connect(m_address, m_port); //Connect()

        //메시지 송신 -> ServerCommunication()에서 메시지 수신
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello, I am Client.");
        m_socket.Send(buffer, buffer.Length, SocketFlags.None); //Sned()

        //접속 해제
        m_socket.Shutdown(SocketShutdown.Both);
        m_socket.Close(); //Close()

        Debug.Log("[TCP]End client communication");
    }
    
    /*
     * GUI
     */

    void OnGUI()
    {
        if(m_state == State.SelectHost)
        {
            OnGUISelectHost();
        }
    }

    void OnGUISelectHost()
    {
        if(GUI.Button(new Rect(20, 40, 150, 20), "Launch server.")){
            m_state = State.StartListener; //서버 생성(서버 대기)
        }

        m_address = GUI.TextField(new Rect(20, 100, 200, 20), m_address);
        if(GUI.Button(new Rect(20, 70, 150, 20), "Connect to Server"))
        {
            m_state = State.ClientCommunication; //클라이언트의 접속, 메시지 송신, 접속 해제
        }
    }
}
