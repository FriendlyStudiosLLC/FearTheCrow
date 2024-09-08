using Godot;

namespace FearTheCrow.Scripts.Weapon;

public partial class WeaponModifier : Resource
{
    [Export] public string Name = "Enter Modifier Name";
    [Export] public string Description = "Enter Modifier Description";

    public virtual void Apply(Weapon weapon)
    {
        
    }

    public virtual void Remove(Weapon weapon)
    {
        
    }

}