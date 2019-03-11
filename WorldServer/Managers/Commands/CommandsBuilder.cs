using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SystemData;
using WorldServer.World.Objects;

namespace WorldServer.Managers.Commands
{
    /// <summary>
    /// Declares a method a hangling a command (".commandName").
    /// </summary>
    /// <remarks>
    /// Method must be public static.
    /// 
    /// First parameter is always the player.
    /// Next parameters should be string, integers (uint, short, etc.) or bool (usage with .command 1 .command true .command on)
    /// Optional arguments are allowed.
    /// 
    /// [CommandMethod(EGmLevel.EmpoweredStaff, "This is a test method")]
    /// public static void Test(Player plr, bool boolArg, string targetString = null)
    /// 
    /// Second argument, if exists, will be set to the current player's target.
    /// 
    /// [CommandMethod(EGmLevel.EmpoweredStaff, "This is a test method")]
    /// public static void Other(Player plr, Unit aTarget, ushort anotherArg)
    /// 
    /// If necessary, methods CAN return a boolean (true if command was handled, false otherwise).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    internal class CommandAttribute : Attribute
    {
        public CommandAttribute(EGmLevel accessRequired, string description)
        {
            AccessRequired = accessRequired;
            Description = description;
        }

        public EGmLevel AccessRequired { get; }
        public string Description { get; }

    }

    /// <summary>
    /// Factory responsible of building commands using introspection.
    /// </summary>
    internal class CommandsBuilder
    {
        private CommandsBuilder() { }

        /// <summary>
        /// Builds a list of commands using reflexion on the given class.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="declarative"></param>
        /// <returns></returns>
        public static List<GmCommandHandler> BuildCommands(Type type, params GmCommandHandler[] declarative)
        {
            List<GmCommandHandler> list = new List<GmCommandHandler>();

            list.AddRange(declarative); // Optional declarative commands hard coded by invoker

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                object[] attributes = method.GetCustomAttributes(typeof(CommandAttribute), false);
                if (attributes.Length == 1)
                {
                    GmCommandHandler handler = CreateCommand(method, (CommandAttribute)attributes[0]);
                    if (handler != null)
                        list.Add(handler);
                }
            }

            return list;
        }

        /// <summary>
        /// Creates a new command targeting at given method.
        /// </summary>
        /// <param name="method">Method invoked by command</param>
        /// <param name="commandInfo">Command method attribute providing metadata</param>
        /// <returns>Newly created command or null in case of error</returns>
        private static GmCommandHandler CreateCommand(MethodInfo method, CommandAttribute commandInfo)
        {
            ParameterInfo[] parameters = method.GetParameters();

            // First parameter is always player
            if (parameters.Length == 0 || parameters[0].ParameterType != typeof(Player))
            {
                Log.Error("CommandsBuilder", string.Format("{0}.{1} command must expect the player as first parameter.", method.DeclaringType.Name, method.Name));
                return null;
            }

            bool hasTargetParam = HasTargetParameter(parameters); // Second can be target
            int startIndex = hasTargetParam ? 2 : 1;

            // Other parameters
            for (int i = startIndex; i < parameters.Length; i++)
                if (!CheckParameter(parameters[i]))
                    return null;

            // Method invokation handler
            GmCommandHandler.GmComHandler del = (Player player, ref List<string> rawValues) => {
                return Invoke(method, player, rawValues);
            };

            // Method declaration
            string description = string.Concat(CommandToString(parameters[0]), " : ", commandInfo.Description);
            GmCommandHandler handler = new GmCommandHandler(
                method.Name.ToLowerInvariant(), del, null, commandInfo.AccessRequired, -1, description);
            return handler;
        }

        /// <summary>
        /// Invokes a command's method for given player and raw parameter values.
        /// </summary>
        /// <param name="method">Method to invoke</param>
        /// <param name="player">Player that invoked this command</param>
        /// <param name="rawValues">Raw values given by player</param>
        /// <returns>False if method returned false, true otherwise.</returns>
        private static bool Invoke(MethodInfo method, Player player, List<string> rawValues)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] values = new object[parameters.Length];

            // First parameter is always player
            values[0] = player;

            // Second parameter can be target
            bool hasTargetParam = HasTargetParameter(parameters); // Second can be target
            int startIndex = hasTargetParam ? 2 : 1;

            if (hasTargetParam)
            {
                values[1] = GMUtils.GetTargetOrMe(player);
                if (!parameters[1].ParameterType.IsAssignableFrom(values[0].GetType()))
                {
                    player.SendClientMessage(string.Concat(
                        CommandToString(parameters[1]), " : You must target a ", parameters[1].ParameterType.Name.ToLowerInvariant()), ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    return true;
                }
            }

            // Other parameters
            for (int i = startIndex; i < values.Length; i++)
            {
                values[i] = GetValue(parameters[i], i - startIndex, player, rawValues);
                if (values[i] == DBNull.Value)
                    return true;
            }
            if (values.Length - startIndex < rawValues.Count)
            {
                player.SendClientMessage(string.Concat(
                                        CommandToString(parameters[1]), " : too many parameters"), ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return true;
            }

            rawValues.Clear(); // why not

            try
            {
                object result = method.Invoke(null, values);
                if (result is bool)
                    return (bool)result;
                else
                    return true;
            }
            catch (TargetInvocationException e)
            {
                Exception inner = e.InnerException;
                Log.Error("CommandsBuilder", string.Concat(CommandToString(parameters[0]), " - ", inner.ToString()));
                player.SendClientMessage(string.Concat(
                    CommandToString(parameters[0]), " : ", inner.GetType().Name,
                    string.IsNullOrEmpty(inner.Message) ? "": " - " + inner.Message), ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return true;
            }
        }

        /// <summary>
        /// Checks whether the given paramater is valid for command handling.
        /// </summary>
        /// <param name="parameter">Parameter to check</param>
        /// <returns>True if valid, false otherwise</returns>
        private static bool CheckParameter(ParameterInfo parameter)
        {
            Type type = parameter.ParameterType;

            if (typeof(Unit).IsAssignableFrom(type))
            {
                Log.Error("CommandsBuilder", string.Format(
                    "{0}.{1} command must not expect more that one target parameter.",
                    parameter.Member.DeclaringType.Name, parameter.Member.Name));
                return false;
            }
            else if (type != typeof(Zone_Info)
                && type != typeof(short) && type != typeof(int) && type != typeof(long)
                && type != typeof(ushort) && type != typeof(uint) && type != typeof(ulong)
                && type != typeof(string) && type != typeof(bool)
                && !type.IsEnum)
            {
                Log.Error("CommandsBuilder", string.Format(
                    "{0}.{1} command parameter {2} has illegal value : {3}",
                    parameter.Member.DeclaringType.Name, parameter.Member.Name, parameter.Name, type));
                return false;
            }

            return true; // Valis
        }

        /// <summary>
        /// Gets the method parameter value.
        /// </summary>
        /// <param name="parameter">Parameter reflexion into</param>
        /// <param name="index">Index of the parameter as seen by player</param>
        /// <param name="player">Player bound to</param>
        /// <param name="rawValues"></param>
        /// <returns>Parsed value or DBNull if could not be barsed</returns>
        private static object GetValue(ParameterInfo parameter, int index, Player player, List<string> rawValues)
        {
            if (index < rawValues.Count)
            {
                string rawValue = rawValues[index];
                object value = GMUtils.TryParse(parameter.ParameterType, rawValue);
                if (value == null)
                {
                    player.SendClientMessage(string.Concat(
                        CommandToString(parameter), " : Unexpected value for ", parameter.Name), ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    return DBNull.Value;
                }
                return value;
            }
            else if (parameter.DefaultValue != DBNull.Value)
            {
                return parameter.DefaultValue;
            }
            else
            {
                player.SendClientMessage(string.Concat(
                    CommandToString(parameter), " : ", parameter.Name, " is required"), ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return DBNull.Value;
            }
        }

        /// <summary>
        /// Formats a string representation of the a command.
        /// </summary>
        /// <param name="parameter">One of the method parameters</param>
        /// <returns>Command name and parameters representation</returns>
        private static string CommandToString(ParameterInfo parameter)
        {
            MethodInfo method = (MethodInfo)parameter.Member;
            ParameterInfo[] parameters = method.GetParameters();

            StringBuilder sb = new StringBuilder();
            sb.Append(method.Name.ToLowerInvariant());

            bool hasTargetParam = HasTargetParameter(parameters); // Second can be target
            int startIndex = hasTargetParam ? 2 : 1;
            
            sb.Append("<");
            for (int i = startIndex; i < parameters.Length; i++)
            {
                if (i > startIndex)
                    sb.Append(",");
                sb.Append(parameters[i].Name);
            }
            sb.Append(">");

            return sb.ToString();
        }

        /// <summary>
        /// Checks whether given parameters list has a target parameter as second parameter.
        /// </summary>
        /// <param name="parameters">Paramaters to inspect</param>
        /// <returns>True if so, false otherwise</returns>
        private static bool HasTargetParameter(ParameterInfo[] parameters)
        {
            return parameters.Length >= 2 && typeof(Unit).IsAssignableFrom(parameters[1].ParameterType);
        }
    }
}
