using System;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Utilities;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.Clauses
{
  public class MainFromClause : FromClauseBase
  {
    public MainFromClause (ParameterExpression identifier, Expression querySource): base(null,identifier)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      QuerySource = querySource;
    }

    public Expression QuerySource { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitMainFromClause (this);
    }

    public override Type GetQuerySourceType ()
    {
      return QuerySource.Type;
    }
  }
}