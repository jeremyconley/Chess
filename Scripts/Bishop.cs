using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override bool[,] PossibleMove(){
        bool[,] r = new bool[8,8];//Array of possible moves

        ChessPiece c;
        int i, j;

        /* UP-LEFT */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            i--; //Move up-left 1 tile
            j++;
            if (i < 0 || j >= 8) //If outside of board range
                break;

            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c == null)//If tile is empty
                r[i, j] = true;//This is a valid move
            else {
                if (isWhite != c.isWhite) //Tile contains enemy
                    r[i, j] = true;//This is a valid move
                break; //Stop here, can't go beyond blocking piece
            }
        }

        /* UP-RIGHT */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            i++; //Move up-right 1 tile
            j++;
            if (i >= 8 || j >= 8) //If outside of board range
                break;

            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c == null)//If tile is empty
                r[i, j] = true;//This is a valid move
            else {
                if (isWhite != c.isWhite) //Tile contains enemy
                    r[i, j] = true;//This is a valid move
                break; //Stop here, can't go beyond blocking piece
            }
        }

        /* DOWN-LEFT */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            i--; //Move down-left 1 tile
            j--;
            if (i < 0 || j < 0) //If outside of board range
                break;

            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c == null)//If tile is empty
                r[i, j] = true;//This is a valid move
            else {
                if (isWhite != c.isWhite) //Tile contains enemy
                    r[i, j] = true;//This is a valid move
                break; //Stop here, can't go beyond blocking piece
            }
        }
        /* DOWN-RIGHT */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            i++; //Move down-right 1 tile
            j--;
            if (i >= 8 || j < 0) //If outside of board range
                break;

            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c == null)//If tile is empty
                r[i, j] = true;//This is a valid move
            else {
                if (isWhite != c.isWhite) //Tile contains enemy
                    r[i, j] = true;//This is a valid move
                break; //Stop here, can't go beyond blocking piece
            }
        }

        /* Disable moves that will result in us in Check (Also works if we already are in check)*/
            //Locate King Coordinates
        int[] kingCoordinates = isWhite ? BoardController.Instance.LocateKing(0) : BoardController.Instance.LocateKing(1); //If white, get white king coords, else black
        ChessPiece king = BoardController.Instance.ChessPieces[kingCoordinates[0], kingCoordinates[1]]; //Get King
        //Loop over each possible move for Bishop
        for(int h = 0;h < 8;h++){
            for(int k = 0;k < 8;k++){
                if (r[h,k]) {
                    //We make the move to test (only logically, not with transition)
                    BoardController.Instance.ChessPieces[xPosition, yPosition] = null; //Set our current position to null, about to move
                    ChessPiece testC = BoardController.Instance.ChessPieces[h, k]; //Check for enemy piece on tile we are testing to move (test capture?)
                    BoardController.Instance.ChessPieces[h, k] = this; //Make the test move
                    if(king.InCheck()) { //The King is still in check, move not possible
                        r[h, k] = false; //Not a valid move
                    }
                    //Set Bishop back to its correct position
                    BoardController.Instance.ChessPieces[xPosition, yPosition] = this;
                    //Set test captured enemy back (if one)
                    if (testC != null) { //it must be an enemy
                        BoardController.Instance.ChessPieces[h, k] = testC;
                    } else { //There was not an enemy there
                        BoardController.Instance.ChessPieces[h, k] = null;
                    }
                }
            }
        }


        return r;
    }
}
