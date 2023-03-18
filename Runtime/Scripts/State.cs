/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System.Collections.Generic;

namespace HFSM
{
	public abstract class State
	{
		// this state's parent state machine
		protected readonly StateMachine StateMachine;
		protected readonly List<Transition> Transitions = new();
		
		public State(StateMachine parentStateMachine)
		{
			StateMachine = parentStateMachine;
		}

		public void AddTransition(Transition transition) => Transitions.Add(transition);

		public virtual bool TryToTransition()
		{
			if (StateMachine.TryToTransition())
				return true;
			
			for (int i = 0; i < Transitions.Count; i++)
			{
				var transition = Transitions[i];
				if (transition.TryTransition())
				{
					StateMachine.SetState(transition.ToState);
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