using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // This was helpful: http://weblog.west-wind.com/posts/2012/Feb/01/Dynamic-Types-and-DynamicObject-References-in-C
    // Starts with one top-level variable dictionary
    internal class VariableStack : DynamicObject
    {
        private readonly Engine _engine;
        private readonly Stack<IDictionary<string, object>> _variables;
        private readonly IDictionary<string, object> _topLevel;

        public VariableStack(Engine engine)
        {
            _engine = engine;
             _variables = new Stack<IDictionary<string, object>>();
             _topLevel = new Dictionary<string, object>();
             _variables.Push(_topLevel);
        }

        private VariableStack(VariableStack variableStack)
        {
            _engine = variableStack._engine;
            _variables = new Stack<IDictionary<string, object>>(variableStack._variables.Reverse());
            _topLevel = variableStack._topLevel;
        }

        internal VariableStack Clone()
        {
            return new VariableStack(this);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            IDictionary<string, object> dict = _variables.FirstOrDefault(x => x.ContainsKey(binder.Name));
            if (dict != null)
            {
                result = dict[binder.Name];
                return true;
            }
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _variables.Peek()[binder.Name] = value;
            return true;
        }

        public void Push(IDictionary<string, object> dictionary)
        {
            foreach (KeyValuePair<string, object> kvp in dictionary)
            {
                CheckForDuplicate(kvp.Key);
            }

            _variables.Push(dictionary);
        }

        public void AddTopLevel(string key, object value)
        {
            CheckForDuplicate(key);
            _topLevel[key] = value;
        }

        public void Add(string key, object value)
        {
            CheckForDuplicate(key);
            _variables.Peek()[key] = value;
        }

        private void CheckForDuplicate(string key)
        {
            if (_variables.Any(x => x.ContainsKey(key)))
            {
                _engine.Trace.Warning("Duplicate value for variable {0}.", key);
            }
        }
    }
}
