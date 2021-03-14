using System;
using System.IO;


namespace tictactoe_emergency
{
    class main
    {
        //2d board that keeps track of the location of user inputs
        private static char[,] board = new char[3, 3];

        //array to keep track of what spots are allowed to be input
        private static bool[] freeSpots = new bool[9];

        //main entry point for the program
        static void Main(string[] args)
        {

            //while the player still wants to play games, loop
            while (true)
            {
                //take in user input
                Console.WriteLine("Play(1) or Quit(2)?");
                string playAgain = Console.ReadLine();

                //if user input is 1, start a game instance, else, quit
                if (playAgain.Equals("1") == true)
                {
                    manageGame();
                    continue;
                }
                else
                {
                    break;
                }
            }
        }

        //method that launches a host or player game depending on input
        private static void manageGame()
        {
            //take in user input
            Console.WriteLine("Host(1) or Player(2)? Quit(3)");
            string status = Console.ReadLine();

            //if they choose 1, they want to be a host
            if (status.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                //create client object for the host, initialize the board, and call on hostGame
                Player_Client host = new Player_Client();
                initBoard();
                hostGame(host);
            }
            //if they choose 2, they want to be a connecting player
            else if (status.Equals("2", StringComparison.InvariantCultureIgnoreCase))
            {
                //create client object for the player, initialize the board, and call on clientGame
                Player_Client client = new Player_Client();
                initBoard();
                clientGame(client);

            }
            //any other input should take them back to the main menu
            else
            {
                return;
            }
        }

        //initializes the board with empty spaces and opens up all the free spots
        private static void initBoard()
        {
            int index = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    board[i, j] = ' ';
                    freeSpots[index] = true;
                    index++;
                }
            }

        }

        //method to run a client game that takes in a player_client object to manage the netcode
        private static void clientGame(Player_Client client)
        {

            //ask if they want to do a LAN or online game
            Console.WriteLine("LAN(1) or Online(2)");
            string input = Console.ReadLine();

            //if they want to do a lan game
            if (input.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                //status of game initialization and bool to keep track of the "bad port" message
                int initStatus = -1;
                bool retry = false;

                //while the initStatus is incomplete
                while (initStatus == -1)
                {
                    //if we've taken an input and the port failed, print out bad port
                    if (retry == true)
                    {
                        Console.WriteLine("Bad Port");
                    }

                    //call on startLocal() which connects to to a game based on the port number given
                    initStatus = client.startLocal();

                    //if the client was able to initialize, exit
                    if (initStatus == 0)
                    {
                        break;
                    }
                    retry = true;
                }
            }

            //if they want to join an online game
            else if (input.Equals("2", StringComparison.InvariantCultureIgnoreCase))
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

                    //start a remote instance and see if it completes
                    initStatus = client.startRemote(temp);

                    //if the initialization was successful
                    if (initStatus == 0)
                    {
                        //recieve the rooms from the server
                        int[] rooms = client.recieveRooms();

                        //display available rooms and ask for a choice
                        Console.WriteLine("What room would you like to choose?");
                        for (int i = 0; i < rooms.Length; i++)
                        {
                            Console.WriteLine("Room: " + rooms[i] + 1);
                        }

                        string roomChoice = "";

                        //while the room entered is invalid
                        while (true)
                        {
                            //take in the room choice
                            roomChoice = Console.ReadLine();
                            int roomChoiceInt;

                            //check if the given is an integer
                            bool isInt = int.TryParse(roomChoice, out roomChoiceInt);

                            //if it is an integer, check the value
                            if (isInt)
                            {
                                //check if the room choice is a valid one
                                bool valid = checkRoomChoice(rooms, roomChoiceInt);

                                //if the choice is either invalid or outside of the range, loop again
                                if ((valid == false) || (roomChoiceInt > 5 || roomChoiceInt < 0))
                                {
                                    Console.WriteLine("Bad Room Number, Pick again");
                                    continue;
                                }
                            }
                            else if (isInt == false)
                            {
                                Console.WriteLine("Enter an integer");
                                continue;
                            }

                            //if we get a valid input, break
                            break;
                        }

                        //send the room choice to the server
                        client.sendRoom(roomChoice[0]);

                        //exit the loop
                        Console.WriteLine("Game Started");
                        break;
                    }

                    retry = true;
                }

                Console.WriteLine("Waiting for opponent");
            }

            bool game = true;

            //while game is running
            while (game)
            {
                //create array for board
                char[] boardToSend = new char[9];

                //recieve the board and update the board, recieveBoard freezes user while waiting for return
                char[] returnBoard = client.recieveBoard();

                //if the return board is null, then they have quit the game
                if (returnBoard == null)
                {
                    Console.WriteLine("Opponent has quit, ending game");
                    return;
                }

                //if the board is populated, update the board locally
                updateBoard(returnBoard);

                //check if the board is a winning board
                char winner = checkWin();

                //if the winner is x, close the game
                if (winner == 'x')
                {
                    Console.WriteLine(createDisplay() + "\nOpponent Wins\nClosing connection");
                    return;
                }

                //if there is a draw, close the game
                if (winner == 'd')
                {
                    Console.WriteLine(createDisplay() + "\nDraw");
                    return;
                }

                //take user input for position they wanna fill
                int check = displayBoard();

                //update board to put an 'x' where user inputs
                populateBoard(ref boardToSend, check, "client");

                //check to see if the user's input was a winning input
                winner = checkWin();

                //if the client is the winner, send the winning board
                if (winner == 'o')
                {

                    client.sendWin(boardToSend);
                    Console.WriteLine(createDisplay() + "\nYou Win!");
                    return;
                }

                //if it is a draw, send a draw board
                if (winner == 'd')
                {
                    client.sendDraw(boardToSend);
                    Console.WriteLine(createDisplay() + "\nDraw");
                    return;
                }

                //if the board isnt a win, loss, or draw, send the board naturally
                bool sendStatus = client.sendBoard(boardToSend);

                //if the send was failed, they quit the game
                if (sendStatus == false)
                {
                    Console.WriteLine("\nOppenent has quit, closing game");
                    return;
                }

            }
        }

        //function to handle a host player
        private static void hostGame(Player_Client host)
        {
            Console.WriteLine("LAN(1) or Online(2)");
            string input = Console.ReadLine();

            if (input.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                host.hostLocal();
            }
            else if (input.Equals("2", StringComparison.InvariantCultureIgnoreCase))
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

                    //if the status is successful
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

                        //while the room entered is invalid
                        while (true)
                        {
                            //take in the room choice
                            roomChoice = Console.ReadLine();
                            int roomChoiceInt;

                            //check if the given is an integer
                            bool isInt = int.TryParse(roomChoice, out roomChoiceInt);

                            //if it is an integer, check the value
                            if (isInt)
                            {
                                //check if the room choice is a valid one
                                bool valid = checkRoomChoice(rooms, roomChoiceInt);

                                //if the choice is either invalid or outside of the range, loop again
                                if ((valid == false) || (roomChoiceInt > 5 || roomChoiceInt < 0))
                                {
                                    Console.WriteLine("Bad Room Number, Pick again");
                                    continue;
                                }
                            }
                            else if(isInt == false)
                            {
                                Console.WriteLine("Enter an integer");
                                continue;
                            }

                            //if we get a valid input, break
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

            while (game)
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

                if (winner == 'd')
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
                if (winner == 'o')
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

        //method to see if a user's room choice is valid or not
        private static bool checkRoomChoice(int[] rooms, int choice)
        {
            //loop through all the rooms
            for(int i = 0; i < rooms.Length; i++)
            {
                //if the room choice is found in the room list, return true
                if(rooms[i] == choice)
                {
                    return true;
                }
            }
            //return false if the room choice doesnt exist in the room list
            return false;
        }

        //method to update the board and freespots available
        private static void updateBoard(char[] retBoard)
        {
            int index = 0;

            //loop through the whole board
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //fill the board with the contents of the retBoard
                    board[i, j] = retBoard[index];

                    //if the spot being filled is an x or o, make it inaccessible to the user
                    if(retBoard[index] == 'x' || retBoard[index] == 'o')
                    {
                        freeSpots[index] = false;
                    }

                    index++;
                }
            }
        }

        //method to populate the board with the user's input
        private static void populateBoard(ref char[] boardToSend, int position, string type)
        {
            int index = 0;
            //position is updated to be 1 less for a 0-8 scale instead of 1-9
            position = position - 1;

            //loop through the whole board
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //if the current index is the position wanting to be filled, fill it with o or x
                    //depending on the type of user that called this
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

        //method to create the display of the board, returns the board in string format to be displayed
        private static string createDisplay()
        {
            string retVal = "";
            int index = 1;
            //loop through the whole board
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    //if the spot is empty, fill it with a number instead
                    if(board[i,j] == ' ')
                    {
                        retVal += " " + index + " ";
                    }
                    //if the spot isnt empty, fill it with an x or o depending on the content
                    else 
                    {
                        retVal += " " + board[i,j] + " ";
                        freeSpots[index - 1] = false;
                    }

                    //if the index isnt any of the right most positions, add a divider
                    if(index % 3 != 0)
                    {
                        retVal += "|";
                    }
                    index++;
                }

                //if we're on the first or second row, add a horizontal divider
                if(index != 10)
                {
                    retVal += "\n-----------\n";
                }
                
            }

            //return the board in string form
            return retVal;
        }

        //method to display the board to the users and take in an input
        private static int displayBoard()
        {
            //create a string from the board generated by createDisplay()
            string boardDisplay = "\n\nWhich would you like to fill?\n" +
                                    createDisplay();

            //display the board to the user and take an input
            Console.WriteLine(boardDisplay);
            string given = Console.ReadLine();

            //check to see if given input is an int
            int givenInt = 0;
            bool isInt = Int32.TryParse(given, out givenInt);

            //take user input and check if it is an int
            if (isInt == true)
            {
                //if out of bounts, use bad input
                if((givenInt - 1) > 8 || (givenInt - 1) < 0)
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

        //check if the given int is a free spot
        private static bool isAllowed(int check)
        {
            //if the spot is free, return true
            if(freeSpots[check] == true)
            {
                return true;
            }
            //if not, return false
            else
            {
                return false;
            }
        }

        //check win conditions 
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

            //if the board is full and didnt meet any of the win conditions, return d
            if (fullCount == 9)
            {
                return 'd';
            }

            return 'n';
        }
    }
}
