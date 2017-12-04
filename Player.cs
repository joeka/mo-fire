using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export] private float speed = 300;
    private WaterCannon waterCannon;
    private Sprite _sprite;
    private Texture _goLeft;
    private Texture _goRight;
    
    public override void _Ready() {
        waterCannon = (WaterCannon) FindNode("WaterCannon");
        _sprite = (Sprite) FindNode("Sprite");

        _goLeft  = (Texture)ResourceLoader.Load("res://Assets/figure_hz.png");
        _goRight = (Texture)ResourceLoader.Load("res://Assets/figure.png");
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
           _sprite.SetTexture(_goRight);
       }
       if(Input.IsActionPressed("move_left")) {
           movement.x -= 2;
           _sprite.SetTexture(_goLeft);
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