using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Player
{
    public Image panel;
    public Text text;
}

[System.Serializable]
public class PlayerColor
{
    public Color panelColor;
    public Color textColor;
}

public class GameController : MonoBehaviour
{
    //[SerializeField] private GameObject WinCanvas;
    //[SerializeField] private GameObject LoseCanvas;
    //[SerializeField] private GameObject DrawCanvas;
    public GameObject gameOverPanel;
    public Text gameOverText;
    public GameObject restartButton;

    public Text[] buttonList;
    private string playerSide;
    private int moveCount;

    public Player playerX;
    public Player playerO;
    public PlayerColor activePlayerColor;
    public PlayerColor inactivePlayerColor;

    void Awake ()
    {
        SetGameControllerReferenceOnButtons();
        //WinCanvas.SetActive(false);
        //LoseCanvas.SetActive(false);
        //DrawCanvas.SetActive(false);
        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);
        moveCount = 0;
        playerSide = "X";
        SetPlayerColors(playerX, playerO);
    }

    //get the text array of the board 
    void SetGameControllerReferenceOnButtons ()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {           
            buttonList[i].GetComponentInParent<GridSpace>().SetGameControllerReference(this);
        }
    }

    void SetPlayerSide(string side)
    {
        playerSide = side;
    }

    public string GetPlayerSide()
    {
        return playerSide;
    }

    public void EndTurn ()
    {
        moveCount++;
        //First row
        if (buttonList[1].text == playerSide && buttonList[2].text == playerSide && buttonList[3].text == playerSide)
        {
            GameOver(playerSide);
        }
        //Second row
        else if (buttonList[4].text == playerSide && buttonList[5].text == playerSide && buttonList[0].text == playerSide)
        {
            GameOver(playerSide);
        }
        //Thrid row
        else if (buttonList[6].text == playerSide && buttonList[7].text == playerSide && buttonList[8].text == playerSide)
        {
            GameOver(playerSide);
        }
        //First col
        else if (buttonList[1].text == playerSide && buttonList[4].text == playerSide && buttonList[6].text == playerSide)
        {
            GameOver(playerSide);
        }
        //Second col
        else if (buttonList[2].text == playerSide && buttonList[7].text == playerSide && buttonList[0].text == playerSide)
        {
            GameOver(playerSide);
        }
        //Third col
        else if (buttonList[3].text == playerSide && buttonList[5].text == playerSide && buttonList[8].text == playerSide)
        {
            GameOver(playerSide);
        }
        //x
        else if (buttonList[1].text == playerSide && buttonList[0].text == playerSide && buttonList[8].text == playerSide)
        {
            GameOver(playerSide);
        }
        //x2
        else if (buttonList[3].text == playerSide && buttonList[0].text == playerSide && buttonList[6].text == playerSide)
        {
            GameOver(playerSide);
        }

        //Checks for draw
        else if (moveCount >= 9)
        {
            //DrawCanvas.SetActive(true);
            GameOver("draw");
        }
        else
        {
            ChangeSides();
        }
    }

    void SetPlayerColors(Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.text.color = activePlayerColor.textColor;
        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        oldPlayer.text.color = inactivePlayerColor.textColor;
    }

    void GameOver(string winningPlayer) 
    {
        //Disable clicking
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = false;
        }
        if (winningPlayer == "draw")
        {
            SetGameOverText("It's a Draw!");
        }
        else
        {
            SetGameOverText(winningPlayer + " Wins!");
        }
        gameOverPanel.SetActive(true);
        restartButton.SetActive(true);
    }

    void SetGameOverText(string value)
    {
        gameOverPanel.SetActive(true);
        gameOverText.text = value;
    }

    //This method is called after the player made a move.
    void ChangeSides () 
    {
        playerSide = (playerSide == "X") ? "O" : "X"; // Note: Capital Letters for "X" and "O"
        if (playerSide == "X")
        {
            SetPlayerColors(playerX, playerO);
        }
        else
        {
            SetPlayerColors(playerO, playerX);
        }
    }

    //When Play Again button is clicked
    public void RestartGame ()
    {
        //we need to change the player symbol.
        playerSide = "X";

        moveCount = 0;

        //WinCanvas.SetActive(false);
        //LoseCanvas.SetActive(false);
        //DrawCanvas.SetActive(false);

        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);
        SetPlayerColors(playerX, playerO);
        SetBoardInteractable(true);

        //Clear board. Needs work.
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].text = "";
        }
    }

    void SetBoardInteractable (bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }
}
