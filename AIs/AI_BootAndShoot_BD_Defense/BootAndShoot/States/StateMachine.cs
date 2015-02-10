using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BootAndShoot
{
    public class StateMachine<T>
    {
        private T owner;
        private State<T> currentState { get; set; }
        private State<T> previousState { get; set; }
        private State<T> globalState { get; set; }

        public StateMachine(T owner)
        {
            this.owner = owner;
            currentState = null;
            previousState = null;
            globalState = null;
        }
        //use these methods to initialize the FSM
        public void SetCurrentState(State<T> s) { currentState = s; }
        public void SetGlobalState(State<T> s) { globalState = s; }
        public void SetPreviousState(State<T> s) { previousState = s; }
        public void Update()
        {
            //if a global state exists, call its execute method
            if (globalState != null)
            {
                globalState.Execute(owner);
            }
            //same for the current state
            if (currentState != null)
            {
                currentState.Execute(owner);
            }
        }
        public bool  IsInState(State<T> state)
        {
            if (currentState == state)
            {
                return true;
            } 
            return false;
        }

        public void ChangeState(State<T> newState)
        {
            if (newState != currentState)
            {
                //keep a record of the previous state
                previousState = currentState;
                //call the exit method of the existing state
                currentState.Exit(owner);
                //change state to the new state
                currentState = newState;
                //call the entry method of the new state
                if (currentState.Enter(owner))
                {
                    currentState.Execute(owner);
                }
            }
            
        }

        //change state back to the previous state
        public void RevertToPreviousState()
        {
            ChangeState(previousState);
        }


        public bool HandleMessage(Telegram msg)
        {
            //first see if the current state is valid and that it can handle
            //the message
            if ((currentState != null) && (currentState.OnMessage(owner, msg)))
            {
                return true;
            }
            //if not, and if a global state has been implemented, send 
            //the message to the global state
            if (globalState != null && globalState.OnMessage(owner, msg))
            {
                return true;
            }

            return false;

        }
    }
}
