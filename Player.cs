using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export] private float speed = 300;
    private WaterCannon waterCannon;
    
    public override void _Ready() {
        waterCannon = (WaterCannon) FindNode("WaterCannon");
    }

    public override void _PhysicsProcess(float delta) {
       Vector2 movement = new Vector2(0, 0);

       if(Input.IsActionPressed("move_up")) {
           movement.y -= 1;
       }
       if(Input.IsActionPressed("move_down")) {
           movement.y += 1;
       }
       if(Input.IsActionPressed("move_right")) {
           movement.x += 2;
       }
       if(Input.IsActionPressed("move_left")) {
           movement.x -= 2;
       }

        if(movement.Length() > 0) {
            movement = movement.Normalized();
            waterCannon.LookAt(GetGlobalPosition() + movement);
            MoveAndCollide(movement * delta * speed);
        }
    }

   public override void _Process(float delta) {

   }
}