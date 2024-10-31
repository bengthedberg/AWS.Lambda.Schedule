using Amazon.CDK;

namespace AWS.Lambda.Schedule.CDK
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new ApplicationStack(app, "ScheduleLambda", new StackProps
            {
            });
            app.Synth();
        }
    }
}