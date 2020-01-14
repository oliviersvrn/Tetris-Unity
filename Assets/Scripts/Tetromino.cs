using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Tetromino
{
    public Name id { get; }
    public Color color { get; }
    public int[][] blocks { get; }

    public Vector2 position { get; set; }
    public Rotation rotation { get; set; }

    public Tetromino(Name id, Rotation rotation = Rotation.UP, Vector2 position = new Vector2())
    {
        this.id = id;

        this.rotation = rotation;
        this.position = position;

        this.color = colors[(int)id];
        this.blocks = GetBlockData(id, rotation);
    }

    public void Move(int x, int y)
    {
        position = new Vector2(position.x + x, position.y + y);
    }

    public void Move(Vector2 vector)
    {
        position += vector;
    }

    public void Rotate(int dx)
    {
        int[][] tmp = new int[][] {new int[]{0,0,0,0,0},new int[]{0,0,0,0,0},new int[]{0,0,0,0,0},new int[]{0,0,0,0,0},new int[]{0,0,0,0,0}};

        dx = dx >= 1 ? 1 : -1;

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (dx > 0)
                    tmp[i][j] = blocks[4 - j][i];
                else
                    tmp[i][j] = blocks[j][4 - i];
            }
        }
        
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
                blocks[i][j] = tmp[i][j];

        rotation = (Rotation)(((int)rotation + dx + 4) % 4);
    }

    public int[][] GetBlockData(Name id, Rotation rotation)
    {
        int[][] data = new int[][] {new int[]{0,0,0,0,0},new int[]{0,0,0,0,0},new int[]{0,0,0,0,0},new int[]{0,0,0,0,0},new int[]{0,0,0,0,0}};

        if (rotation == Rotation.UP)
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    data[i][j] = blockData[(int)id][i][j];
        }
        if (rotation == Rotation.LEFT)
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    data[i][j] = blockData[(int)id][j][4 - i];
        }
        else if (rotation == Rotation.DOWN)
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    data[i][j] = blockData[(int)id][4 - i][4 - j];
        }
        else if (rotation == Rotation.RIGHT)
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    data[i][j] = blockData[(int)id][4 - j][i];
        }

        return data;
    }

    public enum Name
    {
        I, J, L, O, S, T, Z
    }

    public enum Rotation
    {
        UP, RIGHT, DOWN, LEFT
    }

    public static Color[] colors = {
        Color.cyan,
        Color.blue,
        new Color(1f, 0.5f, 0f),
        Color.yellow,
        Color.green,
        new Color(0.5f, 0f, 1f),
        Color.red
    };

    static int[][][] blockData = new int[][][] {
        new int[][] {
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 1, 1, 1, 1},
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0}
        },

        new int[][] {
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 1, 0, 0, 0},
            new int[] {0, 1, 1, 1, 0},
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0}
        },

        new int[][] {
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 1, 0},
            new int[] {0, 1, 1, 1, 0},
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0}
        },

        new int[][] {
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 1, 1, 0},
            new int[] {0, 0, 1, 1, 0},
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0}
        },

        new int[][] {
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 1, 1, 0},
            new int[] {0, 1, 1, 0, 0},
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0}
        },

        new int[][] {
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 1, 0, 0},
            new int[] {0, 1, 1, 1, 0},
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0}
        },

        new int[][] {
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 1, 1, 0, 0},
            new int[] {0, 0, 1, 1, 0},
            new int[] {0, 0, 0, 0, 0},
            new int[] {0, 0, 0, 0, 0}
        }
    };
}
