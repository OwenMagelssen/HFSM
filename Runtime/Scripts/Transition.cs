/******************************************************************
 * Copyright (C) 2023 Optic Nerve Interactive. All rights reserved.
 * https://opticnerveinteractive.com
 ******************************************************************/

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
}