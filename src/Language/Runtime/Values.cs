using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLanguage.Language.Language;

namespace TestLanguage.Language.Runtime
{
    public class RuntimeValue
    {
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public RuntimeType Type { get; set; }
        public bool IsConstant { get; set; }

        public RuntimeValue(RuntimeType type, bool constant = false)
        {
            Type = type;
            IsConstant = constant;
        }

        public override string ToString()
        {
            return to_string();
        }

        public virtual string to_string()
        {
            return Enum.GetName<RuntimeType>(Type) ?? "<NULLTYPE>";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings());
        }
    }

    public class NullValue : RuntimeValue
    {
        public NullValue() : base(RuntimeType.Null)
        {

        }

        public override string to_string()
        {
            return "undefined";
        }
    }

    public class StringValue : RuntimeValue
    {
        public string Value { get; set; }
        public StringValue(string value) : base(RuntimeType.String)
        {
            Value = value;
        }

        public override string to_string()
        {
            return Value;
        }
    }

    public class BooleanValue : RuntimeValue
    {
        public Boolean? Value { get; set; }
        public BooleanValue(bool? value = null) : base(RuntimeType.Boolean)
        {
            Value = value;
        }

        public override string to_string()
        {
            if (Value.HasValue)
            {
                return Value.Value.ToString();
            }
            return "undefined";
        }
    }

    public class NumberValue : RuntimeValue
    {
        public decimal? Value { get; set; }
        public NumberValue(decimal? value) : base(RuntimeType.Number)
        {
            Value = value;
        }

        public override string to_string()
        {
            if (Value.HasValue)
            {
                return Value.Value.ToString();
            }
            return "undefined";
        }
    }

    public class ObjectValue : RuntimeValue
    {
        public Dictionary<string, RuntimeValue> Properties { get; set; }
        public ObjectValue(Dictionary<string, RuntimeValue> props) : base(RuntimeType.Object)
        {
            Properties = props;
        }

        public override string to_string()
        {
            return base.ToJson();
        }
    }

    public delegate RuntimeValue? FunctionCall(List<RuntimeValue> args, LangEnvironment env);

    public class NativeFuncValue : RuntimeValue
    {
        [JsonIgnore]
        public FunctionCall Call { get; set; }
        public NativeFuncValue(FunctionCall call) : base(RuntimeType.func_native)
        {
            Call = call;
        }

        public override string to_string()
        {
            return base.ToJson();
        }
    }

    public class FuncValue : RuntimeValue
    {
        public string Name { get; set; }
        public List<string> Paramaters { get; set; }
        public List<Stmt> Body { get; set; }
        [JsonIgnore]
        public LangEnvironment Env { get; set; }

        public FuncValue(string name, List<string> paramaters, List<Stmt> body, LangEnvironment env) : base(RuntimeType.func)
        {
            Name = name;
            Paramaters = paramaters;
            Body = body;
            Env = env;
        }

        public override string to_string()
        {
            return base.ToJson();
        }
    }


    public enum RuntimeType
    {
        Null,
        Number,
        Boolean,
        String,
        Object,
        func_native,
        func,
    }
}
