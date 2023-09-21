using System;
using System.Collections.Generic;
using TestLanguage.Language.Manage;

namespace TestLanguage.Language.Runtime
{
    public class LangEnvironment
    {
        private static LangEnvironment CreateGlobalEnvironment()
        {
            LangEnvironment langEnvironment = new LangEnvironment(false);

            langEnvironment.DeclareVariable("true", new BooleanValue(true), true);
            langEnvironment.DeclareVariable("false", new BooleanValue(false), true);
            langEnvironment.DeclareVariable("null", new NullValue(), true);

            langEnvironment.DeclareVariable("print", new NativeFuncValue((args, scope) =>
            {
                RuntimeValue arg_0 = args.FirstOrDefault() ?? new NullValue();

                string arg0 = arg_0.to_string();
                args.RemoveAt(0);

                if(args.Count > 0)
                {
                    Console.WriteLine(arg0, args.ToArray());
                }
                else
                {
                    Console.WriteLine(arg0);
                }

                return new NullValue();
            }), true);

            return langEnvironment;
        }


        public LangEnvironment? parent;
        public Dictionary<string, RuntimeValue> variables;

        public LangEnvironment(LangEnvironment parent)
        {
            this.parent = parent;
            this.variables = new Dictionary<string, RuntimeValue>();
        }

        public LangEnvironment(bool registerGlobals)
        {
            if (registerGlobals)
            {
                this.parent = parent ?? CreateGlobalEnvironment();
            }
            this.variables = new Dictionary<string, RuntimeValue>();
        }

        public RuntimeValue DeclareVariable(string varname, RuntimeValue value, bool isConstant) 
        {
            if (variables.ContainsKey(varname))
            {
                throw new RuntimeException($"Cannot declare already defined variable: '{varname}'.");
            }
            value.IsConstant = isConstant;
            variables[varname] = value;
            return value;
        }

        public RuntimeValue AssignVar(string varname, RuntimeValue value)
        {
            LangEnvironment env = resolve(varname);
            if (env.variables[varname].IsConstant)
            {
                throw new RuntimeException($"Cannot set constant variable: '{varname}'.");
            }

            env.variables[varname] = value;
            return value;
        }

        public RuntimeValue lookupVar(string varname)
        {
            LangEnvironment env = resolve(varname);
            return env.variables[varname];
        }

        public LangEnvironment resolve(string varname)
        {
            if (variables.ContainsKey(varname))
            {
                return this;
            }
            else if(parent != null)
            {
                return parent.resolve(varname);
            }
            else
            {
                throw new RuntimeException($"Cannot find undefined variable: '{varname}'.");
            }
        }
    }
}
