using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Paint : MonoBehaviour
{

    //public Piece[,] pieces = new Piece[8, 8];
    public IDictionary<Piece, PiecePaint> pieces = new Dictionary<Piece, PiecePaint>();

    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private bool hasKilled;

    private PiecePaint selectedPiece;

    public CanvasGroup alertCanvas;
    public float lastAlert;
    private bool alertActive;

    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;


    public NextScene ns;
    public CheckersBoard checkers;


    private void Start()
    {
        ns = FindObjectOfType<NextScene>();

        checkers = new CheckersBoard();

        checkers.client = FindObjectOfType<Client>();

        if (ns.mode == 2)
        {
            Alert(checkers.client.players[0].name + " versus " + checkers.client.players[1].name);
        }
        else
        {
            Alert("White player's turn");
        }

        checkers.MovePieces += (p, x, y) => MovePiece(pieces[p], x, y);
        checkers.OnDestr += (p) => DestroyImmediate(pieces[p].gameObject);
        checkers.OnKing += (p) => pieces[p].transform.Rotate(Vector3.right * 180);
        //checkers.client.OnTryMove += (x1, y1, x2, y2) => TryMove(x1, y1, x2, y2);
        checkers.OnEndTurn += (isWhiteTurn, client) =>
        {
            var str = null as string;
            if (client)
            {
                str = isWhiteTurn ? checkers.client.players[0].name : checkers.client.players[1].name;
            }
            else
            {
                str = isWhiteTurn ? "White" : "Black";
            }
            Alert(str + " players turn");
        };

        if (checkers.client != null)
        {
            checkers.client.OnTryMove += (x1, y1, x2, y2) => TryMove(x1, y1, x2, y2);
        }

        checkers.isWhiteTurn = true;
        checkers.isWhite = true;

        GenerateBoard();
    }

    void Update()
    {   
        UpdateAlert();
        Debug.Log("UpdateMouseOver");
        UpdateMouseOver();
        Debug.Log(checkers.client == null);

        if (ns.mode == 3)
        {
            Debug.Log("Bot");
            checkers.AIRealise();
        }

        if (true)//(checkers.isWhite) ? checkers.isWhiteTurn : !checkers.isWhiteTurn)
        {
            Debug.Log("Yes!");
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;


            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("SelectPiece!");
                //selectedPiece = pieces[checkers.GetPiece(x, y)];
                SelectPiece(x, y);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("TryMove!");
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
                //Debug.Log(startDrag.x + " " + startDrag.y + " " + x + " " + y);
            }
            else if (selectedPiece != null)
            {
                Debug.Log("UpdatePieceDrag!");
                UpdatePieceDrag(selectedPiece);
            }
        }
    }

    public void SelectPiece(int x, int y)
    {
        // Out of bounds
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
        {
            return;
        }

        var p = checkers.GetPiece(x, y);

        var forcedPieces = checkers.ScanForPossibleMove();
        if (p != null && p.isWhite == checkers.isWhiteTurn && (checkers.client != null ? checkers.client.isHost == checkers.isWhite : true))
        {
            var pp = pieces[p];
            if (forcedPieces.Count == 0)
            {
                selectedPiece = pp;
                checkers.selectedPiece = p;
                startDrag = mouseOver;
                //Debug.Log(selectedPiece.name + "Start: " + startDrag);
            }
            else
            {
                // Look for the piece our forced pieces List
                // Мы не смогли найти шашку, которую мы на самом деле выбираем (которая должна бить)
                // Для продолжения хода
                if (forcedPieces.Find(fp => fp == p) == null)
                    return;

                // Если смогли найти (есть в списке)
                selectedPiece = pp;
                checkers.selectedPiece = p;
                startDrag = mouseOver;
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

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
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

    private void UpdatePieceDrag(PiecePaint p)
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }

    public void TryMove(int x1, int y1, int x2, int y2)
    {
        var forcedPieces = checkers.ScanForPossibleMove();

        // Multiplayer Support
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        var p = checkers.GetPiece(x1, y1);

        //MovePiece(selectedPiece, x2, y2);

        // Out of bounds
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8)
        {
            //Если мы нажали вне границы то переносим шашку на стартувую позицию
            if (p != null)
            {
                MovePiece(pieces[p], x1, y1);
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            checkers.selectedPiece = null;
            return;
        }

        if (p != null)
        {
            // If it has not moved (если шашку не перемещали)
            if (endDrag == startDrag)
            {
                MovePiece(pieces[p], x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                checkers.selectedPiece = null;
                return;
            }

            // Check if its a valid move
            if (checkers.ValidMove(p, x1, y1, x2, y2))
            {
                // Did we kill anything
                // If this is a jump
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    // Для получения средней шашки между двумя
                    Piece midle = checkers.GetPiece((x1 + x2) / 2, (y1 + y2) / 2);
                    if (midle != null)
                    {
                        // Удаляем среднюю шашку между двумя
                        checkers.SetPiece(null, (x1 + x2) / 2, (y1 + y2) / 2);
                        DestroyImmediate(pieces[midle].gameObject);
                        checkers.hasKilled = true; // не забыть убрать
                    }
                }

                // Were we supposed to kill anything?
                if (forcedPieces.Count != 0 && !checkers.hasKilled) // не забыть убрать
                {
                    MovePiece(pieces[p], x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    checkers.selectedPiece = null;
                    return;
                }

                checkers.SetPiece(p, x2, y2);
                checkers.SetPiece(null, x1, y1);
                MovePiece(pieces[p], x2, y2);
                selectedPiece = null;

                startDrag = Vector2.zero;

                checkers.EndTurn(x1, y1, x2, y2);
            }
            else
            {
                MovePiece(pieces[p], x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                checkers.selectedPiece = null;
                return;
            }
        }
    }

    public void GenerateBoard()
    {
        // Generate White Team
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                //Generate our Piece
                GeneratePiece((oddRow) ? x : x + 1, y);
            }
        }

        // Generate Black Team
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                //Generate our Piece
                GeneratePiece((oddRow) ? x : x + 1, y);
            }
        }
    }

    private void GeneratePiece(int x, int y)
    {
        bool isPieceWhite = (y > 4) ? false : true;
        // Spawn of whitePieces or blackPiece
        GameObject go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;
        //go.transform.SetParent(transform);
        var pp = go.GetComponent<PiecePaint>();
        var p = new Piece(isPieceWhite, false);
        pieces.Add(p, pp);
        checkers.SetPiece(p, x, y);
        MovePiece(pp, x, y);
    }


    public void MovePiece(PiecePaint p, int x, int y)
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

    public void UpdateAlert()
    {
        if (alertActive)
        {
            if (Time.time - lastAlert > 1.5f)
            {
                alertCanvas.alpha = 1 - ((Time.time - lastAlert) - 1.5f);

                if (Time.time - lastAlert > 2.5f)
                {
                    alertActive = false;
                }
            }
        }
    }


}
