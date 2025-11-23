/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

namespace HFSM
{
	public abstract class StateLogic<T> where T : StateData
	{
		public T Data;
		
		public abstract void OnEnter(State<T> previousState);

		public abstract void OnExit(State<T> nextState);

		public abstract void OnUpdate(float deltaTime);
	}
}