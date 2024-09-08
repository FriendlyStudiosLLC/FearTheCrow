using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace FearTheCrow.Scripts.Weapon;

public enum FiringType
{
	Single_Action,
	Pump_Action,
	Full_Auto,
	Bolt_Action,
	Burst_Action
}

[GlobalClass]
[Tool]
public partial class Weapon : Node3D
{
	[Export] public int currentAmmo = 50;
	[Export] public int MaxAmmoPerMagazine = 50;
	[Export] public int Magazines = 25;
	[Export] public bool InfiniteAmmo = false;
	[Export] public Godot.Collections.Array<WeaponModifier> Modifiers;
	[Export] public FiringType FireType;
	[Export] public PackedScene Projectile;
	[Export] public float RatePerSecond = 0.5f;
	[Export] public float BaseSprayRadius = 0.125f;
	[Export] public float BaseMaxRange = 1000;
	[Export] public PackedScene Bullethole;
	[Export] public float ImpactForce = 75.0f;
	[Export] public float PlayerRecoilFactor = 75.0f;
	[Export] public bool Multi = false;
	private bool _isFiringFullAuto = false; 

	private Task _fullAutoTask;
	private bool _coolDown;


	public override void _Ready()
	{
		foreach (var modifier in Modifiers)
		{
			modifier.Apply(this);
		}
	}

	public void AddModifier(WeaponModifier modifier)
	{
		Modifiers.Add(modifier);
		modifier.Apply(this);
	}

	public void RemoveModifier(WeaponModifier modifier)
	{
		Modifiers.Remove(modifier);
		modifier.Remove(this);
	}

	public void ProcessPrimary()
	{
		if (_coolDown) return;
		GD.Print("Processing Fire");
		switch (FireType)
		{
			case FiringType.Single_Action:
				ProcessSingle_Action();;
				break;
			case FiringType.Pump_Action:
				ProccessPump_Action();
				break;
			case FiringType.Full_Auto:
				if (_fullAutoTask == null || _fullAutoTask.IsCompleted) 
				{ 
					_isFiringFullAuto = true;
					_fullAutoTask = ProcessFull_Auto(); // Start a new full-auto task if none is active or the previous one is completed
				}
				break;
			case FiringType.Bolt_Action:
				ProcessBolt_Action();
				break;
			case FiringType.Burst_Action:
				ProcessBurst_Action();
				break;
			default:
				break;
		}
	}

	public async void ProcessSingle_Action()
	{
		{
			_coolDown = true;
			Fire();
			await Task.Delay(0);
		}
	}


	public async void ProccessPump_Action()
	{
		_coolDown = true;
		Fire();
		await Task.Delay(500); // 500ms delay 
		_coolDown = false;
	}

	public async Task ProcessFull_Auto()
	{
		while (_isFiringFullAuto && !_coolDown) 
		{
			_coolDown = true;
			Fire();

			// Introduce a small delay before resetting _coolDown
			await Task.Delay(10); // Adjust this delay as needed

			_coolDown = false; 

			await Task.Delay((int)(RatePerSecond * 1000)); 
		}
	}

	public async void ProcessBolt_Action()
	{
		_coolDown = true;
		Fire();
		await Task.Delay(800); // 800ms delay
		_coolDown = false;
	}

	public async void ProcessBurst_Action()
	{
		_coolDown = true;

		// Number of shots in a burst (typically 3)
		int burstCount = 3;

		for (int i = 0; i < burstCount; i++)
		{
			Fire();
			// Delay between shots in the burst (adjust as needed)
			await Task.Delay(100); // 100ms delay
		}

		// Delay after the burst before the weapon can fire again
		await Task.Delay((int)(RatePerSecond  * 1000));

		_coolDown = false;
	}


	private void Fire()
	{
		if (Projectile == null)
		{
			if(!Multi)
				HitscanFire();
			else
				MultiHitscanFire();
		}
		else
		{
			ProjectileFire();
		}
	}

	private Camera3D GetPlayerCamera()
	{
		return GetParent<WeaponManager>().PlayerCamera;
	
	}

	private void MultiHitscanFire(int blastCount = 12)
	{
		for (int i = 0; i < blastCount; i++)
		{
			HitscanFire();
		}
	}

	private void HitscanFire()
	{
		var cam = GetPlayerCamera(); // Consider making this more flexible

		var Center = GetViewport().GetTexture().GetSize() / 2;
		var RayOrigin = cam.ProjectRayOrigin(Center);
		var RayDirection = cam.ProjectRayNormal(Center + new Vector2((float)GD.RandRange(-BaseSprayRadius, BaseSprayRadius), (float)GD.RandRange(-BaseSprayRadius, BaseSprayRadius)));
		var RayEnd = RayOrigin + RayDirection * BaseMaxRange;

		var New_Intersection = PhysicsRayQueryParameters3D.Create(RayOrigin, RayEnd);
		var Intersection = GetWorld3D().DirectSpaceState.IntersectRay(New_Intersection);

		if (Intersection.ContainsKey("collider") && Intersection.ContainsKey("position") && Intersection.ContainsKey("normal"))
		{
			var hit = Intersection["collider"].AsGodotObject() as Node;
			if (hit != null) 
			{
				GD.Print(hit.Name);

				var decal = Bullethole.Instantiate() as Node3D;
				this.GetTree().Root.AddChild(decal);

				if (decal != null)
				{
					decal.GlobalTransform = new Transform3D(Intersection["normal"].AsBasis(), Intersection["position"].AsVector3());
				}
				float scaledImpactForce = ImpactForce * (1 + (1 / RatePerSecond)); // Adjust the scaling factor as needed
				if (hit is RigidBody3D rigidBody)
				{
					// Calculate scaled impact force
				   

					rigidBody.ApplyCentralImpulse(RayDirection * scaledImpactForce); 
				}

				Player player = GetParent<WeaponManager>().Player;
				if (player != null && !player.IsOnFloor())
				{
					player.ApplyImpulse(-RayDirection * scaledImpactForce * PlayerRecoilFactor, GlobalTransform.Origin); 
				
				}
			}
		}

		// ... apply damage to target
		// Play muzzle flash, sound effects, etc.
	}
	
	private void ProjectileFire()
	{
		var cam = GetPlayerCamera(); // Consider making this more flexible

		var Center = GetViewport().GetTexture().GetSize() / 2;
		var RayOrigin = cam.ProjectRayOrigin(Center);
		var RayDirection = cam.ProjectRayNormal(Center);

		// Instantiate a projectile 
		var projectile = Projectile.Instantiate<Projectile>();
		GetParent().AddChild(projectile);

		// Set projectile's position and rotation
		projectile.GlobalTransform = new Transform3D(Basis.LookingAt(-RayDirection, Vector3.Up), RayOrigin); 

		// Play muzzle flash, sound effects, etc.
	}

	public void ResetFire()
	{
		_coolDown = false;
		_isFiringFullAuto = false;
	}

}
