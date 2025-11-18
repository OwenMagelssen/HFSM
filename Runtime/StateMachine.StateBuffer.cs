/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

namespace HFSM
{
	public partial class StateMachine
	{
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
	}
}