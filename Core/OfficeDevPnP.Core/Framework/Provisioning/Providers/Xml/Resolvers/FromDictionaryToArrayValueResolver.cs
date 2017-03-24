﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections;

namespace OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a Dictionary into an Array of objects
    /// </summary>
    internal class FromDictionaryToArrayValueResolver<TKey, TValue> : IValueResolver
    {
        public string Name
        {
            get { return (this.GetType().Name); }
        }

        private String _keyField;
        private String _valueField;
        private Type _targetArrayItemType;

        public FromDictionaryToArrayValueResolver(Type targetArrayItemType,
            LambdaExpression keySelector, LambdaExpression valueSelector)
        {
            this._targetArrayItemType = targetArrayItemType;

            var keyField = keySelector.Body as MemberExpression ?? ((UnaryExpression)keySelector.Body).Operand as MemberExpression;
            var valueField = valueSelector.Body as MemberExpression ?? ((UnaryExpression)valueSelector.Body).Operand as MemberExpression;

            this._keyField = keyField.Member.Name;
            this._valueField = valueField.Member.Name;
        }

        public object Resolve(object source, object destination, object sourceValue)
        {
            var sourceDictionary = sourceValue != null && sourceValue is IEnumerable<KeyValuePair<TKey, TValue>> ?
                sourceValue as IEnumerable<KeyValuePair<TKey, TValue>>:
                source as IEnumerable<KeyValuePair<TKey, TValue>>;

            if (null == sourceDictionary)
            {
                throw new ArgumentException("Invalid source object. Expected type implementing IEnumerable<KeyValuePair<TKey, TValue>>", "source");
            }

            var listType = typeof(List<>);
            var resultType = this._targetArrayItemType.MakeArrayType();

            var result = (Array)Activator.CreateInstance(resultType, sourceDictionary.Count());
            var i = 0;
            foreach (var item in sourceDictionary)
            {
                var resultItem = Activator.CreateInstance(this._targetArrayItemType);
                resultItem.GetType().GetProperty(this._keyField, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(resultItem, item.Key);
                resultItem.GetType().GetProperty(this._valueField, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(resultItem, item.Value);
                result.SetValue(resultItem, i++);
            }

            return (result);
        }
    }
}
