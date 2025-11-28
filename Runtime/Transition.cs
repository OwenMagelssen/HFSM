/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System;
using UnityEngine;

namespace HFSM
{
	public abstract class Transition<T> where T : StateData
	{
		public State<T> DestinationState { get; private set; }
		public readonly string Name;

		public Transition(State<T> destinationState, string name = null)
		{
			if (destinationState == null) Debug.LogError("DestinationState of a Transition cannot be null");
			DestinationState = destinationState;
			Name = string.IsNullOrEmpty(name) ? "To " + DestinationState.Name : name;
		}

		public abstract bool TryTransition();
	}

	public class ConditionTransition<T> : Transition<T> where T : StateData
	{
		private readonly Func<bool> _condition;

		public ConditionTransition(State<T> destinationState, Func<bool> condition, string name = null) 
			: base(destinationState, name)
		{
			_condition = condition;
		}

		public override bool TryTransition() => _condition();
	}
}