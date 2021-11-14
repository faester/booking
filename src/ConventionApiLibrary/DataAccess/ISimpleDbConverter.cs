using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Amazon.SimpleDB.Model;
using Attribute = Amazon.SimpleDB.Model.Attribute;

namespace ConventionApiLibrary.DataAccess
{
    public interface ISimpleDbConverter<T>
    {
        T Translate(string itemName, List<Attribute> itemAttributes);
        string GetItemName(T item);
        void PopulateValues(T item, List<ReplaceableAttribute> attributes, List<Attribute> deletes);
        string GetSimpleDbFieldNameFor(Expression<Func<T, object>> dtoField);
        T CreateInstance(string name);
    }
}