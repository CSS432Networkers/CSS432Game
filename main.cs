using System;
using System.IO;


namespace tictactoe_emergency
{
    class main
    {
        private static char[,] board = new char[3,3];
        private static bool[] freeSpots = new bool[9];
        static void Main(string[] args)
        {

            Console.WriteLine("Host(1) or Player(2)?");
            string status = Console.ReadLine();

            int choice = 3;

            if(status.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                choice = 0;
            }
            else if(status.Equals("2", StringComparison.InvariantCultureIgnoreCase))
            {
                choice = 1;
            }

            if(choice == 0)
            {
                Player_Client host = new Player_Client();
                initBoard();
                hostGame(host);
            }

            else if (choice == 1)
            {
                Player_Client client = new Player_Client();
                initBoard();
                clientGame(client);
            }

            else
            {
                Console.WriteLine("Not Valid Input");
            }

        }

        private static void initBoard()
        {
            int index = 0;
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    board[i, j] = ' ';
                    freeSpots[index] = true;
                    index++;
                }
            }
            
        }

        private static void clientGame(Player_Client client)
        {
            Console.WriteLine("LAN(1) or Online(2)");
            string input = Console.ReadLine();

            if (input.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                int initStatus = -1;
                bool retry = false;

                while(initStatus == -1)
                {
                    if(retry == true)
                    {
                        Console.WriteLine("Bad Port");
                    }

                    initStatus = client.startLocal();
                    if (initStatus == 0)
                    {
                        break;
                    }
                    retry = true;
                }
            }
            else if (input.Equals("2", StringComparison.InvariantCultureIgnoreCase))
            {
                int initStatus = -1;
                bool retry = false;

                //while the server name is bad
                while(initStatus == -1)
                {
                    //print bad server name if we've retried atleast once
                    if(retry == true)
                    {
                        Console.WriteLine("Bad Server Name");
                    }

                    //take server name input
                    Console.WriteLine("Input Server Name: (ex: csslab11.uwb.edu)");
                    string temp = Console.ReadLine();

                    //if the status is successful, break
                    initStatus = client.startRemote(temp);
                    if (initStatus == 0)
                    {
                        int[] rooms = client.recieveRooms();
                        Console.WriteLine("What room would you like to choose?");
                        for (int i = 0; i < rooms.Length; i++)
                        {
                            Console.WriteLine(rooms[i] + 1);
                        }
                        string roomChoice = "";

                        while(true)
                        {
                            roomChoice = Console.ReadLine();
                            if (int.Parse(roomChoice) > 5 || int.Parse(roomChoice) < 0)
                            {
                                Console.WriteLine("Bad Room Number");
                                continue;
                            }
                            break;
                        }

                        client.sendRoom(roomChoice[0]);

                        Console.WriteLine("Game Started");
                        break;
                    }

                    retry = true;
                }

                Console.WriteLine("Waiting for opponent");
            }

            bool game = true;

            while (game)
            {
                //create array for board
                char[] boardToSend = new char[9];

                //recieve the board and update the board, recieveBoard freezes user while waiting for return
                char[] returnBoard = client.recieveBoard();
                if(returnBoard == null)
                {
                    Console.WriteLine("Opponent has quit, ending game");
                    return;
                }

                updateBoard(returnBoard);

                char winner = checkWin();
                if (winner == 'x')
                {
                    Console.WriteLine(createDisplay() + "\nOpponent Wins\nClosing connection");
                    return;
                }

                if (winner == 'd')
                {
                    Console.WriteLine(createDisplay() + "\nDraw");
                    return;
                }

                //take user input for position they wanna fill
                int check = displayBoard();

                //update board to put an 'x' where user inputs
                populateBoard(ref boardToSend, check, "client");

                winner = checkWin();

                if (winner == 'o')
                {

                    client.sendWin(boardToSend);
                    Console.WriteLine(createDisplay() + "\nYou Win!");
                    return;
                }

                if (winner == 'd')
                {
                    client.sendDraw(boardToSend);
                    Console.WriteLine(createDisplay() + "\nDraw");
                    return;
                }

                //send the board
                bool sendStatus = client.sendBoard(boardToSend);
                if(sendStatus == false)
                {
                    Console.WriteLine("\nOppenent has quit, closing game");
                    return;
                }

            }
        }

        private static void hostGame(Player_Client host)
        {
            Console.WriteLine("LAN(1) or Online(2)");
            string input = Console.ReadLine();

            if(input.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                host.hostLocal();
            }
            else if(input.Equals("2", StringComparison.InvariantCultureIgnoreCase))
            {
                int initStatus = -1;
                bool retry = false;

                //while the server name is bad
                while (initStatus == -1)
                {
                    //print bad server name if we've retried atleast once
                    if (retry == true)
                    {
                        Console.WriteLine("Bad Server Name");
                    }

                    //take server name input
                    Console.WriteLine("Input Server Name: (ex: csslab11.uwb.edu)");
                    string temp = Console.ReadLine();
 
                    //if the status is successful, break
                    initStatus = host.startRemote(temp);
                    if (initStatus == 0)
                    {
                        int[] rooms = host.recieveRooms();
                        Console.WriteLine("What room would you like to choose?");
                        for (int i = 0; i < rooms.Length; i++)
                        {
                            Console.WriteLine(rooms[i] + 1);
                        }
                        string roomChoice = "";

                        while (true)
                        {
                            roomChoice = Console.ReadLine();
                            if (int.Parse(roomChoice) > 5 || int.Parse(roomChoice) < 0)
                            {
                                Console.WriteLine("Bad Room Number");
                                continue;
                            }
                            break;
                        }

                        host.sendRoom(roomChoice[0]);

                        Console.WriteLine("Game Started");
                        break;
                    }

                    retry = true;
                }
            }

            bool game = true;

            while(game)
            {
                //create array for board
                char[] boardToSend = new char[9];

                //take user input for position they wanna fill
                int check = displayBoard();

                //update board to put an 'x' where user inputs
                populateBoard(ref boardToSend, check, "host");

                char winner = checkWin();
                if (winner == 'x')
                {
                    host.sendWin(boardToSend);
                    Console.WriteLine(createDisplay() + "\nYou Win!");
                    return;
                }
                
                if(winner == 'd')
                {
                    host.sendDraw(boardToSend);
                    Console.WriteLine(createDisplay() + "\nDraw");
                    return;
                }

                //send the board
                bool sendStatus = host.sendBoard(boardToSend);
                if (sendStatus == false)
                {
                    Console.WriteLine("\nOppenent has quit, closing game");
                    return;
                }

                //recieve the board and update the board, recieveBoard freezes user while waiting for return
                char[] returnBoard = host.recieveBoard();
                if (returnBoard == null)
                {
                    Console.WriteLine("\nOpponent has quit, ending game");
                    return;
                }

                updateBoard(returnBoard);

                winner = checkWin();
                if(winner == 'o')
                {
                    Console.WriteLine(createDisplay() + "\nOpponent Wins\nClosing connection");
                    return;
                }

                if (winner == 'd')
                {
                    Console.WriteLine(createDisplay() + "\nDraw");
                    return;
                }
            }       
        }

        private static void updateBoard(char[] retBoard)
        {
            int index = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    board[i, j] = retBoard[index];

                    if(retBoard[index] == 'x' || retBoard[index] == 'o')
                    {
                        freeSpots[index] = false;
                    }

                    index++;
                }
            }
        }

        private static void populateBoard(ref char[] boardToSend, int position, string type)
        {
            int index = 0;
            position = position - 1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if(index == position)
                    {
                        if(type.Equals("host"))
                        {
                            boardToSend[index] = 'x';
                            board[i, j] = 'x';

                        }
                        else if(type.Equals("client"))
                        {
                            boardToSend[index] = 'o';
                            board[i, j] = 'o';

                        }
                        
                    }
                    else
                    {
                        boardToSend[index] = board[i, j];
                    }
                    index++;
                }
            }
        }

        private static string createDisplay()
        {
            string retVal = $"";
            int index = 1;
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if(board[i,j] == ' ')
                    {
                        retVal += " " + index + " ";
                    }
                    else 
                    {
                        retVal += " " + board[i,j] + " ";
                        freeSpots[index - 1] = false;
                    }

                    if(index % 3 != 0)
                    {
                        retVal += "|";
                    }
                    index++;
                }

                if(index != 10)
                {
                    retVal += "\n-----------\n";
                }
                
            }

            return retVal;
        }

        private static int displayBoard()
        {
            string boardDisplay = "\n\nWhich would you like to fill?\n" +
                                    createDisplay();

            Console.WriteLine(boardDisplay);
            string given = Console.ReadLine();

            //take user input and check if it is an int
            if (Int32.TryParse(given, out int givenInt))
            {
                if((givenInt - 1) > 8)
                {
                    Console.WriteLine("Bad input");
                    return displayBoard();         
                }
                //if this position is a legal position, return it and update freespots
                if(isAllowed(givenInt -1))
                {
                    freeSpots[givenInt - 1] = false;
                    return givenInt;
                }
                //if not, ask again
                else
                {
                    Console.WriteLine("Bad input");
                    return displayBoard();
                }
            }

            //if user input is bad, ask again
            else
            {
                Console.WriteLine("Bad input");
                return displayBoard();
            }
        }

        //check the freespots 
        private static bool isAllowed(int check)
        {
            if(freeSpots[check] == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static char checkWin()
        {
            char x = 'x';
            char o = 'o';

            if(board[0,0] == x)
            {
                if(board[0,1] == x && board[0,2] == x)
                {
                    return x;
                }
                else if(board[1,1] == x && board[2,2] == x)
                {
                    return x;
                }
                else if(board[1,0] == x && board[2,0] == x)
                {
                    return x;
                }
            }

            if(board[0,1] == x)
            {
                if (board[1, 1] == x && board[2, 1] == x)
                {
                    return x;
                }
            }

            if (board[0, 2] == x)
            {
                if (board[1, 1] == x && board[2, 0] == x)
                {
                    return x;
                }
                else if (board[1, 2] == x && board[2, 2] == x)
                {
                    return x;
                }
            }

            if(board[1,0] == x)
            {
                if(board[1,1] == x && board[1,2] == x)
                {
                    return x;
                }
            }

            if (board[2, 0] == x)
            {
                if (board[2, 1] == x && board[2, 2] == x)
                {
                    return x;
                }
            }

            if (board[0, 0] == o)
            {
                if (board[0, 1] == o && board[0, 2] == o)
                {
                    return o;
                }
                else if (board[1, 1] == o && board[2, 2] == o)
                {
                    return o;
                }
                else if (board[1, 0] == o && board[2, 0] == o)
                {
                    return o;
                }
            }
            if (board[0, 1] == o)
            {
                if (board[1, 1] == o && board[2, 1] == o)
                {
                    return o;
                }
            }
            if (board[0, 2] == o)
            {
                if (board[1, 1] == o && board[2, 0] == o)
                {
                    return o;
                }
                else if (board[1, 2] == o && board[2, 2] == o)
                {
                    return o;
                }
            }
            if (board[1, 0] == o)
            {
                if (board[1, 1] == o && board[1, 2] == o)
                {
                    return o;
                }
            }
            if (board[2, 0] == o)
            {
                if (board[2, 1] == o && board[2, 2] == o)
                {
                    return o;
                }
            }

            int fullCount = 0;

            for (int i = 0; i < freeSpots.Length; i++)
            {
                if (freeSpots[i] == false)
                {
                    fullCount++;
                }
            }

            if (fullCount == 9)
            {
                return 'd';
            }

            return 'n';
        }
    }
}
