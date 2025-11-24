/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

namespace HFSM
{
	public interface IStateMachine
	{
		public bool SetState(string stateName);

		public bool SetState(string stateName, out int id);

		public bool SetState(int id);

		public void Initialize();
		
		public void Update(float deltaTime);
	}
}