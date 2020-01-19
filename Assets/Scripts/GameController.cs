using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GridController _grid = null;

    private Tetromino _current = null;
    private Tetromino _shadow = null;
    private List<Tetromino>[] _bags = new List<Tetromino>[7];

    private double _fallTimer;
    private double _groundTimer;
    private double _groundTimeMax;
    private int _onGroundReset;
    private int _onGroundMaxReset;

    [SerializeField]
    private double _fallSpeed = 1;

    [SerializeField]
    private bool _showShadow = true;

    public static GameController instance = null;

    void Awake()
    {
		if (instance != null)
			Destroy(gameObject);
		else
			instance = this;
    }

    void Start()
    {
        _fallTimer = 0;
        _groundTimer = 0;
        _groundTimeMax = 0.5f;

        _onGroundReset = 0;
        _onGroundMaxReset = 3;

        AudioManager.instance.Play("Main Theme");
    }

    void Update()
    {
        if (Time.timeScale == 0.0f)
            return;

        if (_current == null)
            DrawTetromino();

        if (!TestTetrominoPosition(new Vector2(0, -1)))
        {
            _groundTimer += Time.deltaTime;
        }
        else if (_groundTimer > 0)
        {
            _onGroundReset++;
            if (_onGroundReset < _onGroundMaxReset)
                _groundTimer = 0;
        }

        _fallTimer += Time.deltaTime;

        if (_fallTimer >= _fallSpeed)
        {
            if (TestTetrominoPosition(new Vector2(0, -1)))
            {
                EraseTetromino(_current);
                _current.Move(0, -1);
                SpawnTetromino(_current);
            }
            else if (_groundTimer >= _groundTimeMax)
                PlaceTetromino(_current);

            _fallTimer = 0;
        }
    }

    public void Left()
    {
        if (!TestTetrominoPosition(new Vector2(-1, 0)))
            return;

        EraseTetromino(_current);
        _current.Move(-1, 0);
        AudioManager.instance.Play("Move");
        SpawnTetromino(_current);
    }

    public void Right()
    {
        if (!TestTetrominoPosition(new Vector2(1, 0)))
            return;

        EraseTetromino(_current);
        _current.Move(1, 0);
        AudioManager.instance.Play("Move");
        SpawnTetromino(_current);
    }

    public void Down()
    {
        if (!TestTetrominoPosition(new Vector2(0, -1)))
            return;

        EraseTetromino(_current);
        _current.Move(0, -1);
        AudioManager.instance.Play("Move");
        SpawnTetromino(_current);
    }

    public void Drop()
    {
        for (int i = -1; i >= -20; i--)
        {
            if (!TestTetrominoPosition(new Vector2(0, i)))
            {
                EraseTetromino(_current);
                _current.Move(0, i + 1);
                PlaceTetromino(_current);
                break;
            }
        }
    }

    public void RotateLeft()
    {
        EraseTetromino(_current);
        Rotate(-1);
        SpawnTetromino(_current);
    }

    public void RotateRight()
    {
        EraseTetromino(_current);
        Rotate(1);
        SpawnTetromino(_current);
    }

    public void Hold()
    {
        
    }

    List<Vector2> GetTetrominoAsBlockList(Tetromino piece)
    {
        List<Vector2> blockList = new List<Vector2>();
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (piece.blocks[y][x] == 1)
                    blockList.Add(new Vector2(x + piece.position.x, 4-y + piece.position.y));
            }
        }
        return blockList;
    }

    void SpawnTetromino(Tetromino piece)
    {
        if (_showShadow)
            RenderShadow();

        List<Vector2> blockList = GetTetrominoAsBlockList(piece);
        _grid.SetBlock(blockList.ToArray(), piece.color, true);
    }

    void EraseTetromino(Tetromino piece)
    {
        List<Vector2> blockList = GetTetrominoAsBlockList(piece);
        _grid.UnsetBlock(blockList.ToArray(), true);
    }

    void RenderShadow()
    {
        if (_current == null)
        {
            _shadow = null;
            return;
        }

        for (int i = -1; i >= -22; i--)
        {
            if (!TestTetrominoPosition(new Vector2(0, i)))
            {
                if (_shadow != null)
                    _grid.UnsetBlock(GetTetrominoAsBlockList(_shadow).ToArray(), true);

                _shadow = new Tetromino(_current.id, _current.rotation, _current.position);
                _shadow.Move(0, i + 1);

                Color color = _shadow.color;
                color.a = 0.3f;

                _grid.SetBlock(GetTetrominoAsBlockList(_shadow).ToArray(), color, true);
                break;
            }
        }
    }

    bool TestTetrominoPosition(Vector2 offset)
    {
        if (_current == null)
            return false;

        List<Vector2> blockList = GetTetrominoAsBlockList(_current);

        for (int i = 0; i < blockList.Count; i++)
            blockList[i] += offset;

        return _grid.IsBlockAvailable(blockList.ToArray());
    }

    void PlaceTetromino(Tetromino piece)
    {
        AudioManager.instance.Play("Drop");

        List<Vector2> blockList = GetTetrominoAsBlockList(piece);
        _grid.SetBlock(blockList.ToArray(), piece.color, false);

        SpawnTetromino(_current);
        _current = null;

        _fallTimer = 0;
        _groundTimer = 0;
        _onGroundReset = 0;

        var lines = _grid.CheckFullLines();

        if (lines.Length >= 4)
            AudioManager.instance.Play("Four Lines");
        else if (lines.Length >= 1)
            AudioManager.instance.Play("Line");

        foreach (var line in lines)
        {
            _grid.UnsetLine(line, true, true);
        }
    }

    void Rotate(int dx)
    {
        List<Vector2> offsets = GetRotationOffset(_current, dx);
        int offset_id = -1;

        for (int i = 0; i < offsets.Count; i++)
        {
            Tetromino copy = new Tetromino(_current.id, _current.rotation, _current.position);

            copy.Move(offsets[i]);
            copy.Rotate(dx);
            if (_grid.IsBlockAvailable(GetTetrominoAsBlockList(copy).ToArray()))
            {
                offset_id = i;
                break;
            }
        }

        if (offset_id != -1)
        {
            _current.Rotate(dx);
            _current.Move(offsets[offset_id]);
            AudioManager.instance.Play("Rotate");
        }
    }

    List<Vector2> GetRotationOffset(Tetromino piece, int dx)
    {
        List<Vector2> offset = new List<Vector2>();

        int from_r = (int)piece.rotation;
        int to_r = ((int)piece.rotation + dx + 4) % 4;

        if (piece.id == Tetromino.Name.I)
        {
            for (int i = 0; i < 5; i++)
            {
                offset.Add(new Vector2());
                offset[i] = rotation_offset_I[i, from_r];
                offset[i] -= rotation_offset_I[i, to_r];
            }
        }
        else if (piece.id == Tetromino.Name.O)
        {
            offset.Add(new Vector2());
            offset[0] = rotation_offset_O[0, from_r];
            offset[0] -= rotation_offset_O[0, to_r];
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                offset.Add(new Vector2());
                offset[i] = rotation_offset_other[i, from_r];
                offset[i] -= rotation_offset_other[i, to_r];
            }
        }

        return offset;
    }

    void DrawTetromino()
    {
        DrawBags();

        _current = _bags[0][0];
        _bags[0].RemoveAt(0);
        _bags[0].Add(_bags[1][0]);
        _bags[1].RemoveAt(0);
    }

    void DrawBags()
    {
        for (int i = 0; i < _bags.Length; i++)
        {
            if (_bags[i] == null || _bags[i].Count == 0)
            {
                _bags[i] = new List<Tetromino>();
                int[] randomArray = GetRandomizedArray(7);
                for (int j = 0; j < 7; j++)
                {
                    _bags[i].Add(new Tetromino((Tetromino.Name)randomArray[j], Tetromino.Rotation.UP, new Vector2(2, 18)));
                }
            }
        }
    }

    static int[] GetRandomizedArray(int length)
    {
        int[] arr = new int[length];
        
        for (int i = 0; i < length; i++)
            arr[i] = i;

        for (int i = arr.Length - 1; i > 0; i--)
        {
            var r = Random.Range(0, i);
            var tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }

        return arr;
    }

    static Vector2[,] rotation_offset_other = new Vector2[,] {
        {new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0)},
        {new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(-1, 0)},
        {new Vector2(0, 0), new Vector2(1, -1), new Vector2(0, 0), new Vector2(-1, -1)},
        {new Vector2(0, 0), new Vector2(0, 2), new Vector2(0, 0), new Vector2(0, 2)},
        {new Vector2(0, 0), new Vector2(1, 2), new Vector2(0, 0), new Vector2(-1, 2)}
    };

    static Vector2[,] rotation_offset_I = new Vector2[,] {
        {new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, 1)},
        {new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1)},
        {new Vector2(2, 0), new Vector2(0, 0), new Vector2(-2, 1), new Vector2(0, 1)},
        {new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1)},
        {new Vector2(2, 0), new Vector2(0, -2), new Vector2(-2, 0), new Vector2(0, 2)}
    };

    static Vector2[,] rotation_offset_O = new Vector2[,] {
        {new Vector2(0, 0), new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0)}
    };
}
