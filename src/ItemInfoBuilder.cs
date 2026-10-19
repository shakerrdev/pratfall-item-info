using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace ItemInfo;

internal sealed class ItemInfo
{
    public string Name;
    public string Description;
    public readonly List<string> Stats = new();
}

internal sealed class ItemInfoBuilder
{
    private readonly Config _config;
    private readonly Dictionary<ulong, double> _fuseStarted = new();

    public ItemInfoBuilder(Config config) => _config = config;

    public ItemInfo Build(InteractableComponent interactable)
    {
        if (!interactable.IsValid() || interactable.Entity is not Node3D root || !root.IsValid())
            return null;

        var entity = interactable.Entity;
        var scene = Path.GetFileNameWithoutExtension(root.SceneFilePath ?? "");
        var info = new ItemInfo
        {
            Name = interactable.GetInteractableName(),
            Description = ItemDescriptions.For(scene)
        };

        AddFood(entity, info);
        AddWeapons(entity, info);
        AddLights(entity, info);
        AddExplosives(entity, root, info);
        AddTerrainMakers(entity, info);
        AddGadgets(entity, info);

        if (entity.StickyComponent.IsValid() && !info.Stats.Contains(Labels.Sticky))
            info.Stats.Add(Labels.Sticky);

        if (string.IsNullOrEmpty(info.Description) && info.Stats.Count == 0)
            return null;
        return info;
    }

    private void AddFood(IEntity entity, ItemInfo info)
    {
        var food = entity.InteractableHealthPotionComponent;
        if (!food.IsValid())
            return;

        info.Stats.Add($"Heals {food.HealthValue} HP");

        var buff = entity.InteractableActivatePlayerBuffComponent;
        if (buff.IsValid() && buff.Buffs != null)
        {
            if (_config.RevealSillyBeans)
            {
                foreach (var type in buff.Buffs)
                    info.Stats.Add($"{BuffText.Describe(type)} for {buff.Duration}s");
            }
            else
            {
                info.Stats.Add($"Mystery effect for {buff.Duration}s");
            }
        }

        var freeze = entity.InteractableActivateFreezeBootComponent;
        if (freeze.IsValid())
            info.Stats.Add($"Ice boots for {freeze.Duration}s");

        var knockout = entity.InteractableSetUnconsciousComponent;
        if (knockout.IsValid())
            info.Stats.Add($"Knocks you out for {knockout.Duration:0.#}s");

        var voice = entity.InteractableFoodVoiceModifierComponent;
        if (voice.IsValid())
        {
            var effect = voice.VoiceEffectType switch
            {
                VoiceEffectType.High => "High-pitched voice",
                VoiceEffectType.Low => "Deep voice",
                _ => "Funny voice"
            };
            info.Stats.Add($"{effect} for {voice.Duration}s");
        }
    }

    private static void AddWeapons(IEntity entity, ItemInfo info)
    {
        var infinite = InfiniteAmmo();

        var gun = entity.InteractableGunComponent;
        if (gun.IsValid())
            info.Stats.Add(Uses("Shots", Reflect.NetVar(gun, "_currentAmmo", gun.Ammo), gun.Ammo, infinite));

        var nailGun = entity.InteractableHitscanGunComponent;
        if (nailGun.IsValid())
        {
            info.Stats.Add(Uses("Bursts", Reflect.NetVar(nailGun, "_currentAmmo", nailGun.Ammo), nailGun.Ammo, infinite));
            info.Stats.Add($"{nailGun.BurstAmount} nails per burst, {nailGun.PlayerDamage} damage each");
        }

        var laser = entity.InteractableLaserGunComponent;
        if (laser.IsValid())
        {
            info.Stats.Add(Uses("Shots", Reflect.NetVar(laser, "_currentAmmo", laser.Ammo), laser.Ammo, infinite));
            info.Stats.Add($"Bounces {laser.Bounces} times, explodes {laser.ExplosionDelay:0.#}s later");
        }

        var grapple = entity.InteractableGrappleHookComponent;
        if (grapple.IsValid())
            info.Stats.Add(Uses("Shots", Reflect.NetVar(grapple, "_currentAmmo", grapple.Ammo), grapple.Ammo, infinite));

        var zipline = entity.InteractableZiplineLauncherComponent;
        if (zipline.IsValid())
            info.Stats.Add(Uses("Ziplines", Reflect.NetVar(zipline, "_currentAmmo", zipline.Ammo), zipline.Ammo, infinite));

        var drillLauncher = entity.InteractableDrillLauncherComponent;
        if (drillLauncher.IsValid())
        {
            var fired = Reflect.NetVar(drillLauncher, "_isFired", false);
            info.Stats.Add(infinite ? "Drills: ∞" : fired ? "Empty (drill already fired)" : "Loaded: 1 drill");
        }

        var fireworks = entity.InteractableFireworksComponent;
        if (fireworks.IsValid())
            info.Stats.Add(Uses("Rockets", Reflect.NetVar(fireworks, "_ammo", fireworks.Amount), fireworks.Amount, false));

        var driller = entity.InteractableDrillerComponent;
        if (driller.IsValid())
        {
            var started = Reflect.Get(driller, "_hasStarted", false);
            var used = Reflect.Get(driller, "_currentDrillTime", 0f);
            info.Stats.Add(started
                ? $"Drilling: {Dur(driller.DrillDuration - used)} left"
                : $"Drills for {driller.DrillDuration:0.#}s, {driller.DrillDamage} damage on hit");
        }
    }

    private static void AddLights(IEntity entity, ItemInfo info)
    {
        var battery = entity.InteractableBatteryComponent;
        if (battery.IsValid())
        {
            var used = Reflect.NetVar(battery, "_currentDuration", 0);
            var left = Math.Max(0, battery.Duration - used);
            var line = left > 0 ? $"Battery: {Dur(left)} left" : "Battery empty";
            if (battery.Toggleable && left > 0)
                line += Reflect.NetVar(battery, "_isOn", false) ? " (on)" : " (off)";
            info.Stats.Add(line);
        }

        var flare = entity.InteractableFlareComponent;
        if (flare.IsValid())
        {
            var burned = Reflect.Get(flare, "_currentTime", 0f);
            info.Stats.Add(burned > 0.05f
                ? $"Burns out in {Dur(flare.Duration - burned)}"
                : $"Burns for {Dur(flare.Duration)} ({Dur(flare.DurationWhenStuckToPlayer)} on a player)");
        }
    }

    private void AddExplosives(IEntity entity, Node3D root, ItemInfo info)
    {
        var bomb = entity.ExplosionComponent;
        if (bomb.IsValid() && !entity.InteractableTeleporterComponent.IsValid())
        {
            var job = Reflect.Get<JobHandle>(bomb, "_detonateJob");
            var id = root.GetInstanceId();
            if (job.IsInQueue())
            {
                var now = Godot.Time.GetTicksMsec() / 1000.0;
                if (!_fuseStarted.TryGetValue(id, out var start))
                    _fuseStarted[id] = start = now;
                info.Stats.Add($"Explodes in {Math.Max(0, bomb.DetonationDelay - (now - start)):0.0}s!");
            }
            else
            {
                _fuseStarted.Remove(id);
                if (bomb.DetonationDelay > 0)
                    info.Stats.Add($"Fuse: {bomb.DetonationDelay:0.#}s");
            }

            if (bomb.DetonationRadius > 0)
                info.Stats.Add($"Blast radius {bomb.DetonationRadius:0.#}m");
            AddMaterial(bomb.MaterialConfig, info);
        }

        var impact = entity.InteractableExplosionComponent;
        if (impact.IsValid())
        {
            info.Stats.Add($"Blast radius {impact.Radius:0.#}m");
            AddMaterial(impact.MaterialConfig, info);
        }

        var cluster = entity.SpawnGrenadesOnExplosionComponent;
        if (cluster.IsValid())
            info.Stats.Add($"Splits into {cluster.Amount} bombs");
    }

    private static void AddTerrainMakers(IEntity entity, ItemInfo info)
    {
        var branches = entity.InverseExplosionSnakeComponent;
        if (branches.IsValid())
        {
            info.Stats.Add($"{branches.Branches} branches, {branches.BranchLength:0.#}m long");
            if (!entity.InteractableChameleonBranchComponent.IsValid())
                AddMaterial(branches.Material, info);
        }

        var bridge = entity.GenerateBridgeTrajectoryComponent;
        if (bridge.IsValid())
        {
            info.Stats.Add($"Bridge up to {bridge.MaxTravelDistance:0.#}m");
            AddMaterial(bridge.Material, info);
        }

        var clod = entity.GenerateBlobOnContactComponent;
        if (clod.IsValid() && ProceduralCaveComponent.Instance.IsValid())
        {
            var id = clod.MaterialId?.Value ?? 0;
            if (id != 0)
                AddMaterial(ProceduralCaveComponent.Instance.GetMaterialForId(id), info);
        }
    }

    private static void AddGadgets(IEntity entity, ItemInfo info)
    {
        var gumball = entity.InteractableGumballComponent;
        if (gumball.IsValid())
        {
            info.Stats.Add($"{gumball.PopChance}% chance to pop an existing bubble");
            if (Reflect.NetVar(gumball, "_wasUsed", false))
                info.Stats.Add("Already chewed");
        }

        var cloud = entity.InteractableGravityModifierComponent;
        if (cloud.IsValid())
        {
            var used = Reflect.NetVar(cloud, "_currentDuration", 0f);
            info.Stats.Add($"Float time: {Dur(cloud.Duration - used)} left");
        }

        var thumper = entity.InteractableThumperComponent;
        if (thumper.IsValid())
        {
            var used = Reflect.Get(thumper, "_duration", 0f);
            info.Stats.Add(Reflect.NetVar(thumper, "_isActive", false)
                ? $"Thumping: {Dur(thumper.Duration - used)} left"
                : $"Thumps for {Dur(thumper.Duration)}");
        }

        var extractor = entity.InteractableExtractorComponent;
        if (extractor.IsValid())
            info.Stats.Add($"Pumps out {extractor.ClodAmount} clods");

        var warp = entity.InteractableTeleporterComponent;
        if (warp.IsValid())
            info.Stats.Add($"Warps you {warp.EffectDelay:0.#}s after landing");

        var horn = entity.InteractableBatHornComponent;
        if (horn.IsValid())
            info.Stats.Add($"Range {horn.Range}m");

        var treasure = entity.TreasureComponent;
        if (treasure.IsValid())
            info.Stats.Add($"Worth {treasure.GoldValue} gold");

        var poop = entity.InteractableDogPoopComponent;
        if (poop.IsValid())
            info.Stats.Add($"{poop.ExplodeChance:0}% chance to burst on a hard hit");
    }

    private static void AddMaterial(MaterialConfig material, ItemInfo info)
    {
        if (!material.IsValid() || !material.Name.IsValid())
            return;
        string name = material.Name.GetLocalizedValue();
        if (!string.IsNullOrEmpty(name))
            info.Stats.Add($"Material: {name}");
    }

    private static bool InfiniteAmmo()
    {
        try
        {
            return CustomGameManager.GetGameSettings(CustomGameSettings.InfiniteAmmo, defaultValue: false);
        }
        catch
        {
            return false;
        }
    }

    private static string Uses(string label, int left, int max, bool infinite) =>
        infinite ? $"{label}: ∞" : $"{label}: {Math.Max(0, left)}/{max}";

    private static string Dur(float seconds)
    {
        seconds = Math.Max(0, seconds);
        return seconds >= 60 ? $"{(int)seconds / 60}:{(int)seconds % 60:00}" : $"{Mathf.CeilToInt(seconds)}s";
    }

    public void Forget(ulong instanceId) => _fuseStarted.Remove(instanceId);

    public void ForgetStale()
    {
        if (_fuseStarted.Count == 0)
            return;
        var stale = new List<ulong>();
        foreach (var id in _fuseStarted.Keys)
            if (GodotObject.InstanceFromId(id) == null)
                stale.Add(id);
        foreach (var id in stale)
            _fuseStarted.Remove(id);
    }
}

internal static class Labels
{
    public const string Sticky = "Sticks to surfaces";
}

internal static class BuffText
{
    public static string Describe(PlayerBuffType type)
    {
        var player = Player.LocalPlayer;
        return type switch
        {
            PlayerBuffType.PickaxeHighSpeed => "Super fast pickaxe",
            PlayerBuffType.PickaxeOneHit => "Pickaxe breaks blocks in one hit",
            PlayerBuffType.LowGravity => player.IsValid() && player.PlayerBuffComponent_LowGravity.IsValid()
                ? $"Low gravity (x{player.PlayerBuffComponent_LowGravity.GravityScale:0.##})"
                : "Low gravity",
            PlayerBuffType.Glow => "You glow in the dark",
            PlayerBuffType.DoubleJump => "Double jump",
            PlayerBuffType.KnockbackResistance => "Big and sturdy: resists knockback",
            PlayerBuffType.HealOverTime => player.IsValid() && player.PlayerBuffComponent_HealOverTime.IsValid()
                ? $"Regenerate {player.PlayerBuffComponent_HealOverTime.HealAmount} HP every {player.PlayerBuffComponent_HealOverTime.HealInterval:0.##}s"
                : "Regenerate health",
            PlayerBuffType.Armor => player.IsValid() && player.PlayerBuffComponent_Armor.IsValid()
                ? $"+{player.PlayerBuffComponent_Armor.ArmorAmount} armor"
                : "Armor",
            PlayerBuffType.HugeFlares => "Your flares are huge",
            PlayerBuffType.BouncyPlayer => "Bouncy: you bounce off surfaces",
            PlayerBuffType.AutoWalk => "Your legs walk on their own",
            PlayerBuffType.PoopGold => "You poop gold coins",
            PlayerBuffType.FreezePop => "Ice boots",
            PlayerBuffType.GrapeJuice => "Dizzy",
            _ => type.ToString()
        };
    }
}
