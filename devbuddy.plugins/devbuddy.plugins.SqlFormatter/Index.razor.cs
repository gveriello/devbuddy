using devbuddy.common.Applications;
using devbuddy.common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace devbuddy.plugins.SqlFormatter
{
    public partial class Index : AppComponentBase
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] private ToastService ToastService { get; set; }
        [Parameter]
        public string SqlQuery { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<string> SqlQueryChanged { get; set; }

        private string SqlToFormat = string.Empty;
        private string formattedSql = string.Empty;
        private bool copied = false;
        private string selectedDialect = "Standard";
        private List<string> validationErrors = [];
        private ElementReference textAreaRef;

        // Autocomplete
        private bool AnySuggestions => Suggestions.Count != 0;
        private List<Suggestion> Suggestions = [];
        private int selectedSuggestionIndex = 0;
        private int autocompleteTop = 0;
        private int autocompleteLeft = 0;
        private string currentWord = string.Empty;

        protected override void OnInitialized()
        {
            SqlToFormat = SqlQuery;
            UpdateFormattedSql();
        }

        protected override void OnParametersSet()
        {
            if (SqlToFormat != SqlQuery)
            {
                SqlToFormat = SqlQuery;
                UpdateFormattedSql();
            }
        }

        private void OnDialectChanged()
        {
            UpdateFormattedSql();
        }
        public void ClearInput()
        {
            SqlToFormat = string.Empty;
            SqlQuery = string.Empty;
            formattedSql = string.Empty;
        }

        private async Task OnSqlChanged()
        {
            UpdateFormattedSql();
            await SqlQueryChanged.InvokeAsync(SqlToFormat);
            UpdateSuggestions();
        }

        private void UpdateFormattedSql()
        {
            ValidateSql();
            formattedSql = FormatSql(SqlToFormat);
        }

        private void ValidateSql()
        {
            validationErrors.Clear();

            if (string.IsNullOrWhiteSpace(SqlToFormat))
                return;

            var sql = SqlToFormat.ToUpper();
            var dialect = GetDialect();

            // Controllo parentesi bilanciate
            var openParen = SqlToFormat.Count(c => c == '(');
            var closeParen = SqlToFormat.Count(c => c == ')');
            if (openParen != closeParen)
                validationErrors.Add($"Parentesi non bilanciate: {openParen} aperte, {closeParen} chiuse");

            // Controllo SELECT senza FROM (eccetto per alcuni dialetti)
            if (sql.Contains("SELECT") && !sql.Contains("FROM") && selectedDialect != "MySQL")
            {
                if (!sql.Contains("DUAL")) // Oracle permette SELECT senza FROM con DUAL
                    validationErrors.Add("SELECT senza FROM (potrebbe essere un errore)");
            }

            // Controllo virgole doppie
            if (SqlToFormat.Contains(",,"))
                validationErrors.Add("Virgole doppie trovate");

            // Controllo JOIN senza ON (eccetto CROSS JOIN)
            if (Regex.IsMatch(sql, @"\bJOIN\b(?!\s+ON)") && !sql.Contains("CROSS"))
            {
                if (!Regex.IsMatch(sql, @"\bJOIN\b\s+\w+\s+ON\b"))
                    validationErrors.Add("JOIN senza clausola ON");
            }

            // Controllo parole chiave mal scritte comuni
            var commonMisspellings = new Dictionary<string, string>
        {
            { @"\bSELCT\b", "SELECT" },
            { @"\bFROM\b.*\bWHERE\b.*\bFROM\b", "FROM duplicato" },
            { @"\bORDER\s+BT\b", "ORDER BY" },
            { @"\bGROUP\s+BT\b", "GROUP BY" }
        };

            foreach (var (pattern, correction) in commonMisspellings)
            {
                if (Regex.IsMatch(sql, pattern))
                    validationErrors.Add($"Possibile errore di battitura: {correction}");
            }

            // Validazioni specifiche per dialetto
            ValidateDialectSpecific(sql, dialect);
        }

        private void ValidateDialectSpecific(string sql, SqlDialect dialect)
        {
            switch (dialect.Name)
            {
                case "TSql":
                    if (sql.Contains("LIMIT"))
                        validationErrors.Add("T-SQL usa TOP invece di LIMIT");
                    break;

                case "MySQL":
                    if (sql.Contains("TOP"))
                        validationErrors.Add("MySQL usa LIMIT invece di TOP");
                    break;

                case "Oracle":
                    if (sql.Contains("LIMIT"))
                        validationErrors.Add("Oracle usa ROWNUM o FETCH FIRST invece di LIMIT");
                    break;
            }
        }

        private string FormatSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return string.Empty;

            var dialect = GetDialect();
            var formatted = sql.Trim();
            var indentLevel = 0;
            var result = new System.Text.StringBuilder();

            // Dividi per keyword principali
            var majorKeywords = new[] { "SELECT", "FROM", "WHERE", "JOIN", "INNER JOIN",
            "LEFT JOIN", "RIGHT JOIN", "ORDER BY", "GROUP BY", "HAVING" };

            var tokens = TokenizeSql(formatted);
            var currentLine = new System.Text.StringBuilder();

            foreach (var token in tokens)
            {
                var upper = token.ToUpper();

                if (majorKeywords.Any(k => upper.StartsWith(k)))
                {
                    if (currentLine.Length > 0)
                    {
                        result.AppendLine(ApplySyntaxHighlighting(currentLine.ToString(), dialect));
                        currentLine.Clear();
                    }
                    currentLine.Append(new string(' ', indentLevel * 4));
                }

                if (token == "(")
                {
                    currentLine.Append(token);
                    result.AppendLine(ApplySyntaxHighlighting(currentLine.ToString(), dialect));
                    currentLine.Clear();
                    indentLevel++;
                    currentLine.Append(new string(' ', indentLevel * 4));
                }
                else if (token == ")")
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                    if (currentLine.ToString().Trim().Length > 0)
                    {
                        result.AppendLine(ApplySyntaxHighlighting(currentLine.ToString(), dialect));
                        currentLine.Clear();
                    }
                    currentLine.Append(new string(' ', indentLevel * 4));
                    currentLine.Append(token);
                }
                else
                {
                    if (currentLine.Length > 0 && !token.StartsWith(","))
                        currentLine.Append(" ");
                    currentLine.Append(token);
                }
            }

            if (currentLine.Length > 0)
                result.AppendLine(ApplySyntaxHighlighting(currentLine.ToString(), dialect));

            return result.ToString();
        }

        private List<string> TokenizeSql(string sql)
        {
            var tokens = new List<string>();
            var current = new System.Text.StringBuilder();
            var inString = false;
            var stringChar = ' ';

            for (int i = 0; i < sql.Length; i++)
            {
                var c = sql[i];

                if ((c == '\'' || c == '"') && (i == 0 || sql[i - 1] != '\\'))
                {
                    if (!inString)
                    {
                        if (current.Length > 0)
                        {
                            tokens.Add(current.ToString());
                            current.Clear();
                        }
                        inString = true;
                        stringChar = c;
                        current.Append(c);
                    }
                    else if (c == stringChar)
                    {
                        current.Append(c);
                        tokens.Add(current.ToString());
                        current.Clear();
                        inString = false;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else if (inString)
                {
                    current.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                }
                else if (c == '(' || c == ')' || c == ',' || c == ';')
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                tokens.Add(current.ToString());

            return tokens;
        }


        public async Task PasteFromClipboard()
        {
            try
            {
                var clipboardText = await JSRuntime.InvokeAsync<string>("navigator.clipboard.readText");
                SqlToFormat = clipboardText;
            }
            catch (Exception ex)
            {
                validationErrors.Clear();
                validationErrors.Add($"Errore nell'accesso alla clipboard: {ex.Message}");
            }
        }


        public async Task CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(formattedSql))
            {
                var plainText = Regex.Replace(formattedSql, "<.*?>", string.Empty);
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", plainText);
                ToastService.Show("HTML copiato negli appunti", ToastLevel.Success);
            }
        }

        private string ApplySyntaxHighlighting(string line, SqlDialect dialect)
        {
            var result = line;

            // Evidenzia keywords del dialetto
            foreach (var keyword in dialect.Keywords)
            {
                var tempLine = line.ToUpper();
                if (tempLine.Contains(keyword))
                {
                    var pattern = $@"\b{Regex.Escape(keyword)}\b";
                    result = Regex.Replace(result, pattern,
                        $"<b>{keyword}</b>",
                        RegexOptions.IgnoreCase);
                }
            }

            // Evidenzia funzioni del dialetto
            foreach (var function in dialect.Functions)
            {
                var tempLine = line.ToUpper();
                if (tempLine.Contains(function)) continue;

                var pattern = $@"\b{Regex.Escape(function)}\b\s*\(";
                result = Regex.Replace(result, pattern,
                    $"<b>{function}</b>(",
                    RegexOptions.IgnoreCase);
            }

            // Evidenzia stringhe
            //result = Regex.Replace(result, @"'([^']*)'",
            //    "<span class=\"sql-string\">\"$1\"</span>");
            //result = Regex.Replace(result, @"""([^""]*)""",
            //    "<span class=\"sql-string\">\"$1\"</span>");

            // Evidenzia numeri
            result = Regex.Replace(result, @"\b(\d+\.?\d*)\b",
                "<i>$1</i>");

            // Evidenzia commenti
            result = Regex.Replace(result, @"--(.*)$",
                "<i>--$1</i>");

            return result;
        }

        private void UpdateSuggestions()
        {
            var cursorPos = SqlToFormat.Length; 
            var textBeforeCursor = SqlToFormat.Substring(0, Math.Min(cursorPos, SqlToFormat.Length));

            // Trova l'ultima parola parziale
            var match = FindLastPartialWord().Match(textBeforeCursor);
            if (match.Success)
            {
                currentWord = match.Groups[1].Value;
                if (currentWord.Length >= 2) // Mostra suggerimenti dopo 2 caratteri
                {
                    Suggestions = GetSuggestions(currentWord);
                    selectedSuggestionIndex = 0;

                    autocompleteTop = 100;
                    autocompleteLeft = 20;
                    return;
                }
            }
            Suggestions.Clear();
        }

        private List<Suggestion> GetSuggestions(string partial)
        {
            var dialect = GetDialect();
            var suggestions = new List<Suggestion>();
            var partialUpper = partial.ToUpper();

            // Keywords
            foreach (var keyword in dialect.Keywords.Where(k => k.StartsWith(partialUpper)))
            {
                suggestions.Add(new Suggestion
                {
                    Text = keyword,
                    Type = "keyword",
                    Description = "Parola chiave SQL"
                });
            }

            // Funzioni
            foreach (var function in dialect.Functions.Where(f => f.StartsWith(partialUpper)))
            {
                suggestions.Add(new Suggestion
                {
                    Text = function + "()",
                    Type = "function",
                    Description = GetFunctionDescription(function, dialect)
                });
            }

            // Tipi di dato
            foreach (var dataType in dialect.DataTypes.Where(d => d.StartsWith(partialUpper)))
            {
                suggestions.Add(new Suggestion
                {
                    Text = dataType,
                    Type = "type",
                    Description = "Tipo di dato"
                });
            }

            return suggestions.Take(10).ToList();
        }

        private string GetFunctionDescription(string function, SqlDialect dialect)
        {
            var descriptions = new Dictionary<string, string>
        {
            { "COUNT", "Conta il numero di righe" },
            { "SUM", "Somma valori numerici" },
            { "AVG", "Calcola la media" },
            { "MAX", "Restituisce il valore massimo" },
            { "MIN", "Restituisce il valore minimo" },
            { "CONCAT", "Concatena stringhe" },
            { "UPPER", "Converte in maiuscolo" },
            { "LOWER", "Converte in minuscolo" },
            { "LENGTH", "Restituisce la lunghezza" },
            { "NOW", "Data e ora corrente" },
            { "GETDATE", "Data e ora corrente (T-SQL)" }
        };

            return descriptions.TryGetValue(function, out var desc) ? desc : "Funzione SQL";
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (!AnySuggestions)
                return;

            switch (e.Key)
            {
                case "ArrowDown":
                    selectedSuggestionIndex = Math.Min(selectedSuggestionIndex + 1, Suggestions.Count - 1);
                    break;
                case "ArrowUp":
                    selectedSuggestionIndex = Math.Max(selectedSuggestionIndex - 1, 0);
                    break;
                case "Enter":
                    if (Suggestions.Count != 0)
                    {
                        InsertSuggestion(Suggestions[selectedSuggestionIndex]);
                    }
                    break;
            }
            StateHasChanged();
        }

        private void InsertSuggestion(Suggestion suggestion)
        {
            var textBeforeCursor = SqlToFormat;
            var newText = Regex.Replace(textBeforeCursor, @"(\w+)$", suggestion.Text);
            SqlToFormat = newText;
            Suggestions.Clear();
            UpdateFormattedSql();
        }

        private SqlDialect GetDialect()
        {
            return selectedDialect switch
            {
                "TSql" => SqlDialect.TSql,
                "PostgreSQL" => SqlDialect.PostgreSQL,
                "MySQL" => SqlDialect.MySQL,
                "Oracle" => SqlDialect.Oracle,
                "SQLite" => SqlDialect.SQLite,
                _ => SqlDialect.Standard
            };
        }

        public class Suggestion
        {
            public string Text { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        public class SqlDialect
        {
            public string Name { get; set; } = string.Empty;
            public List<string> Keywords { get; set; } = [];
            public List<string> Functions { get; set; } = [];
            public List<string> DataTypes { get; set; } = [];

            public static SqlDialect Standard => new()
            {
                Name = "Standard",
                Keywords = [ "SELECT", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "OUTER",
                "ON", "AND", "OR", "NOT", "IN", "EXISTS", "BETWEEN", "LIKE", "IS", "NULL",
                "ORDER", "BY", "GROUP", "HAVING", "DISTINCT", "AS", "ASC", "DESC",
                "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "CREATE", "ALTER", "DROP",
                "TABLE", "INDEX", "VIEW", "DATABASE", "UNION", "INTERSECT", "EXCEPT",
                "CASE", "WHEN", "THEN", "ELSE", "END" ],
                Functions = [ "COUNT", "SUM", "AVG", "MAX", "MIN", "UPPER", "LOWER", "LENGTH",
                "CONCAT", "SUBSTRING", "TRIM", "COALESCE", "CAST" ],
                DataTypes = [ "INT", "VARCHAR", "CHAR", "TEXT", "DATE", "DATETIME", "DECIMAL",
                "FLOAT", "BOOLEAN", "BLOB" ]
            };

            public static SqlDialect TSql => new()
            {
                Name = "TSql",
                Keywords = new(Standard.Keywords) { "TOP", "PIVOT", "UNPIVOT", "CROSS", "APPLY",
                "OUTER", "MERGE", "OUTPUT", "TRY", "CATCH", "BEGIN", "END", "TRANSACTION" },
                Functions = new(Standard.Functions) { "GETDATE", "DATEADD", "DATEDIFF", "ISNULL",
                "STUFF", "PATINDEX", "CHARINDEX", "NEWID", "ROW_NUMBER", "RANK", "DENSE_RANK" },
                DataTypes = new(Standard.DataTypes) { "NVARCHAR", "NCHAR", "MONEY", "SMALLMONEY",
                "UNIQUEIDENTIFIER", "XML", "VARBINARY" }
            };

            public static SqlDialect PostgreSQL => new()
            {
                Name = "PostgreSQL",
                Keywords = new(Standard.Keywords) { "RETURNING", "LATERAL", "OVER", "PARTITION",
                "WINDOW", "ARRAY", "JSONB", "SERIAL", "GENERATE", "SERIES" },
                Functions = new(Standard.Functions) { "NOW", "ARRAY_AGG", "STRING_AGG", "GENERATE_SERIES",
                "JSONB_BUILD_OBJECT", "JSONB_AGG", "REGEXP_MATCH", "TO_CHAR", "TO_DATE" },
                DataTypes = new(Standard.DataTypes) { "SERIAL", "BIGSERIAL", "JSONB", "JSON", "UUID",
                "INET", "CIDR", "MACADDR", "ARRAY", "HSTORE" }
            };

            public static SqlDialect MySQL => new()
            {
                Name = "MySQL",
                Keywords = new(Standard.Keywords) { "LIMIT", "OFFSET", "AUTO_INCREMENT", "ENGINE",
                "CHARSET", "COLLATE", "PARTITION", "SHOW", "DESCRIBE", "EXPLAIN" },
                Functions = new(Standard.Functions) { "NOW", "CURDATE", "CURTIME", "DATE_FORMAT",
                "STR_TO_DATE", "IFNULL", "GROUP_CONCAT", "FIND_IN_SET", "LAST_INSERT_ID" },
                DataTypes = new(Standard.DataTypes) { "TINYINT", "SMALLINT", "MEDIUMINT", "BIGINT",
                "TINYTEXT", "MEDIUMTEXT", "LONGTEXT", "ENUM", "SET", "YEAR" }
            };

            public static SqlDialect Oracle => new()
            {
                Name = "Oracle",
                Keywords = new(Standard.Keywords) { "ROWNUM", "CONNECT", "START", "WITH", "PRIOR",
                "DUAL", "FETCH", "FIRST", "NEXT", "ROWS", "ONLY", "MINUS" },
                Functions = new(Standard.Functions) { "SYSDATE", "NVL", "NVL2", "DECODE", "TO_CHAR",
                "TO_DATE", "TO_NUMBER", "MONTHS_BETWEEN", "ADD_MONTHS", "TRUNC" },
                DataTypes = new(Standard.DataTypes) { "NUMBER", "VARCHAR2", "NVARCHAR2", "CLOB",
                "NCLOB", "BLOB", "TIMESTAMP", "INTERVAL", "RAW" }
            };

            public static SqlDialect SQLite => new()
            {
                Name = "SQLite",
                Keywords = new(Standard.Keywords) { "AUTOINCREMENT", "PRAGMA", "ATTACH", "DETACH",
                "VACUUM", "ANALYZE", "EXPLAIN", "QUERY", "PLAN" },
                Functions = new(Standard.Functions) { "DATETIME", "JULIANDAY", "STRFTIME", "RANDOM",
                "ABS", "ROUND", "TYPEOF", "TOTAL", "GROUP_CONCAT", "LAST_INSERT_ROWID" },
                DataTypes = new(Standard.DataTypes) { "INTEGER", "REAL", "TEXT", "BLOB", "NUMERIC" }
            };
        }

        [GeneratedRegex(@"(\w+)$")]
        private static partial Regex FindLastPartialWord();
    }
}
