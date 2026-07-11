using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArenaFall.Utilities
{
    /// <summary>
    /// Generic state machine for AI and game state management.
    /// Provides state transitions with enter/update/exit lifecycle.
    /// </summary>
    public class StateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, IState> _states = new();
        private IState _currentState;
        
        public TState CurrentStateType { get; private set; }
        public TState PreviousStateType { get; private set; }

        /// <summary>
        /// Register a state with its enum type.
        /// </summary>
        public void RegisterState(TState type, IState state)
        {
            _states[type] = state;
        }

        /// <summary>
        /// Transition to a new state.
        /// </summary>
        public void TransitionTo(TState newState)
        {
            if (!_states.ContainsKey(newState))
            {
                Debug.LogError($"[StateMachine] State {newState} not registered!");
                return;
            }

            if (_currentState != null)
            {
                _currentState.Exit();
            }

            PreviousStateType = CurrentStateType;
            CurrentStateType = newState;
            _currentState = _states[newState];
            _currentState.Enter();

            Debug.Log($"[StateMachine] {PreviousStateType} -> {CurrentStateType}");
        }

        /// <summary>
        /// Update the current state.
        /// </summary>
        public void Update()
        {
            _currentState?.Update();
        }

        /// <summary>
        /// Fixed update for physics-based states.
        /// </summary>
        public void FixedUpdate()
        {
            _currentState?.FixedUpdate();
        }

        /// <summary>
        /// Check if the machine is in a specific state.
        /// </summary>
        public bool IsInState(TState state) => CurrentStateType.Equals(state);

        /// <summary>
        /// Get a state instance by type.
        /// </summary>
        public IState GetState(TState type)
        {
            return _states.TryGetValue(type, out var state) ? state : null;
        }

        /// <summary>
        /// Clear all registered states.
        /// </summary>
        public void Clear()
        {
            _currentState?.Exit();
            _currentState = null;
            _states.Clear();
        }
    }

    /// <summary>
    /// Interface for state implementations.
    /// </summary>
    public interface IState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }

    /// <summary>
    /// Base state class with common functionality.
    /// </summary>
    public abstract class BaseState : IState
    {
        protected MonoBehaviour Owner;
        protected StateMachine<Enum> StateMachine;

        protected BaseState(MonoBehaviour owner, StateMachine<Enum> stateMachine)
        {
            Owner = owner;
            StateMachine = stateMachine;
        }

        public abstract void Enter();
        public abstract void Update();
        public virtual void FixedUpdate() { }
        public abstract void Exit();
    }
}
