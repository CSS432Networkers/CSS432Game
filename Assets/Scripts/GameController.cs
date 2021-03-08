using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text[] buttonList;
    private string playerSide;
    public GameObject gameOverPanel;
    public Text gameOverText;

    void Awake()
    {
        SetGameControllerReferenceOnButtons();
        playerSide = "X";
        gameOverPanel.SetActive(false);
    }

    public void SetGameControllerReferenceOnButtons()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<GridSpace>().SetGameControllerReference(this);
        }
    }

    public string GetPlayerSide()
    {
        return playerSide;
    }

    public void EndTurn()
    {
        // Rows
        for (int i = 0; i < 9; i += 3)
        {
            if (buttonList[i].text == playerSide && buttonList[i + 1].text == playerSide && buttonList[i + 2].text == playerSide)
            {
                GameOver();
            }
        }
        // Columns
        for (int i = 0; i < 3; i++)
        {
            if (buttonList[i].text == playerSide && buttonList[i + 3].text == playerSide && buttonList[i + 6].text == playerSide)
            {
                GameOver();
            }
        }
        // Diagonals
        if (buttonList[0].text == playerSide && buttonList[4].text == playerSide && buttonList[8].text == playerSide)
        {
            GameOver();
        }
        if (buttonList[2].text == playerSide && buttonList[4].text == playerSide && buttonList[6].text == playerSide)
        {
            GameOver();
        }
        ChangeSides();
    }

    void ChangeSides()
    {
        playerSide = (playerSide == "X") ? "O" : "X";
    }

    void GameOver()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = false;
        }
        gameOverPanel.SetActive(true);
        gameOverText.text = playerSide + " Wins!";
    }
}
