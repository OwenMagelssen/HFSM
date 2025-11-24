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
		
		public IState NearestCommonAncestorWith(IState state)
		{
			IState a = iParent;
			IState b = state.iParent;

			while (a != null && b != null)
			{
				if (a == b) return a;
				a = a.iParent;
				b = b.iParent;
			}

			return null;
		}

		public bool IsSiblingOf(IState state)
		{
			return state.iParent == iParent;
		}

		public bool IsDescendantOf(IState state)
		{
			return state.IsAncestorOf(this);
		}

		public bool IsAncestorOf(IState state)
		{
			if (state == null) return false;
			IState ancestor = state.iParent;

			while (ancestor != null)
			{
				if (ancestor == this) return true;
				ancestor = ancestor.iParent;
			}

			return false;
		}
	}
}