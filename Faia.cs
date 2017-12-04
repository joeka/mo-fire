using Godot;
using System;

public class Faia : Particles2D
{
    private const int MinParticleAmount = -10;

    private World _world;
    private Light2D _light;

    private bool _flacker;
    private float _flackerTimeCount;

    public int CellIndex{get;set;}
    public World.Point CellPos {get;set;}

    public override void _Ready(){
        _world = (World)GetNode("/root/World");
        _light = (Light2D)GetNode("Light2D");

        var mat = (ParticlesMaterial)ProcessMaterial;

        var gradient = new Gradient();
        gradient.AddPoint(2048*0.0f, new Color(255,   0, 0));
        gradient.AddPoint(2048*0.2f, new Color(255,  90, 0));
        gradient.AddPoint(2048*0.4f, new Color(255, 109, 0));
        gradient.AddPoint(2048*0.6f, new Color(255, 154, 0));
        gradient.AddPoint(2048*0.8f, new Color(255, 206, 0));
        gradient.AddPoint(2048*1.0f, new Color(255, 226, 6));

        var gradientTex = new GradientTexture();
        gradientTex.SetGradient(gradient);
        gradientTex.SetWidth(2048);

        var scale = new Curve();
        scale.AddPoint(new Vector2(0.0f, 1.0f));
        scale.AddPoint(new Vector2(1.0f, 0.5f));

        var scaleTex = new CurveTexture();
        scaleTex.SetCurve(scale);

        var matCopy = new ParticlesMaterial() {
            EmissionShape         = ParticlesMaterial.EMISSION_SHAPE_SPHERE,
            EmissionSphereRadius  = 10,
            FlagDisableZ          = true,
            Spread                = 180,
            Gravity               = new Vector3(0, -98, 0),
            InitialVelocity       = 2,
            InitialVelocityRandom = 1,
            AngularVelocity       = 10,
            RadialAccel           = -15,
            Scale                 = 3,
            ColorRamp             = gradientTex,
            ScaleCurve            = scaleTex
        };

        this.ProcessMaterial = matCopy;
    }

    public override void _Process(float delta) {
        _flackerTimeCount += delta;

        if (_flacker) {
            if (_flackerTimeCount > 1) {
                _flacker = false;
                _flackerTimeCount = 0;
            }

            _light.Energy = 0.5f + 0.3f*(float)Math.Sin(2 * Math.PI * 10 * _flackerTimeCount);
        } else {
            _light.Energy = 0.5f + 0.1f*(float)Math.Sin(2 * Math.PI * 1 * _flackerTimeCount);
        }
    }

    public void Extinguish(int delta) {
        var mat = (ParticlesMaterial)ProcessMaterial;    
        var newAmount = mat.Gravity.y + delta;
        if (newAmount >= MinParticleAmount) {
            GetParent().RemoveChild(this);
            _world.RemoveFire(this);
            _flacker = false;
        } else {
            mat.Gravity = new Vector3(0, newAmount, 0);
            if (!_flacker) {
                _flacker = true;
                _flackerTimeCount = 0;
            }
        }
   }

}