using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient
{
    public static class ListenChannel
    {
        public const int DebugChannel = 0x7FFFFFFF;
        public const int PublicChannel = 0;
    }

    [Flags]
    public enum AgentListScope
    {
        Parcel = 1,
        ParcelOwner = 2,
        Region = 4
    }

    [Flags]
    public enum AgentInfo
    {
        AlwaysRun = 0x1000,
        HasAttachments = 0x0002,
        InAutopilot = 0x2000,
        IsAway = 0x0040,
        IsBusy = 0x0800,
        IsCrouching = 0x0400,
        IsFlying = 0x0001,
        IsInAir = 0x0100,
        InMouselook = 0x0008,
        OnObject = 0x0020,
        HasScriptedAttachments = 0x0004,
        IsSitting = 0x0010,
        IsTyping = 0x0200,
        IsWalking = 0x0080
    }

    [Flags]
    public enum AgentData
    {
        IsOnline = 1,
        LegacyName = 2,
        CreationDate = 3,
        PaymentInfo = 8
    }
}
