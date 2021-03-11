using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class MenuController : MonoBehaviour
{
    // using SerializeField so that we can see these variables
    // in the Unity inspector while keeping them private
    [SerializeField] private GameObject MainMenuCanvas;
    [SerializeField] private GameObject ServerWaitingCanvas;
    [SerializeField] private GameObject ClientWaitingCanvas;
    [SerializeField] private GameObject GamePlayCanvas;

    [SerializeField] private GameObject WinCanvas;
    [SerializeField] private GameObject LoseCanvas;
    [SerializeField] private GameObject DrawCanvas;

    public Text ServerIP;
    public Text ServerPort;

    private string selectedRoom;
    private bool roomCreated = false;

    void Start() 
    {

        MainMenuCanvas.SetActive(true);
        ServerWaitingCanvas.SetActive(false);
        ClientWaitingCanvas.SetActive(false);
        GamePlayCanvas.SetActive(false);
        WinCanvas.SetActive(false);
        LoseCanvas.SetActive(false);
        DrawCanvas.SetActive(false); 
    }

    //Contains methods in the main menu
    //==================================================
    public void NewRoomButtonClicked()
    {
        MainMenuCanvas.SetActive(false);
        ServerWaitingCanvas.SetActive(true);
        ClientWaitingCanvas.SetActive(false);  



    }
    public void JoinRoomButtonClicked()
    {
        MainMenuCanvas.SetActive(false);
        ServerWaitingCanvas.SetActive(false);
        ClientWaitingCanvas.SetActive(true);  
    }
    public void QuitButtonClicked()
    {
        Debug.Log("Quitting game!");
        Application.Quit();
    }

    #region ButtonClicked
    //This method runs when the server is hosting but the user press the back button
    public void ServerBackButtonClicked()
    {
        //close the connection first before going back to the main menu

        MainMenuCanvas.SetActive(true);
        ServerWaitingCanvas.SetActive(false);
    }
    //This method is used in some different menus,
    // It will prompt the player to the main menu
    public void BackButtonClicked()
    {
        MainMenuCanvas.SetActive(true);
        ClientWaitingCanvas.SetActive(false);  
        GamePlayCanvas.SetActive(false);
    }
    // This is basically like the Back button but we need to close the client and server 
    // before going back to the main menu. 
    public void ExitPlayingButtonClicked()
    {
        //TODO:
        // If the player exits in the middle of the game
        // Set the player to lose, the opponent wins.

        //If the the result has been set, skip to this part.
        MainMenuCanvas.SetActive(true);
        GamePlayCanvas.SetActive(false);
    }

    // goes back to main menu after a win,lose,or draw game
    public void ExitButtonClicked()
    {
        MainMenuCanvas.SetActive(true);
        GamePlayCanvas.SetActive(false);
        WinCanvas.SetActive(false);
        LoseCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
    }


    //Contains methods in the joining room menu
    //Room list will only show rooms that are available to join.
    //==================================================
    public void ConnectButtonClicked()
    {
        //The example room list is just for demonstration purpose, 
        //I'm afraid there no further implementation can be done using lower level networking implementation
        //We need to figure out how to generate a room list.

        //Player has selected to be the client
        //TODO:

        //Figuring out how to check if the selected room is okay to join

        //Get the server info from the selected room in the drop list
        
        //Create the client information

        //Establish the connection between the client and the server
    }


    #endregion






 
}
