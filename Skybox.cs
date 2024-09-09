using Godot;
using System;
using Environment = Godot.Environment;

[Tool]
public partial class Skybox : Node3D
{
	[Export] public Viewport Front;
	[Export] public Viewport Back;
	[Export] public Viewport Right;
	[Export] public Viewport Left;
	[Export] public Viewport Top;
	[Export] public Viewport Bottom;
	[Export] public ShaderMaterial skybox;

	[Export] public Environment env;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
			
			var sky = env.Sky.GetMaterial() as ShaderMaterial;
			sky.SetShaderParameter("front", Front.GetTexture());
			sky.SetShaderParameter("back", Back.GetTexture());
			sky.SetShaderParameter("right", Right.GetTexture());
			sky.SetShaderParameter("left", Left.GetTexture());
			sky.SetShaderParameter("top", Top.GetTexture());
			sky.SetShaderParameter("bottom", Bottom.GetTexture());
			
		
	}
}
