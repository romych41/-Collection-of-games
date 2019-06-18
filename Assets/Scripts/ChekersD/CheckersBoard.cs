using System.Collections.Generic;
using UnityEngine;
using System;

public class CheckersBoard
{
    private readonly Piece[,] _pieces = new Piece[8, 8];
    public bool IsWhiteTurn { get; private set; } = true;

    public bool HasKilled { get; set; }

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


    public void EndTurn(Piece movedPiece, int x1, int y1, int x2, int y2)
    {
        MovePieces?.Invoke(movedPiece, x2, y2);
        CheckForKing(movedPiece, x1, y2);

        // Our message
        if (Client != null && Client.isHost != IsWhiteTurn)
        {
            string msg = "CMOV|";
            msg += x1 + "|";
            msg += y1 + "|";
            msg += x2 + "|";
            msg += y2.ToString();
            Client.Send(msg);
        }

        // Сканировать на возможный ход (если убили). Для продолжения хода.
        if (HasForcedMoves(x2, y2) && HasKilled)
        {
            HasKilled = false;
            return;
        }

        HasKilled = false;
        IsWhiteTurn = !IsWhiteTurn;
        OnEndTurn?.Invoke(IsWhiteTurn, Client);
        CheckVictory();
    }

//    private void EndTurnAi(Piece movedPiece, int x1, int y1, int x2, int y2)
//    {
//        CheckForKing(movedPiece, x2, y2);
//        MovePieces?.Invoke(movedPiece, x2, y2);
//
//        if (HasForcedMoves(x2, y2) && HasKilled)
//        {
//            HasKilled = false;
//            return;
//        }
//
//        HasKilled = false;
//        IsWhiteTurn = !IsWhiteTurn;
//        CheckVictory();
//    }

    private void CheckForKing(Piece p, int xo, int yo)
    {
        if (p != null && !p.IsKing &&
            ((p.IsWhite && yo == 7) || (!p.IsWhite && yo == 0)))
        {
            p.IsKing = true;
            OnKing?.Invoke(p);
        }
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

        else if (!hasBlack)
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
    public bool HasForcedMoves(int x, int y)
        => _pieces[x, y].IsForceToMove(_pieces, x, y) != null;

    public List<Piece> ScanForForcedMoves()
    {
        var forcedPieces = new List<Piece>();

        // Check all the pieces
        // 8 вместо pieces.Lenght потому что удаляются 
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (_pieces[i, j] != null &&
                    _pieces[i, j].IsWhite == IsWhiteTurn &&
                    _pieces[i, j].IsForceToMove(_pieces, i, j) != null)
                {
                    forcedPieces.Add(_pieces[i, j]);
                }
            }
        }

        return forcedPieces;
    }

    public void AiRealise()
    {
        if (IsWhiteTurn) return;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (_pieces[i, j] != null &&
                    // this is here because race conditions
                    _pieces[i, j].IsWhite == IsWhiteTurn &&
                    _pieces[i, j].IsForceToMove(_pieces, i, j) != null)
                {
                    var selectedPiece = _pieces[i, j];
                    var (xo, yo) = selectedPiece.IsForceToMove(_pieces, i, j) ??
                                   throw new ArgumentException(nameof(selectedPiece));
                    // Did we kill anything
                    // If this is a jump
                    if (Mathf.Abs(xo - i) == 2)
                    {
                        // Для получения средней шашки между двумя
                        var p = _pieces[(i + xo) / 2, (j + yo) / 2];
                        if (p != null)
                        {
                            // Удаляем среднюю шашку между двумя
                            _pieces[(i + xo) / 2, (j + yo) / 2] = null;
                            OnDestr?.Invoke(p);
                            HasKilled = true;
                        }
                    }

                    MovePiece((i, j), (xo, yo));
                    EndTurn(selectedPiece, i, j, xo, yo);
                    return;
                }
            }
        }

        for (int i = 7; i >= 0; i--)
        {
            for (int j = 7; j >= 0; j--)
            {
                var p = _pieces[i, j];
                if (p != null && p.IsWhite == IsWhiteTurn)
                {
                    if (j < 7 && i < 7 && _pieces[i, j].IsKing &&
                        IsValidMove(_pieces[i, j], i, j, i + 1, j + 1))
                    {
                        MovePiece((i, j), (i + 1, j + 1));
                        EndTurn(p, i, j, i + 1, j + 1);
                        return;
                    }

                    if (i > 0 && j < 7 && _pieces[i, j].IsKing &&
                        IsValidMove(_pieces[i, j], i, j, i - 1, j + 1))
                    {
                        MovePiece((i, j), (i - 1, j + 1));
                        EndTurn(p, i, j, i - 1, j + 1);
                        return;
                    }

                    if (i > 0 && j > 0 &&
                        IsValidMove(_pieces[i, j], i, j, i - 1, j - 1))
                    {
                        MovePiece((i, j), (i - 1, j - 1));
                        EndTurn(p, i, j, i - 1, j - 1);
                        return;
                    }

                    if (i < 7 &&
                        IsValidMove(_pieces[i, j], i, j, i + 1, j - 1))
                    {
                        MovePiece((i, j), (i + 1, j - 1));
                        EndTurn(p, i, j, i + 1, j - 1);
                        return;
                    }
                }
            }
        }
    }

    public void MovePiece((int x, int y) from, (int x, int y) to)
    {
        var temp = _pieces[from.x, from.y];
        _pieces[to.x, to.y] = temp;
        _pieces[from.x, from.y] = null;
    }

    public bool CanBeMoved(int x, int y)
    {
        // Out of bound
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
        {
            return false;
        }

        var p = GetPiece(x, y);
        var forcedMoves = ScanForForcedMoves();
        return p != null &&
               p.IsWhite == IsWhiteTurn &&
               (!Client || Client.isHost == IsWhiteTurn) &&
               (forcedMoves.Count == 0 || forcedMoves.Find(fp => fp == p) != null);
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
        int deltaMoveX = Mathf.Abs(x1 - x2);
        // Нам понадобится -1 когда мы находимся в черной команде
        int deltaMoveY = y2 - y1;

        // For white team
        if (isWhite || isKing)
        {
            // For normal jump
            if (deltaMoveX == 1 && deltaMoveY == 1)
            {
                return true;
            }
            // For kill jump
            // kill piece

            if (deltaMoveX == 2 && deltaMoveY == 2)
            {
                // Для получения средней шашки между двумя
                var p = _pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                if (p != null && p.IsWhite != isWhite)
                    return true;
            }
        }

        // For black team
        if (!isWhite || isKing)
        {
            if (deltaMoveX == 1 && deltaMoveY == -1)
            {
                return true;
            }

            // kill piece
            if (deltaMoveX == 2 && deltaMoveY == -2)
            {
                // Для получения средней шашки между двумя
                var p = _pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                if (p != null && p.IsWhite != isWhite)
                    return true;
            }
        }

        return false;
    }
}