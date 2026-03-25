using Polly;
using Polly.Registry;

namespace SubsTracker.IntegrationTests.Configuration;

public class TestPipelineProvider : ResiliencePipelineProvider<string>
{
    private readonly ResiliencePipeline _pipeline;

    public TestPipelineProvider(ResiliencePipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override ResiliencePipeline GetPipeline(string key)
        => _pipeline;

    public override bool TryGetPipeline(string key, out ResiliencePipeline pipeline)
    {
        pipeline = _pipeline;
        return true;
    }

    public override bool TryGetPipeline<TResult>(string key, out ResiliencePipeline<TResult> pipeline)
    {
        pipeline = null!;
        return false;
    }
}
