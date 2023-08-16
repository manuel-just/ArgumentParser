using System;
using System.Collections.Generic;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ArgumentParserTests")]

namespace ArgumentParser
{
    /// <summary>
    /// Utility class to parse command line arguments. Define parameters with custom parsing functions and map argument lists to them.
    /// </summary>
    public class Parser
    {
        private readonly Parameter[] _parameters;

        public Parameter this[string key] => _parameters.First(p => p.Aliases.Contains(key));

        /// <summary>
        /// Generic accessor to a value by parameter name
        /// </summary>
        /// <typeparam name="T">The parameter type</typeparam>
        /// <param name="name">The parameter name</param>
        /// <returns>The parsed parameter value</returns>
        public T Value<T>(string name) => this[name].Value<T>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters">Parameter definitions</param>
        public Parser(params Parameter[] parameters)
        {
            Dictionary<string, int> duplicates = parameters.SelectMany(p => p.Aliases)
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .ToDictionary(x => x.Key, y => y.Count());
            if (0 < duplicates.Count)
            {
                throw new ArgumentException($"Aliases defined more than once. ({string.Join(", ", duplicates.Select(d => $"{d.Value}x {d.Key}"))}).", nameof(parameters));
            }
            _parameters = parameters.OrderBy(p => p.Type).ToArray();
        }

        /// <summary>
        /// Maps an array of argument strings to the registered parameters. Expects namend and flag arguments before required and required before optional. Parsing is only done when accessing the parameter's values.
        /// </summary>
        /// <param name="args">The arguments to be mapped to the parameters</param>
        /// <param name="helpText">A help text that can be used to display to the user</param>
        /// <returns>Whether all arguments could be mapped a parameter</returns>
        /// <remarks>
        /// Throws if no argument is mapped to a required parameter
        /// </remarks>
        public bool MapStrict(string[] args, out string helpText)
        {
            helpText = null;
            List<string> unmapped = Map(args);

            if (0 < unmapped.Count)
            {
                helpText = $"Unexpected Argument{(1 < unmapped.Count ? "s" : "")}: [{string.Join(", ", unmapped)}].\r\nExpected:\r\n{string.Join<Parameter>("\r\n", _parameters)}.";

                return false;
            }
            return string.Empty == helpText;
        }

        /// <summary>
        /// Maps an array of argument strings to the registered parameters. Expects namend and flag arguments before required and required before optional. Parsing is only done when accessing the parameter's values.
        /// </summary>
        /// <param name="args">The arguments to be mapped to the parameters</param>
        /// <returns>A list of argument strings that were not mapped to any parameter</returns>
        /// <remarks>
        /// Throws if no argument is mapped to a required parameter
        /// </remarks>
        public List<string> Map(string[] args)
        {
            List<string> unmapped = new List<string>();
            foreach (Parameter p in _parameters)
            {
                p.StringValue = null;
            }

            bool lookingForNamed = true;
            Parameter parameter = null;
            foreach (string argument in args)
            {
                if (null != parameter)
                {
                    parameter.StringValue = argument;
                    parameter = null;
                }
                else
                {
                    if (lookingForNamed)
                    {
                        lookingForNamed = GetByAlias(argument, out parameter);
                    }
                    if (!lookingForNamed)
                    {
                        if (GetNext(Parameter.ParameterType.Required, out parameter) || GetNext(Parameter.ParameterType.Optional, out parameter))
                        {
                            parameter.StringValue = argument;
                            parameter = null;
                        }
                        else
                        {
                            unmapped.Add(argument);
                        }
                    }
                }
            }
            if (GetNext(Parameter.ParameterType.Required, out parameter))
            {
                throw new InvalidOperationException($"No argument provided for required {parameter}.");
            }
            return unmapped;
        }

        private bool GetByAlias(string alias, out Parameter parameter)
        {
            parameter = _parameters.FirstOrDefault(p => null == p.StringValue && p.Aliases.Contains(alias));
            if (null == parameter)
            {
                return false;
            }
            if (Parameter.ParameterType.Flag == parameter.Type)
            {
                parameter.StringValue = alias;
                parameter = null;
                return true;
            }
            return Parameter.ParameterType.Named == parameter.Type;
        }

        private bool GetNext(Parameter.ParameterType type, out Parameter parameter)
        {
            parameter = _parameters.FirstOrDefault(p => type == p.Type && null == p.StringValue);
            return null != parameter;
        }
    }
}
