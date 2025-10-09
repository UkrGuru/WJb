using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UkrGuru.WJb.Tests
{
    public class WorkerTests
    {
        private readonly ILogger<Worker> _logger = new NullLogger<Worker>();

        // Test-specific Worker class to control behavior and track method calls
        private class TestWorker : Worker
        {
            public int StartJobResult { get; set; }
            public bool ProcessJobResult { get; set; }
            public bool ThrowInProcessJob { get; set; }
            public bool ThrowInStartJob { get; set; }
            public bool ThrowInFinishJob { get; set; }
            public bool FinishJobCalled { get; private set; }
            public int FinishJobId { get; private set; }
            public bool FinishJobExecResult { get; private set; }

            public TestWorker(ILogger<Worker> logger) : base(logger) { }

            public override int NoDelay => 0;
            public override int MinDelay => 100;
            public override int NewDelay => 1000;
            public override int MaxDelay => 20000;

            public override async Task<int> StartJobAsync(CancellationToken stoppingToken)
            {
                stoppingToken.ThrowIfCancellationRequested(); // Respect cancellation token
                if (ThrowInStartJob)
                    throw new System.Exception("Start failed");
                return await Task.FromResult(StartJobResult);
            }

            public override async Task<bool> ProcessJobAsync(int jobId, CancellationToken stoppingToken)
            {
                stoppingToken.ThrowIfCancellationRequested(); // Respect cancellation token
                if (ThrowInProcessJob)
                    throw new System.Exception("Process failed");
                return await Task.FromResult(ProcessJobResult);
            }

            public override async Task FinishJobAsync(int jobId, bool exec_result, CancellationToken stoppingToken)
            {
                stoppingToken.ThrowIfCancellationRequested(); // Respect cancellation token
                FinishJobCalled = true;
                FinishJobId = jobId;
                FinishJobExecResult = exec_result;
                if (ThrowInFinishJob)
                    throw new System.Exception("Finish failed");
                await Task.CompletedTask;
            }
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(int.MaxValue, true)]
        public async Task DoWorkAsync_Handles_JobId_Cases(int jobId, bool expectedFinishCalled)
        {
            var worker = new TestWorker(_logger)
            {
                StartJobResult = jobId,
                ProcessJobResult = true
            };

            var delay = await worker.DoWorkAsync(CancellationToken.None);

            Assert.Equal(expectedFinishCalled ? worker.NoDelay : worker.NewDelay, delay);
            Assert.Equal(expectedFinishCalled, worker.FinishJobCalled);
        }

        [Fact]
        public async Task DoWorkAsync_Returns_NewDelay_When_StartJobAsync_Returns_Zero()
        {
            // Arrange
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 0 // Simulate no job available
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var delay = await worker.DoWorkAsync(cancellationToken);

            // Assert
            Assert.Equal(worker.NewDelay, delay); // Should return NewDelay (1000)
            Assert.False(worker.FinishJobCalled); // FinishJobAsync should not be called
        }

        [Fact]
        public async Task DoWorkAsync_Returns_NoDelay_When_Job_Processes_Successfully()
        {
            // Arrange
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 42, // Simulate a valid job ID
                ProcessJobResult = true // Simulate successful job processing
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var delay = await worker.DoWorkAsync(cancellationToken);

            // Assert
            Assert.Equal(worker.NoDelay, delay); // Should return NoDelay (0)
            Assert.True(worker.FinishJobCalled); // FinishJobAsync should be called
            Assert.Equal(42, worker.FinishJobId); // Correct job ID
            Assert.True(worker.FinishJobExecResult); // Correct execution result
        }

        [Fact]
        public async Task DoWorkAsync_Returns_NewDelay_When_ProcessJobAsync_Throws()
        {
            // Arrange
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 42, // Simulate a valid job ID
                ThrowInProcessJob = true // Simulate exception in ProcessJobAsync
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var delay = await worker.DoWorkAsync(cancellationToken);

            // Assert
            Assert.Equal(worker.NewDelay, delay); // Should return NewDelay (1000)
            Assert.True(worker.FinishJobCalled); // FinishJobAsync should be called
            Assert.Equal(42, worker.FinishJobId); // Correct job ID
            Assert.False(worker.FinishJobExecResult); // Should pass false due to exception
        }

        [Fact]
        public async Task DoWorkAsync_Returns_NewDelay_When_StartJobAsync_Throws()
        {
            // Arrange
            var worker = new TestWorker(_logger)
            {
                ThrowInStartJob = true // Simulate exception in StartJobAsync
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var delay = await worker.DoWorkAsync(cancellationToken);

            // Assert
            Assert.Equal(worker.NewDelay, delay); // Should return NewDelay (1000)
            Assert.False(worker.FinishJobCalled); // FinishJobAsync should not be called
        }

        [Fact]
        public async Task DoWorkAsync_Returns_NewDelay_When_FinishJobAsync_Throws()
        {
            // Arrange
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 42, // Simulate a valid job ID
                ProcessJobResult = true, // Simulate successful job processing
                ThrowInFinishJob = true // Simulate exception in FinishJobAsync
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var delay = await worker.DoWorkAsync(cancellationToken);

            // Assert
            Assert.Equal(worker.NewDelay, delay); // Should return NewDelay (1000) due to outer catch
            Assert.True(worker.FinishJobCalled); // FinishJobAsync should be called
            Assert.Equal(42, worker.FinishJobId); // Correct job ID
            Assert.True(worker.FinishJobExecResult); // Correct execution result
        }

        [Fact]
        public async Task DoWorkAsync_Handles_Cancellation_Before_StartJobAsync()
        {
            // Arrange
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 42 // Simulate a valid job ID
            };
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Act
            var delay = await worker.DoWorkAsync(cts.Token);

            // Assert
            Assert.Equal(worker.NewDelay, delay); // Should return NewDelay (1000) due to cancellation
            Assert.False(worker.FinishJobCalled); // FinishJobAsync should not be called
        }

        [Fact]
        public async Task DoWorkAsync_Can_Run_Multiple_Times_Consistently()
        {
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 1,
                ProcessJobResult = true
            };

            var delay1 = await worker.DoWorkAsync(CancellationToken.None);
            var delay2 = await worker.DoWorkAsync(CancellationToken.None);

            Assert.Equal(worker.NoDelay, delay1);
            Assert.Equal(worker.NoDelay, delay2);
            Assert.True(worker.FinishJobCalled);
        }

        [Fact]
        public async Task DoWorkAsync_Uses_Configured_Delay_Values()
        {
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 0
            };

            Assert.Equal(1000, worker.NewDelay);
            Assert.Equal(0, worker.NoDelay);
            Assert.Equal(100, worker.MinDelay);
            Assert.Equal(20000, worker.MaxDelay);
        }

        [Fact]
        public async Task DoWorkAsync_Catches_All_Exceptions()
        {
            var worker = new TestWorker(_logger)
            {
                ThrowInStartJob = true
            };

            var exceptionThrown = false;

            try
            {
                await worker.DoWorkAsync(CancellationToken.None);
            }
            catch
            {
                exceptionThrown = true;
            }

            Assert.False(exceptionThrown); // Should be caught internally
        }

        [Fact]
        public async Task DoWorkAsync_Is_Thread_Safe()
        {
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 42,
                ProcessJobResult = true
            };

            var tasks = Enumerable.Range(0, 10)
                .Select(_ => worker.DoWorkAsync(CancellationToken.None))
                .ToArray();

            await Task.WhenAll(tasks);

            Assert.True(worker.FinishJobCalled);
        }

        [Fact]
        public async Task DoWorkAsync_Handles_Cancellation_During_FinishJobAsync()
        {
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 42,
                ProcessJobResult = true,
                ThrowInFinishJob = false
            };

            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel before FinishJobAsync

            var delay = await worker.DoWorkAsync(cts.Token);

            Assert.Equal(worker.NewDelay, delay);
        }

        [Fact]
        public async Task DoWorkAsync_FinishJobAsync_Throws_After_State_Set()
        {
            var worker = new TestWorker(_logger)
            {
                StartJobResult = 42,
                ProcessJobResult = true,
                ThrowInFinishJob = true
            };

            var delay = await worker.DoWorkAsync(CancellationToken.None);

            Assert.True(worker.FinishJobCalled);
            Assert.Equal(42, worker.FinishJobId);
            Assert.True(worker.FinishJobExecResult); // Should still reflect correct result
            Assert.Equal(worker.NewDelay, delay); // Exception should be caught
        }
    }
}