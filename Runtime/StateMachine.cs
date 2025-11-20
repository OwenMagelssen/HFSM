/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HFSM
{
	public partial class StateMachine
	{
		public State RootState { get; private set; }

		public State ActiveState
		{
			get => _activeState;
			protected set
			{
				_activeState = value;
				ActiveStateBuffer.SetBufferFromState(_activeState);
				OnStateChanged?.Invoke(_activeState);
			}
		}

		private State _activeState;
		public event Action<State> OnStateChanged;
		public ReadOnlyCollection<State> AllStates => _allStates.AsReadOnly();
		private List<State> _allStates = new();
		protected Dictionary<int, State> StateDictionary { get; private set; }
		protected StateBuffer ActiveStateBuffer = new StateBuffer();
		public event Action OnInitialized;
		public bool Initialized { get; private set; }

		public StateMachine()
		{
			StateDictionary = new Dictionary<int, State>();
		}

		~StateMachine()
		{
			var state = ActiveState;

			while (state != null)
			{
				state.OnExit(null);
				state = state.Parent;
			}
		}

		public void SetRootState(State rootState)
		{
			RootState = rootState;
			RegisterState(RootState);
		}

		public void RegisterState(State state)
		{
			if (!StateDictionary.TryAdd(state.Id, state))
			{
				LogError($"Duplicate state name {state.Name} cannot be added to the StateMachine.");
			}
			
			if (_allStates.Contains(state)) return;
			_allStates.Add(state);
		}

		public bool SetState(string stateName)
		{
			int id = State.NameToID(stateName);
			return SetState(id);
		}

		public bool SetState(string stateName, out int id)
		{
			id = State.NameToID(stateName);
			return SetState(id);
		}

		public bool SetState(int id)
		{
			if (StateDictionary.TryGetValue(id, out State state))
				return SetState(state);

			LogError($"State with ID {id.ToString()} does not exist");
			return false;
		}


		public bool SetState(State state)
		{
			if (state == ActiveState
			    || state == null
			    || state.IsAncestorOf(ActiveState)) return false;

			var nextState = state;

			while (nextState.DefaultSubState != null)
			{
				// setting this to null ensures null is the argument given to OnEnter
				// when default substates are activated
				nextState.ActiveSubState = null;
				nextState = nextState.DefaultSubState;
			}

			var formerState = ActiveState;
			var commonAncestor =  formerState == null ? RootState : formerState.NearestCommonAncestorWith(nextState);
			ActiveState = nextState; // ActiveStateBuffer is updated when ActiveState is set

			if (formerState != null)
			{
				// exit all previously active states, passing newly active
				// siblings as "nextState" argument if they exist, otherwise passing in ActiveState
				var fs = formerState;
				while (fs != commonAncestor)
				{
					bool foundSibling = false;

					for (int i = 0, n = ActiveStateBuffer.Count; i < n; i++)
					{
						var s = ActiveStateBuffer.States[i];
						if (!s.IsSiblingOf(fs)) continue;
						fs.OnExit(s);
						foundSibling = true;
						break;
					}

					if (!foundSibling) fs.OnExit(nextState);
					fs = fs.Parent;
				}

				if (state != nextState) // the state passed into this function had substates
					commonAncestor = formerState.NearestCommonAncestorWith(state);
			}

			// if former state isn't null, commonAncestor is already active,
			// so we start with its active substate (i.e. commonAncestor index + 1)
			int firstStateToEnter = ActiveStateBuffer.IndexOf(commonAncestor) + 1;

			if (firstStateToEnter >= 0 && firstStateToEnter < ActiveStateBuffer.Count - 1)
			{
				for (int i = firstStateToEnter,  n = ActiveStateBuffer.Count; i < n; i++)
				{
					var s = ActiveStateBuffer.States[i];

					if (s.Parent == null)
					{
						s.OnEnter(null);
					}
					else
					{
						s.OnEnter(s.Parent.ActiveSubState);
						s.Parent.ActiveSubState = s;
					}
				}
			}

			return true;
		}
		
		public void Initialize()
		{
			foreach (var state in AllStates)
				state.Initialize();

			Initialized = true;
			SetState(RootState);
			OnStart();
			OnInitialized?.Invoke();
			RootState.OnEnter(null);
		}

		public void Update(float deltaTime)
		{
			if (ActiveStateBuffer.CheckForTransitions(out State nextState))
			{
				SetState(nextState);
				return;
			}

			ActiveStateBuffer.UpdateAll(deltaTime);
		}
		
		public virtual void LogError(string error) { }
		
		protected virtual void OnStart() { }
	}
}