using EDMS.Core.Queue;

namespace EDMS.Tests;

public class QueueEngineTests
{
    [Fact]
    public void MM1_KnownValues_ShouldMatchExpected()
    {
        var engine = new MM1Engine();
        var result = engine.Compute(new QueueRequest
        {
            ModelType = "MM1",
            Lambda = 4,
            Mu = 8
        });

        Assert.Equal(0.5, result.Rho, 3);
        Assert.Equal(0.5, result.Lq, 3);
        Assert.Equal(0.125, result.Wq, 3);
    }

    [Fact]
    public void MM1_Unstable_ShouldThrow()
    {
        var engine = new MM1Engine();
        Assert.Throws<InvalidOperationException>(() => engine.Compute(new QueueRequest
        {
            ModelType = "MM1",
            Lambda = 9,
            Mu = 8
        }));
    }

    [Fact]
    public void MG1_WithCvOne_ShouldApproximateMM1()
    {
        var mm1 = new MM1Engine().Compute(new QueueRequest
        {
            ModelType = "MM1",
            Lambda = 4,
            Mu = 8
        });

        // Cv = sqrt(sigmaS2) * mu = 1  => sigmaS2 = 1 / mu^2 = 1/64
        var mg1 = new MG1Engine().Compute(new QueueRequest
        {
            ModelType = "MG1",
            Lambda = 4,
            Mu = 8,
            SigmaS2 = 1.0 / 64.0
        });

        Assert.Equal(mm1.Lq, mg1.Lq, 3);
        Assert.Equal(mm1.Wq, mg1.Wq, 3);
    }

    [Fact]
    public void GG1_MissingVariance_ShouldThrow()
    {
        var engine = new GG1Engine();
        Assert.Throws<ArgumentException>(() => engine.Compute(new QueueRequest
        {
            ModelType = "GG1",
            Lambda = 4,
            Mu = 8,
            SigmaS2 = 0.1
        }));
    }
}