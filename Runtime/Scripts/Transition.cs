/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System;

namespace HFSM
{
    public abstract class Transition
    {
	    public State DestinationState { get; private set; }

	    public Transition(State transitionDestination)
	    {
		    DestinationState = transitionDestination;
	    }
	    
	    public abstract bool TryTransition();
    }

    public class ConditionTransition : Transition
    {
	    private readonly Func<bool> _condition;
	    
	    public ConditionTransition(State transitionDestination, Func<bool> condition) : base(transitionDestination)
	    {
		    _condition = condition;
	    }

	    public override bool TryTransition() => _condition();
    }
}