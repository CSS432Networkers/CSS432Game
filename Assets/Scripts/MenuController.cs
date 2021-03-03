using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    // using SerializeField so that we can see these variables
    // in the Unity inspector while keeping them private
    [SerializeField] private GameObject MainMenuCanvas;
    [SerializeField] private GameObject CreatingRoomCanvas;
    [SerializeField] private GameObject WaitingCanvas;
    [SerializeField] private GameObject JoiningRoomCanvas;
    [SerializeField] private GameObject GamePlayCanvas;

    [SerializeField] private GameObject WinCanvas;
    [SerializeField] private GameObject LoseCanvas;
    [SerializeField] private GameObject DrawCanvas;

    private string selectedRoom;
    private bool roomCreated = false;

    void Start() 
    {
        MainMenuCanvas.SetActive(true);
        CreatingRoomCanvas.SetActive(false);
        WaitingCanvas.SetActive(false);
        JoiningRoomCanvas.SetActive(false);
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
        CreatingRoomCanvas.SetActive(true);
        WaitingCanvas.SetActive(false);
        JoiningRoomCanvas.SetActive(false);
        GamePlayCanvas.SetActive(false);
    }
    public void JoinRoomButtonClicked()
    {
        MainMenuCanvas.SetActive(false);
        CreatingRoomCanvas.SetActive(false);
        WaitingCanvas.SetActive(false);
        JoiningRoomCanvas.SetActive(true);
        GamePlayCanvas.SetActive(false);
    }
    public void QuitButtonClicked()
    {
        Debug.Log("Quitting game!");
        Application.Quit();
    }

    #region ButtonClicked
    //==============================================================================
    //When the player hit the Create Button in the creating room menu
    public void CreateButtonClicked()
    {
        
        //The user has selected to be the server
        //TODO:
        //Initialize the server
        //Create a server with a random port number.
        //Pair the room name with the port number so that the client can find the port
        //from the room name.
        //Set roomCreated to true

        if (roomCreated)
        {
            MainMenuCanvas.SetActive(true);
            CreatingRoomCanvas.SetActive(false);
            WaitingCanvas.SetActive(true);
            JoiningRoomCanvas.SetActive(false);
            GamePlayCanvas.SetActive(false);
        }
    }

    //This method is used in some different menus,
    // It will prompt the player to the main menu
    public void BackButtonClicked()
    {
        MainMenuCanvas.SetActive(true);
        CreatingRoomCanvas.SetActive(false);
        WaitingCanvas.SetActive(false);
        JoiningRoomCanvas.SetActive(false);
        GamePlayCanvas.SetActive(false);
    }

    // This is basically like the Back button but we need to close the client and server 
    // before going back to the main menu. 
    public void ExitButtonClicked()
    {
        //TODO:
        // If the player exits in the middle of the game
        // Set the player to lose, the opponent wins.

        //If the the result has been set, skip to this part.
        MainMenuCanvas.SetActive(true);
        CreatingRoomCanvas.SetActive(false);
        WaitingCanvas.SetActive(false);
        JoiningRoomCanvas.SetActive(false);
        GamePlayCanvas.SetActive(false);

        WinCanvas.SetActive(false);
        LoseCanvas.SetActive(false);
        DrawCanvas.SetActive(false);
    }


    //Contains methods in the joining room menu
    //Room list will only show rooms that are available to join.
    //==================================================
    public void JoinButtonClicked()
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
