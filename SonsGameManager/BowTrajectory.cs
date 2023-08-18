using RedLoader;
using Sons.Items.Core;
using Sons.Weapon;
using UnityEngine;

namespace SonsGameManager;

[RegisterTypeInIl2Cpp]
public class BowTrajectory : MonoBehaviour
{
    public static bool Initialized { get; private set; }
    
    private BowWeaponController _bowWeaponController;
    private RangedWeapon _weapon;
    
    public static void Init()
    {
        if (Initialized)
            return;

        Initialized = true;
        
        // var rock = ItemDatabaseManager.ItemById(476);
        var rock = ItemDatabaseManager.ItemById(474);
        var rockWeapon = rock._heldPrefab.GetComponent<RangedWeapon>();

        var bow = ItemDatabaseManager.ItemById(443);
        var bowWeapon = bow._heldPrefab.GetComponent<RangedWeapon>();
        
        bowWeapon._impactTargetPrefab = rockWeapon._impactTargetPrefab;
        bowWeapon._trajectoryPathPrefab = rockWeapon._trajectoryPathPrefab;
        bowWeapon.ShowImpactLocation(true);
        bow._heldPrefab.gameObject.AddComponent<BowTrajectory>();
    }

    // static CustomBowData()
    // {
    //     ClassInjector.RegisterTypeInIl2Cpp<CustomBowData>();
    // }

    private void Awake()
    {
        _weapon = GetComponent<RangedWeapon>();
    }

    private void Update()
    {
        if (!_weapon._renderableIsLoaded)
            return;
        
        if(!_bowWeaponController)
            _bowWeaponController = GetComponentInChildren<BowWeaponController>(true);

        _weapon.ShowImpactLocation(_bowWeaponController._isCharging);
    }
}