using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GridController grid = null;

    private Tetromino current = null;
    private Tetromino shadow = null;
    private List<Tetromino>[] bags = new List<Tetromino>[7];

    private double fallTimer;
    private double groundTimer;
    private double groundTimeMax;
    private int onGroundReset;
    private int onGroundMaxReset;

    [SerializeField]
    private double fallSpeed = 1;
    
    [SerializeField]
    private bool showShadow = true;

    void Start()
    {
        fallTimer = 0;
        groundTimer = 0;
        groundTimeMax = 0.5f;

        onGroundReset = 0;
        onGroundMaxReset = 3;

        AudioManager.instance.Play("Main Theme");
    }

    void Update()
    {
        // ugly ew
        if (Time.timeScale == 0.0f)
            return;

        if (current == null)
            DrawTetromino();

        EraseTetromino(current);

        if (!TestTetrominoPosition(new Vector2(0, -1)))
        {
            groundTimer += Time.deltaTime;
        }
        else if (groundTimer > 0)
        {
            onGroundReset++;
            if (onGroundReset < onGroundMaxReset)
                groundTimer = 0;
        }

        fallTimer += Time.deltaTime;

        if (fallTimer >= fallSpeed)
        {
            if (TestTetrominoPosition(new Vector2(0, -1)))
                current.Move(0, -1);
            else if (groundTimer >= groundTimeMax)
                PlaceTetromino(current);

            fallTimer = 0;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && TestTetrominoPosition(new Vector2(-1, 0)))
        {
            current.Move(-1, 0);
            AudioManager.instance.Play("Move");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && TestTetrominoPosition(new Vector2(1, 0)))
        {
            current.Move(1, 0);
            AudioManager.instance.Play("Move");
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && TestTetrominoPosition(new Vector2(0, -1)))
        {
            current.Move(0, -1);
            AudioManager.instance.Play("Move");
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            for (int i = -1; i >= -20; i--)
            {
                if (!TestTetrominoPosition(new Vector2(0, i)))
                {
                    current.Move(0, i + 1);
                    PlaceTetromino(current);
                    break;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Z))
            Rotate(-1);
        if (Input.GetKeyDown(KeyCode.D))
            Rotate(1);

        if (current != null)
            SpawnTetromino(current);
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
        if (showShadow)
            RenderShadow();

        List<Vector2> blockList = GetTetrominoAsBlockList(piece);
        grid.SetBlock(blockList.ToArray(), piece.color, true);
    }

    void EraseTetromino(Tetromino piece)
    {
        List<Vector2> blockList = GetTetrominoAsBlockList(piece);
        grid.UnsetBlock(blockList.ToArray(), true);
    }

    void RenderShadow()
    {
        if (current == null)
        {
            shadow = null;
            return;
        }

        for (int i = -1; i >= -22; i--)
        {
            if (!TestTetrominoPosition(new Vector2(0, i)))
            {
                if (shadow != null)
                    grid.UnsetBlock(GetTetrominoAsBlockList(shadow).ToArray(), true);

                shadow = new Tetromino(current.id, current.rotation, current.position);
                shadow.Move(0, i + 1);

                Color color = shadow.color;
                color.a = 0.3f;

                grid.SetBlock(GetTetrominoAsBlockList(shadow).ToArray(), color, true);
                break;
            }
        }
    }

    bool TestTetrominoPosition(Vector2 offset)
    {
        if (current == null)
            return false;

        List<Vector2> blockList = GetTetrominoAsBlockList(current);

        for (int i = 0; i < blockList.Count; i++)
            blockList[i] += offset;

        return grid.IsBlockAvailable(blockList.ToArray());
    }

    void PlaceTetromino(Tetromino piece)
    {
        AudioManager.instance.Play("Drop");

        List<Vector2> blockList = GetTetrominoAsBlockList(piece);
        grid.SetBlock(blockList.ToArray(), piece.color, false);

        SpawnTetromino(current);
        current = null;

        fallTimer = 0;
        groundTimer = 0;
        onGroundReset = 0;

        var lines = grid.CheckFullLines();

        if (lines.Length >= 4)
            AudioManager.instance.Play("Four Lines");
        else if (lines.Length >= 1)
            AudioManager.instance.Play("Line");

        foreach (var line in lines)
        {
            grid.UnsetLine(line, true, true);
        }
    }

    void Rotate(int dx)
    {
        List<Vector2> offsets = GetRotationOffset(current, dx);
        int offset_id = -1;

        for (int i = 0; i < offsets.Count; i++)
        {
            Tetromino copy = new Tetromino(current.id, current.rotation, current.position);

            copy.Move(offsets[i]);
            copy.Rotate(dx);
            if (grid.IsBlockAvailable(GetTetrominoAsBlockList(copy).ToArray()))
            {
                offset_id = i;
                break;
            }
        }

        if (offset_id != -1)
        {
            current.Rotate(dx);
            current.Move(offsets[offset_id]);
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

        current = bags[0][0];
        bags[0].RemoveAt(0);
        bags[0].Add(bags[1][0]);
        bags[1].RemoveAt(0);
    }

    void DrawBags()
    {
        for (int i = 0; i < bags.Length; i++)
        {
            if (bags[i] == null || bags[i].Count == 0)
            {
                bags[i] = new List<Tetromino>();
                int[] randomArray = GetRandomizedArray(7);
                for (int j = 0; j < 7; j++)
                {
                    bags[i].Add(new Tetromino((Tetromino.Name)randomArray[j], Tetromino.Rotation.UP, new Vector2(2, 18)));
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
