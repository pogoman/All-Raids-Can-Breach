﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using Verse.AI.Group;

namespace PogoAI.Patches
{
    internal class JobGiver_AISapper
    {
        [HarmonyPatch(typeof(RimWorld.JobGiver_AISapper), "TryGiveJob")]
        static class JobGiver_AISapper_TryGiveJob_Patch
        {
            static bool Prefix(Pawn pawn, ref Job __result, RimWorld.JobGiver_AISapper __instance)
            {
                IntVec3 intVec = pawn.mindState.duty.focus.Cell;
                if (intVec.IsValid && (float)intVec.DistanceToSquared(pawn.Position) < 100f && intVec.GetRoom(pawn.Map) == pawn.GetRoom(RegionType.Set_All) && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors, RegionType.Set_Passable))
                {
                    pawn.GetLord().Notify_ReachedDutyLocation(pawn);
                    return false;
                }
                if (!intVec.IsValid)
                {
                    if (!pawn.Map.attackTargetsCache.TargetsHostileToFaction(pawn.Faction).Any(x => !x.ThreatDisabled(pawn)))
                    {
                        return false;
                    }
                    Thing attackTarget;
                    if (!pawn.Map.listerThings.AllThings.Where(x => x.Faction == Faction.OfPlayer).TryRandomElement(out attackTarget))
                    {
                        return false;
                    }

                    intVec = attackTarget.Position;
                }
                if (!pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.PassAllDestroyableThings))
                {
                    return false;
                }

                var customTuning = new PathFinderCostTuning() { costBlockedWallExtraForNaturalWalls = 1000 };
                using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, intVec,
                    TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings, false, true, false), PathEndMode.OnCell, customTuning))
                {
                    IntVec3 cellBeforeBlocker;
                    Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                    //Log.Message($"tuned: pawn {pawn} job {pawn.CurJob} start {pawnPath.FirstNode} finish {pawnPath.LastNode} cost {pawnPath.TotalCost} thing {thing?.def}");
                    if (thing != null)
                    {
                        Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, __instance.canMineMineables, __instance.canMineNonMineables);
                        if (job != null)
                        {
                            __result = job;
                            return false;
                        }
                    }
                    else
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.Goto, cellBeforeBlocker, 500, true);
                        __result = job;
                        return false;
                    }
                }
                return false;
            }
        }
    }
}