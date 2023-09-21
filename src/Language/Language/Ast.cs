using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace TestLanguage.Language.Language
{
    public class Stmt
    {
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NodeType Kind { get; set; }

        public Stmt(NodeType kind)
        {
            Kind = kind;
        }
    }

    public class LangProgram : Stmt
    {
        public List<Stmt> Body { get; set; }

        public LangProgram() : base(NodeType.Program)
        {
            Body = new List<Stmt>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class VarDeclaration : Stmt
    {
        public bool IsConstant { get; set; }
        public string Identifier { get; set; }
        public Expr? Value { get; set; }
        public VarDeclaration(string identifier, Expr? value, bool constant) : base(NodeType.VarDeclStmt)
        {
            Identifier = identifier;
            Value = value;
            IsConstant = constant;
        }
    }

    public class FuncDeclaration : Stmt
    {
        public string Name { get; set; }
        public List<string> Paramaters { get; set; }
        public List<Stmt> Body { get; set; }
        public FuncDeclaration(string name, List<string> paramaters, List<Stmt> body) : base(NodeType.FunctionDeclStmt)
        {
            Name = name;
            Paramaters = paramaters;
            Body = body;
        }
    }

    public class Expr : Stmt
    {
        public Expr(NodeType kind) : base(kind) { }
    }

    public class VarAssignment : Expr
    {
        public Expr Assignee { get; set; }
        public Expr? Value { get; set; }
        public VarAssignment(Expr identifier, Expr? value) : base(NodeType.VarAssignStmt)
        {
            Assignee = identifier;
            Value = value;
        }
    }

    public class BinaryExpr : Expr
    {
        public Expr Left { get; set; }
        public string Operator { get; set; }
        public Expr Right { get; set; }

        public BinaryExpr(Expr left, string _operator, Expr right) : base(NodeType.BinaryExpr)
        {
            Left = left;
            Operator = _operator;
            Right = right;
        }
    }

    public class CallExpr : Expr
    {
        public List<Expr> Args { get; set; }
        public Expr Call { get; set; }

        public CallExpr(Expr call, List<Expr> args) : base(NodeType.CallExpr)
        {
            Call = call;
            Args = args;
        }
    }

    public class MemberExpr : Expr
    {
        public Expr Object { get; set; }
        public Expr Property { get; set; }
        public bool Computed { get; set; }

        public MemberExpr(Expr obj, Expr prop, bool computed) : base(NodeType.MemberExpr)
        {
            Object = obj;
            Property = prop;
            Computed = computed;
        }
    }

    public class IdentifierExpr : Expr
    {
        public string? Symbol { get; set; }
        public IdentifierExpr(string? symbol) : base(NodeType.Identifier)
        {
            Symbol = symbol;
        }
    }

    public class NumberLiteralExpr : Expr
    {
        public decimal? Value { get; set; }
        public NumberLiteralExpr(decimal? value) : base(NodeType.NumericLiteral)
        {
            Value = value;
        }
    }

    public class NullLiteralExpr : Expr
    {
        public object? Value { get; set; }
        public NullLiteralExpr() : base(NodeType.NullLiteral)
        {
            Value = null;
        }
    }

    public class StringLiteralExpr : Expr
    {
        public string Value { get; set; }
        public StringLiteralExpr(string value) : base(NodeType.StringLiteral)
        {
            Value = value;
        }
    }

    public class Property : Expr
    {
        public string Key { get; set; }
        public Expr? Value { get; set; }
        public Property(string key, Expr? value = null) : base(NodeType.Property)
        {
            Key = key;
            Value = value;
        }
    }

    public class ObjectLiteralExpr : Expr
    {
        public List<Property> Properties { get; set; } 
        public ObjectLiteralExpr(List<Property> properties) : base(NodeType.ObjectLiteral)
        {
            Properties = properties;
        }
    }

    public enum NodeType
    {
        //Stmt
        Program,
        VarDeclStmt,
        FunctionDeclStmt,

        // Expr
        VarAssignStmt,
        MemberExpr,
        CallExpr,

        // Literal
        Property,
        ObjectLiteral,
        NumericLiteral,
        NullLiteral,
        StringLiteral,
        Identifier,
        BinaryExpr,
    }
}
