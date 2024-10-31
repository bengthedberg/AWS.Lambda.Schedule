using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Scheduler;
using Constructs;
using static Amazon.CDK.AWS.Scheduler.CfnSchedule;

namespace AWS.Lambda.Schedule.CDK
{
    public class ApplicationStack : Stack
    {
        internal ApplicationStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var buildOption = new BundlingOptions()
            {
                Image = Runtime.DOTNET_8.BundlingImage,
                User = "root",
                OutputType = BundlingOutput.ARCHIVED,
                Command = new string[]{
               "/bin/sh",
                "-c",
                " dotnet tool install -g Amazon.Lambda.Tools"+
                " && dotnet build"+
                " && dotnet lambda package --output-package /asset-output/Schedule.Lambda.zip -c Release --f net8 -farch arm64"
                }
            };

            var handler = new Function(this, "scheduler-lambda", new FunctionProps
            {
                MemorySize = 512,
                Timeout = Duration.Seconds(30),
                Runtime = Runtime.DOTNET_8,
                Architecture = Architecture.ARM_64,
                Handler = "Schedule.Lambda::Schedule.Lambda.Function::FunctionHandler",
                Code = Code.FromAsset("./src/Schedule.Lambda/", new Amazon.CDK.AWS.S3.Assets.AssetOptions
                {
                    Bundling = buildOption
                })
            });

            var schedulerRole = new Role(this, "scheduler-role", new RoleProps
            {
                AssumedBy = new ServicePrincipal("scheduler.amazonaws.com")
            });

            var inlinePolicy = new Policy(this, "schedule-policy", new PolicyProps
            {
                PolicyName = "ScheduleToInvokeLambdas",
                Roles = new[] { schedulerRole },
                Statements = new[]
                 {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new [] { "lambda:InvokeFunction"},
                        Resources = new [] { handler.FunctionArn }
                    })
                }
            });

            var scheduleGroup = new CfnScheduleGroup(this, "schedule-group", new CfnScheduleGroupProps
            {
                Name = "schedule-group-lambda"
            });

            var schedule = new CfnSchedule(this, "schedule-lambda", new CfnScheduleProps
            {
                GroupName = scheduleGroup.Name,
                FlexibleTimeWindow = new FlexibleTimeWindowProperty
                {
                    Mode = "OFF",
                },
                ScheduleExpression = "rate(5 minute)",
                Target = new TargetProperty
                {
                    Arn = handler.FunctionArn,
                    RoleArn = schedulerRole.RoleArn
                },
                Name = "schedule-lambda"
            });

        }
    }
}