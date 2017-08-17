using System;
using CSScriptLibrary;

namespace Http.Gateway.Configuration
{
    public class RoutingRule
    {
        private string _expression = null;
        private readonly dynamic _script = null;

        public string Expression
        {
            get
            {
                return _expression;
            }
            private set
            {
                _expression = value;
                if (!string.IsNullOrWhiteSpace(_expression))
                {
                    var trimmed = _expression;

                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        IsDefault = true;
                    }
                }
                else
                {
                    IsDefault = true;
                }
            }
        }

        public string BackendId { get; private set; }

        public bool IsDefault { get; private set; }

        public bool IsValid { get; private set; }

        public string ExpressionError { get; private set; }

        public string Script
        {
            get
            {
                return
$@"using System;
public class Script
    {{
        public bool ShouldRoute(Http.Gateway.Configuration.RequestRoutingProperties request)
        {{
            return {Expression};
        }}
}}";
            }
        }

        public RoutingRule(string expression, string backendId)
        {
            if (string.IsNullOrWhiteSpace(backendId))
            {
                throw new ArgumentNullException(nameof(backendId));
            }

            BackendId = backendId;
            Expression = expression;

            if (!IsDefault)
            {

                try
                {
                    _script = CSScript.Evaluator.LoadCode(Script);
                    IsValid = true;
                }
                catch (csscript.CompilerException e)
                {
                    ExpressionError = e.Message;
                }
            }
            else
            {
                IsValid = true;
            }
        }

        public bool Match(Http.Gateway.Configuration.RequestRoutingProperties request)
        {
            return IsValid 
                   ? (IsDefault ? true : _script.ShouldRoute(request)) 
                   : false;
        }
    }
}