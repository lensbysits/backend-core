﻿using System.Text;

namespace Lens.Core.Lib.Extensions;

public static class ExceptionExtensions
{
    public static Dictionary<string, object>? GetSerializableDataDictionary(this Exception e, bool includeInnerExceptionData = false)
    {
        if (e == null || e.Data == null || e.Data.Count == 0)
        {
            return null;
        }

        Dictionary<string, object> dataResult = new();

        foreach (var k in e.Data.Keys)
        {
            if (k == null) // this use case has happened before
            {
                continue;
            }

            dataResult.TryAdd(k.ToString()!, e.Data[k]!);
        }

        if (includeInnerExceptionData && e.InnerException != null)
        {
            var innerExceptionData = GetSerializableDataDictionary(e.InnerException, includeInnerExceptionData);

            if (innerExceptionData != null && innerExceptionData.Count > 0)
            {
                foreach (var k in innerExceptionData.Keys)
                {
                    if (!dataResult.ContainsKey(k.ToString()))
                    {
                        dataResult.TryAdd(k.ToString(), innerExceptionData[k]);
                    }
                }
            }
        }

        return dataResult;
    }


    public static string GetFullExceptionData(this Exception e)
    {
        if (e == null)
        {
            return string.Empty;
        }

        StringBuilder exceptionMessage = new StringBuilder(e.Message);

        exceptionMessage.Append(" (" + e.GetType().Name + "):" + Environment.NewLine);
        exceptionMessage.AppendLine(string.Concat("\t Source: ", e.Source ?? ""));
        exceptionMessage.AppendLine(string.Concat("\t Stack Trace: ", e.StackTrace ?? ""));

        if (e.InnerException != null)
        {
            exceptionMessage.AppendLine("Inner Exception: ");


            var fullE = GetFullExceptionData(e.InnerException);

            if (!string.IsNullOrEmpty(fullE))
            {
                exceptionMessage.AppendLine(fullE);
            }
        }

        return exceptionMessage.ToString();
    }
}
