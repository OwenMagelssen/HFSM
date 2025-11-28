/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

namespace HFSM
{
	public partial class StateMachine<T> where T : StateData
	{
		protected class StateBuffer
		{
			public State<T>[] States = { };
			public int Count { get; private set; }

			public int Capacity
			{
				get => _capacity;
				set
				{
					if (value <= _capacity) return;
					_capacity = value;
					State<T>[] newBuffer = new State<T>[_capacity];

					for (int i = 0; i < States.Length; i++)
						newBuffer[i] = States[i];

					States = newBuffer;
				}
			}

			private int _capacity;

			public int IndexOf(State<T> state)
			{
				for (int i = 0, n = States.Length; i < n; i++)
				{
					if (state == States[i])
						return i;
				}

				return -1;
			}

			// Assembles the state buffer ordered from root to leaf
			public void SetBufferFromState(State<T> state)
			{
				int count = 0;
				State<T> s = state;

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

			public bool CheckForTransitions(out State<T> nextState, out Transition<T> transition)
			{
				for (int i = 0; i < Count; i++)
				{
					if (States[i].TryToTransition(out State<T> destinationState, out Transition<T> t))
					{
						nextState = destinationState;
						transition = t;
						return true;
					}
				}

				nextState = null;
				transition = null;
				return false;
			}

			public void UpdateAll(float deltaTime)
			{
				for (int i = 0; i < Count; i++)
					States[i].OnUpdate(deltaTime);
			}
		}
	}
}