using System.Collections;
using System.Collections.Generic;

namespace TapCommon
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
			{(int)proto_common.Commands.AUTH, new OperationCondition(ClientState.Unlogged, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.PLAYER_MOVE, new OperationCondition(ClientState.Logged|ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.TURN, new OperationCondition(ClientState.Logged|ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.ATTACK, new OperationCondition(ClientState.Logged|ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.PING, new OperationCondition(ClientState.Logged, 1, ExecutionMethod.AlwaysExecute)},
            {(int)proto_common.Commands.DAMAGE, new OperationCondition(ClientState.Logged|ClientState.InBattle, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.JOIN_GAME, new OperationCondition(ClientState.Logged|ClientState.SwitchGameServer, 1, ExecutionMethod.Queued)},
            {(int)proto_common.Commands.CHANGE_NICKNAME, new OperationCondition(ClientState.Logged, 1, ExecutionMethod.AlwaysExecute)},
            {(int)proto_common.Commands.FIND_ROOM, new OperationCondition(ClientState.Logged, 1, ExecutionMethod.AlwaysExecute)}

		};
	};
}
