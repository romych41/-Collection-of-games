using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckersBoard : MonoBehaviour
{
    public static CheckersBoard Instance {set; get;}

    public Piece[,] pieces = new Piece[8,8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    public CanvasGroup alertCanvas;
    private float lastAlert;
    private bool alertActive;

    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    // Для проверки цвета шашки
    public bool isWhite;
    private bool isWhiteTurn;
    private bool hasKilled;

    private Piece selectedPiece;
    private List<Piece> forcedPieces;

    private List<Piece> forcedAIPieces;
    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private Client client;

    private NextScene ns;

    void Start()
    {
        Instance = this;
        // For client
        client = FindObjectOfType<Client>();

        if(client)
        {
            isWhite = client.isHost;
            Alert(client.players[0].name + " versus " + client.players[1].name);
        }
        else
        {
            Alert("White player's turn");
        }

        ns = FindObjectOfType<NextScene>();

        isWhiteTurn = true;
        forcedPieces = new List<Piece>();
        forcedAIPieces = new List<Piece>();
        GenerateBoard();
    }

    void Update()
    {
        if(ns.PlayWithAI)
        {
            AIRealise();
        }

        UpdateAlert();
        UpdateMouseOver();
        
        // if it is my turn
        if((isWhite)?isWhiteTurn:!isWhiteTurn)
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if(selectedPiece != null)
            {
                UpdatePieceDrag(selectedPiece);
            }

            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);
            }
            if (Input.GetMouseButtonUp(0))
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
                //Debug.Log(startDrag.x + " " + startDrag.y + " " + x + " " + y);
            }
        }
        
    }

    private void UpdateMouseOver()
    {
        // if its my turn 
        // camera check
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }  

        RaycastHit hit;
        
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            // изначально было неправильное смещение, по этому нужно отнять boardOffset, чтобы начиналось с 0
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }

    }

    private void UpdatePieceDrag(Piece p)
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }  

        RaycastHit hit;
        
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }

    private void SelectPiece(int x, int y)
    {
        // Out of bounds
        if (x < 0 || x >= 8|| y < 0 || y >= 8)
        {
            return;
        }

        Piece p = pieces[x, y];
        if (p != null && p.isWhite == isWhite)
        {
            if(forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
                //Debug.Log(selectedPiece.name + "Start: " + startDrag);
            }
            else
            {
                // Look for the piece our forced pieces List
                // Мы не смогли найти шашку, которую мы на самом деле выбираем
                if(forcedPieces.Find(fp => fp == p) == null)
                    return;
                
                // Если смогли найти
                selectedPiece = p;
                startDrag = mouseOver;
            }
        }
    }

    public void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForPossibleMove();

        // Multiplayer Support
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        //MovePiece(selectedPiece, x2, y2);

        // Out of bounds
        if (x2 < 0 || x2 >= 8|| y2 < 0 || y2 >= 8)
        {
            //Если мы нажали вне границы то переносим шашку на стартувую позицию
            if(selectedPiece != null)
            {
                MovePiece(selectedPiece, x1, y1);
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if(selectedPiece != null)
        {
            // If it has not moved (если шашку не перемещали)
            if(endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            // Check if its a valid move
            if(selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                // Did we kill anything
                // If this is a jump
                if(Mathf.Abs(x2-x1) == 2)
                {   
                    // Для получения средней шашки между двумя
                    Piece p = pieces[(x1+x2)/2, (y1+y2)/2];
                    if(p != null)
                    {
                        // Удаляем среднюю шашку между двумя
                        pieces[(x1+x2)/2, (y1+y2)/2] = null;
                        DestroyImmediate(p.gameObject);
                        hasKilled = true;
                    }
                }

                // Were we suposed to kill anything?
                if(forcedPieces.Count != 0 && !hasKilled)
                {
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }

                
                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }

        // Check if we are out of bounds
        // If there a selected Piece
        //Debug.Log("StartFrag: " + startDrag + " End Drog: " + endDrag);
    }

    private void EndTurn()
    {
        // Для двойного прижка
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        // Promotions
        // King
        if(selectedPiece != null)
        {
            // Белая шашка приземлилась на конец борда
            if(selectedPiece.isWhite && !selectedPiece.isKing && y == 7)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            // Черная шашка приземлилась на конец борда
            if(!selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
        }

        // Our message
        if(client)
        {
            string msg = "CMOV|";
            msg += startDrag.x.ToString() + "|";
            msg += startDrag.y.ToString() + "|";
            msg += endDrag.x.ToString() + "|";
            msg += endDrag.y.ToString();

            client.Send(msg);
        }

        selectedPiece = null;
        startDrag = Vector2.zero;

        // Сканировать на возможный ход (если убили)
        if(ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        hasKilled = false;
        CheckVictory();

        if(!client)
        {
            isWhite = !isWhite;
            if(isWhite)
            {
                Alert("White player's turn");
            }
            else
            {
                Alert("Black player's turn");
            }
        }
        else
        {
            if(isWhite)
            {
                Alert(client.players[0].name + "'s turn");
            }
            else
            {
                Alert(client.players[1].name + "'s turn");
            }
        }
    }

    private void CheckVictory()
    {
        var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;
        for (int i = 0; i < ps.Length; i++)
        {
            if(ps[i].isWhite)
            {
                hasWhite = true;
            }
            else
            {
                hasBlack = true;
            }
        }

        if(!hasWhite)
        {
            Victory(false);
        }
        if(!hasBlack)
        {
            Victory(true);
        }

    }

    private void Victory(bool isWhite)
    {
        if(isWhite)
            Debug.Log("White team has won");
        else
            Debug.Log("Black team has won");
    }

    // Сканирование для одной шашки(на возможность двойного прижка)
    private List<Piece> ScanForPossibleMove(Piece p, int x, int y)
    {
        forcedPieces = new List<Piece>();

        if(pieces[x, y].isForceToMove(pieces, x, y))
        {
            forcedPieces.Add(pieces[x, y]);
        }
        return forcedPieces;
    }

    private List<Piece> ScanForPossibleMove()
    {
        forcedPieces = new List<Piece>();

        // Check all the pieces
        // 8 вместо pieces.Lenght потому что удаляются 
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                if(pieces[i,j] != null && pieces[i, j].isWhite == isWhiteTurn)
                {
                    if(pieces[i,j].isForceToMove(pieces, i, j))
                    {
                        forcedPieces.Add(pieces[i, j]);
                    }
                }
            }
        }
        return forcedPieces;
    }

    private void GenerateBoard()
    {
        // Generate White Team
        for(int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);
            for(int x = 0; x < 8; x += 2)
            {
                //Generate our Piece
                GeneratePiece((oddRow) ? x : x+1, y);
            }
        }

         // Generate Black Team
        for(int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for(int x = 0; x < 8; x +=2)
            {
                //Generate our Piece
                GeneratePiece((oddRow) ? x : x+1, y);
            }
        }
    }

    private void GeneratePiece(int x, int y)
    {
        bool isPieceWhite = (y > 4) ? false : true;
        // Spawn of whitePieces or blackPiece
        GameObject go = Instantiate((isPieceWhite)?whitePiecePrefab : blackPiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);
    }

    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }


    public void Alert(string text)
    {
        alertCanvas.GetComponentInChildren<Text>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
        alertActive = true;
    }

    private void AIRealise()
    {
        if(!isWhiteTurn)
        {
            int corX = 0;
            int corY = 0;

            for(int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    if(pieces[i,j] != null && pieces[i, j].isWhite == isWhiteTurn)
                    {
                        if(pieces[i,j].isForceToMove(pieces, i, j))
                        {
                            forcedAIPieces.Add(pieces[i, j]);
                            if(forcedAIPieces.Count == 1)
                            {
                                corX = i;
                                corY = j;
                                selectedPiece = pieces[corX, corY];
                            }
                        }
                    }
                }
            }

            int xo = 0;
            int yo = 0;

                if(forcedAIPieces.Count > 0)
                {
                    forcedAIPieces[0].BlackCoord(pieces, corX, corY, out xo, out yo); 
                    forcedAIPieces.RemoveRange(0,forcedAIPieces.Count);
                    Piece p = pieces[(corX+xo)/2, (corY+yo)/2];
                    if(p != null)
                    {
                        // Удаляем среднюю шашку между двумя
                        pieces[(corX+xo)/2, (corY+yo)/2] = null;
                        DestroyImmediate(p.gameObject);
                        hasKilled = true;
                    }
                    selectedPiece = pieces[corX, corY];
                    pieces[xo, yo] = selectedPiece;
                    pieces[corX, corY] = null;
                    MovePiece(selectedPiece, xo, yo);
                    EndTurn();
                }
                else
                {
                    for(int i = 7; i >= 0; i--)
                    {
                        for(int j = 7; j >= 0; j--)
                        {
                            if(pieces[i,j] != null && pieces[i, j].isWhite == isWhiteTurn)
                            {
                                if(i > 0 && j>0 && pieces[i,j].ValidMove(pieces, i, j, i-1, j-1))
                                {
                                    selectedPiece = pieces[i, j];
                                    pieces[i-1, j-1] = selectedPiece;
                                    pieces[i, j] = null;
                                    MovePiece(selectedPiece, i-1, j-1);
                                    EndTurn();
                                    goto metka;
                                }
                                else if(i<7 && pieces[i,j].ValidMove(pieces, i, j, i+1, j-1))
                                {
                                    selectedPiece = pieces[i, j];
                                    pieces[i+1, j-1] = selectedPiece;
                                    pieces[i, j] = null;
                                    MovePiece(selectedPiece, i+1, j-1);
                                    EndTurn();
                                    goto metka;
                                }
                            }
                        }
                    }
                    metka : Debug.Log("Good!");
                }
        }
    }

    public void UpdateAlert()
    {
        if(alertActive)
        {
            if(Time.time - lastAlert > 1.5f)
            {
                alertCanvas.alpha = 1 - ((Time.time - lastAlert) - 1.5f);

                if(Time.time - lastAlert > 2.5f)
                {
                    alertActive = false;
                }
            }
        }
    }

}