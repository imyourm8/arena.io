using System.Collections;
using System.Collections.Generic;

namespace shared.net
{
	public struct OperationCondition 
	{
        public enum ExecutionMethod
        {
            Simultaneous,
            Queued,
            AlwaysExecute
        }
        OperationCondition(ClientState s, int m, ExecutionMethod e)
        {
			state_ = s;
			maxPendingCount_ = m;
            execution_ = e;
		}
		ClientState state_;
        public ClientState State
        {
            get { return state_; }
        }

		int maxPendingCount_;
        public int MaxPendingCount
        {
            get { return maxPendingCount_; }
        }

        ExecutionMethod execution_;
        public ExecutionMethod Execution
        {
            get { return execution_; }
        }

        public static readonly Dictionary<int, OperationCondition> conditionList = new Dictionary<int, OperationCondition>
		{
			{(int)proto_common.Commands.AUTH, new OperationCondition(ClientState.NotLogged, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.LEAVE_GAME, new OperationCondition(ClientState.Logged|ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.PING, new OperationCondition(ClientState.Logged, 1, ExecutionMethod.AlwaysExecute)},
            {(int)proto_common.Commands.JOIN_GAME, new OperationCondition(ClientState.Logged|ClientState.SwitchGameServer, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.CHANGE_NICKNAME, new OperationCondition(ClientState.Logged, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.FIND_GAME, new OperationCondition(ClientState.Logged|ClientState.SwitchGameServer, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.ADMIN_AUTH, new OperationCondition(ClientState.NotLogged, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.STAT_UPGRADE, new OperationCondition(ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.PLAYER_INPUT, new OperationCondition(ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.SYNC_TICK, new OperationCondition(ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.DAMAGE_APPLY, new OperationCondition(ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.DOWNLOAD_MAP, new OperationCondition(ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.CONNECT_TO_LOBBY, new OperationCondition(ClientState.NotLogged, 1, ExecutionMethod.Queued)},
		};
	};
}
