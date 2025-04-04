using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;

namespace devbuddy.plugins.QueryOptimizer
{
    public enum SeverityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class QueryIssue
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public SeverityLevel Severity { get; set; }
        public string Location { get; set; }
    }

    public class OptimizationSuggestion
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string OptimizedQuery { get; set; }
        public string Explanation { get; set; }
        public double EstimatedImprovement { get; set; }
    }

    public class ExecutionPlanNode
    {
        public string Operation { get; set; }
        public string ObjectName { get; set; }
        public double Cost { get; set; }
        public double Rows { get; set; }
        public List<ExecutionPlanNode> Children { get; set; } = new List<ExecutionPlanNode>();
    }

    public class ExecutionPlan
    {
        public ExecutionPlanNode RootNode { get; set; }
        public double TotalCost { get; set; }
        public double EstimatedRows { get; set; }
    }

    public class OptimizationResult
    {
        public string OriginalQuery { get; set; }
        public string OptimizedQuery { get; set; }
        public List<QueryIssue> Issues { get; set; } = new List<QueryIssue>();
        public List<OptimizationSuggestion> Suggestions { get; set; } = new List<OptimizationSuggestion>();
        public ExecutionPlan OriginalPlan { get; set; }
        public ExecutionPlan OptimizedPlan { get; set; }
        public double EstimatedImprovement { get; set; }
        public string ConfidenceLevel { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface IOptimizationRule
    {
        string Id { get; }
        string Description { get; }
        SeverityLevel Severity { get; }
        List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema);
        OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue);
    }

    public class DatabaseSchema
    {
        public Dictionary<string, TableSchema> Tables { get; set; } = new Dictionary<string, TableSchema>();
    }

    public class TableSchema
    {
        public string Name { get; set; }
        public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
        public List<IndexSchema> Indices { get; set; } = new List<IndexSchema>();
    }

    public class ColumnSchema
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
    }

    public class IndexSchema
    {
        public string Name { get; set; }
        public List<string> Columns { get; set; } = new List<string>();
        public bool IsUnique { get; set; }
        public bool IsPrimary { get; set; }
    }

    // Questa classe rappresenta l'albero sintattico astratto di una query SQL
    // In una implementazione reale, questa sarebbe generata da un parser SQL
    public class SqlAst
    {
        public string Type { get; set; } // SELECT, INSERT, UPDATE, DELETE
        public List<SqlSelectColumn> Columns { get; set; } = new List<SqlSelectColumn>();
        public List<SqlFromTable> FromTables { get; set; } = new List<SqlFromTable>();
        public List<SqlJoin> Joins { get; set; } = new List<SqlJoin>();
        public SqlWhereClause Where { get; set; }
        public SqlGroupByClause GroupBy { get; set; }
        public SqlHavingClause Having { get; set; }
        public SqlOrderByClause OrderBy { get; set; }
        public SqlLimitClause Limit { get; set; }
    }

    public class SqlSelectColumn
    {
        public string Expression { get; set; }
        public string Alias { get; set; }
        public bool IsAsterisk { get; set; }
        public string TableName { get; set; } // Per colonne tipo "table.*"
    }

    public class SqlFromTable
    {
        public string TableName { get; set; }
        public string Alias { get; set; }
    }

    public class SqlJoin
    {
        public string Type { get; set; } // INNER, LEFT, RIGHT, FULL
        public string TableName { get; set; }
        public string Alias { get; set; }
        public SqlJoinCondition Condition { get; set; }
    }

    public class SqlJoinCondition
    {
        public string LeftExpression { get; set; }
        public string Operator { get; set; }
        public string RightExpression { get; set; }
    }

    public class SqlWhereClause
    {
        public List<SqlCondition> Conditions { get; set; } = new List<SqlCondition>();
    }

    public class SqlCondition
    {
        public string LeftExpression { get; set; }
        public string Operator { get; set; }
        public string RightExpression { get; set; }
        public List<SqlCondition> NestedConditions { get; set; } = new List<SqlCondition>();
        public string LogicalOperator { get; set; } // AND, OR
    }

    public class SqlGroupByClause
    {
        public List<string> Columns { get; set; } = new List<string>();
    }

    public class SqlHavingClause
    {
        public List<SqlCondition> Conditions { get; set; } = new List<SqlCondition>();
    }

    public class SqlOrderByClause
    {
        public List<SqlOrderByColumn> Columns { get; set; } = new List<SqlOrderByColumn>();
    }

    public class SqlOrderByColumn
    {
        public string Column { get; set; }
        public bool IsAscending { get; set; }
    }

    public class SqlLimitClause
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
    }

    public class SelectStarRule : IOptimizationRule
    {
        public string Id => "select_star";
        public string Description => "Evitare SELECT * e specificare solo le colonne necessarie";
        public SeverityLevel Severity => SeverityLevel.Medium;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            if (ast.Type == "SELECT")
            {
                bool hasSelectStar = ast.Columns.Any(c => c.IsAsterisk && string.IsNullOrEmpty(c.TableName));

                if (hasSelectStar)
                {
                    issues.Add(new QueryIssue
                    {
                        Id = Id,
                        Description = Description,
                        Message = "L'uso di SELECT * può causare performance ridotte. Specifica solo le colonne necessarie.",
                        Severity = Severity,
                        Location = "0:0" // In un'implementazione reale avremmo la posizione precisa
                    });
                }
            }

            return issues;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            if (schema == null || ast.FromTables.Count == 0)
            {
                return null;
            }

            List<string> explicitColumns = new List<string>();

            // Per ogni tabella nella FROM clause o nelle JOIN, aggiungiamo tutte le colonne disponibili
            foreach (var table in ast.FromTables)
            {
                string tableName = table.TableName;
                string tableAlias = !string.IsNullOrEmpty(table.Alias) ? table.Alias : tableName;

                if (schema.Tables.ContainsKey(tableName))
                {
                    foreach (var column in schema.Tables[tableName].Columns)
                    {
                        explicitColumns.Add($"{tableAlias}.{column.Name}");
                    }
                }
            }

            foreach (var join in ast.Joins)
            {
                string tableName = join.TableName;
                string tableAlias = !string.IsNullOrEmpty(join.Alias) ? join.Alias : tableName;

                if (schema.Tables.ContainsKey(tableName))
                {
                    foreach (var column in schema.Tables[tableName].Columns)
                    {
                        explicitColumns.Add($"{tableAlias}.{column.Name}");
                    }
                }
            }

            // Creiamo una nuova query con le colonne esplicite
            string columnsString = string.Join(", ", explicitColumns);

            // Qui dovremmo ricostruire l'intera query, ma per semplicità facciamo solo una sostituzione
            // In una implementazione reale questo utilizzerebbe l'AST e un SQL generator
            string optimizedQuery = "SELECT " + columnsString + " FROM " + ast.FromTables[0].TableName;

            return new OptimizationSuggestion
            {
                Id = "explicit_columns",
                Description = "Selezione esplicita delle colonne",
                OptimizedQuery = optimizedQuery,
                Explanation = "Specificando esattamente le colonne necessarie, si riduce il traffico di rete e il carico sul database.",
                EstimatedImprovement = 15.0
            };
        }
    }

    public class MissingWhereRule : IOptimizationRule
    {
        public string Id => "missing_where";
        public string Description => "Query senza clausola WHERE su tabelle grandi";
        public SeverityLevel Severity => SeverityLevel.Critical;

        public List<QueryIssue> Check(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> issues = new List<QueryIssue>();

            if (ast.Type == "SELECT" && (ast.Where == null || ast.Where.Conditions.Count == 0))
            {
                // Idealmente controlleremmo se le tabelle sono considerate "grandi"
                // basandoci su statistiche dal DB o metadata
                issues.Add(new QueryIssue
                {
                    Id = Id,
                    Description = Description,
                    Message = "Query senza clausola WHERE. Potrebbe causare scansioni di tabella complete.",
                    Severity = Severity,
                    Location = "0:0"
                });
            }

            return issues;
        }

        public OptimizationSuggestion GenerateSuggestion(SqlAst ast, DatabaseSchema schema, QueryIssue issue)
        {
            // Per questo tipo di problema, non possiamo generare automaticamente una soluzione
            // perché richiede conoscenza del dominio per sapere quali condizioni aggiungere
            return new OptimizationSuggestion
            {
                Id = "add_where_condition",
                Description = "Aggiungere una clausola WHERE",
                OptimizedQuery = ast.Type + " ... WHERE <condizione>",
                Explanation = "Aggiungere una clausola WHERE appropriata per limitare i risultati solo ai dati necessari.",
                EstimatedImprovement = 80.0
            };
        }
    }

    public class QueryOptimizer
    {
        private List<IOptimizationRule> optimizationRules;
        private readonly IJSRuntime jsRuntime;
        private readonly string databaseType;

        public QueryOptimizer(IJSRuntime jsRuntime, string databaseType = "sqlserver")
        {
            this.jsRuntime = jsRuntime;
            this.databaseType = databaseType;
            LoadRules();
        }

        private void LoadRules()
        {
            optimizationRules = new List<IOptimizationRule>
            {
                new SelectStarRule(),
                new MissingWhereRule(),
                // Aggiungi altre regole qui
            };
        }

        public async Task<OptimizationResult> OptimizeAsync(string query, string schemaJson = null)
        {
            try
            {
                // In una implementazione reale, chiameremmo un vero parser SQL
                // Per questo esempio, simuliamo il parsing
                SqlAst ast = await ParseSqlAsync(query);

                DatabaseSchema schema = null;
                if (!string.IsNullOrEmpty(schemaJson))
                {
                    schema = JsonSerializer.Deserialize<DatabaseSchema>(schemaJson);
                }

                // Analizza la query per problemi
                List<QueryIssue> issues = AnalyzeQuery(ast, schema);

                // Genera suggerimenti
                List<OptimizationSuggestion> suggestions = GenerateSuggestions(ast, schema, issues);

                // Genera query ottimizzata
                string optimizedQuery = GenerateOptimizedQuery(ast, suggestions);

                // Genera piani di esecuzione (simulati)
                ExecutionPlan originalPlan = GenerateExecutionPlan(query, false);
                ExecutionPlan optimizedPlan = GenerateExecutionPlan(optimizedQuery, true);

                return new OptimizationResult
                {
                    OriginalQuery = query,
                    OptimizedQuery = optimizedQuery,
                    Issues = issues,
                    Suggestions = suggestions,
                    OriginalPlan = originalPlan,
                    OptimizedPlan = optimizedPlan,
                    EstimatedImprovement = CalculateImprovement(issues, originalPlan, optimizedPlan),
                    ConfidenceLevel = issues.Count > 3 ? "Alta" : "Media"
                };
            }
            catch (Exception ex)
            {
                return new OptimizationResult
                {
                    OriginalQuery = query,
                    HasError = true,
                    ErrorMessage = $"Errore nell'analisi della query: {ex.Message}"
                };
            }
        }

        private async Task<SqlAst> ParseSqlAsync(string query)
        {
            // In un'implementazione reale, useremmo una libreria di parsing SQL
            // Per questo esempio, simuliamo un parser molto semplice

            // Potremmo usare un servizio JavaScript per il parsing tramite JSInterop
            // var astJson = await jsRuntime.InvokeAsync<string>("parseSql", query, databaseType);
            // return JsonSerializer.Deserialize<SqlAst>(astJson);

            // Simulazione di un AST semplice
            var ast = new SqlAst { Type = "SELECT" };

            // Rilevamento di SELECT *
            if (query.ToUpperInvariant().Contains("SELECT *"))
            {
                ast.Columns.Add(new SqlSelectColumn { IsAsterisk = true });
            }

            // Rilevamento FROM
            var fromMatch = Regex.Match(query, @"FROM\s+([a-zA-Z0-9_\.]+)(\s+AS\s+|\s+)?([a-zA-Z0-9_]+)?", RegexOptions.IgnoreCase);
            if (fromMatch.Success)
            {
                ast.FromTables.Add(new SqlFromTable
                {
                    TableName = fromMatch.Groups[1].Value,
                    Alias = fromMatch.Groups[3].Success ? fromMatch.Groups[3].Value : null
                });
            }

            // Rilevamento JOIN
            var joinMatches = Regex.Matches(query, @"(INNER|LEFT|RIGHT)?\s*JOIN\s+([a-zA-Z0-9_\.]+)(\s+AS\s+|\s+)?([a-zA-Z0-9_]+)?\s+ON\s+([a-zA-Z0-9_\.]+)\s*=\s*([a-zA-Z0-9_\.]+)", RegexOptions.IgnoreCase);
            foreach (Match match in joinMatches)
            {
                ast.Joins.Add(new SqlJoin
                {
                    Type = match.Groups[1].Success ? match.Groups[1].Value.ToUpperInvariant() : "INNER",
                    TableName = match.Groups[2].Value,
                    Alias = match.Groups[4].Success ? match.Groups[4].Value : null,
                    Condition = new SqlJoinCondition
                    {
                        LeftExpression = match.Groups[5].Value,
                        Operator = "=",
                        RightExpression = match.Groups[6].Value
                    }
                });
            }

            // Rilevamento WHERE
            var whereMatch = Regex.Match(query, @"WHERE\s+(.+?)(\s+GROUP BY|\s+ORDER BY|\s+LIMIT|\s*$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (whereMatch.Success)
            {
                ast.Where = new SqlWhereClause();
                string whereClause = whereMatch.Groups[1].Value.Trim();

                // Parsing semplificato della WHERE (solo per dimostrazione)
                var conditions = Regex.Split(whereClause, @"\s+AND\s+|\s+OR\s+", RegexOptions.IgnoreCase);
                foreach (var condition in conditions)
                {
                    var condMatch = Regex.Match(condition, @"([a-zA-Z0-9_\.]+)\s*([=><]+)\s*(.+)");
                    if (condMatch.Success)
                    {
                        ast.Where.Conditions.Add(new SqlCondition
                        {
                            LeftExpression = condMatch.Groups[1].Value,
                            Operator = condMatch.Groups[2].Value,
                            RightExpression = condMatch.Groups[3].Value.Trim('\'', '"')
                        });
                    }
                }
            }

            return ast;
        }

        private List<QueryIssue> AnalyzeQuery(SqlAst ast, DatabaseSchema schema)
        {
            List<QueryIssue> allIssues = new List<QueryIssue>();

            foreach (var rule in optimizationRules)
            {
                var issues = rule.Check(ast, schema);
                if (issues != null && issues.Count > 0)
                {
                    allIssues.AddRange(issues);
                }
            }

            return allIssues;
        }

        private List<OptimizationSuggestion> GenerateSuggestions(SqlAst ast, DatabaseSchema schema, List<QueryIssue> issues)
        {
            List<OptimizationSuggestion> suggestions = new List<OptimizationSuggestion>();

            foreach (var issue in issues)
            {
                var rule = optimizationRules.FirstOrDefault(r => r.Id == issue.Id);
                if (rule != null)
                {
                    var suggestion = rule.GenerateSuggestion(ast, schema, issue);
                    if (suggestion != null)
                    {
                        suggestions.Add(suggestion);
                    }
                }
            }

            return suggestions;
        }

        private string GenerateOptimizedQuery(SqlAst ast, List<OptimizationSuggestion> suggestions)
        {
            if (suggestions.Count == 0)
            {
                return string.Empty;
            }

            // Per semplicità, prendiamo il primo suggerimento che ha una query ottimizzata
            var suggestion = suggestions.FirstOrDefault(s => !string.IsNullOrEmpty(s.OptimizedQuery));
            return suggestion?.OptimizedQuery ?? string.Empty;
        }

        private ExecutionPlan GenerateExecutionPlan(string query, bool isOptimized)
        {
            // In un'applicazione reale, otterremmo il piano di esecuzione dal database
            // Per questo esempio, generiamo un piano simulato in base alla query

            var plan = new ExecutionPlan();

            if (string.IsNullOrWhiteSpace(query))
            {
                // Se non c'è una query, restituiamo un piano vuoto
                plan.TotalCost = 0;
                plan.EstimatedRows = 0;
                plan.RootNode = null;
                return plan;
            }

            // Analizziamo la query per personalizzare il piano
            bool hasJoin = query.ToUpperInvariant().Contains("JOIN");
            bool hasWhere = query.ToUpperInvariant().Contains("WHERE");
            bool hasOrderBy = query.ToUpperInvariant().Contains("ORDER BY");
            bool hasGroupBy = query.ToUpperInvariant().Contains("GROUP BY");
            bool hasHaving = query.ToUpperInvariant().Contains("HAVING");
            bool hasLimit = query.ToUpperInvariant().Contains("LIMIT") || query.ToUpperInvariant().Contains("TOP");
            bool hasSelectStar = query.ToUpperInvariant().Contains("SELECT *");

            // Identifichiamo le tabelle coinvolte
            var tableMatches = Regex.Matches(query, @"FROM\s+([a-zA-Z0-9_\.]+)|JOIN\s+([a-zA-Z0-9_\.]+)", RegexOptions.IgnoreCase);
            List<string> tables = new List<string>();

            foreach (Match match in tableMatches)
            {
                string table = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (!string.IsNullOrEmpty(table) && !tables.Contains(table))
                {
                    tables.Add(table);
                }
            }

            // Simuliamo un piano più efficiente per la versione ottimizzata
            if (isOptimized)
            {
                // Il costo base dipende dal numero di tabelle e clausole
                double baseCost = tables.Count * 50;
                // Aggiungiamo costi per le varie operazioni
                if (hasJoin) baseCost += 100;
                if (hasWhere) baseCost += 50;
                if (hasOrderBy) baseCost += 200;
                if (hasGroupBy) baseCost += 300;
                if (hasHaving) baseCost += 150;

                plan.TotalCost = baseCost;
                plan.EstimatedRows = 1000;

                // Costruiamo un piano realistico in base alla struttura della query
                if (hasJoin)
                {
                    plan.RootNode = new ExecutionPlanNode
                    {
                        Operation = hasGroupBy ? "Hash Group" : "Hash Join",
                        ObjectName = tables.Count > 1 ? $"{tables[0]}_{tables[1]}" : tables[0],
                        Cost = baseCost * 0.6,
                        Rows = 1000,
                        Children = new List<ExecutionPlanNode>()
                    };

                    // Aggiungiamo i nodi figli in base alle tabelle e clausole
                    if (hasWhere && tables.Count > 0)
                    {
                        plan.RootNode.Children.Add(new ExecutionPlanNode
                        {
                            Operation = "Index Scan",
                            ObjectName = $"{tables[0]}_" + (hasWhere ? "idx" : "pkey"),
                            Cost = baseCost * 0.2,
                            Rows = hasWhere ? 5000 : 10000
                        });
                    }

                    if (tables.Count > 1)
                    {
                        plan.RootNode.Children.Add(new ExecutionPlanNode
                        {
                            Operation = "Index Scan",
                            ObjectName = $"{tables[1]}_pkey",
                            Cost = baseCost * 0.1,
                            Rows = 100
                        });
                    }

                    if (hasOrderBy)
                    {
                        // Aggiungiamo un nodo di ordinamento sopra il join
                        var originalRoot = plan.RootNode;
                        plan.RootNode = new ExecutionPlanNode
                        {
                            Operation = "Sort",
                            ObjectName = null,
                            Cost = baseCost * 0.1,
                            Rows = 1000,
                            Children = new List<ExecutionPlanNode> { originalRoot }
                        };
                    }
                }
                else if (tables.Count > 0)
                {
                    // Query su una singola tabella
                    string operation = "Index Scan";
                    string objectName = $"{tables[0]}_idx";

                    if (hasWhere)
                    {
                        // Utilizzo di indice per la clausola WHERE
                        operation = "Index Scan";
                        objectName = $"{tables[0]}_idx";
                    }
                    else
                    {
                        // Senza WHERE, potremmo usare un indice per ORDER BY o PK scan
                        operation = hasOrderBy ? "Index Scan" : "Sequential Scan";
                        objectName = hasOrderBy ? $"{tables[0]}_idx" : tables[0];
                    }

                    plan.RootNode = new ExecutionPlanNode
                    {
                        Operation = operation,
                        ObjectName = objectName,
                        Cost = baseCost * 0.8,
                        Rows = hasWhere ? 500 : 10000
                    };

                    if (hasGroupBy)
                    {
                        // Aggiungiamo un nodo di aggregazione sopra lo scan
                        var scanNode = plan.RootNode;
                        plan.RootNode = new ExecutionPlanNode
                        {
                            Operation = "Hash Aggregate",
                            ObjectName = null,
                            Cost = baseCost * 0.2,
                            Rows = 100,
                            Children = new List<ExecutionPlanNode> { scanNode }
                        };
                    }

                    if (hasOrderBy)
                    {
                        // Se non stiamo già usando un indice per l'ordinamento, aggiungiamo un nodo di ordinamento
                        if (operation != "Index Scan" || !objectName.EndsWith("_idx"))
                        {
                            var currentRoot = plan.RootNode;
                            plan.RootNode = new ExecutionPlanNode
                            {
                                Operation = "Sort",
                                ObjectName = null,
                                Cost = baseCost * 0.2,
                                Rows = currentRoot.Rows,
                                Children = new List<ExecutionPlanNode> { currentRoot }
                            };
                        }
                    }
                }
            }
            else
            {
                // Query originale, presumibilmente meno efficiente

                // Il costo base dipende dal numero di tabelle e clausole, ma più alto dell'ottimizzata
                double baseCost = tables.Count * 200;
                // Aggiungiamo costi per le varie operazioni
                if (hasJoin) baseCost += 500;
                if (hasWhere) baseCost += 100;
                if (hasOrderBy) baseCost += 400;
                if (hasGroupBy) baseCost += 600;
                if (hasHaving) baseCost += 300;
                if (hasSelectStar) baseCost += 200;

                plan.TotalCost = baseCost;
                plan.EstimatedRows = 1000;

                // Costruiamo un piano realistico in base alla struttura della query
                if (hasJoin)
                {
                    plan.RootNode = new ExecutionPlanNode
                    {
                        Operation = hasGroupBy ? "Hash Group" : "Nested Loop",
                        ObjectName = tables.Count > 1 ? $"{tables[0]}_{tables[1]}" : tables[0],
                        Cost = baseCost * 0.6,
                        Rows = 1000,
                        Children = new List<ExecutionPlanNode>()
                    };

                    // Aggiungiamo i nodi figli in base alle tabelle e clausole
                    if (tables.Count > 0)
                    {
                        plan.RootNode.Children.Add(new ExecutionPlanNode
                        {
                            Operation = "Sequential Scan",
                            ObjectName = tables[0],
                            Cost = baseCost * 0.3,
                            Rows = 10000
                        });
                    }

                    if (tables.Count > 1)
                    {
                        plan.RootNode.Children.Add(new ExecutionPlanNode
                        {
                            Operation = "Sequential Scan",
                            ObjectName = tables[1],
                            Cost = baseCost * 0.2,
                            Rows = 5000
                        });
                    }

                    if (hasOrderBy)
                    {
                        // Aggiungiamo un nodo di ordinamento sopra il join
                        var originalRoot = plan.RootNode;
                        plan.RootNode = new ExecutionPlanNode
                        {
                            Operation = "Sort",
                            ObjectName = null,
                            Cost = baseCost * 0.4,
                            Rows = 1000,
                            Children = new List<ExecutionPlanNode> { originalRoot }
                        };
                    }
                }
                else if (tables.Count > 0)
                {
                    // Query su una singola tabella
                    plan.RootNode = new ExecutionPlanNode
                    {
                        Operation = "Sequential Scan",
                        ObjectName = tables[0],
                        Cost = baseCost * 0.8,
                        Rows = 10000
                    };

                    if (hasGroupBy)
                    {
                        // Aggiungiamo un nodo di aggregazione sopra lo scan
                        var scanNode = plan.RootNode;
                        plan.RootNode = new ExecutionPlanNode
                        {
                            Operation = "Group Aggregate",
                            ObjectName = null,
                            Cost = baseCost * 0.3,
                            Rows = 100,
                            Children = new List<ExecutionPlanNode> { scanNode }
                        };
                    }

                    if (hasOrderBy)
                    {
                        // Aggiungiamo un nodo di ordinamento
                        var currentRoot = plan.RootNode;
                        plan.RootNode = new ExecutionPlanNode
                        {
                            Operation = "Sort",
                            ObjectName = null,
                            Cost = baseCost * 0.4,
                            Rows = currentRoot.Rows,
                            Children = new List<ExecutionPlanNode> { currentRoot }
                        };
                    }
                }
            }

            return plan;
        }

        private double CalculateImprovement(List<QueryIssue> issues, ExecutionPlan originalPlan, ExecutionPlan optimizedPlan)
        {
            // Calcolo semplice basato sulla differenza di costo tra i piani
            if (originalPlan != null && optimizedPlan != null && originalPlan.TotalCost > 0)
            {
                return Math.Min(95, (1 - (optimizedPlan.TotalCost / originalPlan.TotalCost)) * 100);
            }

            // Calcolo alternativo basato sulla gravità dei problemi
            var severityWeights = new Dictionary<SeverityLevel, int>
            {
                { SeverityLevel.Low, 1 },
                { SeverityLevel.Medium, 2 },
                { SeverityLevel.High, 3 },
                { SeverityLevel.Critical, 5 }
            };

            int totalWeight = issues.Sum(i => severityWeights[i.Severity]);
            return Math.Min(95, totalWeight * 10);
        }
    }
}