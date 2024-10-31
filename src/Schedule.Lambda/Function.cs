using System.Text.Json;

using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Schedule.Lambda;

public class Function
{
    public void FunctionHandler(CloudWatchEvent<dynamic> input, ILambdaContext context)
    {
        // All log statements are written to CloudWatch by default. For more information, see
        // https://docs.aws.amazon.com/lambda/latest/dg/csharp-logging.html
        context.Logger.LogLine(JsonSerializer.Serialize(input));
    }
}