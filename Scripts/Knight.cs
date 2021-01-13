using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override bool[,] PossibleMove(){
        bool [,] r = new bool[8,8];

        //Up-Left
        KnightMove(xPosition - 1, yPosition + 2, ref r);
        //Up-Right
        KnightMove(xPosition + 1, yPosition + 2, ref r);

        //Down-Left
        KnightMove(xPosition - 1, yPosition - 2, ref r);
        //Down-Right
        KnightMove(xPosition + 1, yPosition - 2, ref r);

        //Right-Up
        KnightMove(xPosition + 2, yPosition + 1, ref r);
        //Right-Down
        KnightMove(xPosition + 2, yPosition - 1, ref r);

        //Left-Up
        KnightMove(xPosition - 2, yPosition + 1, ref r);
        //Left-Down
        KnightMove(xPosition - 2, yPosition - 1, ref r);


        /* Disable moves that will result in us in Check (Also works if we already are in check)*/
            //Locate King Coordinates
        int[] kingCoordinates = isWhite ? BoardController.Instance.LocateKing(0) : BoardController.Instance.LocateKing(1); //If white, get white king coords, else black
        ChessPiece king = BoardController.Instance.ChessPieces[kingCoordinates[0], kingCoordinates[1]]; //Get King
        //Loop over each possible move for Knight
        for(int i = 0;i < 8;i++){
            for(int j = 0;j < 8;j++){
                if (r[i,j]) {
                    //We make the move to test (only logically, not with transition)
                    BoardController.Instance.ChessPieces[xPosition, yPosition] = null; //Set our current position to null, about to move
                    ChessPiece testC = BoardController.Instance.ChessPieces[i, j]; //Check for enemy piece on tile we are testing to move (test capture?)
                    BoardController.Instance.ChessPieces[i, j] = this; //Make the test move
                    if(king.InCheck()) { //The King is still in check, move not possible
                        r[i, j] = false; //Not a valid move
                    }
                    //Set Pawn back to its correct position
                    BoardController.Instance.ChessPieces[xPosition, yPosition] = this;
                    //Set test captured enemy back (if one)
                    if (testC != null) { //it must be an enemy
                        BoardController.Instance.ChessPieces[i, j] = testC;
                    } else { //There was not an enemy there
                        BoardController.Instance.ChessPieces[i, j] = null;
                    }
                }
            }
        }

        return r;
    }
    public void KnightMove(int x, int y, ref bool[,] r) {
        ChessPiece c;
        if (x >= 0 && x < 8 && y >=0 && y < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[x, y]; //Check for piece on tile to move
            if (c == null) //If tile is empty
                r[x, y] = true; //This is a valid move
            else if (isWhite != c.isWhite) //If the tile contains an enemy
                r[x, y] = true; //This is a valid move
        }
    }
}
