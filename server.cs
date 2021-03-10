using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Player_Server
{
    private int port = 0;
    private Socket clientSocket;
    private Socket listener;

    //the sequence number of the boards THIS side sends
    private int sendNum = 0;

    //the sequence number of the boards THE OTHER side sends
    private int recNum = 0;

    // Start is called before the first frame update
    public void Start()
    {
        //randomizes a port number to be used
        Random rand = new Random();
        port = rand.Next(1, 1000) + 1024;

        // Establish the local endpoint  
        // for the socket. Dns.GetHostName 
        // returns the name of the host  
        // running the application. 
        IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

        //display port for remote player to connect to
        Console.WriteLine("Game Number: " + port);

        // Creation TCP/IP Socket using  
        // Socket Class Costructor 
        listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

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
            listener.Listen(1);
            
            //wait for connection to be made
            while (true)
            {

                //Debug.Log("Waiting connection ... ");

                // Suspend while waiting for 
                // incoming connection Using  
                // Accept() method the server  
                // will accept connection of client 
                clientSocket = listener.Accept();
                break;

            }
        }
        catch (Exception e)
        {
            //write error to screen
            Console.WriteLine(e.ToString());
        }
    }

    //send this board to the remote player and wait for ack, resend if no ack
    public void sendBoard(char[] board)
    {
        //char array that will be used to append the sequence number and message type to passed board
        char[] convert = new char[11];

        //specify that this is a board not an ack
        convert[0] = 'b';

        //specify this board's sequence number and increment
        convert[1] = Convert.ToChar(sendNum);
        sendNum++;

        //add the contents of the passed board to the new message
        int index = 0;
        for(int i = 2; i < 11; i++)
        {
            convert[i] = board[index];
            index++;
        }

        //turn the board into a byte array to send over the socket
        byte[] message = Encoding.GetEncoding("UTF-8").GetBytes(convert);

        //send to the remote player
        clientSocket.Send(message, message.Length, 0);

        byte[] recieve = new Byte[256];
        string data = "";

        //get time of send
        DateTime start = DateTime.UtcNow;

        //wait for response
        while (true)
        {
            // recieve the message
            int numByte = clientSocket.Receive(message);

            //take the data from the message and put it into a string
            data += Encoding.ASCII.GetString(message, 0, numByte);
            Console.WriteLine("Waiting for ack...");

            //once the string has gotten the whole size of the message (11 chars), exit
            if (data.Length == 11)
                break;


            //take the time now and lap incase we take too long to recieve
            DateTime end = DateTime.UtcNow;
            TimeSpan lap = end - start;

            //if we haven't gotten an ack in 3 seconds, resend until timeout
            if(Convert.ToInt32(lap.TotalSeconds) > 3 && Convert.ToInt32(lap.TotalSeconds) < 15)
            {
                clientSocket.Send(message, message.Length, 0);
                continue;
            }

            //if we havent gotten an ack in 15 seconds, player probably close their connection, break
            if(Convert.ToInt32(lap.TotalSeconds) > 15)
            {
                //this is where we go back to main menu and display that the player has left
                break;
            }
        }

        //turn the data string into a char array
        char[] recieved = data.ToCharArray();

        //check if this isnt an ack or its the incorrect one
        if(recieved[0] != 'a' || recieved[1] != sendNum - 1)
        {
            sendBoard(board);
        }
    }

    //called to wait and recieve from the remote player
    public char[] recieveBoard()
    {
        string data = null;
        byte[] message = new byte[256];

        //wait for data
        while(true)
        {
            //sleep just to wait and print that we're waiting for user input
            Thread.Sleep(1000);
            Console.WriteLine("Waiting for user");

            //recieve the input and turn the message into a string
            int numByte = clientSocket.Receive(message);

            data += Encoding.ASCII.GetString(message,
                                        0, numByte);

            //once we've gotten all the data, break
            if (data.Length == 11)
                break;
        
        }

        //turn the data into a player board
        char[] playerBoard = data.ToCharArray();

        //remove the identifiers from the start of the message (message type, sequence number)
        char[] returnBoard = new char[9];
        int index = 0;
        for (int i = 2; i < 11; i++)
        {
            returnBoard[index] = playerBoard[i];
            index++;
        }


        //if message recieved is board
        if (playerBoard[0] == 'b')
        {
            //if this is the correct board
            if(playerBoard[1] == recNum)
            {
                //send the ack back to the sender
                char[] ack = new char[11];
                ack[0] = 'a';
                ack[1] = Convert.ToChar(recNum);

                message = Encoding.GetEncoding("UTF-8").GetBytes(ack);

                clientSocket.Send(message, message.Length, 0);

                //increment the sequence number of recieved boards
                recNum++;
            }
        }

        //if the message is a "win condition" message, this player has lost
        else if(playerBoard[0] == 'w')
        {
            //close the socket and return the updated board
            clientSocket.Close();
            return returnBoard;
        }

        //return the updated board if it isnt a loss
        return returnBoard;
    }

    //function to send the final message stating the user has won
    public void sendWin(char[] board)
    {
        char[] convert = new char[11];

        //specify that this is a board is a win condition board
        convert[0] = 'w';
        convert[1] = Convert.ToChar(sendNum);
        sendNum++;

        int index = 0;
        for (int i = 2; i < 11; i++)
        {
            convert[i] = board[index];
            index++;
        }

        byte[] message = Encoding.GetEncoding("UTF-8").GetBytes(convert);

        //send the board and close the socket
        clientSocket.Send(message, message.Length, 0);
        clientSocket.Close();
    }

}


