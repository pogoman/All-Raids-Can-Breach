using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PogoAI.Patches
{
    internal class LordJob_AssaultColony
    {
        [HarmonyPatch(typeof(RimWorld.LordJob_AssaultColony), MethodType.Constructor, new Type[] { typeof(Faction), 
            typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        static class LordJob_AssaultColony_Ctor
        {
            static void Postfix(ref RimWorld.LordJob_AssaultColony __instance)
            {
                __instance.breachers = true;
                __instance.useAvoidGridSmart = true;
            }
        }
    }
}
