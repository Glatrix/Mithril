using System.Net.Http.Headers;
using TestLanguage.Language.Manage;

namespace TestLanguage.Language.Language
{
    public class Parser
    {
        public List<Token> Tokens { get; set; }

        private bool Not_Eof()
        {
            return Tokens[0].type != TokenType.EOF;
        }

        private Token at()
        {
            return Tokens[0];
        }

        private Token eat()
        {
            Token prev = at();
            Tokens.RemoveAt(0);
            return prev;
        }

        private Token expect(TokenType type, string err)
        {
            Token prev = eat();
            if (prev.type != type)
            {
                throw new ParserException(err);
            }
            return prev;
        }

        public LangProgram Parse(List<Token> tokens)
        {
            Tokens = tokens;

            LangProgram program = new LangProgram();

            while (Not_Eof())
            {
                program.Body.Add(parse_stmt());
            }

            return program;
        }

        //Orders Of Presidence
        // Assignment Expr
        // MemberExpr
        // FunctionCall
        // Logical Expr
        // Comparison
        // Additive Expr
        // Multiplicative Expr
        // Unary Expr
        // Primary Expr
        private Stmt parse_stmt()
        {
            if (at().type == TokenType.Var || at().type == TokenType.Const)
            {
                return parse_var_declaration();
            }
            else if(at().type == TokenType.Func)
            {
                return parse_func_declaration();
            }
            else
            {
                return parse_expr();
            }
        }

        private FuncDeclaration parse_func_declaration()
        {
            eat(); // eat func token

            string? name = expect(TokenType.Identifier, "Expected Function Name after func keyword").value;

            var paramaters = new List<string>();
            foreach(Expr a in parse_args())
            {
                if(a.Kind != NodeType.Identifier)
                {
                    throw new Exception("Function paramaters must be an identifier");
                }
                paramaters.Add((a as IdentifierExpr).Symbol);
            }

            expect(TokenType.OpenBracket, "Expected { following start of function declaration");

            List<Stmt> body = new List<Stmt>();

            while(at().type != TokenType.EOF && at().type != TokenType.CloseBracket)
            {
                body.Add(parse_stmt());
            }

            expect(TokenType.CloseBracket, "Expected } at end of function declaration");

            return new FuncDeclaration(name, paramaters, body);
        }


        private VarDeclaration parse_var_declaration()
        {
            bool isConstant = eat().type == TokenType.Const;
            string identifier = expect(TokenType.Identifier, "Expected IdentifierExpr name following Let or Const keywords").value;

            if (at().type == TokenType.Semicolon)
            {
                eat(); // expect semicolon
                if (isConstant)
                {
                    throw new ParserException("Must assign value to constant expression");
                }
                return new VarDeclaration(identifier, new NullLiteralExpr(), isConstant);
            }

            expect(TokenType.Equals, "Expected '=' after variable declaration");

            Expr expression = parse_expr();

            if (at().type == TokenType.Semicolon)
            {
                eat(); // eat ;
            }

            return new VarDeclaration(identifier, expression, isConstant);
        }

        private Expr parse_expr()
        {
            return parse_assigment_expr();
        }

        private Expr parse_assigment_expr()
        {
            Expr left = parse_object_expr();

            if (at().type == TokenType.Equals)
            {
                eat();
                Expr value = parse_assigment_expr();
                return new VarAssignment(left, value);
            }

            return left;
        }

        public Expr parse_object_expr()
        {
            if(this.at().type != TokenType.OpenBracket)
            {
                return parse_additive_expr();
            }

            eat(); // eat {

            List<Property> props = new List<Property>();

            while (Not_Eof() && at().type != TokenType.CloseBracket)
            {
                // { key: val, }
                // { key }

                string key = expect(TokenType.Identifier, "Object Literal key expected").value;
                if(at().type == TokenType.Comma)
                {
                    eat(); // eat comma 
                    props.Add(new Property(key, null));
                    continue;
                }
                else if (at().type == TokenType.CloseBracket)
                {
                    props.Add(new Property(key, null));
                    continue;
                }

                this.expect(TokenType.Colon, "Expected colon following identifier in Object Literal Expression");
                Expr value = parse_expr();
                props.Add(new Property(key, value));

                if(at().type != TokenType.CloseBracket)
                {
                    expect(TokenType.Comma, "Expected comma or closing bracket following property");
                }
            }

            this.expect(TokenType.CloseBracket, "Object Literal missing closing brace");
            return new ObjectLiteralExpr(props);
        }

        public Expr parse_additive_expr()
        {
            Expr left = parse_multiplicative_expr();

            while (at().value == "+" || at().value == "-")
            {
                string op = eat().value ?? "?";
                Expr right = parse_multiplicative_expr();

                left = new BinaryExpr(left, op, right);
            }

            return left;
        }

        public Expr parse_multiplicative_expr()
        {
            Expr left = parse_call_member_expr();

            while (at().value == "/" || at().value == "*" || at().value == "%" || at().value == "^")
            {
                string op = eat().value ?? "?";
                Expr right = parse_call_member_expr();

                left = new BinaryExpr(left, op, right);
            }

            return left;
        }

        private Expr parse_call_member_expr()
        {
            Expr member = parse_member_expr();

            if (at().type == TokenType.OpenParen)
            {
                return parse_call_expr(member);
            }

            return member;
        }

        private Expr parse_call_expr(Expr caller)
        {
            Expr call_expr = new CallExpr(caller, parse_args());

            if(at().type == TokenType.OpenParen)
            {
                call_expr = parse_call_expr(call_expr);
            }

            return call_expr;
        }

        private List<Expr> parse_args()
        {
            expect(TokenType.OpenParen, "Expected Open Paren");
            
            if(at().type == TokenType.CloseParen)
            {
                eat();
                return new List<Expr>();
            }
            else
            {
                List<Expr> args = parse_argument_list();
                expect(TokenType.CloseParen, "Expected Closing paren after paramaters");
                return args;
            }
        }

        private List<Expr> parse_argument_list()
        {
            List<Expr> args = new List<Expr> { parse_assigment_expr() };

            while (at().type == TokenType.Comma)
            {
                eat(); // eat comma
                args.Add(parse_assigment_expr());
            }

            return args;
        }

        private Expr parse_member_expr()
        {
            Expr obj = parse_primary_expr();

            while(at().type == TokenType.DOT || at().type == TokenType.OpenSquareBracket)
            {
                Token op_tok = eat();

                Expr property;
                bool computed;

                if(op_tok.type == TokenType.DOT)
                {
                    computed = false;
                    property = parse_primary_expr();

                    if(property.Kind != NodeType.Identifier)
                    {
                        throw new ParserException("Cannot use dot operator without right hand side being and identifer");
                    }
                }
                else
                {
                    computed = true;
                    property = parse_expr();
                    expect(TokenType.CloseSquareBracket, "Missing closing square bracket in computed value.");
                }

                obj = new MemberExpr(obj, property, computed);
            }
            return obj;
        }

        private Expr parse_primary_expr()
        {
            TokenType tk = at().type;

            if (tk == TokenType.Identifier)
            {
                return new IdentifierExpr(eat().value);
            }
            else if (tk == TokenType.Null)
            {
                eat();
                return new NullLiteralExpr();
            }
            else if (tk == TokenType.StringLiteral)
            {
                return new StringLiteralExpr(eat().value ?? "");
            }
            else if (tk == TokenType.NumberLiteral)
            {
                return new NumberLiteralExpr(decimal.Parse(eat().value ?? "0"));
            }
            else if (tk == TokenType.OpenParen)
            {
                eat(); // (
                Expr value = parse_expr();
                expect(TokenType.CloseParen, "Close Paren Expected");
                return value;
            }
            else if(tk == TokenType.Semicolon)
            {
                eat();
                return parse_primary_expr();
            }
            else
            {
                throw new ParserException($"Unexpected Token: {tk} -> {at().value}");
            }
        }
    }
}
