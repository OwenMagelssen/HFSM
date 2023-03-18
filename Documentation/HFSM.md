# HFSM Documentation
## Creating an HFSM
To create a HFSM, you need to implement classes inheriting from the following base classes: State, StateMachine, and Transition. 
Once you have those classes, you can create a HFSM like so:  
```
// create the root state machine
var stateMachine = new MyStateMachine(null);

// create the child states (these can also be state machines, since StateMachine inherits from State)
var myState = new MyState(stateMachine); // the argument passed to the constructor should be the parent state machine
var mySubStateMachine = new MyOtherStateMachine(stateMachine);

// create the transitions
var myFirstTransition = new MyFirstTransition(mySubStateMachine); // the argument here is the state to transition to
var mySecondTransition = new MySecondTransition(myState);
var myGlobalTransition = new MyGlobalTransition(mySubStateMachine);

// add the transitions to the states
myState.AddTransition(myFirstTransition);
mySubStatemachine.AddTransition(mySecondTransition);
stateMachine.AddGlobalTransition(myGlobalTransition);

// set the default state
stateMachine.SetDefaultState(myState);

// start the state machine
stateMachine.OnEnter(null);
```