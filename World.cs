using Godot;
using System;
using System.Linq;

public class World : Node2D
{
    // Member variables here, example:
    // private int a = 2;
    // private string b = "textvar";

    private TileMap _map;
    private float _time;
    private float _timeSinceLastSpawn;
    private Random _rnd;

    public override void _Ready()
    {
        // Called every time the node is added to the scene.
        // Initialization here
        _map = (TileMap)FindNode("TileMap");
        _rnd = new Random(DateTime.Now.Millisecond);
    }

    public override void _Process(float delta)
    {
        // Called every frame. Delta is time since last frame.
        // Update game logic here.
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
            
            Console.WriteLine($"{cellPosition.x}, {cellPosition.y} ... {_map.CellSize.x}, {_map.CellSize.y}");

            var scene = (PackedScene)ResourceLoader.Load("res://Faia.tscn");
            var fire = (Node2D)scene.Instance();
            fire.GlobalPosition = cellPosition;
            AddChild(fire);
        }
    }
}
