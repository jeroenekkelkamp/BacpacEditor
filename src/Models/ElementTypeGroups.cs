namespace BacpacEditor.Models;

public static class ElementTypeGroups
{
    public static readonly Dictionary<string, string[]> Groups = new()
    {
        ["--Views"] = ["SqlView"],
        ["--StoredProcedures"] = ["SqlProcedure"],
        ["--Functions"] = ["SqlFunction", "SqlInlineTableValuedFunction", "SqlMultiStatementTableValuedFunction", "SqlScalarFunction"],
        ["--Tables"] = ["SqlTable"],
        ["--Indexes"] = ["SqlIndex", "SqlIndexClustered", "SqlIndexNonClustered"],
        ["--Constraints"] = ["SqlCheckConstraint", "SqlDefaultConstraint", "SqlForeignKeyConstraint", "SqlPrimaryKeyConstraint", "SqlUniqueConstraint"],
        ["--Triggers"] = ["SqlDmlTrigger"],
        ["--Schemas"] = ["SqlSchema"],
        ["--Users"] = ["SqlUser"],
        ["--Roles"] = ["SqlRole"],
        ["--AllObjects"] = [
            "SqlView", "SqlProcedure", "SqlFunction", "SqlInlineTableValuedFunction", 
            "SqlMultiStatementTableValuedFunction", "SqlScalarFunction", "SqlTable", 
            "SqlIndex", "SqlIndexClustered", "SqlIndexNonClustered", "SqlCheckConstraint", 
            "SqlDefaultConstraint", "SqlForeignKeyConstraint", "SqlPrimaryKeyConstraint", 
            "SqlUniqueConstraint", "SqlDmlTrigger", "SqlSchema", "SqlUser", "SqlRole" 
        ]
    };
}
