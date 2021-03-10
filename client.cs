using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Player_Client
{
    private int port = 0;
    private Socket clientSocket;

    //the sequence number of the boards THIS side sends
    private int sendNum = 0;

    //the sequence number of the boards THE OTHER side sends
    private int recNum = 0;

    // Start is called before the first frame update
    public void Start()
    {
        //asks user for the "game number" which is the port of the server from the host
        Console.WriteLine("Input Game Number: ");
        string temp = Console.ReadLine();

        //turn the input into an int
        Int32.TryParse(temp, out port);

        // Get host related information.
        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

        // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
        // an exception that occurs when the host IP Address is not compatible with the address family
        // (typical in the IPv6 case).
        foreach (IPAddress address in hostEntry.AddressList)
        {
            IPEndPoint ipe = new IPEndPoint(address, port);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tempSocket.Connect(ipe);

            if (tempSocket.Connected)
            {
                clientSocket = tempSocket;
                break;
            }

            else
            {
                continue;
            }
        }
    }

    //send this board to the remote player and wait for ack, resend if no ack
    public void sendBoard(char[] board)
    {
        char[] convert = new char[11];

        //specify that this is a board not an ack
        convert[0] = 'b';
        convert[1] = Convert.ToChar(sendNum);
        sendNum++;

        int index = 0;
        for (int i = 2; i < 11; i++)
        {
            convert[i] = board[index];
            index++;
        }

        byte[] message = Encoding.GetEncoding("UTF-8").GetBytes(convert);

        clientSocket.Send(message, message.Length, 0);

        byte[] recieve = new Byte[256];
        string data = "";
        //get time of send
        DateTime start = DateTime.UtcNow;

        //wait for response
        while (true)
        {
            int numByte = clientSocket.Receive(message);

            data += Encoding.ASCII.GetString(message, 0, numByte);
            Console.WriteLine("Waiting for ack...");
            if (data.Length == 11)
                break;


            DateTime end = DateTime.UtcNow;
            TimeSpan lap = end - start;

            //if we haven't gotten an ack in 3 seconds, resend until timeout
            if (Convert.ToInt32(lap.TotalSeconds) > 3 && Convert.ToInt32(lap.TotalSeconds) < 15)
            {
                clientSocket.Send(message, message.Length, 0);
                continue;
            }

            //if we havent gotten an ack in 15 seconds, player probably close their connection, break
            if (Convert.ToInt32(lap.TotalSeconds) > 15)
            {
                //this is where we go back to main menu and display that the player has left
                break;
            }
        }

        char[] recieved = data.ToCharArray();

        //check if this isnt an ack or its the incorrect one
        if (recieved[0] != 'a' || recieved[1] != sendNum - 1)
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
        while (true)
        {
            Thread.Sleep(1000);

            Console.WriteLine("Waiting for user");

            int numByte = clientSocket.Receive(message);

            data += Encoding.ASCII.GetString(message,
                                        0, numByte);

            if (data.Length == 11)
                break;

        }

        //turn the data into a player board
        char[] playerBoard = data.ToCharArray();

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
            if (playerBoard[1] == recNum)
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
        else if (playerBoard[0] == 'w')
        {
            
            clientSocket.Close();
            return returnBoard;
        }

        return returnBoard;
    }

    public void sendWin(char[] board)
    {
        char[] convert = new char[11];

        //specify that this is a board not an ack
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

        clientSocket.Send(message, message.Length, 0);
        clientSocket.Close();
    }
    
}
