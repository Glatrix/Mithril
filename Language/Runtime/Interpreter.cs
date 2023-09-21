using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using TestLanguage.Language.Language;
using TestLanguage.Language.Manage;

namespace TestLanguage.Language.Runtime
{
    public class Interpreter
    {
        public static RuntimeValue eval_program(LangProgram program, LangEnvironment env)
        {
            RuntimeValue lastEvaluated = new NullValue();

            foreach(Stmt statement in program.Body)
            {
                lastEvaluated = evaluate(statement, env);
            }

            return lastEvaluated;
        }

        public static NumberValue eval_numeric_binary_expr(NumberValue lhs, NumberValue rhs, string op)
        {
            if (op == "+")
            { 
                return new NumberValue(lhs.Value.Value + rhs.Value.Value); 
            }
            else if (op == "-")
            { 
                return new NumberValue(lhs.Value.Value - rhs.Value.Value); 
            }
            else if (op == "*") 
            { 
                return new NumberValue(lhs.Value.Value * rhs.Value.Value); 
            }
            else if (op == "/") 
            { 
                return new NumberValue(lhs.Value.Value / rhs.Value.Value); 
            }
            else if(op == "%")
            { 
                return new NumberValue(lhs.Value.Value % rhs.Value.Value); 
            }
            else if(op == "^")
            {
                double l_value = (double)lhs.Value.Value;
                double r_value = (double)rhs.Value.Value;

                return new NumberValue((decimal)Math.Pow(l_value, r_value));
            }
            else
            {
                return new NumberValue(0);
            }
        }


        public static RuntimeValue evaluate(Stmt stmt, LangEnvironment env)
        {
            if (stmt.Kind == NodeType.Program)
            {
                return eval_program(stmt as LangProgram, env);
            }
            else if(stmt.Kind == NodeType.VarDeclStmt)
            {
                return eval_var_declaration(stmt as VarDeclaration, env);
            }
            else if (stmt.Kind == NodeType.FunctionDeclStmt)
            {
                return eval_func_declatation(stmt as FuncDeclaration, env);
            }
            else if (stmt.Kind == NodeType.VarAssignStmt)
            {
                return eval_var_assignment(stmt as VarAssignment, env);
            }
            else if (stmt.Kind == NodeType.BinaryExpr)
            {
                return eval_binary_expr(stmt as BinaryExpr, env);
            }
            else if (stmt.Kind == NodeType.NumericLiteral)
            {
                return new NumberValue((stmt as NumberLiteralExpr)?.Value);
            }
            else if(stmt.Kind == NodeType.NullLiteral)
            {
                return new NullValue();
            }
            else if(stmt.Kind == NodeType.StringLiteral)
            {
                return new StringValue((stmt as StringLiteralExpr).Value);
            }
            else if(stmt.Kind == NodeType.Identifier)
            {
                return eval_identifier(stmt as IdentifierExpr, env);
            }
            else if (stmt.Kind == NodeType.ObjectLiteral)
            {
                return eval_object_expr(stmt as ObjectLiteralExpr, env);
            }
            else if (stmt.Kind == NodeType.CallExpr)
            {
                return eval_call_expr(stmt as CallExpr, env);
            }
            else if (stmt.Kind == NodeType.MemberExpr)
            {
                return eval_member_expr(stmt as MemberExpr, env);
            }
            else
            {
                throw new RuntimeException($"Node Type Not Implimented: {stmt.Kind}");
            }
        }

        private static RuntimeValue eval_func_declatation(FuncDeclaration stmt, LangEnvironment env)
        {
            var func = new FuncValue(stmt.Name, stmt.Paramaters, stmt.Body, env);
            return env.DeclareVariable(stmt.Name, func, true);
        }

        public static RuntimeValue eval_member_expr(MemberExpr mexpr, LangEnvironment env)
        {
            RuntimeValue? ret = null;

            List<string> idents = new List<string>();
            
            while(mexpr.Object is MemberExpr)
            {
                if(mexpr.Property is not null)
                {
                    idents.Add((mexpr.Property as IdentifierExpr).Symbol);
                }
                mexpr = (MemberExpr)mexpr.Object;
            }

            idents.Add((mexpr.Property as IdentifierExpr).Symbol);
            idents.Add((mexpr.Object as IdentifierExpr).Symbol);

            idents.Reverse();

            foreach(var ident in idents)
            {
                if(ret is null)
                {
                    ret = env.lookupVar(ident);
                }
                else if(ret is ObjectValue)
                {
                    ret = (ret as ObjectValue).Properties[ident];
                }
                else
                {
                    throw new RuntimeException("Left side of Member Expression must be an object");
                }
            }


            return ret;
        }


        public static RuntimeValue eval_binary_expr(BinaryExpr binop, LangEnvironment env)
        {
            RuntimeValue lhs = evaluate(binop.Left, env);
            RuntimeValue rhs = evaluate(binop.Right, env);

            if (lhs.Type == RuntimeType.Number && rhs.Type == RuntimeType.Number)
            {
                return eval_numeric_binary_expr(lhs as NumberValue, rhs as NumberValue, binop.Operator);
            }

            return new NullValue();
        }

        private static RuntimeValue eval_var_assignment(VarAssignment? varAssignment, LangEnvironment env)
        {
            if(varAssignment.Assignee.Kind == NodeType.MemberExpr)
            {
                MemberExpr mexpr = (MemberExpr)varAssignment.Assignee;

                RuntimeValue? ret = null;

                List<string> idents = new List<string>();

                while (mexpr.Object is MemberExpr)
                {
                    if (mexpr.Property is not null)
                    {
                        idents.Add((mexpr.Property as IdentifierExpr).Symbol);
                    }
                    mexpr = (MemberExpr)mexpr.Object;
                }

                idents.Add((mexpr.Property as IdentifierExpr).Symbol);
                idents.Add((mexpr.Object as IdentifierExpr).Symbol);

                idents.Reverse();

                for (int i = 0; i < idents.Count; i++)
                {
                    string? ident = idents[i];

                    if (ret is null)
                    {
                        ret = env.lookupVar(ident);
                    }
                    else if (i == idents.Count - 1)
                    {
                        RuntimeValue val = evaluate(varAssignment.Value, env);
                        (ret as ObjectValue).Properties[ident] = val;
                        return val;
                    }
                    else if (ret is ObjectValue)
                    {
                        ret = (ret as ObjectValue).Properties[ident];
                    }
                    else
                    {
                        throw new RuntimeException("Left side of Member Expression must be an object");
                    }
                }
            }
            string varname = (varAssignment.Assignee as IdentifierExpr)?.Symbol ?? "undefined";
            return env.AssignVar(varname, evaluate(varAssignment.Value, env));
        }

        private static RuntimeValue eval_var_declaration(VarDeclaration varDecl, LangEnvironment env)
        {
            return varDecl.Value == null
                ? new NullValue()
                : env.DeclareVariable(varDecl.Identifier, evaluate(varDecl.Value, env), varDecl.IsConstant);
        }

        private static RuntimeValue eval_identifier(IdentifierExpr identifier, LangEnvironment env)
        {
            RuntimeValue val = env.lookupVar(identifier.Symbol);
            return val;
        }

        private static RuntimeValue eval_object_expr(ObjectLiteralExpr obj, LangEnvironment env)
        {
            ObjectValue runtime_obj = new ObjectValue(new Dictionary<string, RuntimeValue>());
            
            foreach(var prop in obj.Properties)
            {
                var runtimeVal = (prop.Value == null) ? env.lookupVar(prop.Key) : evaluate(prop.Value, env);

                runtime_obj.Properties.Add(prop.Key, runtimeVal);
            }

            return runtime_obj;
        }

        private static RuntimeValue? eval_call_expr(CallExpr expr, LangEnvironment env)
        {
            List<RuntimeValue> args = expr.Args.Select((a) => evaluate(a, env)).ToList();
            RuntimeValue fn = evaluate(expr.Call, env);

            if(fn.Type == RuntimeType.func_native)
            {
                FunctionCall callable = (fn as NativeFuncValue).Call;
                return callable(args, env);
            }
            else if (fn.Type == RuntimeType.func)
            {
                var func = fn as FuncValue;
                var scope = new LangEnvironment(func.Env);

                // Create Variables for paramaters list

                for(var i = 0; i < func.Paramaters.Count; i++)
                {
                    string varname = func.Paramaters[i];
                    scope.DeclareVariable(varname, args[i], false);
                }

                return evaluate(new LangProgram() { Body = func.Body } , scope);
            }

            return new NullValue();
        }
    }
}
