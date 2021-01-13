using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAI : MonoBehaviour
{
    public bool isWhite = false; //Initially set to Black
    public static ChessAI Instance{set;get;} //Add for outside reference
    private List<MoveCollection> AIMoves;
    private List<MoveCollection> PlayerMoves;

    enum OpeningMoves {
        KingsPawn,
        QueensPawn,
        Other
    }

    private OpeningMoves opening = OpeningMoves.Other;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void StartGame(bool isWhite) {
        this.isWhite = isWhite;
        if (isWhite) {
            Debug.Log("Spawning on the white team");
        } else {
            Debug.Log("Spawning on the black team");
        }
    }

    /* Calculates every possible moveCollection */
    public void CalculateMoves(){
        AIMoves = new List<MoveCollection>(); //Initialize moveCollection list
        PlayerMoves = new List<MoveCollection>();
        for (int i = 0; i < 8; i++) { //Scan the board
            for (int j = 0; j < 8; j++) {
                ChessPiece c = BoardController.Instance.ChessPieces[i,j];
                if (c != null && c.isWhite == isWhite) { //If we find a piece on our team
                    bool[,] r = c.PossibleMove(); //We need to save the moves with a ref to the piece
                    bool hasAMove = false; //Check if piece can moveCollection
                    for (int h = 0; h < 8; h++) { //Scan the moves
                        for (int k = 0; k < 8; k++) {
                            if (r[h, k]) { //If we have a possible moveCollection
                                hasAMove = true;
                            }
                        }
                    }
                    if (hasAMove) { //Only add if piece has a possible moveCollection
                        AIMoves.Add(new MoveCollection(i, j, r));
                    }
                } else if (c != null && c.isWhite == !isWhite) { //If we find a piece on the enemy team (Player team)
                    bool[,] r = c.PossibleMove(); //We need to save the moves with a ref to the piece
                    bool hasAMove = false; //Check if piece can moveCollection
                    for (int h = 0; h < 8; h++) { //Scan the moves
                        for (int k = 0; k < 8; k++) {
                            if (r[h, k]) { //If we have a possible moveCollection
                                hasAMove = true;
                            }
                        }
                    }
                    if (hasAMove) { //Only add if piece has a possible moveCollection
                        PlayerMoves.Add(new MoveCollection(i, j, r));
                    }
                }
            }
        }
        SelectMove();
    }

    void SelectMove(){
        if (BoardController.Instance.moveCount == 0) {
            //Opening move
            int xPos = 0;
            int yPos = 0;
            for (int i = 0; i < 8; i++) { //Scan rows 2 & 3
                for (int j = 2; j < 4; j++) {
                    ChessPiece c = BoardController.Instance.ChessPieces[i,j];
                    if (c != null) { //If we find a piece
                        xPos = i;
                        yPos = j;
                    }
                }
            }
            if (xPos == 4 && yPos == 3) {
                opening = OpeningMoves.KingsPawn;
            } else if (xPos == 3 && yPos == 3) {
                opening = OpeningMoves.QueensPawn;
            }
            MakeBookMove();
        } else {
            if (AIMoves.Count > 0) {
                MinimaxRoot(3, false);
            } else {
                BoardController.Instance.Stalemate(); //AI has no moves and has not yet been mated
            }
        }        
    }

    void MakeBookMove(){
        switch (opening)
        {
            case OpeningMoves.KingsPawn:
                {
                //Respond with e5
                BoardController.Instance.selectedChessPiece = BoardController.Instance.ChessPieces[4, 6];
                BoardController.Instance.AllowedMoves = BoardController.Instance.ChessPieces[4, 6].PossibleMove(); //Set piece's correct allowed moves
                BoardController.Instance.MoveChessPiece(4, 4);
                BoardController.Instance.moveCount += 1;
                break;
                }
            case OpeningMoves.QueensPawn:
                {
                //Respond with d5
                BoardController.Instance.selectedChessPiece = BoardController.Instance.ChessPieces[3, 6];
                BoardController.Instance.AllowedMoves = BoardController.Instance.ChessPieces[3, 6].PossibleMove(); //Set piece's correct allowed moves
                BoardController.Instance.MoveChessPiece(3, 4);
                BoardController.Instance.moveCount += 1;
                break;
                }
            default:
                {
                MinimaxRoot(3, false);
                break;
                }
        }
    }

    private int EvaluateBoard(){
        int enemyValue = 0;
        for (int i = 0; i < 8; i++) { //Scan the board
            for (int j = 0; j < 8; j++) {
                ChessPiece c = BoardController.Instance.ChessPieces[i,j]; //Check for piece on tile
                if (c != null) { //If we find a enemy piece
                    if (c is Pawn) {
                            enemyValue += 10;
                    } else if (c is Knight || c is Bishop) {
                            enemyValue += 30;
                    } else if (c is Rook) { 
                            enemyValue += 50;
                    } else if (c is Queen) {
                            enemyValue += 90;
                    } else if (c is King) {
                            enemyValue += 900;
                    }
                }
            }
        }
        return enemyValue;
    }

    void MinimaxRoot(int depth, bool isMaximizer){
        int bestEval = -9999;
        int[] bestMoveIndex = new int[2]; //X and Y coordinates of best move
        MoveCollection bestMoveCollection = AIMoves[0];

        foreach (MoveCollection moveCollection in AIMoves)
        {
            for (int i = 0; i < 8; i++) { //Scan possible moves of current piece
                for (int j = 0; j < 8; j++) {
                    if (moveCollection.moves[i, j]) {
                        //Test make the move
                        ChessPiece currentPiece = BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]];
                        BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]] = null; //Set piece's current position to null, about to moveCollection
                        ChessPiece testC = BoardController.Instance.ChessPieces[i, j]; //Check for enemy piece on tile we are testing to moveCollection (test capture?)
                        BoardController.Instance.ChessPieces[i, j] = currentPiece; //Make the test moveCollection

                        int moveEval = Minimax(depth - 1, !isMaximizer);
                        //Undo the test move
                        //Set Piece back to its correct position
                        BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]] = currentPiece;
                        //Set test captured enemy back (if one)
                        if (testC != null) { //it must be an enemy
                            BoardController.Instance.ChessPieces[i, j] = testC;
                        } else { //There was not an enemy there
                            BoardController.Instance.ChessPieces[i, j] = null;
                        } 
                        if (moveEval >= bestEval) { 
                            bestEval = moveEval;
                            bestMoveIndex[0] = i;
                            bestMoveIndex[1] = j;
                            bestMoveCollection = moveCollection;
                        } 
                    }
                }
            }
        }

        BoardController.Instance.selectedChessPiece = BoardController.Instance.ChessPieces[bestMoveCollection.pieceCoordinates[0], bestMoveCollection.pieceCoordinates[1]];
        BoardController.Instance.AllowedMoves = bestMoveCollection.moves; //Set piece's correct allowed moves
        BoardController.Instance.MoveChessPiece(bestMoveIndex[0], bestMoveIndex[1]); //Choose best one
        BoardController.Instance.moveCount += 1;
    }

    int Minimax(int depth, bool isMaximizer){
        int bestEval = 0;
        if (depth == 0) {
            return -EvaluateBoard();
        }

        if (isMaximizer) { //Player moves
            bestEval = -9999;
            foreach (MoveCollection moveCollection in PlayerMoves)
            {
                for (int i = 0; i < 8; i++) { //Scan possible moves of current piece
                    for (int j = 0; j < 8; j++) {
                        if (moveCollection.moves[i, j]) {
                            /* Test make the move */
                            ChessPiece currentPiece = BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]];
                            BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]] = null; //Set piece's current position to null, about to moveCollection
                            ChessPiece testC = BoardController.Instance.ChessPieces[i, j]; //Check for enemy piece on tile we are testing to moveCollection (test capture?)
                            BoardController.Instance.ChessPieces[i, j] = currentPiece; //Make the test moveCollection

                            bestEval = System.Math.Max(bestEval, Minimax(depth - 1, !isMaximizer));

                            /* Undo the move */
                            //Set Piece back to its correct position
                            BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]] = currentPiece;
                            //Set test captured enemy back (if one)
                            if (testC != null) { //it must be an enemy
                                BoardController.Instance.ChessPieces[i, j] = testC;
                            } else { //There was not an enemy there
                                BoardController.Instance.ChessPieces[i, j] = null;
                            }   
                        }
                    }
                }
            }
        } else {
            //Handle else 
            bestEval = 9999;
            foreach (MoveCollection moveCollection in AIMoves)
            {
                for (int i = 0; i < 8; i++) { //Scan possible moves of current piece
                    for (int j = 0; j < 8; j++) {
                        if (moveCollection.moves[i, j]) {
                            /* Test make the move */
                            ChessPiece currentPiece = BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]];
                            BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]] = null; //Set piece's current position to null, about to moveCollection
                            ChessPiece testC = BoardController.Instance.ChessPieces[i, j]; //Check for enemy piece on tile we are testing to moveCollection (test capture?)
                            BoardController.Instance.ChessPieces[i, j] = currentPiece; //Make the test moveCollection
                            bestEval = System.Math.Min(bestEval, Minimax(depth - 1, !isMaximizer));
                            /* Undo the move */
                            //Set Piece back to its correct position
                            BoardController.Instance.ChessPieces[moveCollection.pieceCoordinates[0], moveCollection.pieceCoordinates[1]] = currentPiece;
                            //Set test captured enemy back (if one)
                            if (testC != null) { //it must be an enemy
                                BoardController.Instance.ChessPieces[i, j] = testC;
                            } else { //There was not an enemy there
                                BoardController.Instance.ChessPieces[i, j] = null;
                            }   
                        }
                    }
                }
            }
        }
        return bestEval;
    }

    /* Object to hold ref to certain piece w/ their possible moves*/
    public class MoveCollection {
        public int[] pieceCoordinates = new int[2];
        public bool[,] moves;
        public MoveCollection(int x, int y, bool[,] r){
            pieceCoordinates[0] = x;
            pieceCoordinates[1] = y;
            moves = r;
        }
    }
}