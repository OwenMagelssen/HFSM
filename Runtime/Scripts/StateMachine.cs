/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System.Collections.Generic;

namespace HFSM
{
	public abstract class StateMachine : State
	{
		public State CurrentState { get; private set; }
		protected State DefaultState;
		protected readonly List<Transition> GlobalTransitions = new();
		public bool IsRootStateMachine => ParentStateMachine == null;

		public StateMachine(StateMachine parentParentStateMachine) : base(parentParentStateMachine)
		{
			
		}

		public void SetDefaultState(State defaultState) => DefaultState = defaultState;

		public void AddGlobalTransition(params Transition[] transitions) => GlobalTransitions.AddRange(transitions);

		public void SetState(State state)
		{
			if (state == CurrentState) return;
			var formerState = CurrentState;
			formerState?.OnExit(state);
			CurrentState = state;
			CurrentState?.OnEnter(formerState);
		}

		public override void OnEnter(State previousState)
		{
			SetState(DefaultState);
		}

		public override void OnUpdate()
		{
			CurrentState?.OnUpdate();
		}

		public override void OnLateUpdate()
		{
			CurrentState?.OnLateUpdate();
		}

		public override void OnFixedUpdate()
		{
			if (!TryToTransition())
				CurrentState?.OnFixedUpdate();
		}
		
		public override bool TryToTransition()
		{
			if (!IsRootStateMachine)
			{
				if (ParentStateMachine.TryToTransition()) 
					return true;
			}
			
			for (int i = 0; i < GlobalTransitions.Count; i++)
			{
				var transition = GlobalTransitions[i];
				if (transition.TryTransition())
				{
					SetState(transition.ToState);
					return true;
				}
			}

			for (int i = 0; i < Transitions.Count; i++)
			{
				var transition = Transitions[i];
				if (transition.TryTransition())
				{
					SetState(transition.ToState);
					return true;
				}
			}

			return false;
		}
	}
}