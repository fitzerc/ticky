using FluentAssertions;

namespace Ticky.Core.Tests;

public class TickyTimerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Start_ShouldBe_Ok()
    {
        var sut = new TickyTimer();

        var testValue = "beforeTest";
        var unexpected = "beforeTest";


        var startTimerResult = sut.Start(ts => testValue = ts.ToString(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

        startTimerResult.IsSuccess.Should().BeTrue();

        var count = 0;

        while (count < 10)
        {
            if (testValue != unexpected)
            {
                break;
            }
        }

        testValue.Should().NotBe(unexpected);

        sut.Stop();
    }

    [Test]
    public void Pause_ShouldBe_Ok()
    {
        var sut = new TickyTimer();

        sut.IsRunning().Should().BeFalse();

        var startTimerResult = sut.Start(_ => { });
        startTimerResult.IsSuccess.Should().BeTrue();

        sut.IsRunning().Should().BeTrue();

        var pauseTimerResult = sut.Pause();
        pauseTimerResult.IsSuccess.Should().BeTrue();

        sut.IsRunning().Should().BeFalse();

        sut.Stop();
    }

    [Test]
    public void Stop_Should_StopTicking()
    {
        var sut = new TickyTimer();

        var ticks = 0;

        _ = sut.Start(_ => ticks++, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

        sut.IsRunning().Should().BeTrue();

        var stopTimerResult = sut.Stop();

        var updatedTicks = ticks;

        stopTimerResult.IsSuccess.Should().BeTrue();

        ticks.Should().Be(updatedTicks);

        sut.Stop();
    }

    [Test]
    public void Pause_Unpause_ShouldBe_Ok()
    {
        var sut = new TickyTimer();

        sut.IsRunning().Should().BeFalse();

        var startTimerResult = sut.Start(_ => { });
        startTimerResult.IsSuccess.Should().BeTrue();

        sut.IsRunning().Should().BeTrue();

        var pauseTimerResult = sut.Pause();
        pauseTimerResult.IsSuccess.Should().BeTrue();

        sut.IsRunning().Should().BeFalse();

        var unpauseTimerResult = sut.Unpause();
        unpauseTimerResult.IsSuccess.Should().BeTrue();

        sut.IsRunning().Should().BeTrue();

        sut.Stop();
    }

    [Test]
    public void Pause_WhenNotStarted_Should_FailWithMessage()
    {
        var sut = new TickyTimer();
        var pauseTimerResult = sut.Pause();

        pauseTimerResult.IsFailed.Should().BeTrue();
        pauseTimerResult.Errors.Count.Should().NotBe(0);
        pauseTimerResult.Errors.FirstOrDefault()?.Message.Should().Be(TickyTimer.ErrorStrings.NullStopwatchOnPause);

        sut.Stop();
    }

    [Test]
    public void Unpause_WhenNotStarted_Should_FailWithMessage()
    {
        var sut = new TickyTimer();
        var unpauseTimerResult = sut.Unpause();

        unpauseTimerResult.IsFailed.Should().BeTrue();
        unpauseTimerResult.Errors.Count.Should().NotBe(0);
        unpauseTimerResult.Errors.FirstOrDefault()?.Message.Should().Be(TickyTimer.ErrorStrings.NullStopwatchOnUnpause);

        sut.Stop();
    }
}