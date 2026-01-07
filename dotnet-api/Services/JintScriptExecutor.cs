using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Jint;
using Jint.Native;
using FunctionExecutor.Models;

namespace FunctionExecutor.Services;

public class JintScriptExecutor : IScriptExecutor
{
    private readonly ILogger<JintScriptExecutor> _logger;
    private readonly ScriptExecutorOptions _options;
    
    public JintScriptExecutor(
        ILogger<JintScriptExecutor> logger,
        IOptions<ScriptExecutorOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }
    
    public ExecutionResult Execute(FunctionWrapper wrapper)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            // Create sandboxed Jint engine with limits
            var engine = CreateSandboxedEngine();
            
            // Build lookup structures from pre-loaded cost codes
            var costCodes = wrapper.CostCodes.ToList();
            var byId = costCodes.ToDictionary(c => c.Id);
            
            // Inject helper functions
            InjectHelpers(engine, costCodes, byId);
            
            // Wrap user function in IIFE to capture return value
            var wrappedFunction = $"(function() {{ {wrapper.TheFunction} }})()";
            
            // Execute
            var result = engine.Evaluate(wrappedFunction);
            
            sw.Stop();
            
            // Validate result is a number
            if (!result.IsNumber())
            {
                var actualType = result.Type.ToString();
                return ExecutionResult.Fail(
                    $"Function must return a number, but returned {actualType}",
                    sw.ElapsedMilliseconds);
            }
            
            var numericResult = (decimal)result.AsNumber();
            
            _logger.LogDebug(
                "Executed function {FunctionId} in {ElapsedMs}ms with result {Result}",
                wrapper.Id, sw.ElapsedMilliseconds, numericResult);
            
            return ExecutionResult.Ok(numericResult, sw.ElapsedMilliseconds);
        }
        catch (Jint.Runtime.JavaScriptException jsEx)
        {
            sw.Stop();
            _logger.LogWarning(jsEx, 
                "JavaScript error executing function {FunctionId}", wrapper.Id);
            
            return ExecutionResult.Fail(
                $"JavaScript Error: {jsEx.Message} (Line {jsEx.Location.Start.Line})",
                sw.ElapsedMilliseconds);
        }
        catch (TimeoutException)
        {
            sw.Stop();
            _logger.LogWarning(
                "Timeout executing function {FunctionId}", wrapper.Id);
            
            return ExecutionResult.Fail(
                $"Execution timed out after {_options.TimeoutMs}ms",
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, 
                "Unexpected error executing function {FunctionId}", wrapper.Id);
            
            return ExecutionResult.Fail(
                $"Execution error: {ex.Message}",
                sw.ElapsedMilliseconds);
        }
    }
    
    public (bool IsValid, string? Error) Validate(string functionString)
    {
        try
        {
            var engine = new Engine(options => options.Strict());
            
            // Try to parse without executing
            var wrapped = $"(function() {{ {functionString} }})";
            engine.Execute(wrapped);
            
            return (true, null);
        }
        catch (Jint.Runtime.JavaScriptException jsEx)
        {
            return (false, $"Syntax error: {jsEx.Message} (Line {jsEx.Location.Start.Line})");
        }
        catch (Exception ex)
        {
            return (false, $"Validation error: {ex.Message}");
        }
    }
    
    private Engine CreateSandboxedEngine()
    {
        return new Engine(options => options
            .TimeoutInterval(TimeSpan.FromMilliseconds(_options.TimeoutMs))
            .MaxStatements(_options.MaxStatements)
            .LimitRecursion(_options.MaxRecursionDepth)
            .Strict()
            .CatchClrExceptions()
        );
    }
    
    private void InjectHelpers(
        Engine engine, 
        List<CostCode> costCodes, 
        Dictionary<int, CostCode> byId)
    {
        // getCostCode(id) - Get a single cost code by ID
        engine.SetValue("getCostCode", new Func<int, object?>(id =>
        {
            if (byId.TryGetValue(id, out var code))
            {
                return ConvertToJsObject(code);
            }
            return null;
        }));
        
        // getCostCodes() - Get all pre-loaded cost codes
        engine.SetValue("getCostCodes", new Func<object>(() =>
        {
            return costCodes.Select(ConvertToJsObject).ToArray();
        }));
        
        // getChildren(parentId) - Get direct children of a cost code
        engine.SetValue("getChildren", new Func<int, object>(parentId =>
        {
            var children = costCodes
                .Where(c => c.ParentId == parentId)
                .Select(ConvertToJsObject)
                .ToArray();
            return children;
        }));

        // getDescendants(parentId) - Get all descendants recursively
        engine.SetValue("getDescendants", new Func<int, object>(parentId =>
        {
            var result = new List<object>();
            void CollectDescendants(int pid)
            {
                var children = costCodes.Where(c => c.ParentId == pid).ToList();
                foreach (var child in children)
                {
                    result.Add(ConvertToJsObject(child));
                    CollectDescendants(child.Id);
                }
            }
            CollectDescendants(parentId);
            return result.ToArray();
        }));

        // getParent(id) - Get parent of a cost code
        engine.SetValue("getParent", new Func<int, object?>(id =>
        {
            if (byId.TryGetValue(id, out var code) && code.ParentId.HasValue)
            {
                if (byId.TryGetValue(code.ParentId.Value, out var parent))
                {
                    return ConvertToJsObject(parent);
                }
            }
            return null;
        }));
        
        // Utility: console.log for debugging (outputs to server logs)
        var logs = new List<string>();
        engine.SetValue("console", new
        {
            log = new Action<object?>(msg =>
            {
                var logMsg = msg?.ToString() ?? "null";
                logs.Add(logMsg);
                _logger.LogDebug("JS Console: {Message}", logMsg);
            })
        });
    }
    
    /// <summary>
    /// Converts a CostCode to a plain object for JavaScript consumption
    /// </summary>
    private static object ConvertToJsObject(CostCode code)
    {
        return new
        {
            id = code.Id,
            parentId = code.ParentId,
            name = code.Name,
            value = (double)code.Value  // JS uses double for numbers
        };
    }
}

public class ScriptExecutorOptions
{
    public int TimeoutMs { get; set; } = 1000;       // 1 second default
    public int MaxStatements { get; set; } = 10000;   // Prevent infinite loops
    public int MaxRecursionDepth { get; set; } = 100; // Prevent stack overflow
}
