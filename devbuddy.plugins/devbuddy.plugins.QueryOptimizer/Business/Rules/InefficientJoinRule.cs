using System.Text.RegularExpressions;
using devbuddy.plugins.QueryOptimizer.Business.Contracts;
using devbuddy.plugins.QueryOptimizer.Business.Models.Enum;
using devbuddy.plugins.QueryOptimizer.Business.Schemas;

namespace devbuddy.plugins.QueryOptimizer.Business.Rules
{
    public class InefficientJoinRule : IOptimizationRule
    {
        public string Id => "inefficient_join";
        public string Description => "Join inefficienti tra tabelle";
        public SeverityLevel Severity => SeverityLevel.High;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = [];

            if (ast.Type == "SELECT" && ast.Joins.Count > 0 && schema != null)
            {
                foreach (var join in ast.Joins)
                {
                    if (join.Condition != null)
                    {
                        // Verifichiamo se le colonne di join sono indicizzate
                        bool leftSideIndexed = IsColumnIndexed(join.Condition.LeftExpression, schema);
                        bool rightSideIndexed = IsColumnIndexed(join.Condition.RightExpression, schema);

                        if (!leftSideIndexed || !rightSideIndexed)
                        {
                            issues.Add(new QueryIssue
                            {
                                Id = Id,
                                Description = Description,
                                Message = $"Join tra {join.Condition.LeftExpression} e {join.Condition.RightExpression} potrebbe non utilizzare indici ottimali.",
                                Severity = Severity,
                                Location = "0:0"
                            });
                        }
                    }
                }
            }

            return issues;
        }

        private bool IsColumnIndexed(string columnExpression, DatabaseSchema schema)
        {
            // Formato previsto: table.column o column
            string tableName = "";
            string columnName = columnExpression;

            var parts = columnExpression.Split('.');
            if (parts.Length == 2)
            {
                tableName = parts[0];
                columnName = parts[1];
            }
            else
            {
                return false; // Formato non valido
            }

            // Cerchiamo la tabella nello schema
            if (schema.Tables.ContainsKey(tableName))
            {
                var table = schema.Tables[tableName];

                // Verifichiamo se la colonna è indicizzata
                foreach (var index in table.Indices)
                {
                    if (index.Columns.Contains(columnName) && (index.Columns[0] == columnName || index.IsPrimary))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            // Estraiamo i nomi delle colonne dal messaggio
            var match = Regex.Match(issue.Message, @"Join tra ([^\s]+) e ([^\s]+)");
            if (!match.Success)
                return null;

            string leftColumn = match.Groups[1].Value;
            string rightColumn = match.Groups[2].Value;

            var leftParts = leftColumn.Split('.');
            var rightParts = rightColumn.Split('.');

            if (leftParts.Length != 2 || rightParts.Length != 2)
                return null;

            string leftTable = leftParts[0];
            string leftColName = leftParts[1];
            string rightTable = rightParts[0];
            string rightColName = rightParts[1];

            string createIndexSql = "";

            // Verifichiamo quali indici mancano
            bool leftIndexed = IsColumnIndexed(leftColumn, schema);
            bool rightIndexed = IsColumnIndexed(rightColumn, schema);

            if (!leftIndexed)
            {
                createIndexSql += $"CREATE INDEX idx_{leftTable}_{leftColName} ON {leftTable}({leftColName});\n";
            }

            if (!rightIndexed)
            {
                createIndexSql += $"CREATE INDEX idx_{rightTable}_{rightColName} ON {rightTable}({rightColName});\n";
            }

            if (string.IsNullOrEmpty(createIndexSql))
            {
                return null;
            }

            return new OptimizationSuggestion
            {
                Id = "create_join_indices",
                Description = "Creare indici per migliorare le performance del join",
                OptimizedQuery = createIndexSql,
                Explanation = "Creare indici sulle colonne di join migliorerà significativamente le performance della query.",
                EstimatedImprovement = 65.0
            };
        }
    }
}
