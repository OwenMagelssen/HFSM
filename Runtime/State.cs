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
		public readonly string Name;
		public StateMachine ParentStateMachine { get; protected set; }
		public StateMachine RootStateMachine { get; protected set; }
		protected readonly List<Transition> Transitions = new();
		public ReadOnlyCollection<Transition> ReadOnlyTransitions => Transitions.AsReadOnly();
		public int StateFlags { get; protected set; }
		public bool CanTransition = true;
		
		public State(StateMachine parentStateMachine, string name = "")
		{
			Name = name;
			ParentStateMachine = parentStateMachine;
			var parent = ParentStateMachine;
			
			while (parent != null)
			{
				RootStateMachine = parent;
				parent = parent.ParentStateMachine;
			}
			
			if (RootStateMachine != null)
			{
				RootStateMachine.NamedStatesDictionary.TryAdd(name, this);
				RootStateMachine.RegisterState(this);
			}
		}

		public virtual void AddTransitions(params Transition[] transitions) => Transitions.AddRange(transitions);

		public virtual bool TryToTransition()
		{
			if (!CanTransition) return false;
			if (ParentStateMachine.TryToTransition()) return true;
			
			for (int i = 0; i < Transitions.Count; i++)
			{
				var transition = Transitions[i];
				if (transition.TryTransition())
				{
					if (ParentStateMachine.SetState(transition.DestinationState))
						return true;
				}
			}

			return false;
		}
		
		public abstract void OnEnter(State previousState);
		
		public abstract void OnExit(State nextState);
		
		public abstract void OnUpdate(float deltaTime);
	}
}