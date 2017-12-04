using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class World : Node2D
{
    public enum CellTypes {
        Empty = -1,
        Wall = 0,
        Heater = 1,
        Generic = 2
    }

    private struct FireLocation {
        public Vector2 WorldPosition {get;set;}
        public Point GridPosition {get;set;}
        public int CellIndex {get;set;}

        public FireLocation(Vector2 worldPos, Point gridPos, int tileIndex) {
            this.WorldPosition = worldPos;
            this.CellIndex = tileIndex;
            this.GridPosition = gridPos;
        }
    }

    public struct Point {
        public int X {get; set;}
        public int Y {get; set;}
        public Point(int x, int y) {
            X = x;
            Y = y;
        }
        public Vector2 ToVector() => new Vector2(X, Y);
    }

    private class CellInfo {
        public double Heat;
        public bool HasFire;
        public CellTypes CellType;
        public Point LocalCoords;
        public Vector2 WorldCoords;
        public Point GridCoords;
    }

    private TileMap _map;
    private float _time;
    private float _timeSinceLastSpawn;
    private Random _rnd;
    private Point[] _cellPositions;

    private CellInfo[,] _grid;
	

    public override void _Ready() {
        _map = (TileMap)FindNode("TileMap");
        _rnd = new Random(DateTime.Now.Millisecond);

        _cellPositions = _map.GetUsedCells().OfType<Vector2>()
                             .Select(t => new Point((int)t.x, (int)t.y))
                             .ToArray();

        var minX = _cellPositions.Min(t => t.X);
        var maxX = _cellPositions.Max(t => t.X);
        var minY = _cellPositions.Min(t => t.Y);
        var maxY = _cellPositions.Max(t => t.Y);

        _grid = new CellInfo[maxX - minX + 1, maxY - minY + 1];

        for (int x = 0; x < _grid.GetLength(0); x++){
            for (int y = 0; y < _grid.GetLength(1); y++){
                _grid[x, y] = new CellInfo { 
                    CellType = CellTypes.Empty, 
                    HasFire  = false,
                    Heat     = 0
                };
            }
        }

        foreach (var cellPos in _cellPositions) {
            var p = new Point(cellPos.X - minX, cellPos.Y - minY);

            _grid[p.X, p.Y].LocalCoords = cellPos;
            _grid[p.X, p.Y].CellType    = (CellTypes)_map.GetCell(cellPos.X, cellPos.Y);
            _grid[p.X, p.Y].WorldCoords = _map.MapToWorld(cellPos.ToVector()) + new Vector2(0, _map.CellSize.y / 2);
            _grid[p.X, p.Y].GridCoords  = p;

            if (_grid[p.X, p.Y].CellType == CellTypes.Heater) {
                _grid[p.X, p.Y].Heat = 100;
            }
        }
    }

    public void RemoveFire(Faia fire) {
        _grid[fire.CellPos.X, fire.CellPos.Y].Heat = 0; // immernoch scheiße
        _grid[fire.CellPos.X, fire.CellPos.Y].HasFire = false;
    }

    private IEnumerable<CellInfo> FlatGrid() {
        for (int x = 0; x < _grid.GetLength(0); x++) {
            for (int y = 0; y < _grid.GetLength(1); y++) {
                yield return _grid[x, y];
            }
        }
    }

    public override void _Process(float delta) {
        _time += delta;

        for (int x = 1; x < _grid.GetLength(0)-1; x++) {
            for (int y = 1; y < _grid.GetLength(1)-1; y++) {
                if (_grid[x,y].HasFire) {
                    _grid[x-1,y].Heat += 1.2 * delta*10;        // increase Heat by x every 100ms
                    _grid[x+1,y].Heat += 1.2 * delta*10;
                    _grid[x,y-1].Heat += 1.2 * delta*10;
                    _grid[x,y+1].Heat += 1.2 * delta*10;
                }
            }
        }
        
        FlatGrid().Where(ShouldBurn).ForEach(SpawnFaia);
    }

	private void SpawnFaia(CellInfo p) {
        var scene           = (PackedScene)ResourceLoader.Load("res://Faia.tscn");
        var fire            = (Faia)scene.Instance();

        fire.GlobalPosition = p.WorldCoords;
        fire.CellIndex      = _map.GetCell(p.LocalCoords.X, p.LocalCoords.Y);
        fire.CellPos        = p.GridCoords;

        AddChild(fire);
        p.HasFire = true;
	}

    // PREDICATES

    private Func<CellInfo, bool> IsCellType(CellTypes t) {
        return (c) => c.CellType == t;
    }

    private bool ShouldBurn(CellInfo cell) {
        return cell.Heat >= 100 && !cell.HasFire;
    }

}

public static class Extensions {
    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action) {
        foreach (var i in ie) {
            action(i);
        }
    }
}