using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//Player_Client class is the netcode used to communicate between two instances of a tiktaktoe game
//It has functions that allow for connection and creation for both a host and client
public class Player_Client
{
    //port number to be used in later sections
    private int port = 0;

    //socket that will be used for connecting to the server/client
    private Socket clientSocket;

    //socket used to accept incoming connections when hosting a LAN
    private Socket listener;

    //the sequence number of the boards THIS side sends
    private int sendNum = 0;

    //the sequence number of the boards THE OTHER side sends
    private int recNum = 0;

    /**
     * hostLocal
     * this function is used to create a LAN game for a HOST instance. It will generate a random 
     * port number that will be displayed to the user and the user will have to give the number to
     * the person they are trying to play with.
    **/
    public void hostLocal()
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

    /*
     * startLocal
     * Function that is used to join a LAN game as a CLIENT. To be able to use this, a hostLocal
     * must have been called on a local network machine and is currently running a LAN host instance.
     * 
     * Return:
     * returns an int based on the status of the connection. If the port number that was used was not
     * correct, it will catch an error and return a -1. Status must be handled by main.cs
     */
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

            //try to connect and if connection is failed, return -1
            try
            {
                tempSocket.Connect(ipe);
            }
            catch (Exception e)
            {
                return -1;
            }

            //if the connection is successful, return 0 for success
            if (tempSocket.Connected)
            {
                clientSocket = tempSocket;
                return 0;
            }

            //if this address is not viable, go to next address
            else
            {
                continue;
            }
        }

        //if we've exhausted all addresses and havent connected, return -1
        return -1;
    }

    /**
     *  startRemote
     *  function used to create an instance of an online multiplayer game. The user must specify the
     *  LinuxLab server that the server is being ran on to connect to it. 
     *  
     *  Return
     *  returns an int, 0 if the connection is successful and -1 if the connection has failed
     **/
    public int startRemote(string addr)
    {

        IPHostEntry hostEntry = null;

        //check the host entry with the given server, if not able to resolve, return -1
        try
        {
            // Get host related information.
            hostEntry = Dns.GetHostEntry(addr);

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
            //get enpoint using the address and given port of 4183, my specific ID so that there is no port complication
            IPEndPoint ipe = new IPEndPoint(address, 4183);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //try to connect using the endpoint, if the endpoint connection is failed, return -1
            try
            {
                tempSocket.Connect(ipe);
            }
            catch(Exception e)
            {
                return -1;
            }

            //if the connection is successful, return success
            if (tempSocket.Connected)
            {
                clientSocket = tempSocket;
                return 0;
            }

            else
            {
                continue;
            }
        }

        //if we've exhausted all the addresses and cant connect, return failure
        return -1;
    }

    /**
     *  sendBoard
     *  function that takes a char array and turns it into a "board" message to be sent to the other party
     *  over the socket.
     *  
     *  Return
     *  returns a bool based on the status of the send. If the board is not sent properly (server dies, other player disconnects)
     *  then the function returns a false, if the message is sent properly (recieve an ack) then it returns true
     **/
    public bool sendBoard(char[] board)
    {
        //char array that is appended with the necessary info
        char[] convert = new char[11];

        //specify that this is a board not an ack
        convert[0] = 'b';
        //give this message a sequence number
        convert[1] = Convert.ToChar(sendNum);
        //increase the sequence number
        sendNum++;

        //populate the conver array with the contents of the given array
        int index = 0;
        for (int i = 2; i < 11; i++)
        {
            convert[i] = board[index];
            index++;
        }

        //turn the array into a byte array
        byte[] message = Encoding.GetEncoding("UTF-8").GetBytes(convert);

        //try to send it to the other player, if the send fails (socket forcibly closed), return false
        try
        {
            clientSocket.Send(message, message.Length, 0);
        }
        catch(Exception e)
        {
            return false;
        };
        
        //string to append data to 
        string data = "";

        //get time of send
        DateTime start = DateTime.UtcNow;

        //counter for amount of times a recieve is attempted
        int retryCount = 0;
        
        //wait for response
        while (true)
        {
            Console.WriteLine("Waiting for ack...");
            
            //increment retry count each time a recieve is tried
            retryCount++;
            int numByte = clientSocket.Receive(message);

            //if we've retried 3 times, return false
            if (retryCount == 3)
            {
                return false;
            }

            //fill data with the message from the other player
            data += Encoding.ASCII.GetString(message, 0, numByte);
            
            if (data.Length == 11)
                break;

            //lap the timer
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
                return false;
            }
        }
        
        //turn the string into a char array
        char[] recieved = data.ToCharArray();

        //check to see if the message type is of 'e', meaning that the opponent left (only happens when playing online multiplayer)
        if (recieved[0] == 'e')
        {
            return false;
        }

        //check if this isnt an ack or if its the incorrect one and try again
        if (recieved[0] != 'a' || recieved[1] != sendNum - 1)
        {
            sendBoard(board);
        }

        //if we've passed all test cases return true
        return true;
        
    }

    /*
     * recieveBoard
     * function called when wanting to get the new board information (while we're waiting for opponent to send in
     * their move)
     * 
     * Return
     * returns a char array representing the new board that the opponent has sent through the socket. Return null
     * if the opponent disconnected or if the server died
     */
    public char[] recieveBoard()
    {
        //string to save data to from message buff
        string data = null;
        byte[] message = new byte[256];

        //counter for amount of retries to recieve
        int retryCount = 0;

        //wait for data
        while (true)
        {
            //sleep to wait for travel
            Thread.Sleep(1000);

            Console.WriteLine("Waiting for user");
            
            //increment retry on every attempt to recieve
            retryCount++;

            int numByte = clientSocket.Receive(message);
            
            //save the data 
            data += Encoding.ASCII.GetString(message,
                                        0, numByte);

            //if we've retried 3 times, return null
            if (retryCount == 3)
            {
                return null;
            }

            //once we've gotten all the data, break
            if (data.Length == 11)
                break;

        }

        //turn the data into a player board
        char[] playerBoard = data.ToCharArray();

        //populate the return board with only the board values and not the message values (message type, seq num)
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

        //if we recieve message type 'e' it means the opponent disconnected, return null
        if (playerBoard[0] == 'e')
        {
            return null;
        }

        //if message type w, close the socket and return the board
        else if (playerBoard[0] == 'w')
        {     
            clientSocket.Close();
            return returnBoard;
        }
        else if(playerBoard[0] == 'd')
        {
            clientSocket.Close();
            return returnBoard;
        }
        
        //if we've passed all test cases, return the board
        return returnBoard;
    }

    /*
     * sendWin
     * function to send a message with a 'w' type message, specifying that the user has won
     */
    public void sendWin(char[] board)
    {
        //char array to convert board into a message
        char[] convert = new char[11];

        //specify that this is a win message
        convert[0] = 'w';
        convert[1] = Convert.ToChar(sendNum);
        sendNum++;

        //populate the message with the board values
        int index = 0;
        for (int i = 2; i < 11; i++)
        {
            convert[i] = board[index];
            index++;
        }

        //turn it into a byte array
        byte[] message = Encoding.GetEncoding("UTF-8").GetBytes(convert);

        //send the message to the server/opponent and close
        clientSocket.Send(message, message.Length, 0);
        clientSocket.Close();
    }

    public void sendDraw(char[] board)
    {
        //char array to convert board into a message
        char[] convert = new char[11];

        //specify that this is a win message
        convert[0] = 'd';
        convert[1] = Convert.ToChar(sendNum);
        sendNum++;

        //populate the message with the board values
        int index = 0;
        for (int i = 2; i < 11; i++)
        {
            convert[i] = board[index];
            index++;
        }

        //turn it into a byte array
        byte[] message = Encoding.GetEncoding("UTF-8").GetBytes(convert);

        //send the message to the server/opponent and close
        clientSocket.Send(message, message.Length, 0);
        clientSocket.Close();
    }
    
}
