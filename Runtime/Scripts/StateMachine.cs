/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mono.Cecil.Cil;
using UnityEngine;

namespace HFSM
{
	public abstract class StateMachine : State
	{
		public State CurrentState { get; private set; }

		public State CurrentTopLevelState => currentTopLevelState;
		protected State currentTopLevelState;
		public event Action OnTopLevelStateChanged;
		public State DefaultState { get; private set; }
		protected readonly List<Transition> GlobalTransitions = new();
		public ReadOnlyCollection<Transition> ReadOnlyGlobalTransitions => GlobalTransitions.AsReadOnly();
		public bool IsRootStateMachine => ParentStateMachine == null;

		public StateMachine(StateMachine parentParentStateMachine) : base(parentParentStateMachine)
		{
			
		}

		public void SetDefaultState(State defaultState) => DefaultState = defaultState;

		public void AddGlobalTransitions(params Transition[] transitions) => GlobalTransitions.AddRange(transitions);

		private void SetCurrentTopLevelState(State state)
		{
			currentTopLevelState = state;
			var parent = ParentStateMachine;
			
			while (parent != null)
			{
				parent.currentTopLevelState = state;
				parent = parent.ParentStateMachine;
			}

			OnTopLevelStateChanged?.Invoke();
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
			if (!IsRootStateMachine)
				if (ParentStateMachine.TryToTransition()) return true;
			
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