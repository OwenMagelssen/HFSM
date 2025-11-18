/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System.Collections.Generic;

namespace HFSM
{
	public abstract class State
	{
		public readonly string Name;
		public readonly int Id;
		public bool Enabled = true;
		public bool CanTransition = true;
		public State Parent { get; protected set; }
		public State ActiveSubState { get; set; }
		public int StateFlags { get; protected set; }
		

		protected readonly StateMachine StateMachine;
		public State DefaultSubState { get; private set; }
		protected State[] SubStates = { };
		private List<State> _subStatesList = new();
		protected Transition[] Transitions = { };

		public State(StateMachine stateMachine, State parent, string name)
		{
			Name = name;
			Id = NameToID(Name);
			StateMachine = stateMachine;
			Parent = parent;
			StateMachine.RegisterState(this);
		}

		public static int NameToID(string str)
		{
			uint hash = 2166136261;

			foreach (char c in str)
				hash = (hash ^ c) * 16777619;

			return unchecked((int) hash);
		}

		public void Initialize()
		{
			SubStates = _subStatesList.ToArray();
			_subStatesList.Clear();
			DefaultSubState = SubStates.Length > 0 ? SubStates[0] : null;
		}

		private void AddSubState(State state)
		{
			_subStatesList.Add(state);
		}

		public void AddTransitions(params Transition[] transitions)
		{
			Transition[] newTransitions = new Transition[Transitions.Length + transitions.Length];

			for (int i = Transitions.Length, n = newTransitions.Length; i < n; i++)
				newTransitions[i] = transitions[i - SubStates.Length];

			Transitions = newTransitions;
		}

		public bool TryToTransition(out State nextState)
		{
			if (!CanTransition)
			{
				nextState = null;
				return false;
			}

			// array foreach is highly optimized
			foreach (var transition in Transitions)
			{
				if (!transition.DestinationState.Enabled) continue;
				if (transition.TryTransition())
				{
					nextState = transition.DestinationState;
					return true;
				}
			}

			nextState = null;
			return false;
		}

		public State NearestCommonAncestorWith(State state)
		{
			State a = Parent;
			State b = state.Parent;

			while (a != null && b != null)
			{
				if (a == b) return a;
				a = a.Parent;
				b = b.Parent;
			}

			return null;
		}

		public bool IsSiblingOf(State state)
		{
			return state.Parent == Parent;
		}

		public bool IsDescendantOf(State state)
		{
			return state.IsAncestorOf(this);
		}

		public bool IsAncestorOf(State state)
		{
			State ancestor = state.Parent;

			while (ancestor != null)
			{
				if (ancestor == this) return true;
				ancestor = ancestor.Parent;
			}

			return false;
		}

		public abstract void OnEnter(State previousState);

		public abstract void OnExit(State nextState);

		public abstract void OnUpdate(float deltaTime);
	}
}