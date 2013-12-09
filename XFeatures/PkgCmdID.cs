// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace XFeatures
{
    static class PkgCmdIDList
    {
        public const uint cmdidSlnLoadCommand =        0x100;
        public const uint cmdidDupSelection = 0x200;
        public const uint cmdidRssFeedViewer = 0x300;
        public const uint cmdidXFeaturesSettings = 0x400;
        public const uint cmdidAlignAssignments = 0x0500;
        public const uint cmdidFAFFileopen = 0x0501;
        public const uint cmdidMultiWordFind = 0x0502;
        public const uint cmdEmailCodeSnippet = 0x0600;
        public const uint cmdFindLastTarget = 0x0601;
        public const uint cmdFindLine = 0x0602;
        public const uint cmdInsertifdef = 0x0603;
        public const uint cmdInsertifndef = 0x0604;
        public const uint cmdInsertOneTimeInclude = 0x0605;
        public const uint cmdBreakatMain = 0x0606;
        public const uint cmdLineToTop = 0x0607;
        public const uint cmdidForceGC = 0x0700;
    };
}