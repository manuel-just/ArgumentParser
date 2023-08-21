using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArgumentParser
{
    /// <summary>
    /// Parse function delegate
    /// </summary>
    /// <param name="s">The string to parse</param>
    /// <returns>The parsed object</returns>
    public delegate object Parse(string s);

    /// <summary>
    /// Parameter definition class. Use those to initialize a <see cref="Parser"/>
    /// </summary>
    public sealed class Parameter
    {
        private readonly Parse _parse;
        private readonly object _defaultValue;

        /// <summary>
        /// Order is important!
        /// </summary>
        internal enum ParameterType
        {
            Named = 0,
            Flag = 1,
            Required = 2,
            Optional = 3,
        }

        /// <summary>
        /// Distinction for Parser
        /// </summary>
        internal ParameterType Type { get; }

        /// <summary>
        /// Primary name
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// String value to be parsed
        /// </summary>
        internal string StringValue { get; set; } = null;

        /// <summary>
        /// List of names to identify parameter, including <see cref="Name"/>.
        /// </summary>
        internal ReadOnlyCollection<string> Aliases { get; }

        /// <summary>
        /// Base Constructor
        /// </summary>
        /// <param name="name">Primary name</param>
        /// <param name="parse">Parse function to parse the <see cref="StringValue"/> with</param>
        /// <param name="defaultValue">Default value to provide, when <see cref="StringValue"/> is null</param>
        /// <param name="alternateNames">Alternative names. Will make up the <see cref="Aliases"/> in conjunction with the <see cref="Name"/></param>
        private Parameter(ParameterType type, string name, Parse parse, object defaultValue, IEnumerable<string> alternateNames)
        {
            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _parse = parse ?? (x => x);
            _defaultValue = defaultValue;
            Aliases = new string[] { name }
                .Concat(alternateNames)
                .Select(n => n ?? throw new ArgumentNullException("alternative names may not be null."))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Parses the mapped value
        /// </summary>
        /// <typeparam name="T">The return type of</typeparam>
        /// <returns></returns>
        public T Value<T>()
        {
            return (T)Convert.ChangeType(null == StringValue ? _defaultValue : _parse(StringValue), typeof(T));
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ParameterType.Flag:
                    return $"{string.Join(", ", Aliases)} (Flag, optional)";
                case ParameterType.Named:
                    return $"{string.Join(", ", Aliases)} <value> (Named, optional)";
                case ParameterType.Required:
                    return $"{string.Join(", ", Aliases)} (Positional, required)";
                case ParameterType.Optional:
                    return $"{string.Join(", ", Aliases)} (Positional, optional)";
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Named Parameter (such as: <c>app.exe --filename C:\temp\inputfile.txt</c>)
        /// </summary>
        /// <param name="name">Primary name</param>
        /// <param name="defaultValue">Default value for <see cref="Value{T}"/></param>
        /// <param name="parse">Parse function to use for <see cref="Value{T}"/>. Defaults to the string identity function.</param>
        /// <param name="alternateNames">Alternative names. Will make up the <see cref="Aliases"/> in conjunction with the <see cref="Name"/></param>
        public static Parameter Named(string name, object defaultValue, Parse parse = null, params string[] aliases)
        {
            return new Parameter(ParameterType.Named, name, parse, defaultValue, aliases);
        }

        /// <summary>
        /// Flag Parameter (such as: <c>app.exe --verbose</c>). Defaults the <see cref="Value{T}"/> to false and the parse function alawys evaluates true.
        /// </summary>
        /// <param name="name">Primary name</param>
        /// <param name="alternateNames">Alternative names. Will make up the <see cref="Aliases"/> in conjunction with the <see cref="Name"/></param>
        public static Parameter Flag(string name, params string[] aliases)
        {
            return new Parameter(ParameterType.Flag, name, _ => true, false, aliases);
        }

        /// <summary>
        /// Optional Parameter (such as: <c>app.exe C:\temp\outputfile.txt</c>). Defaults the <see cref="Value{T}"/> to the provided devalut value.
        /// </summary>
        /// <param name="name">Primary name</param>
        /// <param name="defaultValue">Default value for <see cref="Value{T}"/></param>
        /// <param name="parse">Parse function to use for <see cref="Value{T}"/>. Defaults to the string identity function.</param>
        public static Parameter Optional(string name, object defaultValue, Parse parse = null)
        {
            return new Parameter(ParameterType.Optional, name, parse, defaultValue, new string[0]);
        }

        /// <summary>
        /// Required Parameter (such as: <c>app.exe C:\temp\inputfile.txt</c>).
        /// </summary>
        /// <param name="name">Primary name</param>
        /// <param name="parse">Parse function to use for <see cref="Value{T}"/>. Defaults to the string identity function.</param>
        public static Parameter Required(string name, Parse parse = null)
        {
            return new Parameter(ParameterType.Required, name, parse, null, new string[0]);
        }
    }
}