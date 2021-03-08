using System; 
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
  
namespace Server { 
  
class Program { 
  
private string host = "localhost"; // needs to be local ip address
private int port = 0;    //needs to be randomly generated

// Main Method 
static void Main(string[] args) 
{ 

    Random rand = new Random();
    port = Random.Next(1,1000) + 1024;

    // Establish the local endpoint  
    // for the socket. Dns.GetHostName 
    // returns the name of the host  
    // running the application. 
    IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
    IPAddress ipAddr = ipHost.AddressList[0]; 
    IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port); 
  
    //maybe print out the host name and port so that client side can connect to server?
    //have no idea how we'd create the connection on client side without providing both
    //server name and port number.
    //---------------cout << "Host: " << Dns.GetHostName() << endl;---------------
    //---------------cout << "Game Number: " << port << endl;---------------

    // Creation TCP/IP Socket using  
    // Socket Class Costructor 
    Socket listener = new Socket(ipAddr.AddressFamily, 
                 SocketType.Stream, ProtocolType.Tcp); 
  
    try { 
          
        // Using Bind() method we associate a 
        // network address to the Server Socket 
        // All client that will connect to this  
        // Server Socket must know this network 
        // Address 
        listener.Bind(localEndPoint); 
  
        // Using Listen() method we create  
        // the Client list that will want 
        // to connect to Server 
        listener.Listen(10); 
  
        while (true) { 
              
            Console.WriteLine("Waiting connection ... "); 
  
            // Suspend while waiting for 
            // incoming connection Using  
            // Accept() method the server  
            // will accept connection of client 
            Socket clientSocket = listener.Accept(); 
  
            // Data buffer 
            byte[] bytes = new Byte[37]; 
            string data = null; 
  
            while (true) { 
  
                int numByte = clientSocket.Receive(bytes); 
                  
                data += Encoding.ASCII.GetString(bytes, 
                                           0, numByte); 
                                             
                if (data.IndexOf("<EOF>") > -1) 
                    break; 
            } 

            char[] playerBoard = Encoding.Unicode.GetChars(bytes);
            
            //waiting for player to input and send
            while(true)
            {
                if(/*player locks in*/)
                {
                    //update playerBoard
                    break;
                }
            }

            // Send a message to Client  
            // using Send() method 
            clientSocket.Send(message); 
        }

        //when game ends close socket and end game
        clientSocket.Shutdown(SocketShutdown.Both); 
        clientSocket.Close();  
    } 
      
    catch (Exception e) { 
        Console.WriteLine(e.ToString()); 
    } 
} 
} 
} 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class Player_Server : MonoBehaviour
{
    
    private string host = "";
    private int port = 0;
    private Socket clientSocket;

    // Start is called before the first frame update
    void Start()
    {
        Random rand = new Random();
        port = rand.Next(1, 1000) + 1024;

        // Establish the local endpoint  
        // for the socket. Dns.GetHostName 
        // returns the name of the host  
        // running the application. 
        IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

        //maybe print out the host name and port so that client side can connect to server?
        //have no idea how we'd create the connection on client side without providing both
        //server name and port number.
        //---------------cout << "Host: " << Dns.GetHostName() << endl;---------------
        //---------------cout << "Game Number: " << port << endl;---------------

        // Creation TCP/IP Socket using  
        // Socket Class Costructor 
        Socket listener = new Socket(ipAddr.AddressFamily,
                     SocketType.Stream, ProtocolType.Tcp);

        try
        {

            // Using Bind() method we associate a 
            // network address to the Server Socket 
            // All client that will connect to this  
            // Server Socket must know this network 
            // Address 
            listener.Bind(localEndPoint);

            // Using Listen() method we create  
            // the Client list that will want 
            // to connect to Server 
            listener.Listen(10);

            //wait for connection to be made
            while (true)
            {

                Debug.Log("Waiting connection ... ");

                // Suspend while waiting for 
                // incoming connection Using  
                // Accept() method the server  
                // will accept connection of client 
                clientSocket = listener.Accept();

                // turn the board into a char array and convert it to bytes to send through the socket.
                char[] playerBoard  /*= board object turned into char array*/;
                byte[] message = Encoding.GetEncoding("UTF-8").GetBytes(playerBoard);

                // send the board to the remote player for them to modify
                clientSocket.Send(message);

                break;

                //once connection has been made, 
                while (true)
                {
                    int numByte = clientSocket.Receive(bytes);

                    data += Encoding.ASCII.GetString(bytes,
                                               0, numByte);

                    if (data.IndexOf("<EOF>") > -1)
                        break;
                }

            }
        }
        catch (Exception e)
        {
            //write error to screen
            //Console.WriteLine(e.ToString());
        }
    }

    //send this board to the remote player
    void sendBoard(byte[] message)
    {
        clientSocket.Send(message);
    }

    char[] byteToChar(byte[] message)
    {
        return Encoding.GetEncoding("UTF-8").GetChars(message);
    }
    
    
    //char[] boardToArr(/*board object*/)
    //{
        //char[] playerBoard;
        // function to turn the board object into an array

        //return char[] playerBoard;
    //}
    
    //called to wait and recieve from the remote player
    void recieveBoard()
    {
        string data = null;
        byte[] message = new byte[100];

        //wait for data
        while(true)
        {
            int numByte = clientSocket.Receive(message);

            data += Encoding.ASCII.GetString(message,
                                        0, numByte);

            if (data.IndexOf("<EOF>") > -1)
                break;
        
        }

        char[] playerBoard = data.ToCharArray(0, data.Length - 1);
    }

    // Update is called once per frame
    void Update()
    {
        //we can use Update to check when to close the connection 
        //(when the player hits the exit button in the middle of the game, or after a game)
    }

}


