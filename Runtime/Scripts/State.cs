/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HFSM
{
	public abstract class State
	{
		protected readonly StateMachine ParentStateMachine;
		protected readonly List<Transition> Transitions = new();
		public ReadOnlyCollection<Transition> ReadOnlyTransitions => Transitions.AsReadOnly();
		
		public State(StateMachine parentParentStateMachine)
		{
			ParentStateMachine = parentParentStateMachine;
		}

		public void AddTransitions(params Transition[] transitions) => Transitions.AddRange(transitions);

		public virtual bool TryToTransition()
		{
			if (ParentStateMachine.TryToTransition())
				return true;
			
			for (int i = 0; i < Transitions.Count; i++)
			{
				var transition = Transitions[i];
				if (transition.TryTransition())
				{
					ParentStateMachine.SetState(transition.ToState);
					return true;
				}
			}

			return false;
		}
		
		public abstract void OnEnter(State previousState);
		
		public abstract void OnExit(State nextState);
		
		public abstract void OnUpdate();
		
		public abstract void OnLateUpdate();
		
		public abstract void OnFixedUpdate();
	}
}