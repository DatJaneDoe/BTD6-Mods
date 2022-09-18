

using MelonLoader;
using Assets.Scripts.Unity.UI_New.InGame;


using Harmony;


using System.Linq;

using Assets.Scripts.Unity;
using Assets.Main.Scenes;


using Assets.Scripts.Models.Bloons;


using Assets.Scripts.Models.Bloons.Behaviors;


using Bloon = Assets.Scripts.Simulation.Bloons.Bloon;

using Assets.Scripts.Models.Rounds;

using Assets.Scripts.Unity.UI_New.InGame.Stats;

using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Display;
using Assets.Scripts.Unity.Display;
using BTD_Mod_Helper.Extensions;
using UnityEngine;
using Vector2 = Assets.Scripts.Simulation.SMath.Vector2;
using BTD_Mod_Helper.Api;
using Assets.Scripts.Simulation.Bloons;
using Assets.Scripts.Models.ServerEvents;
using Assets.Scripts.Models;
using Assets.Scripts.Simulation.Bloons.Behaviors;
using Assets.Scripts.Simulation.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.SimulationBehaviors;
using Assets.Scripts.Unity.Towers.Mods;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Utils;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Weapons;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Behaviors.Emissions;
using Assets.Scripts.Models.Towers.Projectiles.Behaviors;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Simulation.Objects;
using Assets.Scripts.Simulation.Towers.Projectiles;
using Assets.Scripts.Simulation.Towers.Emissions;

namespace BossTemplate
{

    public class BossTemplate : BloonsTD6Mod
    {

        // Initial values you can adjust
        public static string bossName = "Boss Name Template"; // Sets boss name for top display

        public static Color32 color = new Color32(155, 155, 255, 255); // Color for boss name

        public static float initialBossHealth = 20000;   // Initial boss health

        public static float bossSpeed = 0.9f; // Boss speed. BAD speed by default

       public static BloonProperties bossImmunity = BloonProperties.None; // Damage immunities for the boss. None by default

        public static int bossRound = 40;

        public static float bossScaleX = 1.5f;

        public static float bossScaleY = 1.5f;

        public static float bossScaleZ = 1.5f;

        // These variables store data on the boss for performing mechanics

        public static float currentBossHealth = 0;

        public static float bossPercTrack = 0;

        public static Vector2 bossPosition = new Vector2(0,0);

        public override void OnApplicationStart()
        {


            base.OnApplicationStart();
            MelonLogger.Msg("Boss Bloon Template");


        }
       
      
      
        [HarmonyPatch(typeof(TitleScreen), "Start")]
        public class BossCreation
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                
                
                     
                
                        // Creates a copy of a Fortified Bad
                        BloonModel boss = Game.instance.model.GetBloon("Bad").Clone().Cast<BloonModel>();

                        boss.id = "UntitledBoss";
                        boss.name = "UntitledBoss";
                        boss.tags = new string[] { "Boss", "Moabs", "Bad" };


                        // Boss has no immunities by default. Can be changed if you'd like

                        boss.bloonProperties = BloonProperties.None;


                        // Change the display of the boss to a ModDisplay.


                        //  boss.display = ModContent.GetDisplayGUID<InsertCustomDisplayHere>();


                        // Increases the bosses hitbox by 50%
                        boss.radius *= 1.5f;

                
                        // Remove all damage states, unless you want to add your own.
                        boss.damageDisplayStates = new DamageStateModel[] { };

                        boss.RemoveBehaviors<DamageStateModel>();

                        boss.RemoveBehaviors<SpawnChildrenModel>();



                        
                        boss.speed = bossSpeed;

                        boss.maxHealth = initialBossHealth;

                        //  boss.isBoss = true;

                        // Registers the boss bloon
                      

                        Game.instance.model.bloons = Game.instance.model.bloons.Take(0).Append(boss).Concat(Game.instance.model.bloons.Skip(0)).ToArray();

                       


                

                foreach (RoundSetModel round in Game.instance.model.roundSets)
                {
                    // Spawns the boss on the boss round
                    round.rounds[bossRound - 1].groups[0].bloon = "UntitledBoss";
                    round.rounds[bossRound - 1].groups[0].count = 1;

                   
                }

            }

            

        }
     

        [HarmonyPatch(typeof(RoundDisplay), nameof(RoundDisplay.OnUpdate))]
        public class RoundDisplayModification
        {
            [HarmonyPostfix]

            public static void Postfix(RoundDisplay __instance)
            {
              
          

                if (currentBossHealth > 0)
                {
                
                    // Sets the round display to show the boss name, health, and the round
                    
                    __instance.text.text = bossName + " - " + (int)(currentBossHealth) + "          " + (InGame.instance.bridge.GetCurrentRound() + 1) + "/80";

                    __instance.text.color = color;
                   


                }
             
            }

        }


        public override void OnUpdate()
        {
            base.OnUpdate();

            if (InGame.instance != null)
            {
                if (InGame.instance.bridge != null)
                {
                    if (InGame.instance.bridge.GetAllBloons() != null)
                    {
                        bool foundBoss = false;
                        // Checks if the boss is active

                        foreach (BloonToSimulation bloonSimulation in InGame.instance.bridge.GetAllBloons())
                        {

                            Bloon bloon = bloonSimulation.GetBloon();

                            if (bloon.bloonModel.id.Contains("UntitledBoss"))
                            {
                 
                                foundBoss = true;


                                // Records all the boss's details for boss mechanics


                                bloon.Scale.X = bossScaleX;
                                bloon.Scale.Y = bossScaleY;
                                bloon.Scale.Z = bossScaleZ;

                                bossPercTrack = bloon.PercThroughMap();
                                currentBossHealth = bloon.health;
                                bossPosition = bloon.Position.ToVector2();


                            }
                        }



                        if (!foundBoss)
                        {
                            // If the boss isnt active, resets the health lost per cycle and sets the health to 0 to clear the display

                            currentBossHealth = 0;
                        }

                    }


                }

            }

        }



    }
}