using System.Collections.Generic;

namespace ItemInfo;

internal static class ItemDescriptions
{
    private const string Food = "Hold Use to eat it and heal. Look at a teammate while holding it to feed them instead.";
    private const string Ball = "A toy. Throw it around, or play fetch with your dog.";
    private const string Clod = "A chunk of terrain. Throw it and it splats into a small blob of its material where it lands.";

    private static readonly Dictionary<string, string> ByScene = new()
    {
        ["interactable_food_base"] = Food,
        ["interactable_food_berry"] = Food,
        ["interactable_food_can"] = Food,
        ["interactable_food_ham"] = Food,
        ["interactable_food_milk"] = Food,
        ["interactable_food_icecream"] = Food,
        ["interactable_cheese"] = Food,
        ["interactable_health_potion"] = Food,
        ["interactable_food_lolly"] = "Food that heals, then gives you ice boots: the ground freezes where you walk.",
        ["interactable_food_wine"] = "Food that heals, then knocks you out for a moment. Big and sturdy players shrug it off.",
        ["interactable_food_squeakers"] = "Food that heals and changes your voice for a while.",
        ["interactable_food_deepdishpizza"] = "Food that heals and changes your voice for a while.",
        ["interactable_food_buff_base"] = "A Silly Bean. Eat it to heal and get a random-looking effect for a while.",

        ["interactable_gun"] = "Shoots projectiles. Hold Use to fire.",
        ["interactable_gun_flare_sticky"] = "Shoots sticky flares that cling to walls and light up the cave.",
        ["interactable_gun_flare_zero_g"] = "Shoots flares that float instead of falling.",
        ["interactable_gun_foam"] = "Shoots foam that hardens into solid terrain where it lands. Good for footholds and plugging gaps.",
        ["interactable_gun_grenade_sticky"] = "Shoots sticky bombs.",
        ["interactable_gun_grenade_zero_g"] = "Shoots floating bombs.",
        ["interactable_gun_hitscan"] = "Fires bursts of nails. Nails chip small holes into terrain and hurt players and creatures.",
        ["interactable_laser_gun"] = "Fires a beam that bounces off walls, then blows up along its path after a short delay.",
        ["interactable_grapple_hook"] = "Shoot it at a wall to latch on, then hang from and climb the rope.",
        ["interactable_zipline_launcher"] = "Shoot it to string a zipline from where you stand to where it hits. Ride it across gaps.",
        ["interactable_drill_launcher"] = "Fires a drill head that bores a tunnel in a straight line.",
        ["interactable_driller"] = "A big drill. Set it down and start it to bore through the ground, damaging anything in its way.",
        ["interactable_fireworks"] = "Light it and it launches a volley of glowing rockets that light up the cave.",

        ["interactable_flare"] = "Throw it to light up the area. Burns out after a while, and faster if it sticks to a player.",
        ["interactable_flare_sticky"] = "A flare that sticks to whatever it hits.",
        ["interactable_flare_huge"] = "A huge flare that lights up a big area.",
        ["interactable_flare_zero_g"] = "A flare that floats instead of falling.",
        ["interactable_fireworks_projectile"] = "A glowing rocket from a firework.",
        ["interactable_flashlight"] = "Press Use to switch it on or off. Only drains while it's on.",
        ["interactable_lantern"] = "A light that drains while it's on.",

        ["interactable_grenade_base"] = "Explodes a few seconds after you throw it. Catching it again stops the fuse.",
        ["interactable_grenade_bomb"] = "A bomb. Explodes a few seconds after you throw it and blasts away terrain. Catching it again stops the fuse.",
        ["interactable_grenade_bouncy"] = "A bouncy bomb. Bounces around before it explodes.",
        ["interactable_grenade_bouncy_continous"] = "A bouncy bomb that keeps exploding as it bounces.",
        ["interactable_grenade_sticky"] = "A sticky bomb. Sticks to whatever it hits, then explodes.",
        ["interactable_grenade_cluster"] = "Explodes and scatters several smaller bombs.",
        ["interactable_cluster_subgrenade"] = "A fragment of a cluster bomb. About to explode.",
        ["interactable_grenade_cloudy"] = "Explodes into fluffy cloud terrain you can stand on.",
        ["interactable_grenade_tasty"] = "Explodes into tasty terrain.",
        ["interactable_grenade_bubble"] = "Explodes into a big water bubble.",
        ["interactable_grenade_water"] = "Bursts into a splash of water that changes the terrain around it.",
        ["interactable_grenade_foam"] = "Splats foam that hardens into solid terrain wherever it bounces.",
        ["interactable_grenade_zero_g"] = "A bomb that floats instead of falling.",
        ["interactable_hot_grenade"] = "A time bomb. It starts ticking on its own and beeps faster as it gets close, so get rid of it in time.",
        ["interactable_explosives"] = "Explodes on impact when thrown. If you just drop it, it can go off on its own shortly after.",

        ["interactable_grenade_branches"] = "A bean. Throw it and it sprouts branches of terrain from where it sticks. Handy for bridges and ledges.",
        ["interactable_grenade_branch_chameleon"] = "A seed pod. Throw it and it sprouts branches made of whatever material it sticks to.",
        ["interactable_grenade_gold"] = "A bean. Throw it and it sprouts branches of gold. Dig them out for coins.",
        ["interactable_generate_bridge"] = "A bean. Throw it and a vine bridge grows along its flight path.",

        ["interactable_teleporter"] = "Throw it. A few seconds after it lands, the player who threw it is warped to it. Picking it back up cancels the warp.",
        ["interactable_extractor"] = "Throw it at terrain. It sticks, then pumps out clods of that material.",
        ["interactable_thumper"] = "Throw it onto terrain. It sticks and starts thumping, which lures out the worm. It stops on its own after a while.",
        ["interactable_cloud_bit"] = "A bit of cloud. While you hold it, gravity flips and you float upward. It shrinks as it's used up, then disappears.",
        ["interactable_horn"] = "Blast it to push away nearby players and creatures.",
        ["interactable_megaphone"] = "While you hold it, your voice carries much further.",
        ["interactable_whistle"] = "A whistle. Blow it to make some noise.",
        ["interactable_gumball"] = "Chew it to blow a bubble, and keep chewing to make the bubble bigger. Chewing again while a bubble is out may pop it.",
        ["interactable_crown"] = "Pick it up to wear the crown.",
        ["interactable_treasure"] = "Treasure. Worth a lot of gold if you get it out of the cave.",
        ["interactable_color_balloon"] = "A paint balloon. Throw it hard at a player to splash them with color.",
        ["interactable_dog_poop_bag"] = "Hit something hard with it and there's a chance it bursts. Gross.",
        ["interactable_plunger"] = "Sticks to whatever it hits when you throw it.",
        ["interactable_bounce_spikes"] = "Throw it and it sprouts a bouncy mushroom platform each time it bounces.",

        ["interactable_ball"] = Ball,
        ["interactable_basketball"] = Ball,
        ["interactable_beach_ball"] = Ball,
        ["interactable_bouncy_ball"] = Ball,
        ["interactable_football"] = Ball,
        ["interactable_american_football"] = Ball,
        ["interactable_frisbee"] = "A frisbee. It glides when you throw it.",
        ["interactable_soda_can"] = "An empty soda can. Just for throwing.",
        ["interactable_toy_car"] = "A toy car. Just for fun.",
        ["interactable_dinosaur"] = "A toy dinosaur. Just for fun.",
        ["interactable_flag"] = "A flag. Just for fun.",

        ["interactable_material_chunk"] = Clod,
    };

    public static string For(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return null;
        if (ByScene.TryGetValue(sceneName, out var text))
            return text;
        if (sceneName.StartsWith("interactable_food_buff_"))
            return ByScene["interactable_food_buff_base"];
        if (sceneName.StartsWith("interactable_material_chunk"))
            return Clod;
        return null;
    }
}
