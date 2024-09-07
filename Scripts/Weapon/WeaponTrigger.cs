using System.ComponentModel;
using Godot;
using Label = System.Reflection.Emit.Label;

namespace Project.Scripts.Weapon;
[Icon("res://icon.svg")]
[GlobalClass][Tool]
public partial class WeaponTrigger : Resource
{
    
    [Export] public WeaponTriggerType Type;
    [Export] public WeaponTriggerModifier Modifier;
}