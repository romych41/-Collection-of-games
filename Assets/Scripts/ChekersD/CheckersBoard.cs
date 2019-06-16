using System.Collections.Generic;
using UnityEngine;
using System;

public class CheckersBoard
{
    private readonly Piece[,] _pieces = new Piece[8, 8];
    public bool IsWhiteTurn { get; private set; } = true;
    public bool HasKilled;

    public Piece SelectedPiece;

    public Client Client;


    // Объявляем делегат
    public delegate void Move(Piece p, int x, int y);

    // События
    public event Move MovePieces;
    public event Action<Piece> OnKing;
    public event Action<Piece> OnDestr;

    public event Action<bool, Client> OnEndTurn;


    public Piece GetPiece(int x1, int y1)
    {
        return _pieces[x1, y1];
    }

    public void SetPiece(Piece p, int x, int y)
    {
        _pieces[x, y] = p;
    }


    public void EndTurn(int x1, int y1, int x2, int y2)
    {
        // Для двойного прижка
        // int x = (int)endDrag.x;
        // int y = (int)endDrag.y;

        // Promotions
        // King
        if (SelectedPiece != null)
        {
            // Белая шашка приземлилась на конец борда
            if (SelectedPiece.IsWhite && !SelectedPiece.IsKing && y2 == 7)
            {
                SelectedPiece.IsKing = true;
                //selectedPiece.transform.Rotate(Vector3.right * 180);
                OnKing?.Invoke(SelectedPiece);
            }

            // Черная шашка приземлилась на конец борда
            if (!SelectedPiece.IsWhite && !SelectedPiece.IsKing && y2 == 0)
            {
                SelectedPiece.IsKing = true;
                //selectedPiece.transform.Rotate(Vector3.right * 180);
                OnKing?.Invoke(SelectedPiece);
            }
        }

        // Our message
        if (Client != null)
        {
            string msg = "CMOV|";
            msg += x1 + "|";
            msg += y1 + "|";
            msg += x2 + "|";
            msg += y2.ToString();
            Client.Send(msg);
        }

        SelectedPiece = null;

        // Сканировать на возможный ход (если убили). Для продолжения хода.
        if (ScanForForcedMoves(x2, y2).Count != 0 && HasKilled)
        {
            HasKilled = false;
            return;
        }
        IsWhiteTurn = !IsWhiteTurn;
        HasKilled = false;
        OnEndTurn?.Invoke(IsWhiteTurn, Client);
        
        CheckVictory();

        
    }

    private void EndTurnAi(int xo, int yo)
    {
        if (SelectedPiece != null)
        {
            // Белая шашка приземлилась на конец борда
            if (SelectedPiece.IsWhite && !SelectedPiece.IsKing && yo == 7)
            {
                SelectedPiece.IsKing = true;
                OnKing?.Invoke(SelectedPiece);
            }

            // Черная шашка приземлилась на конец борда
            if (!SelectedPiece.IsWhite && !SelectedPiece.IsKing && yo == 0)
            {
                SelectedPiece.IsKing = true;
                OnKing?.Invoke(SelectedPiece);
            }
        }

        
        MovePieces?.Invoke(SelectedPiece, xo, yo);
        SelectedPiece = null;
        if (ScanForForcedMoves(xo, yo).Count != 0 && HasKilled)
        {
            HasKilled = false;
            return;
        }

        IsWhiteTurn = !IsWhiteTurn;
        HasKilled = false;
        
        
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
                if (_pieces[i, j] != null)
                {
                    //hasWhite = true;
                    if (_pieces[i, j].IsWhite)
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
    private List<Piece> ScanForForcedMoves(int x, int y)
    {
        var forcedPieces = new List<Piece>();

        if (_pieces[x, y].IsForceToMove(_pieces, x, y))
        {
            forcedPieces.Add(_pieces[x, y]);
        }

        return forcedPieces;
    }

    public List<Piece> ScanForForcedMoves()
    {
        var forcedPieces = new List<Piece>();

        // Check all the pieces
        // 8 вместо pieces.Lenght потому что удаляются 
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (_pieces[i, j] != null && _pieces[i, j].IsWhite == IsWhiteTurn)
                {
                    if (_pieces[i, j].IsForceToMove(_pieces, i, j))
                    {
                        forcedPieces.Add(_pieces[i, j]);
                    }
                }
            }
        }

        return forcedPieces;
    }

    public void AiRealise()
    {
        if (!IsWhiteTurn)
        {
            int corX = 0;
            int corY = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (_pieces[i, j] != null && _pieces[i, j].IsWhite == IsWhiteTurn)
                    {
                        if (_pieces[i, j].IsForceToMove(_pieces, i, j))
                        {
                            corX = i;
                            corY = j;
                            SelectedPiece = _pieces[corX, corY];
                            goto exit_loop;
                        }
                    }
                }
            }

            exit_loop:

            if (SelectedPiece != null)
            {
                SelectedPiece.BlackCoord(_pieces, corX, corY, out var xo, out var yo);
                // Did we kill anything
                // If this is a jump
                if (Mathf.Abs(xo - corX) == 2)
                {
                    // Для получения средней шашки между двумя
                    var p = _pieces[(corX + xo) / 2, (corY + yo) / 2];
                    if (p != null)
                    {
                        // Удаляем среднюю шашку между двумя
                        _pieces[(corX + xo) / 2, (corY + yo) / 2] = null;
                        OnDestr?.Invoke(p);
                        HasKilled = true;
                    }
                }

                SelectedPiece = _pieces[corX, corY];
                _pieces[xo, yo] = SelectedPiece;
                _pieces[corX, corY] = null;
               
                EndTurnAi(xo, yo);
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                {
                    for (int j = 7; j >= 0; j--)
                    {
                        if (_pieces[i, j] != null && _pieces[i, j].IsWhite == IsWhiteTurn)
                        {
                            if (j < 7 && i < 7 && _pieces[i, j].IsKing && 
                                IsValidMove(_pieces[i, j], i, j, i + 1, j + 1))
                            {
                                SelectedPiece = _pieces[i, j];
                                _pieces[i + 1, j + 1] = SelectedPiece;
                                _pieces[i, j] = null;
                                EndTurnAi(i + 1, j + 1);
                                return;
                            }

                            if (j < 7 && i > 0 && _pieces[i, j].IsKing &&
                                IsValidMove(_pieces[i, j], i, j, i - 1, j + 1))
                            {
                                SelectedPiece = _pieces[i, j];
                                _pieces[i - 1, j + 1] = SelectedPiece;
                                _pieces[i, j] = null;
                                EndTurnAi(i - 1, j + 1);
                                return;
                            }

                            if (i > 0 && j > 0 && 
                                IsValidMove(_pieces[i, j], i, j, i - 1, j - 1))
                            {
                                SelectedPiece = _pieces[i, j];
                                _pieces[i - 1, j - 1] = SelectedPiece;
                                _pieces[i, j] = null;
                                EndTurnAi(i - 1, j - 1);
                                return;
                            }

                            if (i < 7 && 
                                IsValidMove(_pieces[i, j], i, j, i + 1, j - 1))
                            {
                                SelectedPiece = _pieces[i, j];
                                _pieces[i + 1, j - 1] = SelectedPiece;
                                _pieces[i, j] = null;
                                EndTurnAi(i + 1, j - 1);
                               return;
                            }
                        }
                    }
                }
            }
        }
    }

    public bool IsValidMove(Piece piece, int x1, int y1, int x2, int y2)
    {
        var isWhite = piece.IsWhite;
        var isKing = piece.IsKing;
        // If you are moving on top of another piece
        if (_pieces[x2, y2] != null)
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
                    Piece p = _pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.IsWhite != isWhite)
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
                    var p = _pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.IsWhite != isWhite)
                        return true;
                }
            }
        }

        return false;
    }
}