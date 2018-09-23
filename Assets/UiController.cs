using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour {

    public Text scoreText;
    public List<Piece> nextSpawnDisplay;
	// Use this for initialization
	void Start () {
        GameController gc = GameObject.Find("Grid").GetComponent<GameController>();
        gc.OnScoreChange += OnScoreChange;
        gc.OnNextSpawnChange += OnNextSpawnChange;
	}

    public void OnScoreChange(int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }

    public void OnNextSpawnChange(List<Piece.PieceColor> newColors)
    {
        //set all transparent
        foreach(Piece p in nextSpawnDisplay)
        {
            p.SetSelfTransparent();
        }

        //bring back just enough
        for(int i = 0; i < newColors.Count; i++)
        {
            Debug.Log(newColors[i]);
            nextSpawnDisplay[i].Init(newColors[i]);
        }
    }
}
