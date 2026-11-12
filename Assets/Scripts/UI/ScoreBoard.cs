using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    public int index; // set this in inspector for each slot (0,1,2...)
    public int score;
    public TMP_Text scoreText;
    public TMP_Text dateText;

    private void Start()
    {
        LoadScore();
        UpdateScore();
    }

    public void UpdateScore()
    {
        if (score != 0)
        {
            scoreText.text = NumberFormatter.FormatNumber(score) + " m";

            // Load saved date, if exists
            string dateKey = "ScoreDate" + index;
            if (GameData.HasKey(dateKey))
            {
                dateText.text = GameData.GetString(dateKey);
            }
            else
            {
                dateText.text = System.DateTime.Now.ToString("dd MMM yyyy");
            }
        }
        else
        {
            scoreText.text = "0 m";
            dateText.text = "---";
        }
    }

    public void SaveScore()
    {
        GameData.SetInt("Score" + index, score);
        GameData.SetString("ScoreDate" + index, System.DateTime.Now.ToString("dd MMM yyyy"));
    }

    public void LoadScore()
    {
        string scoreKey = "Score" + index;
        if (GameData.HasKey(scoreKey))
        {
            score = GameData.GetInt(scoreKey);
        }
    }
}