// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents the first expression in a LINQ query, which acts as the main query source.
  /// It is generated by <see cref="ExpressionTreeParser"/> when an <see cref="ParsedExpression"/> tree is parsed.
  /// This node usually marks the end (i.e. the first node) of an <see cref="IExpressionNode"/> chain that represents a query.
  /// </summary>
  public class MainSourceExpressionNode : IQuerySourceExpressionNode
  {
    public MainSourceExpressionNode (string associatedIdentifier, Expression expression)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("associatedIdentifier", associatedIdentifier);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckTypeIsAssignableFrom ("expression.Type", expression.Type, typeof (IEnumerable));

      QuerySourceType = expression.Type;
      QuerySourceElementType = ReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (expression.Type) ?? typeof (object);

      AssociatedIdentifier = associatedIdentifier;
      ParsedExpression = expression;
    }

    public Type QuerySourceElementType { get; private set; }
    public Type QuerySourceType { get; set; }
    public Expression ParsedExpression { get; private set; }
    public string AssociatedIdentifier { get; set; }

    public IExpressionNode Source
    {
      get { return null; }
    }

    public Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);

      // query sources resolve into references that point back to the respective clauses
      return QuerySourceExpressionNodeUtility.ReplaceParameterWithReference (
          this, 
          inputParameter, 
          expressionToBeResolved, 
          clauseGenerationContext);
    }

    public QueryModel Apply (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      if (queryModel != null)
        throw new ArgumentException ("QueryModel has to be null because MainSourceExpressionNode marks the start of a query.", "queryModel");

      var mainFromClause = CreateMainFromClause (clauseGenerationContext);
      var defaultSelectClause = new SelectClause (new QuerySourceReferenceExpression (mainFromClause));
      return new QueryModel (mainFromClause, defaultSelectClause) { ResultTypeOverride = QuerySourceType };
    }

    private MainFromClause CreateMainFromClause (ClauseGenerationContext clauseGenerationContext)
    {
      var fromClause = new MainFromClause (
          AssociatedIdentifier,
          QuerySourceElementType,
          ParsedExpression);

      clauseGenerationContext.AddContextInfo (this, fromClause);
      return fromClause;
    }
  }
}
