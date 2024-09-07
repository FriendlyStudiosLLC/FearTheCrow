using Godot;
using System;
using Project.Scripts.Weapon;

[GlobalClass]
public partial class Weapon : Node3D
{
	[Export] WeaponTrigger[] Triggers;
	[Export] WeaponFeature[] Features;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
