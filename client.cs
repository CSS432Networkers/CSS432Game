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
    public int startLocal()
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

            try
            {
                tempSocket.Connect(ipe);
            }
            catch (Exception e)
            {
                return -1;
            }

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
        return 0;
    }

    // Start is called before the first frame update
    public int startRemote()
    {
        Console.WriteLine("Input Server Name: (ex: csslab11.uwb.edu)");
        string temp = Console.ReadLine();

        IPHostEntry hostEntry = null;

        try
        {
            // Get host related information.
            hostEntry = Dns.GetHostEntry(temp);

        }
        catch (SocketException e)
        {
            //Console.WriteLine("Bad Server Name");
            return -1;
        };

        // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
        // an exception that occurs when the host IP Address is not compatible with the address family
        // (typical in the IPv6 case).
        foreach (IPAddress address in hostEntry.AddressList)
        {
            IPEndPoint ipe = new IPEndPoint(address, 4183);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                tempSocket.Connect(ipe);
            }
            catch(Exception e)
            {
                return -1;
            }

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
        return 0;
    }

    //send this board to the remote player and wait for ack, resend if no ack
    public bool sendBoard(char[] board)
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

        int retryCount = 0;
        
        //wait for response
        while (true)
        {
            Console.WriteLine("Waiting for ack...");
            retryCount++;
            int numByte = clientSocket.Receive(message);

            if (retryCount == 3)
            {
                return false;
            }

            data += Encoding.ASCII.GetString(message, 0, numByte);
            
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

        if (recieved[0] == 'e')
        {
            return false;
        }

        //check if this isnt an ack or its the incorrect one
        if (recieved[0] != 'a' || recieved[1] != sendNum - 1)
        {
            sendBoard(board);
        }
        return true;
        
    }

    //called to wait and recieve from the remote player
    public char[] recieveBoard()
    {
        string data = null;
        byte[] message = new byte[256];
        int retryCount = 0;

        //bool properData = false;
        //wait for data
        while (true)
        {
            Thread.Sleep(1000);

            Console.WriteLine("Waiting for user");
            retryCount++;

            int numByte = clientSocket.Receive(message);

            data += Encoding.ASCII.GetString(message,
                                        0, numByte);

            /*
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != '\0')
                {
                    properData = true;
                    break;
                }
            }
            */

            if (retryCount == 3)
            {
                return null;
            }

            //once we've gotten all the data, break
            if (data.Length == 11)// && properData == true)
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

        if (playerBoard[0] == 'e')
        {
            return null;
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
