using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : IEquatable<Piece>
{
    public bool IsWhite { get; }
    public bool IsKing { get; set; }

    private readonly Guid _guid;

    public Piece(bool isWhite, bool isKing = false)
    {
        IsWhite = isWhite;
        IsKing = isKing;
        _guid = Guid.NewGuid();
    }


    public bool Equals(Piece other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _guid.Equals(other._guid);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Piece) obj);
    }

    public override int GetHashCode()
    {
        return _guid.GetHashCode();
    }

    public static bool operator ==(Piece left, Piece right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Piece left, Piece right)
    {
        return !Equals(left, right);
    }

    public (int x, int y)? IsForceToMove(Piece[,] board, int x, int y)
    {
        if (IsWhite || IsKing)
        {
            // Проверяем две клетке слева от нас и справа от нас
            // Kill Top left 
            if (x >= 2 && y <= 5)
            {
                var p = board[x - 1, y + 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.IsWhite != IsWhite)
                {
                    // Check it its possible to land after the jump (можем преземлиться)
                    if (board[x - 2, y + 2] == null)
                    {
                        return (x - 2, y + 2);
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
                        return (x + 2, y + 2);
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
                        return (x - 2, y - 2);
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
                        return (x + 2, y - 2);
                    }
                }
            }
        }

        return null;
    }
}