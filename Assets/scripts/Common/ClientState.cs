using System.Collections;

namespace TapCommon
{
	public enum ClientState 
    {
		Unlogged = 1,
		Logged = 1<<1,
		Banned = 1<<2,
		SwitchGameServer = 1<<3,
        InBattle = 1<<4
	}
}
