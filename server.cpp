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

const int N = 2;            //connecttion request limit
const int MAXSIZE = 11;   //max databuf size

int clientSock = 0;
int hostSock = 0;

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

//readData() acts as the thread to run the data read and response to the client
//it is called when a new thread is created on main, which is created on new connections.
//it takes in the SD and interacts with it to receive the data and send back the count
//of reads that it has done.
void *readData(void* status)
{
    int thisStatus = *(int*)status;

    int readSize = 0;

    while(true)
    {
        if(clientSock == 0 || hostSock == 0)
        {
            //cout << hostSock << " " << thisStatus << endl;
            continue;
        }
        else if(thisStatus == 1)
        {
            char databuf[MAXSIZE];             //databuf array for incoming data
            int retryCount = 0;
            while(readSize != 11)
            {
                retryCount++;
                readSize = read(clientSock, databuf, MAXSIZE);
                if(retryCount >= 3)
                {
                    break;
                }
            }

            if(retryCount >= 3)
            {
                databuf[0] = 'e';
                write(clientSock, databuf, MAXSIZE);
                cout << "client disconnected" << endl;
                close(hostSock);
                close(clientSock);
                break;
            }

            readSize = 0;

            write(hostSock, databuf, MAXSIZE);

            if(databuf[0] == 'w')
            {
                close(hostSock);
                close(clientSock);
                break;
            }
            else if(databuf[0] == 'd')
            {
                close(hostSock);
                close(clientSock);
                break;
            }

            continue;
        }
        else if(thisStatus == 0)
        {
            char databuf[MAXSIZE];             //databuf array for incoming data
            int retryCount = 0;

            while(readSize != 11)
            {
                retryCount++;
                readSize = read(hostSock, databuf, MAXSIZE);

                if(retryCount >= 3)
                {
                    break;
                }
            }

            if(retryCount >= 3)
            {
                databuf[0] = 'e';
                write(clientSock, databuf, MAXSIZE);
                cout << "host disconnected" << endl;
                close(hostSock);
                close(clientSock);
                break;
            }
            
            readSize = 0;

            write(clientSock, databuf, MAXSIZE);

            if(databuf[0] == 'w')
            {
                close(hostSock);
                close(clientSock);
                break;
            }
            else if(databuf[0] == 'd')
            {
                close(hostSock);
                close(clientSock);
                break;
            }

            continue;
        }
    }

    return 0;
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

    //listen to up to 2 connection requests
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
    pthread_t hostThread;
    pthread_t clientThread;
    //loop while waiting for new connection
    while(true)
    {
        int pthreadStatus = 0;

        if(hostConn == 0)
        {
            cout << "waiting for host" << endl;
            //create a newSD when accepting a connection
            hostSock = accept(serverSD, (struct sockaddr*) &clientAddr, &clientAddrSize);
            
            //if the SD fails, print the failure but continue
            if(clientSock == -1 || hostSock == -1)
            {
                printf("\nFailed to connect to client\n");
                continue;
            }

            int hostStatus = 0;
            //if the connection completes, create a new thread and begin the data read/return process, passing through the SD
            pthreadStatus = pthread_create(&hostThread, NULL, readData, (void*)&hostStatus);
            cout << "host connected" << endl;
            hostConn = 1;
            continue;
        }
        else if(hostConn == 1)
        {
            cout << "waiting for client" << endl;
            clientSock = accept(serverSD, (struct sockaddr*) &clientAddr, &clientAddrSize);
            
            //if the SD fails, print the failure but continue
            if(clientSock == -1 || hostSock == -1)
            {
                printf("\nFailed to connect to client\n");
                continue;
            }

            int clientStatus = 1;
            //if the connection completes, create a new thread and begin the data read/return process, passing through the SD
            pthreadStatus = pthread_create(&clientThread, NULL, readData, (void*)&clientStatus);
            cout << "client connected" << endl;
            hostConn = 2;
            continue;
        }

        //if the SD fails, print the failure but continue
        if(clientSock == -1 || hostSock == -1)
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
        
    }
    //join the thread after it finishes to save resources.
    pthread_join(clientThread, NULL);
    pthread_join(hostThread, NULL);

    //finish
    return 0;
}
