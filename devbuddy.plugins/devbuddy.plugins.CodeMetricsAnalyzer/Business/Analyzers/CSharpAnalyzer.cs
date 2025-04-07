using devbuddy.plugins.CodeMetricsAnalyzer.Business.Analyzers.Base;
using devbuddy.plugins.CodeMetricsAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Text.RegularExpressions;

namespace devbuddy.plugins.CodeMetricsAnalyzer.Business.Analyzers
{
    public class CSharpAnalyzer : LanguageAnalyzerBase
    {
        public override async Task AnalyzeFileAsync(string fileContent, FileMetrics fileMetrics)
        {
            var lines = fileContent.Split('\n');
            fileMetrics.CommentLines = CountCommentLines(lines);
            fileMetrics.CodeLines = CountCodeLines(lines);

            // Parse C# code using Roslyn
            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = await tree.GetRootAsync();

            // Analyze classes
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classNode in classes)
            {
                var classMetrics = AnalyzeClass(classNode, tree);
                fileMetrics.Classes.Add(classMetrics);
            }

            // Calculate average complexity metrics
            if (fileMetrics.Classes.Count > 0)
            {
                double totalComplexity = 0;
                int methodCount = 0;

                foreach (var classMetric in fileMetrics.Classes)
                {
                    foreach (var methodMetric in classMetric.Methods)
                    {
                        totalComplexity += methodMetric.CyclomaticComplexity;
                        methodCount++;
                    }
                }

                if (methodCount > 0)
                {
                    fileMetrics.CyclomaticComplexity = totalComplexity / methodCount;
                }
            }

            // Calculate maintainability index
            int distinctOperators = CountDistinctOperators(root);
            fileMetrics.MaintainabilityIndex = CalculateMaintainabilityIndex(fileMetrics.CodeLines, fileMetrics.CyclomaticComplexity, distinctOperators);

            // Calculate comments ratio
            if (fileMetrics.CodeLines > 0)
            {
                fileMetrics.CommentsRatio = (double)fileMetrics.CommentLines / fileMetrics.CodeLines * 100;
            }
        }

        private ClassMetrics AnalyzeClass(ClassDeclarationSyntax classNode, SyntaxTree tree)
        {
            var classMetrics = new ClassMetrics
            {
                ClassName = classNode.Identifier.Text
            };

            var lineSpan = tree.GetLineSpan(classNode.Span);
            classMetrics.StartLine = lineSpan.StartLinePosition.Line + 1;
            classMetrics.EndLine = lineSpan.EndLinePosition.Line + 1;
            classMetrics.LineCount = classMetrics.EndLine - classMetrics.StartLine + 1;

            // Calculate class coupling
            classMetrics.CouplingDegree = CalculateCouplingDegree(classNode);

            // Calculate cohesion
            classMetrics.CohesionMetric = CalculateCohesion(classNode);

            // Analyze methods
            var methods = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodNode in methods)
            {
                var methodMetrics = AnalyzeMethod(methodNode, tree);
                classMetrics.Methods.Add(methodMetrics);
            }

            // Calculate maintainability index for class
            if (classMetrics.Methods.Count > 0)
            {
                classMetrics.MaintainabilityIndex = classMetrics.Methods.Average(m => m.MaintainabilityIndex);
            }

            return classMetrics;
        }

        private MethodMetrics AnalyzeMethod(MethodDeclarationSyntax methodNode, SyntaxTree tree)
        {
            var methodMetrics = new MethodMetrics
            {
                MethodName = methodNode.Identifier.Text
            };

            var lineSpan = tree.GetLineSpan(methodNode.Span);
            methodMetrics.StartLine = lineSpan.StartLinePosition.Line + 1;
            methodMetrics.EndLine = lineSpan.EndLinePosition.Line + 1;
            methodMetrics.LineCount = methodMetrics.EndLine - methodMetrics.StartLine + 1;

            // Count parameters
            methodMetrics.ParameterCount = methodNode.ParameterList.Parameters.Count;

            // Calculate cyclomatic complexity
            methodMetrics.CyclomaticComplexity = CalculateCyclomaticComplexity(methodNode);

            // Calculate cognitive complexity
            methodMetrics.CognitiveComplexity = CalculateCognitiveComplexity(methodNode);

            // Calculate maintainability index
            int distinctOperators = CountDistinctOperators(methodNode);
            methodMetrics.MaintainabilityIndex = CalculateMaintainabilityIndex(methodMetrics.LineCount, methodMetrics.CyclomaticComplexity, distinctOperators);

            return methodMetrics;
        }

        private double CalculateCyclomaticComplexity(MethodDeclarationSyntax methodNode)
        {
            // Base complexity is 1
            int complexity = 1;

            // Add 1 for each branch point
            complexity += methodNode.DescendantNodes().OfType<IfStatementSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<SwitchSectionSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<CatchClauseSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<WhileStatementSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<DoStatementSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<ForStatementSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<ForEachStatementSyntax>().Count();

            // Add complexity for conditional operators
            complexity += methodNode.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();

            // Add complexity for logical operators in conditions
            var conditions = methodNode.DescendantNodes().OfType<BinaryExpressionSyntax>()
                .Where(bes => bes.Kind() == SyntaxKind.LogicalAndExpression || bes.Kind() == SyntaxKind.LogicalOrExpression);
            complexity += conditions.Count();

            return complexity;
        }

        private double CalculateCognitiveComplexity(MethodDeclarationSyntax methodNode)
        {
            // A simplified version of cognitive complexity
            // In a real implementation this would be more sophisticated
            int complexity = 0;

            // Count nesting levels and apply weights
            complexity += CountNestedBlocks(methodNode) * 2;

            // Count branch points
            complexity += methodNode.DescendantNodes().OfType<IfStatementSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<SwitchSectionSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<CatchClauseSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<WhileStatementSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<DoStatementSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<ForStatementSyntax>().Count();
            complexity += methodNode.DescendantNodes().OfType<ForEachStatementSyntax>().Count();

            // Add complexity for logical operators in conditions (weighted more than in cyclomatic)
            var conditions = methodNode.DescendantNodes().OfType<BinaryExpressionSyntax>()
                .Where(bes => bes.Kind() == SyntaxKind.LogicalAndExpression || bes.Kind() == SyntaxKind.LogicalOrExpression);
            complexity += conditions.Count() * 2;

            return complexity;
        }

        private int CountNestedBlocks(SyntaxNode node, int currentDepth = 0)
        {
            int maxDepth = currentDepth;

            foreach (var child in node.ChildNodes())
            {
                if (IsBlockNode(child))
                {
                    int childDepth = CountNestedBlocks(child, currentDepth + 1);
                    maxDepth = Math.Max(maxDepth, childDepth);
                }
                else
                {
                    int childDepth = CountNestedBlocks(child, currentDepth);
                    maxDepth = Math.Max(maxDepth, childDepth);
                }
            }

            return maxDepth;
        }

        private bool IsBlockNode(SyntaxNode node)
        {
            return node is IfStatementSyntax ||
                   node is ForStatementSyntax ||
                   node is ForEachStatementSyntax ||
                   node is WhileStatementSyntax ||
                   node is DoStatementSyntax ||
                   node is SwitchStatementSyntax ||
                   node is TryStatementSyntax;
        }

        private double CalculateCouplingDegree(ClassDeclarationSyntax classNode)
        {
            // In una vera implementazione, questa funzione richiederebbe l'uso di SemanticModel
            // per ottenere le informazioni sui simboli e sull'accoppiamento tra classi

            // Per questa demo, utilizziamo un approccio semplificato basato sull'analisi sintattica
            var typeReferences = new HashSet<string>();

            // Find the namespace of this class
            var namespaceNode = classNode.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            string currentNamespace = namespaceNode?.Name.ToString() ?? "";

            // Count class members that use other types
            foreach (var member in classNode.Members)
            {
                // Check field declarations
                if (member is FieldDeclarationSyntax fieldDecl)
                {
                    foreach (var variable in fieldDecl.Declaration.Variables)
                    {
                        // Count the field type
                        if (fieldDecl.Declaration.Type is IdentifierNameSyntax idType)
                        {
                            typeReferences.Add(idType.Identifier.Text);
                        }
                    }
                }

                // Check method return types and parameters
                if (member is MethodDeclarationSyntax methodDecl)
                {
                    if (methodDecl.ReturnType is IdentifierNameSyntax returnType)
                    {
                        typeReferences.Add(returnType.Identifier.Text);
                    }

                    foreach (var param in methodDecl.ParameterList.Parameters)
                    {
                        if (param.Type is IdentifierNameSyntax paramType)
                        {
                            typeReferences.Add(paramType.Identifier.Text);
                        }
                    }
                }
            }

            // Remove common primitive types and current class
            typeReferences.Remove("void");
            typeReferences.Remove("int");
            typeReferences.Remove("string");
            typeReferences.Remove("bool");
            typeReferences.Remove("double");
            typeReferences.Remove("float");
            typeReferences.Remove("object");
            typeReferences.Remove(classNode.Identifier.Text);

            return typeReferences.Count;
        }

        private double CalculateCohesion(ClassDeclarationSyntax classNode)
        {
            // A simplified LCOM (Lack of Cohesion of Methods) calculation
            // Lower value means better cohesion

            var methods = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
            if (methods.Count <= 1)
                return 1.0; // Perfect cohesion for single method or no methods

            var fields = classNode.DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();
            var properties = classNode.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

            int totalPairs = (methods.Count * (methods.Count - 1)) / 2;
            int disjointPairs = 0;

            // Count method pairs that don't share field/property access
            for (int i = 0; i < methods.Count - 1; i++)
            {
                for (int j = i + 1; j < methods.Count; j++)
                {
                    if (!ShareFieldsOrProperties(methods[i], methods[j], fields, properties))
                    {
                        disjointPairs++;
                    }
                }
            }

            double lcom = (double)disjointPairs / totalPairs;

            // Convert to cohesion (inverse of LCOM, scaled to 0-1)
            return 1.0 - lcom;
        }

        private bool ShareFieldsOrProperties(
            MethodDeclarationSyntax method1,
            MethodDeclarationSyntax method2,
            List<FieldDeclarationSyntax> fields,
            List<PropertyDeclarationSyntax> properties)
        {
            var accessedInMethod1 = GetAccessedMembers(method1);
            var accessedInMethod2 = GetAccessedMembers(method2);

            return accessedInMethod1.Intersect(accessedInMethod2).Any();
        }

        private HashSet<string> GetAccessedMembers(MethodDeclarationSyntax method)
        {
            var result = new HashSet<string>();

            foreach (var identifier in method.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                result.Add(identifier.Identifier.Text);
            }

            return result;
        }

        private int CountDistinctOperators(SyntaxNode node)
        {
            var operators = new HashSet<SyntaxKind>();

            foreach (var token in node.DescendantTokens())
            {
                if (token.IsKind(SyntaxKind.PlusToken) ||
                    token.IsKind(SyntaxKind.MinusToken) ||
                    token.IsKind(SyntaxKind.AsteriskToken) ||
                    token.IsKind(SyntaxKind.SlashToken) ||
                    token.IsKind(SyntaxKind.PercentToken) ||
                    token.IsKind(SyntaxKind.AmpersandToken) ||
                    token.IsKind(SyntaxKind.BarToken) ||
                    token.IsKind(SyntaxKind.CaretToken) ||
                    token.IsKind(SyntaxKind.ExclamationToken) ||
                    token.IsKind(SyntaxKind.TildeToken) ||
                    token.IsKind(SyntaxKind.EqualsEqualsToken) ||
                    token.IsKind(SyntaxKind.ExclamationEqualsToken) ||
                    token.IsKind(SyntaxKind.LessThanToken) ||
                    token.IsKind(SyntaxKind.LessThanEqualsToken) ||
                    token.IsKind(SyntaxKind.GreaterThanToken) ||
                    token.IsKind(SyntaxKind.GreaterThanEqualsToken) ||
                    token.IsKind(SyntaxKind.AmpersandAmpersandToken) ||
                    token.IsKind(SyntaxKind.BarBarToken))
                {
                    operators.Add(token.Kind());
                }
            }

            return operators.Count;
        }

        protected override bool IsCommentLine(string line, ref bool inMultilineComment)
        {
            if (inMultilineComment)
            {
                if (line.Contains("*/"))
                {
                    inMultilineComment = false;
                    return true;
                }
                return true;
            }

            if (line.TrimStart().StartsWith("//"))
                return true;

            if (line.Contains("/*"))
            {
                inMultilineComment = !line.Contains("*/");
                return true;
            }

            return false;
        }
    }
}