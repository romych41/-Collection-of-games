using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Paint : MonoBehaviour
{
    public IDictionary<Piece, PiecePaint> pieces = new Dictionary<Piece, PiecePaint>();

    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    public CanvasGroup alertCanvas;
    public float lastAlert;
    private bool _alertActive;

    private readonly Vector3 _boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private readonly Vector3 _pieceOffset = new Vector3(0.5f, 0, 0.5f);
    private Vector2 _startDrag;
    private bool _isSelected;


    private NextScene _ns;
    private CheckersBoard _checkers;


    private void Start()
    {
        _ns = FindObjectOfType<NextScene>();

        _checkers = new CheckersBoard {Client = FindObjectOfType<Client>()};


        if (_ns.mode == GameMode.MultiPlayer)
        {
            Alert(_checkers.Client.players[0].Name + " versus " + _checkers.Client.players[1].Name);
        }
        else
        {
            Alert("White player's turn");
        }

        _checkers.MovePieces += (p, x, y) => MovePiece(pieces[p], x, y);
        _checkers.OnDestr += (p) => DestroyImmediate(pieces[p].gameObject);
        _checkers.OnKing += (p) => pieces[p].transform.Rotate(Vector3.right * 180);
        //checkers.client.OnTryMove += (x1, y1, x2, y2) => TryMove(x1, y1, x2, y2);
        _checkers.OnEndTurn += (isWhiteTurn, client) =>
        {
            string str;
            if (client)
            {
                str = isWhiteTurn ? _checkers.Client.players[0].Name : _checkers.Client.players[1].Name;
            }
            else
            {
                str = isWhiteTurn ? "White" : "Black";
            }

            Alert(str + " players turn");
        };

        if (_checkers.Client != null)
        {
            _checkers.Client.OnTryMove += TryMove;
        }

        GenerateBoard();
    }

    void Update()
    {
        UpdateAlert();
        var mouseOver = UpdateMouseOver();
        if (_ns.mode == GameMode.Bot)
        {
            _checkers.AiRealise();
        }

        int x = (int) mouseOver.x;
        int y = (int) mouseOver.y;
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("SelectPiece!");
            SelectPiece(x, y);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("TryMove!");
            TryMove((int) _startDrag.x, (int) _startDrag.y, x, y);
            //Debug.Log(startDrag.x + " " + startDrag.y + " " + x + " " + y);
        }
        else if (_isSelected)
        {
            Debug.Log("UpdatePieceDrag!");
            UpdatePieceDrag(pieces[_checkers.GetPiece((int) _startDrag.x, (int) _startDrag.y)]);
        }
    }

    private void SelectPiece(int x, int y)
    {
        // Out of bounds
        if (_checkers.CanBeMoved(x, y))
        {
            _startDrag = new Vector2(x, y);
            _isSelected = true;
        }
    }

    private Vector2 UpdateMouseOver()
    {
        if (Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit,
                25.0f,
                LayerMask.GetMask("Board")))
        {
            // изначально было неправильное смещение, по этому нужно отнять boardOffset, чтобы начиналось с 0 
            return new Vector2(hit.point.x - _boardOffset.x, hit.point.z - _boardOffset.z);
        }

        return new Vector2(-1, -1);
    }

    private void UpdatePieceDrag(PiecePaint p)
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 25.0f,
            LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }

    public void TryMove(int x1, int y1, int x2, int y2)
    {
        var startDrag = new Vector2(x1, y1);
        var endDrag = new Vector2(x2, y2);
        var p = _checkers.GetPiece(x1, y1);
        // Out of bounds
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8)
        {
            //Если мы нажали вне границы то переносим шашку на стартувую позицию
            if (p != null)
            {
                MovePiece(pieces[p], x1, y1);
            }

            _isSelected = false;
            return;
        }

        if (p != null)
        {
            // If it has not moved (если шашку не перемещали)
            if (endDrag == startDrag || !_checkers.IsValidMove(p, x1, y1, x2, y2))
            {
                MovePiece(pieces[p], x1, y1);
                _isSelected = false;
                return;
            }

            // Did we kill anything
            // If this is a jump
            if (Mathf.Abs(x2 - x1) == 2)
            {
                // Для получения средней шашки между двумя
                var middle = _checkers.GetPiece((x1 + x2) / 2, (y1 + y2) / 2);
                if (middle != null)
                {
                    // Удаляем среднюю шашку между двумя
                    _checkers.SetPiece(null, (x1 + x2) / 2, (y1 + y2) / 2);
                    DestroyImmediate(pieces[middle].gameObject);
                    _checkers.HasKilled = true;
                }
            }

            // If we did not kill anything and just moved
            // when we were supposed to do a forced move
            var forcedPieces = _checkers.ScanForForcedMoves();
            if (forcedPieces.Count != 0 && _checkers.HasKilled == false)
            {
                MovePiece(pieces[p], x1, y1);
                _isSelected = false;
            }
            else
            {
                _checkers.MovePiece((x1, y1), (x2, y2));
                MovePiece(pieces[p], x2, y2);
                _isSelected = false;
                _checkers.EndTurn(p, x1, y1, x2, y2);
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
        bool isPieceWhite = (y <= 4);
        // Spawn of whitePieces or blackPiece
        var go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab);
        //go.transform.SetParent(transform);
        var pp = go.GetComponent<PiecePaint>();
        var p = new Piece(isPieceWhite);
        pieces.Add(p, pp);
        _checkers.SetPiece(p, x, y);
        MovePiece(pp, x, y);
    }


    public void MovePiece(PiecePaint p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + _boardOffset + _pieceOffset;
    }

    public void Alert(string text)
    {
        alertCanvas.GetComponentInChildren<Text>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
        _alertActive = true;
    }

    public void UpdateAlert()
    {
        if (_alertActive)
        {
            if (Time.time - lastAlert > 1.5f)
            {
                alertCanvas.alpha = 1 - ((Time.time - lastAlert) - 1.5f);

                if (Time.time - lastAlert > 2.5f)
                {
                    _alertActive = false;
                }
            }
        }
    }
}