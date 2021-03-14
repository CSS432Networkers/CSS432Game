#TicTacToe Online Multiplayer
CSS 432 Final Project

Created by Team Networkers: Jorge Alvarez, Nathan Ngo, and Johnny Pham

## Description
This is an implementation of Tic-Tac-Toe using Unity and implementing online multiplayer using C# sockets.


==========================================COMPILATION INSTRUCTIONS==========================================
Have csc.exe included in environment table. See here https://stackoverflow.com/questions/6660512/how-to-get-csc-exe-path to set.

Open cmd and change directory to the location of where you unzipped the folder to.
Executable is included but if you wish to recompile type:

-csc main.cs client.cs

This will create an executable called main.exe.

BEFORE RUNNING MAIN.EXE

SSH into the UWB linux labs, or any remote lab. To do this make sure you are connected to the UW f5 vpn. 
See herehttps://itconnect.uw.edu/connect/uw-networks/about-husky-onnet/use-husky-onnet/#download to download.

Connect through the vpn and move the file server.cpp into any directory and traverse to that directory.

Type in: g++ -lpthread server.cpp -o server

This will result in a file called server being created. Simply type in ./server and it will begin running.

TAKE NOTE OF THE SERVER YOU ARE RUNNING ON (ex: csslab11.uwb.edu)


==========================================EXECUTE INSTRUCTIONS==========================================
To begin playing, simply double click the main.exe file that was generated.
If you are the person who wants to host, press 1, if you want to join a friend, press 2.
If you want to play LAN, press 1, if you want to play online, press 2

If you choose to play online, remember which uwb linux server the server is running on and type its name in (ex: csslab11.uwb.edu)
Choose a room to play with your friend make sure only one of you is a PLAYER and one of you is a HOST.
Select the same room and begin playing.