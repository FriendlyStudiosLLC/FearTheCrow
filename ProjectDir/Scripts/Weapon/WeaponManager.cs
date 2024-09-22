using Godot;
using FearTheCrow.Scripts.Weapon; // Assuming your Weapon script is in this namespace

namespace FearTheCrow.Scripts.Weapon
{
    public partial class WeaponManager : Node
    {
        [Export] public Weapon[] Weapons;
        [Export] public int ActiveWeapon = 0;
        [Export] public Camera3D PlayerCamera;
        [Export] public Player Player;
        
        // Signal to notify when the active weapon changes
        [Signal] public delegate void WeaponChangedEventHandler(Weapon newWeapon);

        private Weapon _currentWeapon; 

        public override void _Ready()
        {
            // Ensure at least one weapon is available
            if (Weapons.Length > 0)
            {
                EquipWeapon(ActiveWeapon);
            }
            else
            {
                GD.PrintErr("No weapons assigned to WeaponManager!");
            }
        }

        // Function to equip a weapon based on its index
        public void EquipWeapon(int weaponIndex)
        {
            if (weaponIndex >= 0 && weaponIndex < Weapons.Length)
            {
                // Hide the current weapon if there is one
                if (_currentWeapon != null)
                {
                    _currentWeapon.Visible = false;
                }

                _currentWeapon = Weapons[weaponIndex];
                _currentWeapon.Visible = true;
                ActiveWeapon = weaponIndex;

                // Optionally, emit a signal to notify other parts of the game about the weapon change
                EmitSignal(nameof(WeaponChanged), _currentWeapon); 
            }
            else
            {
                GD.PrintErr("Invalid weapon index!");
            }
        }

        // Function to get the currently equipped weapon
        public Weapon GetCurrentWeapon()
        {
            return _currentWeapon;
        }

    }
}