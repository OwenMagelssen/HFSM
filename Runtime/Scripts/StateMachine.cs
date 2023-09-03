/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace HFSM
{
	public abstract class StateMachine : State
	{
		public ReadOnlyCollection<State> AllStates => _allStates.AsReadOnly();
		private List<State> _allStates = new();
		public Dictionary<string, State> NamedStatesDictionary { get; private set; }
		public State CurrentState { get; private set; }
		public State CurrentTopLevelState { get; private set; }
		public event Action OnTopLevelStateChanged;
		public State DefaultState { get; private set; }
		protected readonly List<Transition> GlobalTransitions = new();
		public ReadOnlyCollection<Transition> ReadOnlyGlobalTransitions => GlobalTransitions.AsReadOnly();
		public bool IsRootStateMachine => ParentStateMachine == null;

		public StateMachine(StateMachine parentStateMachine, string name = "") : base(parentStateMachine, name)
		{
			if (parentStateMachine == null)
			{
				RootStateMachine = this;
				RegisterState(this);
			}
			else
			{
				ParentStateMachine = parentStateMachine;
				var parent = ParentStateMachine;
				RootStateMachine = parent;
				
				while (parent != null)
				{
					RootStateMachine = parent;
					parent = parent.ParentStateMachine;
				}
			}
			
			NamedStatesDictionary = IsRootStateMachine ? new Dictionary<string, State>() : RootStateMachine.NamedStatesDictionary;
			RootStateMachine.NamedStatesDictionary.TryAdd(name, this);
		}

		public void RegisterState(State state) => _allStates.Add(state);

		public void SetDefaultState(State defaultState)
		{
			DefaultState = defaultState;
		}
		
		public override void AddTransitions(params Transition[] transitions)
		{
			if (IsRootStateMachine)
				Debug.LogWarning("Non-global transitions on a root state machine will never be checked.");
			
			Transitions.AddRange(transitions);
		}

		public void AddGlobalTransitions(params Transition[] transitions)
		{
			GlobalTransitions.AddRange(transitions);
		}

		protected void SetCurrentTopLevelState(State state)
		{
			CurrentTopLevelState = state;
			OnTopLevelStateChanged?.Invoke();
			ParentStateMachine?.SetCurrentTopLevelState(state);
		}

		public bool SetState(string stateName)
		{
			if (NamedStatesDictionary.TryGetValue(stateName, out State state))
				return SetState(state);

			return false;
		}

		public bool SetState(State state)
		{
			if (state == CurrentState) return false;
			var formerState = CurrentState;
			formerState?.OnExit(state);
			CurrentState = state;
			SetCurrentTopLevelState(state);
			CurrentState?.OnEnter(formerState);
			return true;
		}

		public void Initialize() => OnEnter(null);

		public override void OnEnter(State previousState)
		{
			SetState(DefaultState);
		}

		public override void OnExit(State nextState)
		{
			CurrentState = null;
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
			CurrentState?.OnFixedUpdate();
		}

		private bool TryGlobalTransition()
		{
			for (int i = 0; i < GlobalTransitions.Count; i++)
			{
				var transition = GlobalTransitions[i];
				if (transition.TryTransition())
				{
					if (SetState(transition.DestinationState))
						return true;
				}
			}

			return false;
		}

		public override bool TryToTransition()
		{
			if (IsRootStateMachine)
				return TryGlobalTransition();
			
			// if this is a sub state machine...
			
			if (ParentStateMachine.TryToTransition()) 
				return true;
			
			if (TryGlobalTransition()) 
					return true;
			
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
	}
}