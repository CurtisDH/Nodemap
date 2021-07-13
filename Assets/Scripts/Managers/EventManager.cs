using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers.EventManager
{
    public class EventManager : MonoBehaviour
    {
        private static Dictionary<string, dynamic> _eventDictionary = new Dictionary<string, dynamic>();

        #region Listen
        public static void Listen(string eventName, Action method)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                var eventToAdd = _eventDictionary[eventName];
                eventToAdd += method;
                _eventDictionary[eventName] = eventToAdd;
            }
            else
            {
                _eventDictionary.Add(eventName, method);
            }
        }
        public static void Listen<T>(string eventName, Action<T> method)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                var eventToAdd = _eventDictionary[eventName];
                eventToAdd += method;
                _eventDictionary[eventName] = eventToAdd;
            }
            else
            {
                _eventDictionary.Add(eventName, method);
            }
        }
        public static void Listen<T, Q>(string eventName, Action<T, Q> method)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                var eventToAdd = _eventDictionary[eventName];
                eventToAdd += method;
                _eventDictionary[eventName] = eventToAdd;
            }
            else
            {
                _eventDictionary.Add(eventName, method);
            }
        }
        public static void Listen<T, Q, R>(string eventName, Action<T, Q, R> method)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                var eventToAdd = _eventDictionary[eventName];
                eventToAdd += method;
                _eventDictionary[eventName] = eventToAdd;
            }
            else
            {
                _eventDictionary.Add(eventName, method);
            }
        }
        public static void Listen<T, Q, R, Z>(string eventName, Action<T, Q, R, Z> method)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                var eventToAdd = _eventDictionary[eventName];
                eventToAdd += method;
                _eventDictionary[eventName] = eventToAdd;
            }
            else
            {
                _eventDictionary.Add(eventName, method);
            }
        }
        #endregion
        #region Raising Events

        public static void RaiseEvent(string eventName)
        {
            try
            {
                var EventToRaise = _eventDictionary?[eventName] as Action;
                EventToRaise?.Invoke();
            }
            catch
            {
                //Debug.Log(e.Data);
            }

        }
        public static void RaiseEvent<T>(string eventName, T arg)
        {
            try
            {
                var EventToRaise = _eventDictionary?[eventName] as Action<T>;
                EventToRaise?.Invoke(arg);
            }
            catch
            {

            }

        }
        public static void RaiseEvent<T, Q>(string eventName, T arg, Q arg1)
        {
            try
            {
                var EventToRaise = _eventDictionary?[eventName] as Action<T, Q>;
                EventToRaise?.Invoke(arg, arg1);
            }
            catch
            {

            }

        }
        public static void RaiseEvent<T, Q, R>(string eventName, T arg, Q arg1, R arg2)
        {
            try
            {
                var EventToRaise = _eventDictionary?[eventName] as Action<T, Q, R>;
                EventToRaise?.Invoke(arg, arg1, arg2);
            }
            catch
            {

            }

        }
        public static void RaiseEvent<T, Q, R, Z>(string eventName, T arg, Q arg1, R arg2, Z arg3)
        {
            try
            {
                var EventToRaise = _eventDictionary?[eventName] as Action<T, Q, R, Z>;
                EventToRaise?.Invoke(arg, arg1, arg2, arg3);
            }
            catch
            {

            }

        }
        #endregion
        #region Unsubscribing methods
        public static void UnsubscribeEvent(string eventName, Action method)
        {
            var eventToUnsubscribe = _eventDictionary[eventName];
            eventToUnsubscribe -= method;
            _eventDictionary[eventName] = eventToUnsubscribe;
        }

        public static void UnsubscribeEvent<T>(string eventName, Action<T> method)
        {
            var eventToUnsubscribe = _eventDictionary?[eventName];
            eventToUnsubscribe -= method;
            _eventDictionary[eventName] = eventToUnsubscribe;
        }
        public static void UnsubscribeEvent<T, Q>(string eventName, Action<T, Q> method)
        {
            var eventToUnsubscribe = _eventDictionary[eventName];
            eventToUnsubscribe -= method;
            _eventDictionary[eventName] = eventToUnsubscribe;
        }
        public static void UnsubscribeEvent<T, Q, R>(string eventName, Action<T, Q, R> method)
        {
            var eventToUnsubscribe = _eventDictionary[eventName];
            eventToUnsubscribe -= method;
            _eventDictionary[eventName] = eventToUnsubscribe;
        }
        public static void UnsubscribeEvent<T, Q, R, Z>(string eventName, Action<T, Q, R, Z> method)
        {
            var eventToUnsubscribe = _eventDictionary[eventName];
            eventToUnsubscribe -= method;
            _eventDictionary[eventName] = eventToUnsubscribe;
        }
        
        #endregion

    }
}