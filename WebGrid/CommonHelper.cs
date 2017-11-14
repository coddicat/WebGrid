using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Solomonic.WebGrid
{
    internal static class CommonHelper
    {
        public static IDictionary<string, object> ToDictionary(this object data)
        {
            if (data == null) return null;
            if (data is IDictionary<string, object>)
                return data as IDictionary<string, object>;

            const BindingFlags publicAttributes = BindingFlags.Public | BindingFlags.Instance;

            return
                data.GetType()
                    .GetProperties(publicAttributes)
                    .Where(property => property.CanRead)
                    .ToDictionary(property => property.Name, property => property.GetValue(data, null));

        }

        public static string GetDisplayName(PropertyInfo property)
        {
            var displayNameAttribute = property.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
            if (displayNameAttribute != null)
            {
                return displayNameAttribute.DisplayName;
            }

            return null;
        }

        public static string GetDisplayName<TData>(string propertyName)
        {
            var type = typeof(TData);
            var property = type.GetProperty(propertyName);
            return property != null ? GetDisplayName(property) : null;
        }

        public static string GetDisplayFormat(PropertyInfo property)
        {
            var displayFormatAttribute = property.GetCustomAttribute(typeof(DisplayFormatAttribute)) as DisplayFormatAttribute;
            if (displayFormatAttribute != null)
            {
                return displayFormatAttribute.DataFormatString;
            }

            return null;
        }

        public static string GetDisplayFormat<T, TValue>(Expression<Func<T, TValue>> expression)
        {
            var body = expression.Body is UnaryExpression
                ? ((UnaryExpression)expression.Body).Operand as MemberExpression
                : expression.Body as MemberExpression;

            if (body != null && body.NodeType == ExpressionType.MemberAccess)
            {
                var member = (body).Member;
                var prop = (PropertyInfo)member;
                return GetDisplayFormat(prop);
            }

            return null;
        }

        public static string GetDisplayFormat<TData>(string propertyName)
        {
            var type = typeof(TData);
            var property = type.GetProperty(propertyName);
            return property != null ? GetDisplayFormat(property) : null;
        }

        public static string GetDescription(this object value)
        {
            if (value != null)
            {
                var descriptionAttribute =
                    (value.GetType().GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute) ??
                    (value.GetType().GetField(value.ToString()) != null
                        ? value.GetType().GetField(value.ToString()).GetCustomAttribute(typeof(DescriptionAttribute))
                            as
                            DescriptionAttribute
                        : null);
                if (descriptionAttribute != null)
                {
                    return descriptionAttribute.Description;
                }
            }
            return null;
        }

        public static string GetMemberName<T, TValue>(Expression<Func<T, TValue>> expression)
        {
            var body = expression.Body is UnaryExpression
                ? ((UnaryExpression)expression.Body).Operand as MemberExpression
                : expression.Body as MemberExpression;

            if (body != null && body.NodeType == ExpressionType.MemberAccess)
            {
                var member = (body).Member;
                return member.Name;
            }

            return null;
        }

    }
}