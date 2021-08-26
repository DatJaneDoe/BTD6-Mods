

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

namespace BossTemplate
{

    public class BossTemplate : BloonsMod
    {

        // Initial values you can adjust
        public static string bossName = "Untitled Boss"; // Sets boss name for top display

        public static Color32 color = new Color32(200, 200, 200, 255); // Color for boss name

        public static float initialBossHealth = 100;   // Initial boss health

        public static float initialHealthToLose = 5; // Health you lose when the boss leaks and loops to the front

        public static float cycleIncrease = 5; // Each time the boss cycles, you lose more and more health. This is the increase in health lost per cycle
       
        // Incase you want to scale the boss size
        public static float bossScaleX = 2; 
        public static float bossScaleY = 3;


        // These variables store details about the boss to use in boss mechanics and change constantly. 
        public static float currentBossHealth = 0;
        public static float bossPercTrack = 0;
        public static float healthToLose = initialHealthToLose;
        public static Vector2 bossPosition = new Vector2(0, 0);



       
       

        public override void OnApplicationStart()
        {


            base.OnApplicationStart();
            MelonLogger.Msg("Boss Template");


        }
       
      
        class Boss : ModDisplay
        {
            public override string BaseDisplay => "c8a44811c9166a745987fcdb5a92567b";


            public override void ModifyDisplayNode(UnityDisplayNode node)
            {
         
                Set2DTexture(node, "boss");

            }
        }
      
      
        [HarmonyPatch(typeof(TitleScreen), "Start")]
        public class AdvancedTargettingOptionsEngie 
        {
            [HarmonyPostfix]
            public static void Postfix()
            {



                foreach (BloonModel bloon in Game.instance.model.bloons)
                {
                   
                    if (bloon.name.Contains("BadFortified"))
                    {
                        // Creates a copy of a Fortified Bad
                        BloonModel boss = bloon.Clone().Cast<BloonModel>();

                        // Sets the boss display to boss.png
                        boss.display = ModContent.GetDisplayGUID<Boss>();

                        // Boss has no immunities by default. Can be changed if you'd like
                        boss.bloonProperties = BloonProperties.None;
                        boss.tags.Add("Boss");
                        foreach(DamageStateModel state in boss.damageDisplayStates)
                        {
                            // Sets all damage states to the boss
                            state.displayPath = boss.display;
                        }
                       

                        boss.maxHealth = initialBossHealth;

                        boss.isBoss = true;

                        boss.id = "Boss";
                      
                        boss.updateChildBloonModels = true;

                        // Removes children
                        

                        boss.childBloonModels = new Il2CppSystem.Collections.Generic.List<BloonModel> { };
                        SpawnChildrenModel spawnChildrenModel = boss.GetBehavior<SpawnChildrenModel>();
                        spawnChildrenModel.children = new string[] { };

                        // Registers the boss bloon
                        Game.instance.model.bloons = Game.instance.model.bloons.Take(0).Append(boss).Concat(Game.instance.model.bloons.Skip(0)).ToArray();
                    }
                  
                   
                }

                foreach (RoundSetModel round in Game.instance.model.roundSets)
                {
                    // Spawns the boss on round 3 by calling it by id
                    round.rounds[2].groups[0].bloon = "Boss";
                    round.rounds[2].groups[0].count = 1;
                 

             
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

                    __instance.text.text = bossName + " - " + currentBossHealth + "                        " + (InGame.instance.bridge.GetCurrentRound() + 1) + "/80";
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
                       
                            try
                            {
                            foreach (Bloon bloon in InGame.instance.GetBloons()) {
                               //  Bloon bloon = InGame.instance.GetBloons();

                                if (bloon.bloonModel.name.Contains("Boss"))
                                {
                                   
                                    foundBoss = true;

                                    // Allows rounds to be sent even when the boss is active. Stopped working in v27

                                    bloon.spawnRound = 140;

                                    // Records all the boss's details for boss mechanics
                                    bloon.Scale.X = bossScaleX;
                                    bloon.Scale.Y = bossScaleY;
                                    bossPercTrack = bloon.PercThroughMap();
                                    currentBossHealth = bloon.health;
                                    bossPosition = bloon.Position.ToVector2();
                                   
                                    // If the boss reaches the end, moves it to the entrance and deducts lives, or game over if you dont have enough
                                    if (bossPercTrack > 0.98f)
                                    {
                                        bloon.Move(-bloon.path.Length);

                                        if (InGame.instance.GetHealth() < healthToLose)
                                        {
                                            InGame.instance.Lose();
                                        }
                                        else
                                        {
                                            InGame.instance.SetHealth(InGame.instance.GetHealth() - healthToLose);
                                            healthToLose += cycleIncrease;
                                        }
                                    }
                                }
                            }
                            }
                            catch
                            {

                            }
                    
                        
                        if (!foundBoss)
                        {
                            // If the boss isnt active, resets the health lost per cycle and sets the health to 0 to clear the display
                            healthToLose = initialHealthToLose;
                            currentBossHealth = 0;
                        }
                       
                    }


                }

            }

        }


    }
}