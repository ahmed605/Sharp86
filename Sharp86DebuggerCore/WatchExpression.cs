﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaJson;

namespace Sharp86
{
    public class WatchExpression
    {
        public WatchExpression()
        {
        }

        public WatchExpression(Expression expression)
        {
            _expression = expression;
        }

        [Json("number")]
        public int Number;

        [Json("expression")]
        public string ExpressionText
        {
            get { return _expression == null ? null : _expression.OriginalExpression; }
            set { _expression = value == null ? null : new Expression(value); }
        }

        [Json("name")]
        public string Name
        {
            get;
            set;
        }

        public Expression Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        public string EvalAndFormat(DebuggerCore debugger)
        {
            try
            {
                return debugger.ExpressionContext.EvalAndFormat(_expression);
            }
            catch (Exception x)
            {
                return "err:" + x.Message;
            }
        }

        Expression _expression;

        public override string ToString()
        {
            return string.Format("#{0} - {1}", Number, _expression.OriginalExpression);
        }
    }
}
