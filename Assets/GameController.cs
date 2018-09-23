using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public struct Pair
{
    public int x, y;

    public Pair(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
};

public class GameController : MonoBehaviour {

    public Action<int> OnScoreChange;
    public Action<List<Piece.PieceColor>> OnNextSpawnChange;

    public List<List<GridButton>> grid = new List<List<GridButton>>();
    public List<List<Piece>> board = new List<List<Piece>>();

    public GameObject piecePrefab;
    public GameObject gameOverOverlay;
    public GameObject invalidPathOverlay;

    public Pair currentSelectedCoords;

    List<Piece.PieceColor> nextSpawn;

    bool spawnTwo = true;
    bool isInMoveAnim = false;
    bool isGameOver = false;

    int totalScore = 0;

    // Use this for initialization
    void Start () {
        nextSpawn = new List<Piece.PieceColor>();
        currentSelectedCoords.x = -1;
        currentSelectedCoords.y = -1;

		for(int i = 0; i < transform.childCount; i++)
        {
            List<GridButton> tempButtons = new List<GridButton>();
            List<Piece> tempPieces = new List<Piece>();
            Transform t = transform.GetChild(i);
            for(int j = 0; j < t.childCount; j++)
            {
                GridButton b = t.GetChild(j).GetComponent<GridButton>();
                tempButtons.Add(b);
                b.x = i;
                b.y = j;
                b.onPress += SelectButton;

                tempPieces.Add(null);
            }
            grid.Add(tempButtons);
            board.Add(tempPieces);
        }
        Init();
	}

    public void NewGame()
    {
        for (int i = 0; i < board.Count; i++)
        {
            for (int j = 0; j < board[i].Count; j++)
            {
                if (board[i][j] != null)
                {
                    //delete the piece
                    Destroy(board[i][j].gameObject);
                    board[i][j] = null;
                }
            }
        }
        Init();
    }

    void Init()
    {
        gameOverOverlay.SetActive(false);

        currentSelectedCoords.x = -1;
        currentSelectedCoords.y = -1;
        nextSpawn.Clear();
        spawnTwo = false;
        isInMoveAnim = false;
        isGameOver = false;

        totalScore = 0;
        OnScoreChange(totalScore);

        int initialNumPieceToSpawn = 3;
        while (initialNumPieceToSpawn > 0)
        {
            nextSpawn.Add(GetRandomColor());
            initialNumPieceToSpawn--;
        }
        SpawnPieces();
    }

    void SelectButton(int x, int y)
    {
        if (isInMoveAnim || isGameOver) return;

    //    Debug.Log("selected button " + x + " " + y);
        //do something when a button at (x,y) is pressed
        //cases:
        //1. there is a piece at that location, select it
        if(board[x][y] != null)
        {
            currentSelectedCoords.x = x;
            currentSelectedCoords.y = y;
        }
        //2. there is already a selected piece and the new selection is empty, try to move it
        else if(currentSelectedCoords.x != -1 && currentSelectedCoords.y != -1)
        {
            Debug.Log("trying to move");
            ResetAttempts();
            if(TryMove(currentSelectedCoords.x, currentSelectedCoords.y, x, y))
            {
                MovePiece(currentSelectedCoords.x, currentSelectedCoords.y, x, y);

                currentSelectedCoords.x = -1;
                currentSelectedCoords.y = -1;
                if(!CheckBoard(x, y, board[x][y].color))
                {
                    //only spawn pieces when a match is not made through a move
                    SpawnPieces();
                }
            }
            else
            {
                //no path
                StartCoroutine(WarnNoPathFound());
            }
        }
        //3. there is no selected piece and new selection is empty, do nothing
    }

    IEnumerator WarnNoPathFound()
    {
        isInMoveAnim = true;
        invalidPathOverlay.SetActive(true);
        yield return new WaitForSeconds(1);
        invalidPathOverlay.SetActive(false);
        isInMoveAnim = false;
        yield return null;
    }

    ///////////////////////////////////////////////
    /// TODO: Add algorithm to determine valid move path
    ///////////////////////////////////////////////////
    //attempt to find a path from (x1, y1) to (x2, y2)
    //recursive, so x1,y1 is slowly creeping towards x2, y2

    List<List<bool>> attempts;

    void ResetAttempts()
    {
        attempts = new List<List<bool>>();
        for (int i = 0; i < 10; i++)
        {
            List<bool> row = new List<bool>();
            for (int j = 0; j < 10; j++)
            {
                row.Add(false);
            }
            attempts.Add(row);
        }
    }

    bool TryMove(int x1, int y1, int x2, int y2)
    {
        //check if at or out of bounds first
        if(x1 < 0 || x1 >= board.Count || y1 < 0 || y1 >= board.Count
            || x2 < 0 || x2 >= board.Count || y2 < 0 || y2 >= board.Count)
        {
            Debug.Log("out of bounds");
            return false;
        }

        //we're at the destination and its empty
        if (x1 == x2 && y1 == y2 && board[x2][y2] == null)
        {
            Debug.Log("found path?");
            return true;
        }

        //checked here already
        if (attempts[x1][y1] == true)
        {
            Debug.Log("checked already");
            return false;
        }
        else
        {
            attempts[x1][y1] = true;
        }

        //dont look in a certain direction if theres a piece there
        bool pathFound = false;
        if (x1 + 1 < board.Count && board[x1 + 1][y1] == null)
        {
            pathFound = pathFound || TryMove(x1 + 1, y1, x2, y2);
        }
        if (x1 - 1 >= 0 && board[x1 - 1][y1] == null)
        {
            pathFound = pathFound || TryMove(x1 - 1, y1, x2, y2);
        }
        if (y1 + 1 < board.Count && board[x1][y1 + 1] == null)
        {
            pathFound = pathFound || TryMove(x1, y1 + 1, x2, y2);
        }
        if (y1 - 1 >= 0 && board[x1][y1 - 1] == null)
        {
            pathFound = pathFound || TryMove(x1, y1 - 1, x2, y2);
        }
        return pathFound;
    }

    //if there is a valid path from (x1, y1) to (x2, y2), move the piece to the corresponding gridbutton
    void MovePiece(int x1, int y1, int x2, int y2)
    {
        Piece p = board[x1][y1];
        GridButton gb = grid[x2][y2];

        board[x2][y2] = p;
        board[x1][y1] = null;

        p.gameObject.transform.SetParent(gb.gameObject.transform);
        RectTransform rt = p.GetComponent<RectTransform>();
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;

        //////
        /////
        ////
        ////    TODO, add anim/visual movement to pieces
        ////
        /////
        //do fancy animations later
        //StartCoroutine(MoveTransformToLocalZero(p.GetComponent<RectTransform>()));
    }

    IEnumerator MoveTransformToLocalZero(RectTransform t)
    {
        isInMoveAnim = true;
        //move piece back to local position zero
        while (t.localPosition != Vector3.zero)
        {
            Vector3 newPos = Vector3.Lerp(t.localPosition, Vector3.zero, Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }
        t.localPosition = Vector3.zero;

        isInMoveAnim = false;
        yield return null;
    }

    //check to the right, down right, and down 3 directions for matches of the same color
    //remove the pieces while checking here, and count the score based on the pieces removed times a incremental multiplier
    //do right, down right, then down, if a match occurs during any of the 3, 
    //remove all the pieces in that match except the starting position and add to count
    //finally at the end, if necessary, remove the origin piece and add to count
    //at very end, based on count, add to score, and return true/false based on the count.
    bool CheckBoard(int x, int y, Piece.PieceColor color)
    {
        bool connectionMade = false;

        int connectedPieces = 0;
        List<Pair> connectionPieces = new List<Pair>();
        
        //////////////////////////////////////////////////////////
        /// Check Vertical connection
        //////////////////////////////////////////////////////////
        //temp variables
        int tracker = 0;
        List<Pair> currentConnectedPieces = new List<Pair>();
        int negIndex = -1;  //up and left
        int posIndex = 1;   //down and right

        while (x + negIndex >= 0 && board[x + negIndex][y] != null && board[x + negIndex][y].color == color)
        {
            currentConnectedPieces.Add(new Pair(x + negIndex, y));
            tracker++;
            negIndex--;
        }
        while (x + posIndex < board.Count && board[x + posIndex][y] != null && board[x + posIndex][y].color == color)
        {
            currentConnectedPieces.Add(new Pair(x + posIndex, y));
            tracker++;
            posIndex++;
        }
        //connection made (including origin piece) all pieces accounted for to both sides
        if (tracker >= 4)
        {
            connectionMade = true;
            connectedPieces += tracker;
            connectionPieces.AddRange(currentConnectedPieces);
        }

        //////////////////////////////////////////////////////////
        /// Check Horizontal connection
        //////////////////////////////////////////////////////////
        //reset temp variables
        tracker = 0;
        currentConnectedPieces = new List<Pair>();
        negIndex = -1;  //up and left
        posIndex = 1;   //down and right

        while (y + negIndex >= 0 && board[x][y + negIndex] != null && board[x][y + negIndex].color == color)
        {
            currentConnectedPieces.Add(new Pair(x, y + negIndex));
            tracker++;
            negIndex--;
        }
        while (y + posIndex < board[x].Count && board[x][y + posIndex] != null && board[x][y + posIndex].color == color)
        {
            currentConnectedPieces.Add(new Pair(x, y + posIndex));
            tracker++;
            posIndex++;
        }
        //connection made (including origin piece) all pieces accounted for to both sides
        if (tracker >= 4)
        {
            connectionMade = true;
            connectedPieces += tracker;
            connectionPieces.AddRange(currentConnectedPieces);
        }

        //////////////////////////////////////////////////////////
        /// Check topleft-botright diagonal connection
        //////////////////////////////////////////////////////////
        //reset temp variables
        tracker = 0;
        currentConnectedPieces = new List<Pair>();
        negIndex = -1;  //up and left
        posIndex = 1;   //down and right

        while (x + negIndex >= 0 && y + negIndex >= 0 && board[x + negIndex][y + negIndex] != null && board[x + negIndex][y + negIndex].color == color)
        {
            currentConnectedPieces.Add(new Pair(x + negIndex, y + negIndex));
            tracker++;
            negIndex--;
        }
        while (x + posIndex < board.Count && y + posIndex < board[x].Count && board[x + posIndex][y + posIndex] != null && board[x + posIndex][y + posIndex].color == color)
        {
            currentConnectedPieces.Add(new Pair(x + posIndex, y + posIndex));
            tracker++;
            posIndex++;
        }
        //connection made (including origin piece) all pieces accounted for to both sides
        if (tracker >= 4)
        {
            connectionMade = true;
            connectedPieces += tracker;
            connectionPieces.AddRange(currentConnectedPieces);
        }

        //////////////////////////////////////////////////////////
        /// Check botleft-topright diagonal connection
        //////////////////////////////////////////////////////////
        //reset temp variables
        tracker = 0;
        currentConnectedPieces = new List<Pair>();
        negIndex = -1;  //up and left
        posIndex = 1;   //down and right

        while (x - negIndex < board.Count && y + negIndex >= 0 && board[x - negIndex][y + negIndex] != null && board[x - negIndex][y + negIndex].color == color)
        {
            currentConnectedPieces.Add(new Pair(x - negIndex, y + negIndex));
            tracker++;
            negIndex--;
        }
        while (x - posIndex >= 0 && y + posIndex < board[x].Count && board[x - posIndex][y + posIndex] != null && board[x - posIndex][y + posIndex].color == color)
        {
            currentConnectedPieces.Add(new Pair(x - posIndex, y + posIndex));
            tracker++;
            posIndex++;
        }
        //connection made (including origin piece) all pieces accounted for to both sides
        if (tracker >= 4)
        {
            connectionMade = true;
            connectedPieces += tracker;
            connectionPieces.AddRange(currentConnectedPieces);
        }
        if (connectionMade)
        {
            //include origin piece
            connectedPieces++;  
            connectionPieces.Add(new Pair(x, y));
            CalculateScore(connectedPieces);
            CleanUpPieces(connectionPieces);
        }
        return connectionMade;
    }

    void CalculateScore(int numConnectedPieces)
    {
        int multiplier = 1;
        int extraPieces = numConnectedPieces - 5;
        for(int i = 1; extraPieces >= i; i++)
        {
            extraPieces -= i;
            multiplier++;
        }

        int score = GameData.PointsPerPiece * multiplier * numConnectedPieces;
        totalScore += score;
        //do a broadcast here to update ui, or just have the ui in the controller, not a big deal
        OnScoreChange(totalScore);
    }

    void CleanUpPieces(List<Pair> pieces)
    {
        foreach(Pair p in pieces)
        {
            if(board[p.x][p.y] != null)
            {
                Destroy(board[p.x][p.y].gameObject);
                board[p.x][p.y] = null;
            }
        }
    }

    //spawns alternating 2 or 3 pieces randomly on the board, and check the board for each piece spawned to see if it matched
    //with any existing piece
    void SpawnPieces()
    {
        spawnTwo = !spawnTwo;

        while (nextSpawn.Count > 0 && !isGameOver)
        {
            int x = UnityEngine.Random.Range(0, board.Count);
            int y = UnityEngine.Random.Range(0, board.Count);
            if (TrySpawnPiece(x, y, nextSpawn[0]))
            {
                nextSpawn.RemoveAt(0);
            }
        }

        //fill in next spawn pieces
        int numPieceToSpawn = spawnTwo ? 2 : 3;
        while(numPieceToSpawn > 0)
        {
            nextSpawn.Add(GetRandomColor());
            numPieceToSpawn--;
        }
        OnNextSpawnChange(nextSpawn);
    }

    bool TrySpawnPiece(int x, int y, Piece.PieceColor color)
    {
        if (isGameOver) return false;
        //Debug.Log("tryspawn " + x + " " + y);
        if(board[x][y] != null)
        {
            return false;
        }

        GameObject newGamePiece = Instantiate(piecePrefab);
        GameObject gridButton = grid[x][y].gameObject;
        newGamePiece.transform.SetParent(gridButton.transform);
        RectTransform rt = newGamePiece.GetComponent<RectTransform>();
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;

        Piece piece = newGamePiece.GetComponent<Piece>();
        piece.Init(color);
        board[x][y] = piece;

        //check if the new spawn filled in a connection
        CheckBoard(x, y, color);

        //check if board is full
        if (IsBoardFull())
        {
            GameOver();
            return false;
        }

        return true;
    }

    bool IsBoardFull()
    {
        for(int i = 0; i < board.Count; i++)
        {
            for(int j = 0; j < board[i].Count; j++)
            {
                if(board[i][j] == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    Piece.PieceColor GetRandomColor()
    {
        var v = Piece.PieceColor.GetValues(typeof(Piece.PieceColor));
        return (Piece.PieceColor)v.GetValue(UnityEngine.Random.Range(0, v.Length));
    }

    void GameOver()
    {
        isGameOver = true;
        //pop up a game over
        gameOverOverlay.SetActive(true);
    }
}
