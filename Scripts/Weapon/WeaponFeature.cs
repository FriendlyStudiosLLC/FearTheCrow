using Godot;

namespace Project.Scripts.Weapon;

[GlobalClass][Tool]
public partial class WeaponFeature : Resource
{
    [Export] public WeaponFeatureType Type;
}