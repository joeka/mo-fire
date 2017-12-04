using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

public class Settings {

    // ---------------------------------------------------
    // SINGLETON

    private static Settings _settings;
    public static Settings Instance => _settings ?? (_settings = new Settings());

    // ---------------------------------------------------

    private Settings() {}

    public string SceneAfterNextScene {get;set;}


}

public class World : Node2D
{
    public enum CellTypes {
        Empty = -1,
        Wall = 0,
        Heater = 1,
        Ground = 2,
        Wood = 3
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

    private CellInfo[,] _grid;
	
    private Dictionary<CellTypes, double> _cellTypeToHeatIncrease = new Dictionary<CellTypes, double>() {
        { CellTypes.Empty,   0.0 },
        { CellTypes.Ground,  0.8 },
        { CellTypes.Heater,  1.7 },
        { CellTypes.Wall,    0.0 },
        { CellTypes.Wood,    1.6 }
    };

    public override void _Ready() {
        _map = (TileMap)FindNode("TileMap");

        var cellPositions = _map.GetUsedCells().OfType<Vector2>()
                                .Select(t => new Point((int)t.x, (int)t.y))
                                .ToArray();

        var minX = cellPositions.Min(t => t.X);
        var maxX = cellPositions.Max(t => t.X);
        var minY = cellPositions.Min(t => t.Y);
        var maxY = cellPositions.Max(t => t.Y);

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

        foreach (var cellPos in cellPositions) {
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

    public override void _Process(float delta) {
        FlatGrid().Where(IsBurning)
                  .SelectMany(GetNeighbours)
                  .ForEach(cell => cell.Heat += _cellTypeToHeatIncrease[cell.CellType] * delta * 10);   // x*delta*y: x per 1000/y ms

        FlatGrid().Where(ShouldBurn).ForEach(SpawnFaia);

        if (FlatGrid().Where(IsNotWall).All(IsBurning)) {
            Settings.Instance.SceneAfterNextScene = "res://World.tscn";   // dumm
            GetTree().ChangeScene("res://DeadSplash.tscn");
        }

        if (!FlatGrid().Any(IsBurning)) {
            Settings.Instance.SceneAfterNextScene = "res://World.tscn";
            GetTree().ChangeScene("res://Success.tscn");
        }
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

    private IEnumerable<CellInfo> FlatGrid() {
        for (int x = 0; x < _grid.GetLength(0); x++) {
            for (int y = 0; y < _grid.GetLength(1); y++) {
                yield return _grid[x, y];
            }
        }
    }

    private IEnumerable<CellInfo> GetNeighbours(CellInfo cell) {
        yield return _grid[cell.GridCoords.X - 1, cell.GridCoords.Y];
        yield return _grid[cell.GridCoords.X + 1, cell.GridCoords.Y];
        yield return _grid[cell.GridCoords.X, cell.GridCoords.Y - 1];
        yield return _grid[cell.GridCoords.X, cell.GridCoords.Y + 1];
    }

    // PREDICATES

    private static bool IsBurning(CellInfo info) => info.HasFire;

    private static bool IsNotWall(CellInfo info) => info.CellType != CellTypes.Wall;

    private static Func<CellInfo, bool> IsCellType(CellTypes t) => (c) => c.CellType == t;

    private static bool ShouldBurn(CellInfo cell) => cell.Heat >= 100 && !cell.HasFire;

}

public static class Extensions {
    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action) {
        foreach (var i in ie) {
            action(i);
        }
    }
}