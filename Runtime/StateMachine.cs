/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HFSM
{
	public class StateMachine
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

		protected class StateBuffer
		{
			public State[] States = { };
			public int Count { get; private set; }

			public int Capacity
			{
				get => _capacity;
				set
				{
					if (value <= _capacity) return;
					_capacity = value;
					State[] newBuffer = new State[_capacity];

					for (int i = 0; i < States.Length; i++)
						newBuffer[i] = States[i];

					States = newBuffer;
				}
			}

			private int _capacity;

			public int IndexOf(State state)
			{
				for (int i = 0, n = States.Length; i < n; i++)
				{
					if (state == States[i])
						return i;
				}

				return -1;
			}

			public void SetBufferFromState(State state)
			{
				int count = 0;
				State s = state;

				while (s != null)
				{
					count += 1;
					s = s.Parent;
				}

				Count = count;
				if (Capacity < Count) Capacity = Count;
				s = state;

				for (int i = count - 1; i >= 0; i--)
				{
					States[i] = s;
					s = s.Parent;
				}
			}

			public bool CheckForTransitions(out State nextState)
			{
				for (int i = 0; i < Count; i++)
				{
					if (States[i].TryToTransition(out State destinationState))
					{
						nextState = destinationState;
						return true;
					}
				}

				nextState = null;
				return false;
			}

			public void UpdateAll(float deltaTime)
			{
				for (int i = 0; i < Count; i++)
					States[i].OnUpdate(deltaTime);
			}
		}

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
		
		public virtual void LogError(string error) { }

		public void SetRootState(State rootState)
		{
			RootState = rootState;
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
			var commonAncestor = formerState.NearestCommonAncestorWith(nextState);
			ActiveState = nextState; // ActiveStateBuffer is updated when ActiveState is set

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

			int caIndex = ActiveStateBuffer.IndexOf(commonAncestor);

			if (caIndex >= 0 && caIndex < ActiveStateBuffer.Count - 1)
			{
				// commonAncestor is already active, so we start with its active substate
				for (int i = caIndex + 1, n = ActiveStateBuffer.Count; i < n; i++)
				{
					var s = ActiveStateBuffer.States[i];
					s.OnEnter(s.Parent.ActiveSubState);
					s.Parent.ActiveSubState = s;
				}
			}

			return true;
		}
		
		public void Initialize()
		{
			foreach (var state in AllStates)
				state.Initialize();

			Initialized = true;
			ActiveState = RootState; // we use this instead of SetState, which requires ActiveState to not be null
			OnStart();
			OnInitialized?.Invoke();
			RootState.OnEnter(null);
		}
		
		protected virtual void OnStart() { }

		public void Update(float deltaTime)
		{
			if (ActiveStateBuffer.CheckForTransitions(out State nextState))
			{
				SetState(nextState);
				return;
			}

			ActiveStateBuffer.UpdateAll(deltaTime);
		}
	}
}