using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;
using MySql.Data.MySqlClient;

namespace FrameWork.Database
{
    /// <summary>
    /// Facilitates assignment to a particular property of a provided object, given a MySqlReader and the ordinal of the column to read from.
    /// </summary>
    public abstract class MySqlExpressionDataBinder
    {
        public virtual void Assign(object dataObject, MySqlDataReader reader, int ordinal)
        {
        }

        public virtual void AssignObject(object dataObject, object newValue)
        {
        }

        public virtual void Initialize(MemberInfo propInfo)
        {
        }

        public static MySqlExpressionDataBinder GetFor<T>(MemberInfo propInfo)
            where T : DataObject
        {
            Type dataType = null;

            if (propInfo is PropertyInfo)
            {
                dataType = ((PropertyInfo)propInfo).PropertyType;
            }
            else if (propInfo is FieldInfo)
            {
                dataType = ((FieldInfo) propInfo).FieldType;
            }
            if (dataType.IsValueType && !dataType.IsEnum)
            {
                if (dataType == typeof(byte))
                {
                    StructExpressionDataBinder<T, byte> binder = new StructExpressionDataBinder<T, byte>();
                    binder.Initialize(propInfo, (r, i) => r.GetByte(i));
                    return binder;
                }

                if (dataType == typeof(sbyte))
                {
                    StructExpressionDataBinder<T, sbyte> binder = new StructExpressionDataBinder<T, sbyte>();
                    binder.Initialize(propInfo, (r, i) => r.GetSByte(i));
                    return binder;
                }

                if (dataType == typeof(ushort))
                {
                    StructExpressionDataBinder<T, ushort> binder = new StructExpressionDataBinder<T, ushort>();
                    binder.Initialize(propInfo, (r, i) => r.GetUInt16(i));
                    return binder;
                }

                if (dataType == typeof(short))
                {
                    StructExpressionDataBinder<T, short> binder = new StructExpressionDataBinder<T, short>();
                    binder.Initialize(propInfo, (r, i) => r.GetInt16(i));
                    return binder;
                }

                if (dataType == typeof(uint))
                {
                    StructExpressionDataBinder<T, uint> binder = new StructExpressionDataBinder<T, uint>();
                    binder.Initialize(propInfo, (r, i) => r.GetUInt32(i));
                    return binder;
                }

                if (dataType == typeof(int))
                {
                    StructExpressionDataBinder<T, int> binder = new StructExpressionDataBinder<T, int>();
                    binder.Initialize(propInfo, (r, i) => r.GetInt32(i));
                    return binder;
                }

                if (dataType == typeof(ulong))
                {
                    StructExpressionDataBinder<T, ulong> binder = new StructExpressionDataBinder<T, ulong>();
                    binder.Initialize(propInfo, (r, i) => r.GetUInt64(i));
                    return binder;
                }

                if (dataType == typeof(long))
                {
                    StructExpressionDataBinder<T, long> binder = new StructExpressionDataBinder<T, long>();
                    binder.Initialize(propInfo, (r, i) => r.GetInt64(i));
                    return binder;
                }

                if (dataType == typeof(float))
                {
                    StructExpressionDataBinder<T, float> binder = new StructExpressionDataBinder<T, float>();
                    binder.Initialize(propInfo, (r, i) => r.GetFloat(i));
                    return binder;
                }


                if (dataType == typeof(double))
                {
                    StructExpressionDataBinder<T, double> binder = new StructExpressionDataBinder<T, double>();
                    binder.Initialize(propInfo, (r, i) => r.GetDouble(i));
                    return binder;
                }

                if (dataType == typeof(bool))
                {
                    StructExpressionDataBinder<T, bool> binder = new StructExpressionDataBinder<T, bool>();
                    binder.Initialize(propInfo, (r, i) => r.GetBoolean(i));
                    return binder;
                }

                if (dataType == typeof(DateTime))
                {
                    StructExpressionDataBinder<T, DateTime> binder = new StructExpressionDataBinder<T, DateTime>();
                    binder.Initialize(propInfo, (r, i) => r.GetDateTime(i));
                    return binder;
                }

                throw new ArgumentException("StructExpressionBinder", "Failed to match type " + dataType.FullName);
            }

            MySqlExpressionDataBinder classBinder = (MySqlExpressionDataBinder)Activator.CreateInstance(typeof(ClassExpressionDataBinder<,>).MakeGenericType(typeof(T), dataType));
            classBinder.Initialize(propInfo);
            return classBinder;
        }
    }

    public class StructExpressionDataBinder<TDataObject, TValue> : MySqlExpressionDataBinder
        where TDataObject : DataObject where TValue : struct
    {
        private Action<TDataObject, TValue> _assigner;
        private Func<MySqlDataReader, int, TValue> _valueConverter;

        public void Initialize(MemberInfo bindProperty, Func<MySqlDataReader, int, TValue> valueConverter)
        {
            ParameterExpression dataObjectParam = Expression.Parameter(typeof(TDataObject));
            ParameterExpression valueParam = Expression.Parameter(typeof(TValue));
            BinaryExpression assign = Expression.Assign(Expression.MakeMemberAccess(dataObjectParam, bindProperty), valueParam);

            var expression = Expression.Lambda<Action<TDataObject, TValue>>(assign, dataObjectParam, valueParam);
            _assigner = expression.Compile();

            _valueConverter = valueConverter;
        }

        public override void Assign(object dataObject, MySqlDataReader reader, int ordinal)
        {
            _assigner((TDataObject)dataObject, _valueConverter(reader, ordinal));
        }
    }

    public class ClassExpressionDataBinder<TDataObject, TValue> : MySqlExpressionDataBinder
    where TDataObject : DataObject where TValue : class
    {
        private Action<TDataObject, TValue> _assigner;

        public override void Initialize(MemberInfo bindProperty)
        {
            ParameterExpression dataObjectParam = Expression.Parameter(typeof(TDataObject));
            ParameterExpression valueParam = Expression.Parameter(typeof(TValue));
            BinaryExpression assign = Expression.Assign(Expression.MakeMemberAccess(dataObjectParam, bindProperty), valueParam);

            var expression = Expression.Lambda<Action<TDataObject, TValue>>(assign, dataObjectParam, valueParam);
            _assigner = expression.Compile();
        }

        public override void AssignObject(object dataObject, object newValue)
        {
            _assigner((TDataObject)dataObject, (TValue)newValue);
        }
    }
}
