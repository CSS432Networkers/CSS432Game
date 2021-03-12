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
    if(rooms[roomNum][0] != 0 && rooms[roomNum][1] != 0)
    {
        return false;
    }
    return true;
}

int nextEmptyTemp()
{
    for(int i = 0; i < 10; i++)
    {
        if(tempHolders[i] == 0)
        {
            return i;
        }
    }
    return -1;
}

//readData() acts as the thread to run the data read and response to the client
//it is called when a new thread is created on main, which is created on new connections.
//it takes in the SD and interacts with it to receive the data
void* readData(void* details)
{
    //cout << "this thread has room" << details[0] << " and status " << details[1];
    //int thisStatus = *(int*)status;
    int tempIndex = *(int*)details;
    int thisSock = tempHolders[tempIndex];
    int otherSock = -1;

    //int readSize = 0;

    //make a message and specify this is a room list
    char message[11];
    message[0] = 'r';
    int index = 1;

    for(int i = 0; i < 5; i++)
    {
        //if the room is open, add to message
        if(isOpen(i) == true)
        {
            message[index] = '0' + i;
            index++;
        }
    }

    //cout << "sent room list to user" << endl;    
    write(thisSock, message, MAXSIZE);
    read(thisSock, message, MAXSIZE);
    //cout <<"received room selection" << endl;

    int roomChoice = (message[1] - '0') - 1;
    int thisStatus = 0;

    if(rooms[roomChoice][0] != 0)
    {
        cout << "socket " << thisSock << " is client" << endl;
        otherSock = rooms[roomChoice][0];
        thisStatus = 1;
        tempHolders[tempIndex] = 0;
        rooms[roomChoice][1] = thisSock;
    }
    else
    {
        cout << "socket " << thisSock << " is host" << endl;
        tempHolders[tempIndex] = 0;
        rooms[roomChoice][0] = thisSock;
        otherSock = rooms[roomChoice][1];
        while(otherSock == 0)
        {
            otherSock = rooms[roomChoice][1];
        }
    }

    thisSock = rooms[roomChoice][1];
    otherSock = rooms[roomChoice][0];

    while(true)
    {
        if(thisSock == 0 || otherSock == 0)
        {
            //cout << hostSock << " " << thisStatus << endl;
            continue;
        }
        else if(thisStatus == 1)
        {
            char databuf[MAXSIZE];             //databuf array for incoming data
            int retryCount = 0;
            int readSize = 0;
            while(readSize != 11)
            {
                retryCount++;
                readSize = read(otherSock, databuf, MAXSIZE);
                if(retryCount >= 3)
                {
                    break;
                }
            }

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

            write(thisSock, databuf, MAXSIZE);

            if(databuf[0] == 'w')
            {
                rooms[roomChoice][0] = 0;
                rooms[roomChoice][1] = 0;
                close(thisSock);
                close(otherSock);
                break;
            }
            else if(databuf[0] == 'd')
            {
                rooms[roomChoice][0] = 0;
                rooms[roomChoice][1] = 0;
                close(thisSock);
                close(otherSock);
                break;
            }

            continue;
        }
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

            if(databuf[0] == 'w')
            {
                rooms[roomChoice][0] = 0;
                rooms[roomChoice][1] = 0;
                close(thisSock);
                close(otherSock);
                break;
            }
            else if(databuf[0] == 'd')
            {
                rooms[roomChoice][0] = 0;
                rooms[roomChoice][1] = 0;
                close(thisSock);
                close(otherSock);
                break;
            }

            continue;
        }
    }
    return nullptr;
}

//main() is the main entry point for the server. It takes in the params portNumber
//and the amount of iterations that it has to do when receiving from the client.
int main()
{

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
    
    int hostConn = 0;
    pthread_t newThread;

    //loop while waiting for new connection
    while(true)
    {
        pthread_t newThread;
        int pthreadStatus = 0;

        //wait for a player to connect
        int tempSock = accept(serverSD, (struct sockaddr*) &clientAddr, &clientAddrSize);      
        
        //get next empty temp spot
        int tempIndex = nextEmptyTemp();

        //put player socket into temp holders
        tempHolders[tempIndex] = tempSock;

        pthreadStatus = pthread_create(&newThread, NULL, readData, (void*)&tempIndex); 

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
