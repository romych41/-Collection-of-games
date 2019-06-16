using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece
{
    public bool IsWhite { get; }
    public bool IsKing { get; set; }

    public Piece(bool isWhite, bool isKing = false)
    {
        IsWhite = isWhite;
        IsKing = isKing;
    }

    public bool IsForceToMove(Piece[,] board, int x, int y)
    {
        if (IsWhite || IsKing)
        {
            // Проверяем две клетке слева от нас и справа от нас
            // Kill Top left 
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x - 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }

            // Kill Top right 
            if (x <= 5 && y <= 5) // Тут была ошибка (x<=2)
            {
                Piece p = board[x + 1, y + 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x + 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        // Для черной команды
        if (!IsWhite || IsKing)
        {
            // Kill Bot left 
            if (x >= 2 && y >= 2)
            {
                Piece p = board[x - 1, y - 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x - 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }

            // Kill Bot right 
            if (x <= 5 && y >= 2)
            {
                Piece p = board[x + 1, y - 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x + 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void BlackCoord(Piece[,] board, int x, int y, out int xo, out int yo)
    {
        xo = 0;
        yo = 0;
        // Kill Bot left 

        if (IsKing)
        {
            // Проверяем две клетке слева от нас и справа от нас
            // Kill Top left 
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x - 2, y + 2] == null)
                    {
                        xo = x - 2;
                        yo = y + 2;
                    }
                }
            }

            // Kill Top right 
            if (x <= 5 && y <= 5) // Тут была ошибка (x<=2)
            {
                Piece p = board[x + 1, y + 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x + 2, y + 2] == null)
                    {
                        xo = x + 2;
                        yo = y + 2;
                    }
                }
            }

            if (x >= 2 && y >= 2)
            {
                Piece p = board[x - 1, y - 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x - 2, y - 2] == null)
                    {
                        xo = x - 2;
                        yo = y - 2;
                    }
                }
            }

            // Kill Bot right 
            if (x <= 5 && y >= 2)
            {
                Piece p = board[x + 1, y - 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x + 2, y - 2] == null)
                    {
                        xo = x + 2;
                        yo = y - 2;
                    }
                }
            }
        }
        else
        {
            if (x >= 2 && y >= 2)
            {
                Piece p = board[x - 1, y - 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x - 2, y - 2] == null)
                    {
                        xo = x - 2;
                        yo = y - 2;
                    }
                }
            }

            // Kill Bot right 
            if (x <= 5 && y >= 2)
            {
                var p = board[x + 1, y - 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x + 2, y - 2] == null)
                    {
                        xo = x + 2;
                        yo = y - 2;
                    }
                }
            }
        }
    }
}