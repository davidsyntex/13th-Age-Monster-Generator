﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace _13AMonsterGenerator
{
    internal class Monster
    {
        private static readonly List<string> MonsterSizeList = new List<string> {"Standard", "Mook", "Large", "Huge"};

        private static readonly List<string> MonsterRoleList = new List<string>
        {
            "Mook",
            "Archer",
            "Caster",
            "Leader",
            "Spoiler",
            "Troop",
            "Wrecker"
        };

        private static readonly List<string> MonsterTypeList = new List<string>
        {
            "Aberration",
            "Beast",
            "Construct",
            "Demon",
            "Dragon",
            "Giant",
            "Humanoid",
            "Ooze",
            "Plant",
            "Undead"
        };

        private Random _random;

        public Monster(PlayerTier playerTier, List<Ability> abilityList)
        {
            PlayerTier = playerTier;
            ListOfAvailableAbilities = abilityList;
            CreateNewMonsterFromLevel();
        }

        public string Name { get; set; }
        public int Level { get; set; }
        public string MonsterSize { private set; get; }
        public string MonsterRole { private set; get; }
        public string MonsterType { private set; get; }
        public int InitiativeModifier { get; set; }
        public List<Attack> ListOfAttacks { get; set; }
        public List<Ability> ListOfAbilities { get; set; }
        private List<Ability> ListOfAvailableAbilities { get; set; }
        public int ArmourClass { get; set; }
        public int HealthPoints { get; private set; }
        public int PhysicalDefense { get; set; }
        public int MentalDefense { get; set; }
        public PlayerTier PlayerTier { get; private set; }
        public double MonsterDifficultyValue { get; private set; }

        private void CreateNewMonsterFromLevel()
        {
            _random = new Random();

            Name = "Dire Monster 3000";

            PlayerTier.SetMonsterLevelAdjustment(
                PlayerTier.MonsterLevelAdjustmentRange.ElementAt(
                    _random.Next(PlayerTier.MonsterLevelAdjustmentRange.Count)));

            Level = PlayerTier.Level + PlayerTier.MonsterLevelAdjustment;
            if (Level < 0)
            {
                Level = 0;
            }
            else if (Level > 14)
            {
                Level = 14;
            }

            MonsterRole = MonsterRoleList.ElementAt(_random.Next(MonsterRoleList.Count));
            MonsterSize = MonsterSizeList.ElementAt(_random.Next(MonsterSizeList.Count));
            MonsterType = MonsterTypeList.ElementAt(_random.Next(MonsterTypeList.Count));

            if (MonsterSize.Equals("Mook") || MonsterRole.Equals("Mook"))
            {
                MonsterRole = "Mook";
                MonsterSize = "Mook";
            }
            MonsterDifficultyValue = DifficultyValue.GetDifficultyValue(PlayerTier.Tier,
                PlayerTier.MonsterLevelAdjustment, MonsterSize);

            HealthPoints = GetHealthPoinst();
            ArmourClass = Level + 16;
            if (_random.Next(2) == 0)
            {
                PhysicalDefense = Level + 14;
                MentalDefense = Level + 10;
            }
            else
            {
                MentalDefense = Level + 14;
                PhysicalDefense = Level + 10;
            }


            var listOfAttackEffects = new List<Effect>()
            {
                new Effect("12 ongoing fire damage")
            };

            var listOfAttackAbilities = new List<Ability>()
            {
                new Ability(Ability.Trigger.Natural16Plus, listOfAttackEffects)
            };

            var attack = new List<Attack>
            {
                new Attack(Level + 5, Attack.Defense.Ac, Level + 1 * 8, listOfAttackAbilities, "Burning Touch",
                    new Effect("acid damage"))
            };

            ListOfAttacks = attack;

            AddMonsterAbilities();
        }

        private void AddMonsterAbilities()
        {
            ListOfAbilities = new List<Ability>();

            var maxNumberOfAbilities = _random.Next(ListOfAvailableAbilities.Count);
            if (maxNumberOfAbilities == 0)
            {
                maxNumberOfAbilities = 1;
            }

            for (var numberOfAbilities = 0;
                 numberOfAbilities < maxNumberOfAbilities;
                 numberOfAbilities++)
            {
                var randomAbilityNumber = _random.Next(ListOfAvailableAbilities.Count);
                ListOfAbilities.Add(ListOfAvailableAbilities.ElementAt(randomAbilityNumber));
                ListOfAvailableAbilities.RemoveAt(randomAbilityNumber);
            }
        }

        private int GetHealthPoinst()
        {
            int healthPoints;

            if (MonsterSize.Equals("Mook"))
            {
                healthPoints = 5;

                if (Level <= 3)
                {
                    for (int i = 0; i < Level; i++)
                    {
                        healthPoints += 2;
                    }
                }
                else if (Level <= 6 && Level >= 4)
                {
                    switch (Level)
                    {
                        case 4:
                            healthPoints = 14;
                            break;
                        case 5:
                            healthPoints = 18;
                            break;
                        case 6:
                            healthPoints = 23;
                            break;
                    }
                }
                else if (Level >= 7)
                {
                    var maxLevelForMook = Level - 6;
                    healthPoints = GenerateHealthPointsFromLevel(1, maxLevelForMook);
                }
            }
            else
            {
                healthPoints = GenerateHealthPointsFromLevel(0, Level);
            }

            if (MonsterSize.Equals("Large"))
            {
                healthPoints *= 2;
            }
            else if (MonsterSize.Equals("Huge"))
            {
                healthPoints *= 3;
            }

            return healthPoints;
        }

        private static int GenerateHealthPointsFromLevel(int start, int maxLevel)
        {
            var healthPoints = 20;

            for (var i = start; i <= maxLevel; i++)
            {
                if (i == 1)
                {
                    healthPoints += 7;
                }
                else if (i >= 2 && i <= 4)
                {
                    healthPoints += 9;
                }
                else if (i >= 5 && i <= 7)
                {
                    healthPoints += 18;
                }
                else if (i >= 8 && i <= 10)
                {
                    healthPoints += 36;
                }
                else if (i >= 11 && i <= 13)
                {
                    healthPoints += 72;
                }
                else if (i >= 14 && i <= 14)
                {
                    healthPoints += 144;
                }
            }
            return healthPoints;
        }

        public String GetMonster()
        {
            var sb = new StringBuilder();

            sb.Append(MonsterSize.Equals("Standard") || MonsterSize.Equals("Mook") ? "" : MonsterSize + " ");
            sb.Append(Level);
            sb.Append(GetNumberSuffix(Level));
            sb.Append(" level ");

            sb.Append(MonsterRole);
            sb.AppendFormat(" [{0}]", MonsterType);
            sb.Append(" Difficulty Value: ");
            sb.Append(MonsterDifficultyValue);

            return sb.ToString();
        }

        private static string GetNumberSuffix(int number)
        {
            switch (number)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
            }
            return "th";
        }
    }
}