using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace SpaceEngineers.Functional
{
    public static class GenericFunctions
    {
        private static IEnumerable<T> GetBlocksFromGroup<T>(IMyBlockGroup blockGroup, Predicate<T> predicate) where T : class
        {
            List<T> blocks = new List<T>();

            blockGroup.GetBlocksOfType(blocks, (T predicateParam) => predicate(predicateParam));

            if (blocks.Count == 0)
            {
                throw new Exception(String.Format(
                    "Could not find any block of type {0} matching the predicate",
                        typeof(T).Name));
            }

            return blocks;
        }

        private static T GetBlockFromGroup<T>(IMyBlockGroup blockGroup, Predicate<T> predicate) where T : class
        {
            List<T> blocks = new List<T>();

            blockGroup.GetBlocksOfType(blocks, (T predicateParam) => predicate(predicateParam));

            if (blocks.Count > 1)
            {
                throw new Exception(String.Format(
                    "Found more than one block of type {0} matching the predicate",
                        typeof(T).Name));
            }
            else if (blocks.Count == 0)
            {
                throw new Exception(String.Format(
                    "Could not find any block of type {0} matching the predicate",
                        typeof(T).Name));
            }

            return blocks[0];
        }

        private static T ParseBlockFromGroup<T>(IMyBlockGroup blockGroup, string name) where T : class, IMyCubeBlock
        {
            List<T> blocks = new List<T>();

            blockGroup.GetBlocksOfType(blocks, (T predicateParam) => predicateParam.DisplayNameText.ToLower() == name.ToLower());

            if (blocks.Count != 1)
            {
                throw new Exception(String.Format(
                    "Ambiguity when parsing a block of type {0} with name \"{1}\"",
                        typeof(T).Name,
                        name));
            }

            return blocks[0];
        }

        private static IMyBlockGroup GetBlockGroupByName(this Sandbox.ModAPI.IMyGridProgram program, string name)
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();

            program.GridTerminalSystem.GetBlockGroups(groups, (IMyBlockGroup blockGroup) => blockGroup.Name.ToLower() == name);

            if (groups.Count != 1)
            {
                throw new Exception(String.Format("Could not find group with name \"{0}\"", name));
            }

            return groups[0];
        }

        private static IEnumerable<T> GetBlocksOfType<T>(IMyBlockGroup blockGroup) where T : class, IMyCubeBlock
        {
            List<T> blocks = new List<T>();

            blockGroup.GetBlocksOfType(blocks);

            if (blocks.Count == 0)
            {
                throw new Exception(String.Format(
                    "Could not find blocks of type {0}",
                        typeof(T).Name));
            }

            return blocks;
        }
    }
}
