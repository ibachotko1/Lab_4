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
            { "¬", (4, OperatorType.Unary) }, { "!", (4, OperatorType.Unary) },
            { "∧", (3, OperatorType.Binary) }, { "&", (3, OperatorType.Binary) },
            { "∨", (2, OperatorType.Binary) }, { "|", (2, OperatorType.Binary) },
            { "^", (2, OperatorType.Binary) },
            { "→", (1, OperatorType.Binary) }, { "->", (1, OperatorType.Binary) },
            { "↔", (1, OperatorType.Binary) }, { "=", (1, OperatorType.Binary) }
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

                    // Проверяем многосимвольные операторы (например, "->")
                    if (i < formula.Length - 1)
                    {
                        string twoCharOp = formula.Substring(i, 2);
                        if (Operators.ContainsKey(twoCharOp) || twoCharOp == "->")
                        {
                            // Для "->" заменяем на "→" для единообразия
                            tokens.Add(twoCharOp == "->" ? "→" : twoCharOp);
                            i++; // Пропускаем второй символ
                            continue;
                        }
                    }

                    // Одиночный символ
                    tokens.Add(c.ToString());
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
            // Этот метод требует более сложной реализации с построением AST
            // Для простоты пока возвращаем исходную формулу
            // Полная реализация потребовала бы построения дерева выражений и его преобразования
            return formula;
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
                case "!":
                    // Унарное отрицание
                    if (stack.Count < 1)
                    {
                        throw new FormulaParseException("Недостаточно операндов для унарной операции.");
                    }
                    return !stack.Pop();

                case "∧":
                case "&":
                    // Логическое И (конъюнкция)
                    if (stack.Count < 2)
                    {
                        throw new FormulaParseException("Недостаточно операндов для конъюнкции.");
                    }
                    bool op2And = stack.Pop();
                    bool op1And = stack.Pop();
                    return op1And && op2And;

                case "∨":
                case "|":
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
                tokens.Add(buffer.ToString());
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
    }
}