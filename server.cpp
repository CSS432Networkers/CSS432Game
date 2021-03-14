#include <sys/types.h> // socket, bind
#include <sys/socket.h> // socket, bind, listen, inet_ntoa
#include <netinet/in.h> // htonl, htons, inet_ntoa
#include <arpa/inet.h> // inet_ntoa
#include <netdb.h> // gethostbyname
#include <unistd.h> // read, write, close
#include <string> // bzero
#include <netinet/tcp.h> // SO_REUSEADDR
#include <sys/uio.h> // writev
#include <stdio.h>
#include <stdlib.h>
#include <sys/time.h>
#include <cstring>
#include <iostream>
#include <thread>

const int N = 10;         //connecttion request limit
const int MAXSIZE = 11;   //max databuf size

//5 rooms each holds 2 sockets, one for each player
int rooms[5][2];
int tempHolders[10];

const char* portnum;

using namespace std;

//error() is a helper function used to print error types and exit from the program to prevent
//errors from happening farther down the code with the invalid inputs or outputs from other 
//methods.
void error(const char *msg)
{
    cout << "\n";
    perror(msg);
    exit(0);
}

//function to see if a room has 2 players in it
bool isOpen(int roomNum)
{
    //take in the room number and check if both spots are filled with a socket
    if(rooms[roomNum][0] != 0 && rooms[roomNum][1] != 0)
    {
        //if both are filled, return false since room is not open
        return false;
    }
    //if they aren't filled, return true
    return true;
}

//function to look for the next slot in the temp slots array
int nextEmptyTemp()
{
    //loop through all the temp slots
    for(int i = 0; i < 10; i++)
    {
        //return on the next slot that is empty
        if(tempHolders[i] == 0)
        {
            return i;
        }
    }
    return -1;
}

//manageClient() acts as the thread to manage the client connected to the server. It
//handles messages from the client connected and sends them to the correct
//other client that is playing with the client that the thread is handling
void* manageClient(void* details)
{
    //turn the temp index into an int to get the socket from the array of temp holders
    int tempIndex = *(int*)details;
    int thisSock = tempHolders[tempIndex];
    int otherSock = -1;

    //make a message and specify this is a room list with 'r' as first slot
    char message[11];
    message[0] = 'r';
    int index = 1;

    //loop through all the rooms
    for(int i = 0; i < 5; i++)
    {
        //if the room is open, add to message
        if(isOpen(i) == true)
        {
            message[index] = '0' + i;
            index++;
        }
    }

    //send the room list to the client and wait for a response   
    write(thisSock, message, MAXSIZE);
    read(thisSock, message, MAXSIZE);
    
    //take the message and turn it into an int, then subtract 1 since the
    //room choice needs to be from 0-4 not 1-5
    int roomChoice = (message[1] - '0') - 1;

    //status to see if this person is the host of this room or the other player
    int thisStatus = 0;

    //if the room's first slot is not open, this thread is handling the non-host
    if(rooms[roomChoice][0] != 0)
    {
        cout << "socket " << thisSock << " is client" << endl;
        otherSock = rooms[roomChoice][0];

        //this is a client/nonhost
        thisStatus = 1;

        //empty the temp holder and update the room
        tempHolders[tempIndex] = 0;
        rooms[roomChoice][1] = thisSock;
    }
    else
    {
        cout << "socket " << thisSock << " is host" << endl;

        //this is a host, empty the temp holder and update the room
        tempHolders[tempIndex] = 0;
        rooms[roomChoice][0] = thisSock;
        otherSock = rooms[roomChoice][1];

        //wait while the other player connects
        while(otherSock == 0)
        {
            otherSock = rooms[roomChoice][1];
        }
    }

    //set the sockets
    thisSock = rooms[roomChoice][1];
    otherSock = rooms[roomChoice][0];

    //while there is a game going or people are connected
    while(true)
    {
        //if both sockets are empty continue;
        if(thisSock == 0 || otherSock == 0)
        {
            //cout << hostSock << " " << thisStatus << endl;
            continue;
        }

        //if this is a client
        else if(thisStatus == 1)
        {
            //databuf to read the message
            char databuf[MAXSIZE];       
            //counter to see if messages have to resend because read() doesnt halt the thread
            int retryCount = 0;

            //size for the messabe
            int readSize = 0;

            //while the message has not been fully recieved
            while(readSize != 11)
            {
                //increment the retry
                retryCount++;

                //read the message and put it into databuf
                readSize = read(otherSock, databuf, MAXSIZE);
                if(retryCount >= 3)
                {
                    break;
                }
            }

            //if we've retried too many times, send an "error" message to the other player and exit
            if(retryCount >= 3)
            {
                rooms[roomChoice][0] = 0;
                rooms[roomChoice][1] = 0;
                databuf[0] = 'e';
                write(otherSock, databuf, MAXSIZE);
                cout << "client disconnected" << endl;
                close(thisSock);
                close(otherSock);
                break;
            }

            readSize = 0;

            //if we've recieved properly and everything is good, send the message to the other socket
            write(thisSock, databuf, MAXSIZE);

            //if the message recieved is a win or draw, close this socket and empty the room
            if (databuf[0] == 'w' || databuf[0] == 'd')
            {
                cout << "client disconnected from win" << endl;
                rooms[roomChoice][0] = 0;
                rooms[roomChoice][1] = 0;
                close(thisSock);
                break;
            }

            continue;
        }

        //case that this is a host
        else if(thisStatus == 0)
        {

            char databuf[MAXSIZE];             //databuf array for incoming data
            int retryCount = 0;
            int readSize = 0;
            while(readSize != 11)
            {
                retryCount++;
                readSize = read(thisSock, databuf, MAXSIZE);

                if(retryCount >= 3)
                {
                    break;
                }
            }

            if(retryCount >= 3)
            {
                databuf[0] = 'e';
                write(otherSock, databuf, MAXSIZE);
                cout << "host disconnected" << endl;
                rooms[roomChoice][0] = 0;
                rooms[roomChoice][1] = 0;
                close(thisSock);
                close(otherSock);
                break;
            }
            
            readSize = 0;

            write(otherSock, databuf, MAXSIZE);

            if(databuf[0] == 'w' || databuf[0] == 'd')
            {
                cout << "host disconnected from win" << endl;
                rooms[roomChoice][0] = 0;
                rooms[roomChoice][1] = 0;
                close(otherSock);
                break;
            }

            continue;
        }
    }
    return nullptr;
}

//main() is the main entry point for the server. It initializes and binds the socket
//that the server is being run on and waits to accept connection requests from other
//players
int main()
{
    //set portnum to the last 4 digits of my studentID to keep unique
    portnum = "4183";

    //hints: address struct to hold ip type and socktype
    //res: address structs that will contain address info
    struct addrinfo hints, *res;        
    memset(&hints, 0, sizeof(hints));   //clear the structure data
    hints.ai_family = AF_UNSPEC;        //use ipv4 or ipv6
    hints.ai_socktype = SOCK_STREAM;    //use TCP
    hints.ai_flags = AI_PASSIVE;        //fill in my IP for me

    //------------------------begin connection process----------------------------

    //get the address info and fill hints and server info
    int status = getaddrinfo(NULL, portnum, &hints, &res);

    //if not successfull call, print status and exit
    if(status != 0)
    {
        fprintf(stderr, "getaddrinfo error: %s\n", gai_strerror(status));
        exit(0);
    }

    //create a new socket using res's values
    int serverSD = socket(res->ai_family, res->ai_socktype, res->ai_protocol);
    
    //if socket creation fails, error and exit
    if(serverSD == -1)
    {
        error("Bad Socket");
    }

    //lose "address already in use" error message
    const int yes = 1;
    setsockopt(serverSD, SOL_SOCKET, SO_REUSEADDR, (char *) &yes, sizeof(yes));

    //bind socket
    int bindStatus = bind(serverSD, res->ai_addr, res->ai_addrlen);

    //if socket bind fails, exit
    if(bindStatus == -1)
    {
        error("Failed to bind to socket");
        close(serverSD);
    }

    //listen to up to 20 connection requests
    int listenStatus = listen(serverSD, N);

    //if the listen status fails, exit 
    if(listenStatus != 0)
    {
        error("Failed to listen using socket");
        close(serverSD);
    }

    //accept incoming connection
    struct sockaddr_storage clientAddr;
    socklen_t clientAddrSize = sizeof(clientAddr);

    //----------------------------begin connection loop-------------------------------

    //loop while waiting for new connection
    while(true)
    {
        //new thread that will run the new connection
        pthread_t newThread;

        //status of this thread's creation
        int pthreadStatus = 0;

        //wait for a player to connect
        int tempSock = accept(serverSD, (struct sockaddr*) &clientAddr, &clientAddrSize);      
        
        //get next empty temp spot
        int tempIndex = nextEmptyTemp();

        //put player socket into temp holders
        tempHolders[tempIndex] = tempSock;

        //create a new thread that will run this specific player's connection status
        pthreadStatus = pthread_create(&newThread, NULL, manageClient, (void*)&tempIndex);

        //if the SD fails, print the failure but continue
        if(tempSock == -1)
        {
            printf("\nFailed to connect to client\n");
            continue;
        }

        //if the pthread creation fails, error out.
        if(pthreadStatus < 0)
        {
            error("Thread failed to start");
        }

        //cout << clientSock << " " << hostSock << endl;
        //pthread_join(newThread, NULL);
    }

    //finish
    return 0;
}
