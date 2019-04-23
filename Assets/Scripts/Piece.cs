using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isKing;


    public bool isForceToMove(Piece[,] board, int x, int y)
    {
        if (isWhite || isKing)
        {
            // Проверяем две клетке слева от нас и справа от нас
            // Kill Top left 
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.isWhite != isWhite)
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
                if (p != null && p.isWhite != isWhite)
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
        if (!isWhite || isKing)
        {
            // Kill Bot left 
            if (x >= 2 && y >= 2)
            {
                Piece p = board[x - 1, y - 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.isWhite != isWhite)
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
                if (p != null && p.isWhite != isWhite)
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

        if (isKing)
        {
            // Проверяем две клетке слева от нас и справа от нас
            // Kill Top left 
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                // If there is a piece, and it is not the same color as ours
                if (p != null && p.isWhite != isWhite)
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
                if (p != null && p.isWhite != isWhite)
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
                if (p != null && p.isWhite != isWhite)
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
                if (p != null && p.isWhite != isWhite)
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
                if (p != null && p.isWhite != isWhite)
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
                if (p != null && p.isWhite != isWhite)
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


    // Метод для правильного передвежения шашек
    public bool ValidMove(Piece[,] board, int x1, int y1, int x2, int y2)
    {
        // If you are moving on top of another piece
        if (board[x2, y2] != null)
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
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
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
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }

        return false;
    }
}