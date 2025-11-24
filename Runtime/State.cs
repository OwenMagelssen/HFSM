/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System.Collections.Generic;

namespace HFSM
{
	public class State<T> : IState where T : StateData
	{
		public string Name { get; protected set; }
		public int Id { get; protected set; }
		public bool Enabled { get; set; } = true;
		public bool CanTransition { get; set; } = true;
		public IState Parent => m_Parent;
		public readonly State<T> m_Parent;
		public IState DefaultSubState => m_DefaultSubState;
		public State<T> m_DefaultSubState { get; private set; }
		public IState ActiveSubState => m_ActiveSubState;
		public State<T> m_ActiveSubState { get; set; }
		public readonly T StateData;
		
		private readonly StateLogic<T> StateLogic;
		private readonly StateMachine<T> StateMachine;
		private State<T>[] SubStates = { };
		private List<State<T>> _subStatesList = new();
		private Transition<T>[] Transitions = { };

		public State(StateMachine<T> stateMachine, State<T> parent, string name, StateLogic<T> stateLogic, T stateData)
		{
			Name = name;
			Id = NameToID(Name);
			StateData = stateData;
			StateLogic = stateLogic;
			StateLogic.Data = StateData;
			StateMachine = stateMachine;
			StateMachine.RegisterState(this);
			m_Parent = parent;
			m_Parent?.AddSubState(this);
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
			m_DefaultSubState = SubStates.Length > 0 ? SubStates[0] : null;
		}

		private void AddSubState(State<T> state)
		{
			_subStatesList.Add(state);
		}

		public void AddTransitions(params Transition<T>[] transitions)
		{
			Transition<T>[] newTransitions = new Transition<T>[Transitions.Length + transitions.Length];

			for (int i = Transitions.Length, n = newTransitions.Length; i < n; i++)
				newTransitions[i] = transitions[i - SubStates.Length];

			Transitions = newTransitions;
		}

		public bool TryToTransition(out State<T> nextState)
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

		public State<T> NearestCommonAncestorWith(State<T> state)
		{
			State<T> a = m_Parent;
			State<T> b = state.m_Parent;

			while (a != null && b != null)
			{
				if (a == b) return a;
				a = a.m_Parent;
				b = b.m_Parent;
			}

			return null;
		}

		public bool IsSiblingOf(State<T> state)
		{
			return state.Parent == Parent;
		}

		public bool IsDescendantOf(State<T> state)
		{
			return state.IsAncestorOf(this);
		}

		public bool IsAncestorOf(State<T> state)
		{
			if (state == null) return false;
			State<T> ancestor = state.m_Parent;

			while (ancestor != null)
			{
				if (ancestor == this) return true;
				ancestor = ancestor.m_Parent;
			}

			return false;
		}

		public void OnEnter(State<T> previousState)
		{
			StateLogic.OnEnter(previousState);
		}

		public void OnExit(State<T> nextState)
		{
			StateLogic.OnExit(nextState);
		}

		public void OnUpdate(float deltaTime)
		{
			StateLogic.OnUpdate(deltaTime);
		}
	}
}