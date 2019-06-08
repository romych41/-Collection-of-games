﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoreLib;
using System;

public class CheckersBoard
{

    public Piece[,] pieces = new Piece[8, 8];

    // Для проверки цвета шашки
    public bool isWhite;
    public bool isWhiteTurn;
    public bool hasKilled;

    public Piece selectedPiece;

    public Client client;


    // Объявляем делегат
    public delegate void Move(Piece p, int x, int y);

    // События
    public event Move MovePieces;
    public event Action<Piece> OnKing;
    public event Action<Piece> OnDestr;

    public event Action<bool, Client> OnEndTurn;
    

    public Piece GetPiece(int x1, int y1)
    {
        return pieces[x1, y1];
    }

    public void SetPiece(Piece p, int x, int y)
    {
        pieces[x, y] = p;
    }


    public void EndTurn(int x1, int y1, int x2, int y2)
    {
        // Для двойного прижка
        // int x = (int)endDrag.x;
        // int y = (int)endDrag.y;

        // Promotions
        // King
        if (selectedPiece != null)
        {
            // Белая шашка приземлилась на конец борда
            if (selectedPiece.isWhite && !selectedPiece.isKing && y2 == 7)
            {
                selectedPiece.isKing = true;
                //selectedPiece.transform.Rotate(Vector3.right * 180);
                OnKing(selectedPiece);
            }
            // Черная шашка приземлилась на конец борда
            if (!selectedPiece.isWhite && !selectedPiece.isKing && y2 == 0)
            {
                selectedPiece.isKing = true;
                //selectedPiece.transform.Rotate(Vector3.right * 180);
                OnKing(selectedPiece);
            }
        }

        // Our message
        if (client != null)
        {
            string msg = "CMOV|";
            msg += x1.ToString() + "|";
            msg += y1.ToString() + "|";
            msg += x2.ToString() + "|";
            msg += y2.ToString();

            client.Send(msg);
        }

        selectedPiece = null;
        // startDrag = Vector2.zero;
        // x1 = 0;
        // y1 = 0;

        // Сканировать на возможный ход (если убили). Для продолжения хода.
        if (ScanForPossibleMove(selectedPiece, x2, y2).Count != 0 && hasKilled)
        {
            hasKilled = false;
            return;
        }

        Debug.Log("White turn");
        isWhiteTurn = !isWhiteTurn;
        Debug.Log("Black turn");
        isWhite = !isWhite;
        hasKilled = false;
        CheckVictory();

        OnEndTurn(isWhiteTurn, client);

        // if (client == null)
        // {
        //     isWhite = !isWhite;
        //     // if (isWhite)
        //     // {
        //     //     Alert("White player's turn");
        //     // }
        //     // else
        //     // {
        //     //     Alert("Black player's turn");
        //     // }
        //     if (isWhite)
        //     {
        //         Debug.Log("White player's turn");
        //     }
        //     else
        //     {
        //         Debug.Log("Black player's turn");
        //     }
        // }
        // else
        // {
        //     // if (isWhite)
        //     // {
        //     //     Alert(client.players[0].name + "'s turn");
        //     // }
        //     // else
        //     // {
        //     //     Alert(client.players[1].name + "'s turn");
        //     // }
        //     if (isWhite)
        //     {
        //         Debug.Log(client.players[0].name + "'s turn");
        //     }
        //     else
        //     {
        //         Debug.Log(client.players[1].name + "'s turn");
        //     }
        // }

    }

    private void EndTurnAI(int xo, int yo)
    {
        if (selectedPiece != null)
        {
            // Белая шашка приземлилась на конец борда
            if (selectedPiece.isWhite && !selectedPiece.isKing && yo == 7)
            {
                selectedPiece.isKing = true;
                //selectedPiece.transform.Rotate(Vector3.right * 180);
                OnKing(selectedPiece);
            }
            // Черная шашка приземлилась на конец борда
            if (!selectedPiece.isWhite && !selectedPiece.isKing && yo == 0)
            {
                selectedPiece.isKing = true;
                //selectedPiece.transform.Rotate(Vector3.right * 180);
                OnKing(selectedPiece);
            }
        }

        selectedPiece = null;
        //startDrag = Vector2.zero;

        if (ScanForPossibleMove(selectedPiece, xo, yo).Count != 0 && hasKilled)
        {
            hasKilled = false;
            return;
        }

        isWhiteTurn = !isWhiteTurn;
        isWhite = !isWhite;
        hasKilled = false;
        CheckVictory();

    }

    private void CheckVictory()
    {
        bool hasWhite = false, hasBlack = false;
        //var ps = FindObjectsOfType<Piece>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] != null)
                {
                    //hasWhite = true;
                    if(pieces[i, j].isWhite)
                    {
                        hasWhite = true;
                    }
                    else
                    {
                       hasBlack = true;
                    }
                }
            }
        }


        if (!hasWhite)
        {
            Victory(false);
        }
        if (!hasBlack)
        {
            Victory(true);
        }

    }

    private void Victory(bool isWhite)
    {
        if (isWhite)
            Debug.Log("White team has won");
        else
            Debug.Log("Black team has won");
    }

    // Сканирование для одной шашки(на возможность двойного прижка)
    private List<Piece> ScanForPossibleMove(Piece p, int x, int y)
    {
        var forcedPieces = new List<Piece>();

        if (pieces[x, y].isForceToMove(pieces, x, y))
        {
            forcedPieces.Add(pieces[x, y]);
        }
        return forcedPieces;
    }

    public List<Piece> ScanForPossibleMove()
    {
        var forcedPieces = new List<Piece>();

        // Check all the pieces
        // 8 вместо pieces.Lenght потому что удаляются 
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                {
                    if (pieces[i, j].isForceToMove(pieces, i, j))
                    {
                        forcedPieces.Add(pieces[i, j]);
                    }
                }
            }
        }
        return forcedPieces;
    }

    public void AIRealise()
    {
        if (!isWhiteTurn)
        {
            int corX = 0;
            int corY = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                    {
                        if (pieces[i, j].isForceToMove(pieces, i, j))
                        {
                            //forcedAIPieces.Add(pieces[i, j]);

                            //if (forcedAIPieces.Count == 1)
                            //{

                                corX = i;
                                corY = j;
                                selectedPiece = pieces[corX, corY];
                                goto bla;
                            //}
                        }
                    }
                }
            }
            bla:
            int xo = 0;
            int yo = 0;

            if (selectedPiece != null)
            {
                selectedPiece.BlackCoord(pieces, corX, corY, out xo, out yo);
                //forcedAIPieces.RemoveRange(0, forcedAIPieces.Count);
                // Did we kill anything
                // If this is a jump
                if (Mathf.Abs(xo - corX) == 2)
                {
                    // Для получения средней шашки между двумя
                    Piece p = pieces[(corX + xo) / 2, (corY + yo) / 2];
                    if (p != null)
                    {
                        // Удаляем среднюю шашку между двумя
                        pieces[(corX + xo) / 2, (corY + yo) / 2] = null;
                        //DestroyImmediate(p.gameObject);
                        OnDestr(p);
                        hasKilled = true;
                    }
                }
                selectedPiece = pieces[corX, corY];
                pieces[xo, yo] = selectedPiece;
                pieces[corX, corY] = null;
                MovePieces(selectedPiece, xo, yo);
                EndTurnAI(xo, yo);
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                {
                    for (int j = 7; j >= 0; j--)
                    {
                        if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                        {
                            if (j < 7 && i < 7 && pieces[i, j].isKing && ValidMove(pieces[i, j], i, j, i + 1, j + 1))
                            {
                                selectedPiece = pieces[i, j];
                                pieces[i + 1, j + 1] = selectedPiece;
                                pieces[i, j] = null;
                                MovePieces(selectedPiece, i + 1, j + 1);
                                EndTurnAI(i + 1, j + 1);
                                goto metka;
                            }
                            else if (j < 7 && i > 0 && pieces[i, j].isKing && ValidMove(pieces[i, j], i, j, i - 1, j + 1))
                            {
                                selectedPiece = pieces[i, j];
                                pieces[i - 1, j + 1] = selectedPiece;
                                pieces[i, j] = null;
                                MovePieces(selectedPiece, i - 1, j + 1);
                                EndTurnAI(i - 1, j + 1);
                                goto metka;
                            }
                            else if (i > 0 && j > 0 && ValidMove(pieces[i, j], i, j, i - 1, j - 1))
                            {
                                selectedPiece = pieces[i, j];
                                pieces[i - 1, j - 1] = selectedPiece;
                                pieces[i, j] = null;
                                MovePieces(selectedPiece, i - 1, j - 1);
                                EndTurnAI(i - 1, j - 1);
                                goto metka;
                            }
                            else if (i < 7 && ValidMove(pieces[i, j], i, j, i + 1, j - 1))
                            {
                                selectedPiece = pieces[i, j];
                                pieces[i + 1, j - 1] = selectedPiece;
                                pieces[i, j] = null;
                                MovePieces(selectedPiece, i + 1, j - 1);
                                EndTurnAI(i + 1, j - 1);
                                goto metka;
                            }
                        }
                    }
                }
            metka: Debug.Log("Good!");
            }
        }
    }

     public bool ValidMove(Piece piece, int x1, int y1, int x2, int y2)
    {
        var isWhite = piece.isWhite;
        var isKing = piece.isKing;
        // If you are moving on top of another piece
        if (pieces[x2, y2] != null)
        {
            return false;
        }
        // Берем абсолютное значение
        int deltaMove = Mathf.Abs(x1 - x2);
        // Нам понадобится -1 когда мы находимся в черной команде
        int deltaMoveY = y2 - y1;

        // For white team
        if (isWhite || isKing)
        {
            // For normal jump
            if (deltaMove == 1)
            {
                if (deltaMoveY == 1)
                {
                    return true;
                }
            }
            // For kill jump
            // kill piece
            else if (deltaMove == 2)
            {
                if (deltaMoveY == 2)
                {
                    // Для получения средней шашки между двумя
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }

        // For black team
        if (!isWhite || isKing)
        {
            if (deltaMove == 1)
            {
                if (deltaMoveY == -1)
                {
                    return true;
                }
            }
            // kill piece
            else if (deltaMove == 2)
            {
                if (deltaMoveY == -2)
                {
                    // Для получения средней шашки между двумя
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }

        return false;
    }
}