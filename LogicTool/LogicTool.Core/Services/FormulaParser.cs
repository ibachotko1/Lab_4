using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogicTool.Core.Exceptions;
using LogicTool.Core.Enums;
using LogicTool.Core.Models;

namespace LogicTool.Core.Services
{
    /// <summary>
    /// Парсер логических формул. Выполняет лексический анализ, преобразование в обратную польскую запись
    /// и вычисление значений формулы для заданных значений переменных.
    /// </summary>
    public class FormulaParser
    {
        /// <summary>
        /// Словарь операторов с их приоритетами и типами.
        /// Приоритет: чем выше число, тем выше приоритет операции.
        /// </summary>
        public static readonly Dictionary<string, (int precedence, OperatorType type)> Operators =
            new Dictionary<string, (int precedence, OperatorType type)>
        {
            { "¬", (4, OperatorType.Unary) },
            { "∧", (3, OperatorType.Binary) },
            { "∨", (2, OperatorType.Binary) },
            { "^", (2, OperatorType.Binary) },
            { "→", (1, OperatorType.Binary) },
            { "↔", (1, OperatorType.Binary) }
        };

        private static readonly Dictionary<string, string> TwoCharOperatorMap =
            new Dictionary<string, string>
        {
            { "->", "→" },
            { "=>", "↔" }
        };

        private static readonly Dictionary<string, string> SingleCharOperatorMap =
            new Dictionary<string, string>
        {
            { "!", "¬" },
            { "&", "∧" },
            { "|", "∨" },
            { "=", "↔" }
        };

        private static readonly Dictionary<string, string> KeywordOperatorMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "not", "¬" },
            { "and", "∧" },
            { "or", "∨" },
            { "xor", "^" },
            { "impl", "→" },
            { "then", "→" },
            { "equ", "↔" },
            { "eq", "↔" },
            { "iff", "↔" }
        };

        /// <summary>
        /// Выполняет лексический анализ формулы - разбивает строку на токены.
        /// </summary>
        /// <param name="formula">Строка с логической формулой</param>
        /// <returns>Список токенов</returns>
        /// <exception cref="FormulaParseException">Выбрасывается при ошибках лексического анализа</exception>
        /// <example>
        /// Формула "x1 ∧ (x2 ∨ ¬x3)" будет разбита на токены: ["x1", "∧", "(", "x2", "∨", "¬", "x3", ")"]
        /// </example>
        public List<string> Tokenize(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                throw new FormulaParseException("Формула не может быть пустой.");
            }

            var tokens = new List<string>();
            var buffer = new StringBuilder();

            // Проходим по каждому символу формулы
            for (int i = 0; i < formula.Length; i++)
            {
                char c = formula[i];

                // Пробельные символы разделяют токены
                if (char.IsWhiteSpace(c))
                {
                    FlushBuffer(buffer, tokens);
                    continue;
                }

                // Буквы и цифры накапливаем в буфер (для переменных и констант)
                if (char.IsLetterOrDigit(c))
                {
                    buffer.Append(c);
                }
                else
                {
                    // Не буквенно-цифровой символ - разделитель
                    FlushBuffer(buffer, tokens);

                    if (i < formula.Length - 1)
                    {
                        string twoCharOp = formula.Substring(i, 2);
                        if (TwoCharOperatorMap.TryGetValue(twoCharOp, out var mappedTwoChar))
                        {
                            tokens.Add(mappedTwoChar);
                            i++;
                            continue;
                        }
                    }

                    string symbol = c.ToString();
                    tokens.Add(SingleCharOperatorMap.TryGetValue(symbol, out var mappedSingle)
                        ? mappedSingle
                        : symbol);
                }
            }

            // Обрабатываем остаток в буфере
            FlushBuffer(buffer, tokens);

            // Проверяем баланс скобок
            ValidateBrackets(tokens);

            return tokens;
        }

        /// <summary>
        /// Преобразует список токенов в обратную польскую запись (ОПЗ) с использованием алгоритма сортировочной станции.
        /// </summary>
        /// <param name="tokens">Список токенов в инфиксной нотации</param>
        /// <returns>Токены в обратной польской записи</returns>
        /// <exception cref="FormulaParseException">Выбрасывается при ошибках в структуре формулы</exception>
        /// <remarks>
        /// Алгоритм Дейкстры (сортировочная станция):
        /// 1. Читаем токены слева направо
        /// 2. Операнды добавляем в выходную очередь
        /// 3. Операторы и скобки обрабатываем согласно приоритетам
        /// 4. В конце выталкиваем все операторы из стека в выход
        /// </remarks>
        public List<string> ToRPN(List<string> tokens)
        {
            var output = new List<string>();
            var stack = new Stack<string>();

            foreach (string token in tokens)
            {
                if (IsVariable(token) || token == "0" || token == "1")
                {
                    // Переменные и константы сразу в выход
                    output.Add(token);
                }
                else if (token == "(")
                {
                    // Открывающая скобка - в стек
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    // Закрывающая скобка: выталкиваем из стека в выход до открывающей
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }

                    if (stack.Count == 0)
                    {
                        throw new FormulaParseException("Несбалансированные скобки: отсутствует открывающая скобка.");
                    }

                    // Удаляем открывающую скобку из стека
                    stack.Pop();
                }
                else if (Operators.ContainsKey(token))
                {
                    // Оператор: выталкиваем из стека операторы с большим или равным приоритетом
                    while (stack.Count > 0 && stack.Peek() != "(" &&
                           Operators.ContainsKey(stack.Peek()) &&
                           Operators[stack.Peek()].precedence >= Operators[token].precedence)
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
                else
                {
                    throw new FormulaParseException($"Неизвестный токен: {token}");
                }
            }

            // Выталкиваем оставшиеся операторы из стека
            while (stack.Count > 0)
            {
                if (stack.Peek() == "(")
                {
                    throw new FormulaParseException("Несбалансированные скобки: отсутствует закрывающая скобка.");
                }
                output.Add(stack.Pop());
            }

            return output;
        }

        /// <summary>
        /// Вычисляет значение формулы в обратной польской записи для заданных значений переменных.
        /// </summary>
        /// <param name="rpn">Формула в обратной польской записи</param>
        /// <param name="variables">Словарь значений переменных</param>
        /// <returns>Результат вычисления (true/false)</returns>
        /// <exception cref="FormulaParseException">Выбрасывается при ошибках вычисления</exception>
        /// <remarks>
        /// Алгоритм вычисления ОПЗ:
        /// 1. Читаем токены слева направо
        /// 2. Если токен - операнд, помещаем его значение в стек
        /// 3. Если токен - оператор, извлекаем нужное количество операндов из стека,
        ///    применяем оператор и результат помещаем в стек
        /// 4. В конце в стеке должен остаться один элемент - результат вычисления
        /// </remarks>
        public bool EvaluateRPN(List<string> rpn, Dictionary<string, bool> variables)
        {
            var stack = new Stack<bool>();

            foreach (string token in rpn)
            {
                if (IsVariable(token))
                {
                    // Переменная - берем значение из словаря
                    if (!variables.ContainsKey(token))
                    {
                        throw new FormulaParseException($"Неизвестная переменная: {token}");
                    }
                    stack.Push(variables[token]);
                }
                else if (token == "0")
                {
                    stack.Push(false);
                }
                else if (token == "1")
                {
                    stack.Push(true);
                }
                else
                {
                    // Оператор - вычисляем
                    bool result = EvaluateOperator(token, stack);
                    stack.Push(result);
                }
            }

            if (stack.Count != 1)
            {
                throw new FormulaParseException("Некорректное выражение: в стеке осталось не одно значение.");
            }

            return stack.Pop();
        }

        private ExpressionNode BuildExpressionTree(List<string> rpn)
        {
            var stack = new Stack<ExpressionNode>();

            foreach (var token in rpn)
            {
                if (IsVariable(token))
                {
                    stack.Push(new VariableNode(token));
                }
                else if (token == "0" || token == "1")
                {
                    stack.Push(new ConstantNode(token == "1"));
                }
                else if (Operators.TryGetValue(token, out var meta))
                {
                    if (meta.type == OperatorType.Unary)
                    {
                        if (stack.Count < 1)
                        {
                            throw new FormulaParseException("Недостаточно операндов для унарного оператора.");
                        }
                        var operand = stack.Pop();
                        stack.Push(new UnaryNode(operand));
                    }
                    else
                    {
                        if (stack.Count < 2)
                        {
                            throw new FormulaParseException("Недостаточно операндов для бинарного оператора.");
                        }
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(new BinaryNode(token, left, right));
                    }
                }
                else
                {
                    throw new FormulaParseException($"Неизвестный токен: {token}");
                }
            }

            if (stack.Count != 1)
            {
                throw new FormulaParseException("Не удалось построить дерево выражения.");
            }

            return stack.Pop();
        }

        private ExpressionNode RewriteToBasic(ExpressionNode node)
        {
            switch (node)
            {
                case UnaryNode unary:
                    return new UnaryNode(RewriteToBasic(unary.Operand));
                case BinaryNode binary:
                    var left = RewriteToBasic(binary.Left);
                    var right = RewriteToBasic(binary.Right);
                    switch (binary.Operator)
                    {
                        case "^":
                            return new BinaryNode("∨",
                                new BinaryNode("∧", left, Not(right)),
                                new BinaryNode("∧", Not(left), right));
                        case "→":
                            return new BinaryNode("∨", Not(left), right);
                        case "↔":
                            return new BinaryNode("∨",
                                new BinaryNode("∧", left, right),
                                new BinaryNode("∧", Not(left), Not(right)));
                        default:
                            return new BinaryNode(binary.Operator, left, right);
                    }
                default:
                    return node;
            }
        }

        private static UnaryNode Not(ExpressionNode operand) => new UnaryNode(operand);

        private string FormatExpression(ExpressionNode node)
        {
            switch (node)
            {
                case VariableNode variable:
                    return variable.Name;
                case ConstantNode constant:
                    return constant.Value ? "1" : "0";
                case UnaryNode unary:
                    var operand = FormatExpression(unary.Operand);
                    bool needBrackets = unary.Operand is BinaryNode;
                    return $"¬{(needBrackets ? $"({operand})" : operand)}";
                case BinaryNode binary:
                    var left = FormatExpression(binary.Left);
                    var right = FormatExpression(binary.Right);
                    return $"({left} {binary.Operator} {right})";
                default:
                    throw new InvalidOperationException("Неизвестный тип узла выражения.");
            }
        }

        /// <summary>
        /// Выполняет лексический анализ с возвратом типизированных токенов.
        /// </summary>
        /// <param name="formula">Строка с логической формулой</param>
        /// <returns>Список типизированных токенов с информацией о позиции</returns>
        public List<Token> TokenizeWithTypes(string formula)
        {
            var rawTokens = Tokenize(formula);
            var typedTokens = new List<Token>();

            for (int i = 0; i < rawTokens.Count; i++)
            {
                string token = rawTokens[i];
                TokenType tokenType = GetTokenType(token);
                typedTokens.Add(new Token(token, tokenType, i));
            }

            return typedTokens;
        }

        /// <summary>
        /// Преобразует формулу в базис {¬, ∧, ∨} заменой операторов на их эквиваленты.
        /// </summary>
        /// <param name="formula">Исходная формула</param>
        /// <returns>Формула в базисе {¬, ∧, ∨}</returns>
        /// <remarks>
        /// Выполняет следующие замены:
        /// - Импликация: A → B заменяется на ¬A ∨ B
        /// - Эквивалентность: A ↔ B заменяется на (A ∧ B) ∨ (¬A ∧ ¬B)
        /// - XOR: A ^ B заменяется на (A ∧ ¬B) ∨ (¬A ∧ B)
        /// </remarks>
        public string ToBasicBasis(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                throw new FormulaParseException("Формула не может быть пустой.");
            }

            var tokens = Tokenize(formula);
            var rpn = ToRPN(tokens);
            var tree = BuildExpressionTree(rpn);
            var basicTree = RewriteToBasic(tree);
            return FormatExpression(basicTree);
        }

        /// <summary>
        /// Обрабатывает оператор, вычисляя его значение на основе операндов из стека.
        /// </summary>
        /// <param name="token">Строковое представление оператора</param>
        /// <param name="stack">Стек операндов</param>
        /// <returns>Результат операции</returns>
        /// <exception cref="FormulaParseException">Выбрасывается при недостатке операндов или неизвестном операторе</exception>
        private bool EvaluateOperator(string token, Stack<bool> stack)
        {
            switch (token)
            {
                case "¬":
                    // Унарное отрицание
                    if (stack.Count < 1)
                    {
                        throw new FormulaParseException("Недостаточно операндов для унарной операции.");
                    }
                    return !stack.Pop();

                case "∧":
                    // Логическое И (конъюнкция)
                    if (stack.Count < 2)
                    {
                        throw new FormulaParseException("Недостаточно операндов для конъюнкции.");
                    }
                    bool op2And = stack.Pop();
                    bool op1And = stack.Pop();
                    return op1And && op2And;

                case "∨":
                    // Логическое ИЛИ (дизъюнкция)
                    if (stack.Count < 2)
                    {
                        throw new FormulaParseException("Недостаточно операндов для дизъюнкции.");
                    }
                    bool op2Or = stack.Pop();
                    bool op1Or = stack.Pop();
                    return op1Or || op2Or;

                case "^":
                    // Исключающее ИЛИ (XOR)
                    if (stack.Count < 2)
                    {
                        throw new FormulaParseException("Недостаточно операндов для XOR.");
                    }
                    bool op2Xor = stack.Pop();
                    bool op1Xor = stack.Pop();
                    return op1Xor ^ op2Xor;

                case "→":
                    // Импликация (A → B эквивалентно ¬A ∨ B)
                    if (stack.Count < 2)
                    {
                        throw new FormulaParseException("Недостаточно операндов для импликации.");
                    }
                    bool b = stack.Pop();
                    bool a = stack.Pop();
                    return !a || b;

                case "↔":
                    // Эквивалентность (A ↔ B эквивалентно A == B)
                    if (stack.Count < 2)
                    {
                        throw new FormulaParseException("Недостаточно операндов для эквивалентности.");
                    }
                    bool op2Eq = stack.Pop();
                    bool op1Eq = stack.Pop();
                    return op1Eq == op2Eq;

                default:
                    throw new FormulaParseException($"Неизвестный оператор: {token}");
            }
        }

        /// <summary>
        /// Определяет тип токена.
        /// </summary>
        /// <param name="token">Строковое представление токена</param>
        /// <returns>Тип токена</returns>
        /// <exception cref="FormulaParseException">Выбрасывается для неизвестных типов токенов</exception>
        private TokenType GetTokenType(string token)
        {
            if (token == "(")
            {
                return TokenType.LeftParenthesis;
            }
            if (token == ")")
            {
                return TokenType.RightParenthesis;
            }
            if (token == "0" || token == "1")
            {
                return TokenType.Constant;
            }
            if (Operators.ContainsKey(token))
            {
                return TokenType.Operator;
            }
            if (IsVariable(token))
            {
                return TokenType.Variable;
            }

            throw new FormulaParseException($"Неизвестный тип токена: {token}");
        }

        /// <summary>
        /// Проверяет, является ли токен переменной.
        /// </summary>
        /// <param name="token">Строковое представление токена</param>
        /// <returns>true, если токен представляет переменную, иначе false</returns>
        /// <remarks>
        /// Переменная должна начинаться с буквы и не быть оператором.
        /// Поддерживаются многосимвольные имена переменных (например, "x1", "var2").
        /// </remarks>
        private bool IsVariable(string token)
        {
            return token.Length > 0 &&
                   char.IsLetter(token[0]) &&
                   !Operators.ContainsKey(token);
        }

        /// <summary>
        /// Сбрасывает буфер в список токенов, если буфер не пуст.
        /// </summary>
        /// <param name="buffer">Буфер для накопления символов</param>
        /// <param name="tokens">Список токенов</param>
        private void FlushBuffer(StringBuilder buffer, List<string> tokens)
        {
            if (buffer.Length > 0)
            {
                string token = buffer.ToString();
                string lowered = token.ToLowerInvariant();
                if (KeywordOperatorMap.TryGetValue(lowered, out var mapped))
                {
                    tokens.Add(mapped);
                }
                else
                {
                    tokens.Add(token);
                }
                buffer.Clear();
            }
        }

        /// <summary>
        /// Проверяет баланс круглых скобок в списке токенов.
        /// </summary>
        /// <param name="tokens">Список токенов</param>
        /// <exception cref="FormulaParseException">Выбрасывается при несбалансированных скобках</exception>
        private void ValidateBrackets(List<string> tokens)
        {
            int balance = 0;

            foreach (string token in tokens)
            {
                if (token == "(")
                {
                    balance++;
                }
                else if (token == ")")
                {
                    balance--;

                    if (balance < 0)
                    {
                        throw new FormulaParseException("Несбалансированные скобки: лишняя закрывающая скобка.");
                    }
                }
            }

            if (balance > 0)
            {
                throw new FormulaParseException("Несбалансированные скобки: не хватает закрывающих скобок.");
            }
            if (balance < 0)
            {
                throw new FormulaParseException("Несбалансированные скобки: лишние закрывающие скобки.");
            }
        }

        private abstract class ExpressionNode { }

        private sealed class VariableNode : ExpressionNode
        {
            public VariableNode(string name) => Name = name;
            public string Name { get; }
        }

        private sealed class ConstantNode : ExpressionNode
        {
            public ConstantNode(bool value) => Value = value;
            public bool Value { get; }
        }

        private sealed class UnaryNode : ExpressionNode
        {
            public UnaryNode(ExpressionNode operand) => Operand = operand;
            public ExpressionNode Operand { get; }
        }

        private sealed class BinaryNode : ExpressionNode
        {
            public BinaryNode(string op, ExpressionNode left, ExpressionNode right)
            {
                Operator = op;
                Left = left;
                Right = right;
            }

            public string Operator { get; }
            public ExpressionNode Left { get; }
            public ExpressionNode Right { get; }
        }
    }
}