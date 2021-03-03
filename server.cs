using System; 
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
  
namespace Server { 
  
class Program { 
  
private string host = "localhost"; // needs to be local ip address
private int port = 4183;    //needs to be randomly generated

// Main Method 
static void Main(string[] args) 
{ 

    // Establish the local endpoint  
    // for the socket. Dns.GetHostName 
    // returns the name of the host  
    // running the application. 
    IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()); 
    IPAddress ipAddr = ipHost.AddressList[0]; 
    IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port); 
  
    // Creation TCP/IP Socket using  
    // Socket Class Costructor 
    Socket listener = new Socket(ipAddr.AddressFamily, 
                 SocketType.Stream, ProtocolType.Tcp); 
  
    try { 
          
        // Using Bind() method we associate a 
        // network address to the Server Socket 
        // All client that will connect to this  
        // Server Socket must know this network 
        // Address 
        listener.Bind(localEndPoint); 
  
        // Using Listen() method we create  
        // the Client list that will want 
        // to connect to Server 
        listener.Listen(10); 
  
        while (true) { 
              
            Console.WriteLine("Waiting connection ... "); 
  
            // Suspend while waiting for 
            // incoming connection Using  
            // Accept() method the server  
            // will accept connection of client 
            Socket clientSocket = listener.Accept(); 
  
            // Data buffer 
            byte[] bytes = new Byte[37]; 
            string data = null; 
  
            while (true) { 
  
                int numByte = clientSocket.Receive(bytes); 
                  
                data += Encoding.ASCII.GetString(bytes, 
                                           0, numByte); 
                                             
                if (data.IndexOf("<EOF>") > -1) 
                    break; 
            } 

            char[] playerBoard = Encoding.Unicode.GetChars(bytes);
            
            //waiting for player to input and send
            while(true)
            {
                if(/*player locks in*/)
                {
                    //update playerBoard
                    break;
                }
            }

            // Send a message to Client  
            // using Send() method 
            clientSocket.Send(message); 
        }

        //when game ends close socket and end game
        clientSocket.Shutdown(SocketShutdown.Both); 
        clientSocket.Close();  
    } 
      
    catch (Exception e) { 
        Console.WriteLine(e.ToString()); 
    } 
} 
} 
} 
