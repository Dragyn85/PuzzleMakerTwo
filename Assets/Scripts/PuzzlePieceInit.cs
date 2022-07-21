using PuzzleMakerTwo;
using UnityEngine;

public class PuzzlePieceInit
{
    Grid<PuzzlePieceInit> _grid;
    int _x;
    int _y;
    private Knobs _knobs;
    private Texture2D _texture;
    private int _height;
    private int _width;
    

    public PuzzlePieceInit(Grid<PuzzlePieceInit> grid,int x, int y,int height,int width)
    {
        _grid = grid;
        _x = x;
        _y = y;
        _height = height;
        _width = width;
    }

    public bool HasNeighbourTop => _knobs.Top.PuzzlePieceInit != null;
    public bool HasNeighbourLeft => _knobs.Left.PuzzlePieceInit != null;
    public bool HasNeighbourRight => _knobs.Right.PuzzlePieceInit != null;
    public bool HasNeighbourDown => _knobs.Down.PuzzlePieceInit != null;
    public float X => _x;
    public float Y => _y;


    public void SetKnobs()
    {
        SetPieceNeighbour();
        SetKnobsPositionAndGender();
    }

    private void SetPieceNeighbour()
    {
        _knobs.Left.PuzzlePieceInit = _grid.GetGridObject(_x - 1, _y);
        _knobs.Down.PuzzlePieceInit = _grid.GetGridObject(_x, _y - 1);
        _knobs.Top.PuzzlePieceInit = _grid.GetGridObject(_x, _y + 1);
        _knobs.Right.PuzzlePieceInit = _grid.GetGridObject(_x + 1, _y);
    }

    private void SetKnobsPositionAndGender()
    {
        if (_knobs.Top.PuzzlePieceInit != null)
        {
            var top = _knobs.Top.PuzzlePieceInit;
            if (top.GetKnobPos(Vector2.down) == 0)
            {
                _knobs.Top.pos = Random.Range(0.4f, 0.6f);
                _knobs.Top.male =Random.Range(1, 3) == 1;

            }
            
            else
            {
                _knobs.Top.pos = top.GetKnobPos(Vector2.down);
                _knobs.Top.male = !top._knobs.Down.male;
            }
        }

        if (_knobs.Right.PuzzlePieceInit != null)
        {
            var right = _knobs.Right.PuzzlePieceInit;
            if (right.GetKnobPos(Vector2.left) == 0)
            {
                _knobs.Right.pos = Random.Range(0.4f, 0.6f);
                _knobs.Right.male =Random.Range(1, 3) ==1;

            }
            else
            {
                _knobs.Right.pos = right.GetKnobPos(Vector2.left);
                _knobs.Right.male = !right._knobs.Left.male;
            }
        }

        if (_knobs.Down.PuzzlePieceInit != null)
        {
            var down = _knobs.Down.PuzzlePieceInit;
            if (down.GetKnobPos(Vector2.up) == 0)
            {
                _knobs.Down.pos = Random.Range(0.4f, 0.6f);
                _knobs.Down.male =Random.Range(1, 3) ==1;
            }
            else
            {
                _knobs.Down.pos = down.GetKnobPos(Vector2.up);
                _knobs.Down.male = !down._knobs.Top.male;
            }
            
        }

        if (_knobs.Left.PuzzlePieceInit != null)
        {
            var left = _knobs.Left.PuzzlePieceInit;
            if (left.GetKnobPos(Vector2.right) == 0)
            {
                _knobs.Left.pos = Random.Range(0.4f, 0.6f);
                _knobs.Left.male =Random.Range(1, 3) ==1;
            }
            else
            {
                _knobs.Left.pos = left.GetKnobPos(Vector2.right);
                _knobs.Left.male = !left._knobs.Right.male;
                
            }
        }
    }

    private float GetKnobPos(Vector2 knobDirection)
    {
        if (knobDirection == Vector2.down)
            return _knobs.Down.pos;
        else if (knobDirection == Vector2.left)
            return _knobs.Left.pos;
        else if (knobDirection == Vector2.right)
            return _knobs.Right.pos;
        else
        {
            return _knobs.Top.pos;
        }

    }

    public PuzzlePieceInit GetNeighbour(Vector2 direction)
    {
        Vector2 side = new Vector2(_x, _y) + direction;
        return _grid.GetGridObject((int)side.x, (int)side.y);
        
    }

    public bool IsKnobMale(Vector2 direction)
    {
        if (direction == Vector2.up)
            return _knobs.Top.male;
        if (direction == Vector2.down)
            return _knobs.Down.male;
        if (direction == Vector2.right)
            return _knobs.Right.male;
        if (direction == Vector2.left)
            return _knobs.Left.male;
        return true;
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }

    public Texture2D GetMask()
    {
        return _texture;
    }

    public Knobs GetKnobs()
    {
        return _knobs;
    }
}