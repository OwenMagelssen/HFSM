/******************************************************************
 * Copyright (C) 2025 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

namespace HFSM
{
	public interface IState
	{
		public string Name { get; }
		public int Id { get; }
		public bool Enabled { get; set; }
		public bool CanTransition { get; set; }
		public IState iParent { get; }
		public IState iDefaultSubState { get; }
		public IState iActiveSubState { get; }
	}
}