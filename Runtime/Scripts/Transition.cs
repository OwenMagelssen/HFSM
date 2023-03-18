/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

using System;

namespace HFSM
{
    public abstract class Transition
    {
	    public State ToState { get; private set; }

	    public Transition(State transitionTo)
	    {
		    ToState = transitionTo;
	    }
	    
	    public abstract bool TryTransition();
    }

    public class ConditionTransition : Transition
    {
	    private readonly Func<bool> _condition;
	    
	    public ConditionTransition(State transitionTo, Func<bool> condition) : base(transitionTo)
	    {
		    _condition = condition;
	    }

	    public override bool TryTransition() => _condition();
    }
}