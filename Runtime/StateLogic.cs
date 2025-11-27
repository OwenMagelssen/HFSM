/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

namespace HFSM
{
	public abstract class StateLogic<T> where T : StateData
	{
		public T Data;
		
		public virtual void OnInitialize() { }
		
		public virtual void OnEnter(State<T> previousState) { }

		public virtual void OnExit(State<T> nextState) { }

		public virtual void OnUpdate(float deltaTime) { }
	}
}