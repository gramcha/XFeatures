// Guids.cs
// MUST match guids.h
using System;

namespace XFeatures
{
    static class GuidList
    {
        public const string guidXFeaturesPkgString = "3ac9d6e9-a3dc-4a27-a048-f4bb7fe5889b";
        public const string guidXFeaturesCmdSetString = "08e48e14-4b8d-43c4-a22d-9116eed8fb8a";

        public static readonly Guid guidXFeaturesCmdSet = new Guid(guidXFeaturesCmdSetString);
    };
}