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

            Console.WriteLine("Host(h) or player(p)?");
            string status = Console.ReadLine();

            int choice = 3;

            if(status.Equals("h", StringComparison.InvariantCultureIgnoreCase))
            {
                choice = 0;
            }
            else if(status.Equals("p", StringComparison.InvariantCultureIgnoreCase))
            {
                choice = 1;
            }


            if(choice == 0)
            {
                Player_Server host = new Player_Server();
                initBoard();
                host.Start();
                hostGame(host);
            }

            else if (choice == 1)
            {
                Player_Client client = new Player_Client();
                initBoard();
                client.Start();
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
            bool game = true;

            while (game)
            {
                //create array for board
                char[] boardToSend = new char[9];

                //recieve the board and update the board, recieveBoard freezes user while waiting for return
                char[] returnBoard = client.recieveBoard();
                updateBoard(returnBoard);

                char winner = checkWin();
                if (winner == 'x')
                {
                    Console.WriteLine(createDisplay() + "\nOpponent Wins\nClosing connection");
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

                //send the board
                client.sendBoard(boardToSend);

            }
        }

        private static void hostGame(Player_Server host)
        {
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
                    Console.WriteLine(createDisplay() + "You Win!");
                    return;
                }

                //send the board
                host.sendBoard(boardToSend);

                //recieve the board and update the board, recieveBoard freezes user while waiting for return
                char[] returnBoard = host.recieveBoard();
                updateBoard(returnBoard);

                winner = checkWin();
                if(winner == 'o')
                {
                    Console.WriteLine(createDisplay() + "Opponent Wins\nClosing connection");
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
            else if(board[0,1] == x)
            {
                if (board[1, 1] == x && board[2, 1] == x)
                {
                    return x;
                }
            }
            else if (board[0, 2] == x)
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
            else if(board[1,0] == x)
            {
                if(board[1,1] == x && board[1,2] == x)
                {
                    return x;
                }
            }
            else if (board[2, 0] == x)
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
            else if (board[0, 1] == o)
            {
                if (board[1, 1] == o && board[2, 1] == o)
                {
                    return o;
                }
            }
            else if (board[0, 2] == o)
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
            else if (board[1, 0] == o)
            {
                if (board[1, 1] == o && board[1, 2] == o)
                {
                    return o;
                }
            }
            else if (board[2, 0] == o)
            {
                if (board[2, 1] == o && board[2, 2] == o)
                {
                    return o;
                }
            }

            return 'n';
        }
    }
}
