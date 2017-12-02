using Godot;
using System;

public class WaterCannon : Particles2D {
    public override void _Input(InputEvent inputEvent) {
        if(inputEvent.IsAction("shoot")) {
                Emitting = inputEvent.IsPressed();
        }
    }
}
