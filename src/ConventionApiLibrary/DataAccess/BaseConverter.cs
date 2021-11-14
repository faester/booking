using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Amazon.SimpleDB.Model;
using Attribute = Amazon.SimpleDB.Model.Attribute;

namespace ConventionApiLibrary.DataAccess
{
    public abstract class BaseConverter<T>
    {
        private readonly Func<string, T> _factory;
        private readonly Func<T, string> _idPicker;
        private readonly Dictionary<string, Action<T, string>> _mappings = new Dictionary<string, Action<T, string>>();
        private readonly Dictionary<string, Func<T, string>> _accessors = new Dictionary<string, Func<T, string>>();
        private readonly Dictionary<string, string> _memberNameToFieldName = new Dictionary<string, string>();

        protected BaseConverter(Func<string, T> factory, Func<T, string> idPicker)
        {
            _factory = factory;
            _idPicker = idPicker;
        }

        public T CreateInstance(string identifier)
        {
            return _factory(identifier);
        }

        public T Translate(string itemName, List<Attribute> attributes)
        {
            var instance = CreateInstance(itemName);

            foreach (var attribute in attributes)
            {
                if (_mappings.TryGetValue(attribute.Name, out var mapper))
                {
                    mapper(instance, attribute.Value);
                }
            }

            return instance;
        }

        public void AddFieldMapping<V>(string simpleDbName,
            Action<T, string> mapping,
            Func<T, string> accessor,
            Expression<Func<T, V>> selector)
        {
            if (simpleDbName == null)
            {
                throw new ArgumentNullException(nameof(simpleDbName));
            }
            if (accessor == null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }
            _mappings[simpleDbName] = mapping ?? throw new ArgumentNullException(nameof(mapping));

            var memberAccess = ConvertToMemberExpression<V>(selector);
            _memberNameToFieldName[memberAccess.Member.Name] = simpleDbName;
            _accessors[simpleDbName] = accessor;
        }

        private MemberExpression ConvertToMemberExpression<V>(Expression<Func<T, V>> accessor)
        {
            var memberAccess = accessor.Body as MemberExpression;
            if (memberAccess == null)
            {
                throw new ArgumentOutOfRangeException(nameof(accessor), "Should be simple property accessor.");
            }

            return memberAccess;
        }

        public string GetItemName(T item)
        {
            return _idPicker(item);
        }

        public void PopulateValues(T item, List<ReplaceableAttribute> attributes, List<Attribute> deletes)
        {
            foreach (var accessor in _accessors)
            {
                var value = accessor.Value(item);
                if (value == null)
                {
                    deletes.Add(new Attribute(accessor.Key, null));
                }
                else
                {
                    attributes.Add(new ReplaceableAttribute(accessor.Key, value, true));
                }
            }
        }

        protected string GetSimpleDbName(Expression<Func<T, object>> dtoField)
        {
            var memberExpression = ConvertToMemberExpression(dtoField);

            if (!_memberNameToFieldName.TryGetValue(memberExpression.Member.Name, out var name))
            {
                throw new ArgumentException($"There is no mapping from member {memberExpression.Member.Name} to SimpleDb");
            }

            return name;
        }
    }
}