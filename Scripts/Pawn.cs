using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override bool[,] PossibleMove(){
        bool[,] r  = new bool[8, 8];
        ChessPiece c, c2;
        int[] e = BoardController.Instance.EnPassantMove; //Get EnPassantMove array from Board Controller

        //White Turn
        if (isWhite) {
            //Diagonal Left
            if(xPosition != 0 && yPosition != 7) { //If it is not at left most tile and not at the top
                //EnPassant
                if (e[0] == xPosition - 1 && e[1] == yPosition + 1) //If
                    r[xPosition -1, yPosition + 1] = true;

                c = BoardController.Instance.ChessPieces[xPosition - 1, yPosition + 1]; //Check to see if there is a piece on that tile
                if(c != null && !c.isWhite) //If there is a piece there to take, and it is on the opposing team
                    r[xPosition - 1, yPosition + 1] = true; //This is a possible move
            }
            //Diagonal Right
            if(xPosition != 7 && yPosition != 7) { //If it is not at right most tile and not at the top
                //EnPassant
                if (e[0] == xPosition + 1 && e[1] == yPosition + 1) //If
                    r[xPosition + 1, yPosition + 1] = true;

                c = BoardController.Instance.ChessPieces[xPosition + 1, yPosition + 1]; //Check to see if there is a piece on that tile
                if(c != null && !c.isWhite) //If there is a piece there to take, and it is on the opposing team
                    r[xPosition + 1, yPosition + 1] = true; //This is a possible move
            }
            //Middle
            if(yPosition != 7) { //If it is not at the top
                c = BoardController.Instance.ChessPieces[xPosition, yPosition + 1]; //Check to see if there is a piece on that tile
                if(c == null) //If there is no piece blocking the pawn
                    r[xPosition, yPosition + 1] = true; //This is a possible move
            }
            //Middle on First Move
            if(yPosition == 1) { //Check to see if it has been moved
                c = BoardController.Instance.ChessPieces[xPosition, yPosition + 1]; //Check to see if there is a piece on first tile ahead
                c2 = BoardController.Instance.ChessPieces[xPosition, yPosition + 2]; //Check to see if there is a piece on second tile ahead
                if(c == null && c2  == null) //If there is no piece blocking the pawn
                    r[xPosition, yPosition + 2] = true; //This is a possible move
            }
        } else { //Black Turn
            //Diagonal Left
            if(xPosition != 0 && yPosition != 0) { //If it is not at left most tile and not at the bottom

                //EnPassant
                if (e[0] == xPosition - 1 && e[1] == yPosition - 1) //If
                    r[xPosition - 1, yPosition - 1] = true;

                c = BoardController.Instance.ChessPieces[xPosition - 1, yPosition - 1]; //Check to see if there is a piece on that tile
                if(c != null && c.isWhite) //If there is a piece there to take, and it is on the opposing team
                    r[xPosition - 1, yPosition - 1] = true; //This is a possible move
            }
            //Diagonal Right
            if(xPosition != 7 && yPosition != 0) { //If it is not at right most tile and not at the bottom
                //EnPassant
                if (e[0] == xPosition + 1 && e[1] == yPosition - 1) //If
                    r[xPosition + 1, yPosition - 1] = true;

                c = BoardController.Instance.ChessPieces[xPosition + 1, yPosition - 1]; //Check to see if there is a piece on that tile
                if(c != null && c.isWhite) //If there is a piece there to take, and it is on the opposing team
                    r[xPosition + 1, yPosition - 1] = true; //This is a possible move
            }
            //Middle
            if(yPosition != 0) { //If it is not at the bottom
                c = BoardController.Instance.ChessPieces[xPosition, yPosition - 1]; //Check to see if there is a piece on that tile
                if(c == null) //If there is no piece blocking the pawn
                    r[xPosition, yPosition - 1] = true; //This is a possible move
            }
            //Middle on First Move
            if(yPosition == 6) { //Check to see if it has been moved
                c = BoardController.Instance.ChessPieces[xPosition, yPosition - 1]; //Check to see if there is a piece on first tile ahead
                c2 = BoardController.Instance.ChessPieces[xPosition, yPosition - 2]; //Check to see if there is a piece on second tile ahead
                if(c == null && c2  == null) //If there is no piece blocking the pawn
                    r[xPosition, yPosition - 2] = true; //This is a possible move
            }
        }
        
        /* Disable moves that will result in us in Check (Also works if we already are in check)*/
        //Locate King Coordinates
        int[] kingCoordinates = isWhite ? BoardController.Instance.LocateKing(0) : BoardController.Instance.LocateKing(1); //If white, get white king coords, else black
        ChessPiece king = BoardController.Instance.ChessPieces[kingCoordinates[0], kingCoordinates[1]]; //Get King
        //Loop over each possible move for Pawn
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
}
