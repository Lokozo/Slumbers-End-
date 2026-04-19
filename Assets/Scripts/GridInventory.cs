using UnityEngine;

public class GridInventory : MonoBehaviour
{
    public int width = 6;
    public int height = 4;

    private Item[,] grid;

    private void Awake()
    {
        grid = new Item[width, height];
    }

    public bool CanPlaceItem(Item item, int x, int y)
    {
        for (int i = 0; i < item.width; i++)
        {
            for (int j = 0; j < item.height; j++)
            {
                int checkX = x + i;
                int checkY = y + j;

                if (checkX >= width || checkY >= height)
                    return false;

                if (grid[checkX, checkY] != null)
                    return false;
            }
        }
        return true;
    }

    public bool PlaceItem(Item item, int x, int y)
    {
        if (!CanPlaceItem(item, x, y)) return false;

        for (int i = 0; i < item.width; i++)
        {
            for (int j = 0; j < item.height; j++)
            {
                grid[x + i, y + j] = item;
            }
        }

        return true;
    }
}