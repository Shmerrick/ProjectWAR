using System;
using System.Reflection;

namespace FrameWork.Database
{
    /// <summary>
    /// Binds data to a property or field. Fast, but not thread safe and can only assign to a constant object, which must be cloned from.
    /// </summary>
    public abstract class DataBinder
    {
        public abstract void Assign(object o);

        public abstract void Initialize(object target, MethodInfo methodInfo);

        /// <summary>
        /// Returns a DataBinder which uses the provided accessor to set a property on the given object.
        /// </summary>
        /// <param name="target">The object upon which to set the property.</param>
        /// <param name="type">The type of the property.</param>
        /// <param name="method">The set accessor method to invoke.</param>
        /// <returns></returns>
        public static DataBinder GetFor(object target, Type type, MethodInfo method)
        {
            DataBinder binder;

            if (type.IsValueType && !type.IsEnum)
                binder = (DataBinder)Activator.CreateInstance(typeof (StructDataBinder<>).MakeGenericType(type));
            else
                binder = (DataBinder)Activator.CreateInstance(typeof(ClassDataBinder<>).MakeGenericType(type));

            binder.Initialize(target, method);

            return binder;
        }
    }

    public class StructDataBinder<T> : DataBinder where T : struct
    {
        private Action<T> _assignmentAction;

        public override void Initialize(object target, MethodInfo desiredMethod)
        {
            _assignmentAction = (Action<T>) Delegate.CreateDelegate(typeof (Action<T>), target, desiredMethod);
        }

        public override void Assign(object o)
        {
            try
            {
                if (o == null)
                    _assignmentAction(default(T));
                else
                {
                    T conversion = (T)o;
                    _assignmentAction(conversion);
                }
            }
            catch (InvalidCastException)
            {
                Log.Error("StructDataBinder<" + typeof(T).FullName + ">", "Could not convert value " + o + " of type " + o.GetType().FullName);
                throw;
            }            
        }
    }

    public class ClassDataBinder<T> : DataBinder where T : class
    {
        private Action<T> _assignmentAction;

        public override void Initialize(object target, MethodInfo desiredMethod)
        {
            _assignmentAction = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), target, desiredMethod);
        }

        public override void Assign(object o)
        {
            try
            {
                if (o == null)
                    _assignmentAction(null);
                else
                {
                    T conversion = (T)o;
                    _assignmentAction(conversion);
                }
            }
            catch (InvalidCastException)
            {
                Log.Error("ClassDataBinder<" + typeof(T).FullName + ">", "Could not convert value " + o + " of type " + o.GetType().FullName);
                throw;
            }
        }
    }
}
