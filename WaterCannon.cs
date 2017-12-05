using Godot;
using System;
using System.Linq;

public class WaterCannon : Particles2D {
    private const int WaterPower = 100;

    private CollisionPolygon2D collisionPolygone;

    private Area2D area;
    private AudioStreamPlayer _player;
    private bool shooterizing = false;

    public override void _Ready() {
        collisionPolygone = (CollisionPolygon2D) FindNode("CollisionPolygon2D");
        area = (Area2D) FindNode("Area2D");

        _player = (AudioStreamPlayer) FindNode("AudioStreamPlayer");
    }

    public override void _Input(InputEvent inputEvent) {
        if(inputEvent.IsAction("shoot")) {
            bool isPressed = inputEvent.IsPressed();
            Emitting = isPressed;
            shooterizing = isPressed;

            if (isPressed && !_player.IsPlaying()) {
                _player.Play();
            } else if (!isPressed && _player.IsPlaying()) {
                _player.Stop();
            }
        }
    }

    public override void _PhysicsProcess(float delta) {
        if(!shooterizing) return;

        var olapAreas = area.GetOverlappingAreas().Select(t => (Area2D)t).ToArray();

        foreach (var olap in olapAreas) {
            if (olap.GetParent() is Faia) {
                var f = (Faia)olap.GetParent();
                f.Extinguish((int)(delta*WaterPower));
            }   
        }
    }
}
