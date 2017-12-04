using Godot;
using System;

public class Splash : Node2D
{
    // Member variables here, example:
    // private int a = 2;
    // private string b = "textvar";

    public override void _Ready()
    {
        // Called every time the node is added to the scene.
        // Initialization here
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventKey) {
            GetTree().ChangeScene("res://World.tscn");
        }
    }


//    public override void _Process(float delta)
//    {
//        // Called every frame. Delta is time since last frame.
//        // Update game logic here.
//
//    }
}
