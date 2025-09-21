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
            if (PlayerPrefs.HasKey(dateKey))
            {
                dateText.text = PlayerPrefs.GetString(dateKey);
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
        PlayerPrefs.SetInt("Score" + index, score);
        PlayerPrefs.SetString("ScoreDate" + index, System.DateTime.Now.ToString("dd MMM yyyy"));
    }

    public void LoadScore()
    {
        string scoreKey = "Score" + index;
        if (PlayerPrefs.HasKey(scoreKey))
        {
            score = PlayerPrefs.GetInt(scoreKey);
        }
    }
}