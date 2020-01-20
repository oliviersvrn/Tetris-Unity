/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    private GameObject[,] grid_objects = new GameObject[10,20];
    private GameObject[,] joints_v = new GameObject[10,19];
    private GameObject[,] joints_h = new GameObject[9,20];
    private GameObject[] line_effects = new GameObject[20];

    [SerializeField]
    private GameObject _cellPrefab;
    [SerializeField]
    private GameObject _jointHPrefab;
    [SerializeField]
    private GameObject _jointVPrefab;
    [SerializeField]
    private GameObject _lineEffectPrefab;

    private int[,] grid = new int[10,20];
    private int[,] ghost_grid = new int[10,20]; // Used to render shadow blocks
    private Color[,] grid_colors = new Color[10,20];

    void Awake()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                grid_objects[i,j] = Instantiate(_cellPrefab);
                grid_objects[i,j].transform.SetParent(transform);
                grid_objects[i,j].transform.position = new Vector3(i, j, 0);

                if (i < 9)
                {
                    joints_h[i,j] = Instantiate(_jointHPrefab);
                    joints_h[i,j].transform.SetParent(transform);
                    joints_h[i,j].transform.position = new Vector3(i + 0.5f, j, 0);
                }
                if (j < 19)
                {
                    joints_v[i,j] = Instantiate(_jointVPrefab);
                    joints_v[i,j].transform.SetParent(transform);
                    joints_v[i,j].transform.position = new Vector3(i, j + 0.5f, 0);
                }
            }
        }

        for (int i = 0; i < 20; i++)
        {
            line_effects[i] = Instantiate(_lineEffectPrefab);
            line_effects[i].transform.SetParent(transform);
            line_effects[i].transform.position = new Vector3(-3, i, 0);
        }
    }

    void Update()
    {
        RefreshBlocks();
    }

    public void RefreshBlocks()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (grid[j, i] >= 1)
                {
                    MeshRenderer renderer = grid_objects[j, i].GetComponent<MeshRenderer>();
                    renderer.enabled = true;
                    renderer.material.SetColor("_Color", grid_colors[j, i]);
                }
                else
                {
                    MeshRenderer renderer = grid_objects[j, i].GetComponent<MeshRenderer>();
                    renderer.enabled = false;
                }
            }
        }
    }

    public void SetBlock(Vector2[] blockList, Color color, bool ghost, bool joint = true)
    {
        int[] x_array = new int[blockList.Length];
        int[] y_array = new int[blockList.Length];

        for (int i = 0; i < blockList.Length; i++)
        {
            SetBlock(blockList[i], color, ghost);

            if (joint)
            {
                // Search which joints to set and enables their renderers
                // Need refactoring ?
                for (int j = 0; j < i; j++)
                {
                    GameObject obj = null;

                    if (blockList[i].y < 20 && blockList[i].y >= 0 && blockList[i].x < 10 && blockList[i].x >= 0) {
                        if (blockList[i].y < 19 && (int)blockList[i].x == x_array[j] && (int)blockList[i].y + 1 == y_array[j])
                            obj = joints_v[(int)blockList[i].x, (int)blockList[i].y];

                        if (blockList[i].y > 0 && (int)blockList[i].x == x_array[j] && (int)blockList[i].y - 1 == y_array[j])
                            obj = joints_v[(int)blockList[i].x, (int)blockList[i].y - 1];

                        if (blockList[i].x < 9 && (int)blockList[i].x + 1 == x_array[j] && (int)blockList[i].y== y_array[j])
                            obj = joints_h[(int)blockList[i].x, (int)blockList[i].y];

                        if (blockList[i].x > 0 && (int)blockList[i].x - 1 == x_array[j] && (int)blockList[i].y == y_array[j])
                            obj = joints_h[(int)blockList[i].x - 1, (int)blockList[i].y];

                        if (obj != null)
                        {
                            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                            renderer.enabled = true;
                            renderer.material.SetColor("_Color", color);
                        }
                    }
                }
            }
            x_array[i] = (int)blockList[i].x;
            y_array[i] = (int)blockList[i].y;
        }
    }

    public void SetBlock(Vector2[] blockList, Color[] colorList, bool ghost)
    {
        for (int i = 0; i < blockList.Length && i < colorList.Length; i++) {
            SetBlock(blockList[i], colorList[i], ghost);
        }

        if (blockList.Length != colorList.Length) {
            Debug.LogWarning("blockList length is different from colorList length.");
        }
    }

    public void SetBlock(Vector2 block, Color color, bool ghost)
    {
        if (block.x >= 0 && block.x < 10 && block.y >= 0 && block.y < 20)
        {
            grid_colors[(int)block.x, (int)block.y] = color;

            if (ghost)
                ghost_grid[(int)block.x, (int)block.y] = 1;
            else
                grid[(int)block.x, (int)block.y] = 1;
        }
    }

    public void UnsetBlock(Vector2[] blockList, bool ghost)
    {
        for (int i = 0; i < blockList.Length; i++) {
            UnsetBlock(blockList[i], ghost);
        }
    }

    public void UnsetBlock(Vector2 block, bool ghost)
    {
        if (block.x >= 0 && block.x < 10 && block.y >= 0 && block.y < 20)
        {
            if (ghost)
            {
                ghost_grid[(int)block.x, (int)block.y] = 0;
                if (grid[(int)block.x, (int)block.y] == 1)
                    return;
            }
            else
                grid[(int)block.x, (int)block.y] = 0;

            // Need refactoring ?
            if (block.y < 19)
                joints_v[(int)block.x, (int)block.y].GetComponent<MeshRenderer>().enabled = false;
            if (block.y > 0)
                joints_v[(int)block.x, (int)block.y - 1].GetComponent<MeshRenderer>().enabled = false;
            if (block.x < 9)
                joints_h[(int)block.x, (int)block.y].GetComponent<MeshRenderer>().enabled = false;
            if (block.x > 0)
                joints_h[(int)block.x - 1, (int)block.y].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void UnsetLine(int y, bool moveDown, bool effect)
    {
        if (y >= 0 && y < 20)
        {
            for (int x = 0; x < 10; x++)
            {
                UnsetBlock(new Vector2(x, y), false);
                if (effect)
                    line_effects[y].GetComponent<FadeLine>().StartFading(true, () => { MoveDown(y); }, y);
            }

            if (moveDown && !effect)
            {
                MoveDown(y);
            }
        }
    }

    public void MoveDown(int y)
    {
        for (int j = y; j < 19; j++)
        {
            for (int i = 0; i < 10; i++)
            {
                grid[i,j] = grid[i,j+1];
                grid_colors[i,j] = grid_colors[i,j+1];

                if (i < 9)
                {
                    joints_h[i,j].GetComponent<MeshRenderer>().enabled = joints_h[i,j+1].GetComponent<MeshRenderer>().enabled;
                    joints_h[i,j].GetComponent<MeshRenderer>().material.color = joints_h[i,j+1].GetComponent<MeshRenderer>().material.color;
                }

                if (j < 18)
                {
                    joints_v[i,j].GetComponent<MeshRenderer>().enabled = joints_v[i,j+1].GetComponent<MeshRenderer>().enabled;
                    joints_v[i,j].GetComponent<MeshRenderer>().material.color = joints_v[i,j+1].GetComponent<MeshRenderer>().material.color;
                }
                else
                    joints_v[i,j].GetComponent<MeshRenderer>().enabled = false;
            }
        }
        for (int i = 0; i < 10; i++)
        {
            grid[i,19] = 0;
            if (i < 9)
                joints_h[i,19].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public bool IsBlockAvailable(Vector2[] block)
    {
        for (int i = 0; i < block.Length; i++)
        {
            if (!IsBlockAvailable(block[i]))
                return false;
        }
        return true;
    }

    public bool IsBlockAvailable(Vector2 block)
    {
        if ((int)block.x < 0 || (int)block.x >= 10 || (int)block.y < 0)
            return false;

        if ((int)block.y >= 20)
            return true;

        return grid[(int)block.x, (int)block.y] == 0;
    }

    public int[] CheckFullLines()
    {
        List<int> lines = new List<int>();

        for (int y = 19; y >= 0; y--)
        {
            bool found = false;
            for (int x = 0; x < 10; x++)
            {
                if (grid[x,y] == 0) {
                    found = true;
                    break;
                }
            }
            if (!found)
                lines.Add(y);
        }

        return lines.ToArray();
    }
}
*/