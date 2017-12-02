using Godot;
using System;
using System.Linq;

public class World : Node2D
{
    private TileMap _map;
    private float _time;
    private float _timeSinceLastSpawn;
    private Random _rnd;

    public override void _Ready()
    {
        _map = (TileMap)FindNode("TileMap");
        _rnd = new Random(DateTime.Now.Millisecond);
    }

    public override void _Process(float delta)
    {
        _time += delta;

		if (_time - _timeSinceLastSpawn > 2) {
            _timeSinceLastSpawn = _time;
            var tileCount = _map.GetChildCount();

            var cellPositions = _map.GetUsedCells()
                                    .Select(t => (Vector2)t)
                                    .ToArray();

            var cellIndex = _rnd.Next() % cellPositions.Length;
            var cellPosition = cellPositions[cellIndex];

            cellPosition = _map.MapToWorld(cellPosition) + new Vector2(0, _map.CellSize.y / 2);

            var scene = (PackedScene)ResourceLoader.Load("res://Faia.tscn");
            var fire = (Node2D)scene.Instance();
            fire.GlobalPosition = cellPosition;
            AddChild(fire);
        }
    }
}
