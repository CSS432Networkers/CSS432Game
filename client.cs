using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class GetSocket
{
    private string host = "localhost"; // needs to be server ip address
    private int port = 4183;

    // This method requests the home page content for the specified server.
    private static string SocketSendReceive(Socket sock, char[] playerBoard)
    {
        string request = "";//this will be the string that we create using the input of the user on the screen

        for(int i = 0; i < playerBoard.Length; i++)
        {
            request += playerBoard[i];
        }
        Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
        Byte[] bytesReceived = new Byte[256];
        string page = "";

        // Create a socket connection with the specified server and port.
        using(sock) {

            if (sock == null)
                return ("Connection failed");

            // Send request to the server.
            sock.Send(bytesSent, bytesSent.Length, 0);

            //Recieve the reply from the server
            int bytes = 0;
            page = "Default HTML page on " + host + ":\r\n";

            // The following will block until the page is transmitted.
            do {
                bytes = sock.Receive(bytesReceived, bytesReceived.Length, 0);
                page = page + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
            }
            while (bytes > 0);
        }

        return page;
    }

    public static void Main(string[] args)
    {
        cout << "Input Host Name:" << endl;
        cin >> host;

        cout << "Input Game Number:" << endl;
        cin >> port;

        Socket s = null;
        IPHostEntry hostEntry = null;

        // Get host related information.
        hostEntry = Dns.GetHostEntry(host);

        // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
        // an exception that occurs when the host IP Address is not compatible with the address family
        // (typical in the IPv6 case).
        foreach(IPAddress address in hostEntry.AddressList)
        {
            IPEndPoint ipe = new IPEndPoint(address, port);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tempSocket.Connect(ipe);

            if(tempSocket.Connected)
            {
                s = tempSocket;
                break;
            }

            else
            {
                continue;
            }
        }

        //generate new player board
        char[] playerBoard = new char[9];

        //send the first request to the host to retrieve board information
        string result = SocketSendReceive(s, playerBoard);
        
        //game is running
        while(true)
        {
            //generate the new board on return
            for(int i = 0; i < result.Length; i++)
            {
                playerBoard[i] = result[i];
            }

            //process player board and put it on the screen

            //send the inputs of this player to the server and recieve new board info
            result = SocketSendReceive(s, playerBoard);

            //display the new board
            Console.WriteLine(result);
            if(/*game over*/)
            {
                cout << "Game Over" << endl;
                break;
            }
        }

        s.Close();
    }
}