using System;
using System.IO;
using NUnit.Framework;

namespace Tasks
{
	[TestFixture]
	public sealed class ApplicationTest
	{
		public const string PROMPT = "> ";

		private FakeConsole console;
		private System.Threading.Thread applicationThread;

		[SetUp]
		public void StartTheApplication()
		{
			this.console = new FakeConsole();
			var taskList = new TaskList(console);
			this.applicationThread = new System.Threading.Thread(() => taskList.Run());
			applicationThread.Start();
		}

		[TearDown]
		public void KillTheApplication()
		{
			if (applicationThread == null || !applicationThread.IsAlive)
			{
				return;
			}

			applicationThread.Abort();
			throw new Exception("The application is still running.");
		}

		[Test, Timeout(1000)]
		public void ItWorks()
		{
			Execute("show");

			Execute("add project secrets");
			Execute("add task secrets Eat more donuts.");
			Execute("add task secrets Destroy all humans.");

			Execute("show");
			ReadLines(
				"secrets",
				"    [ ] 1: Eat more donuts.",
				"    [ ] 2: Destroy all humans.",
				""
			);

			Execute("add project training");
			Execute("add task training Four Elements of Simple Design");
			Execute("add task training SOLID");
			Execute("add task training Coupling and Cohesion");
			Execute("add task training Primitive Obsession");
			Execute("add task training Outside-In TDD");
			Execute("add task training Interaction-Driven Design");

			Execute("check 1");
			Execute("check 3");
			Execute("check 5");
			Execute("check 6");

			Execute("show");
			ReadLines(
				"secrets",
				"    [x] 1: Eat more donuts.",
				"    [ ] 2: Destroy all humans.",
				"",
				"training",
				"    [x] 3: Four Elements of Simple Design",
				"    [ ] 4: SOLID",
				"    [x] 5: Coupling and Cohesion",
				"    [x] 6: Primitive Obsession",
				"    [ ] 7: Outside-In TDD",
				"    [ ] 8: Interaction-Driven Design",
				""
			);

			Execute("quit");
		}

		[Test, Timeout(1000)]
		public void NonExistingCommandAbandoned()
		{
			// Arrange
			Execute("nonExistingCommand");

			ReadLines("I don't know what the command \"nonExistingCommand\" is.");
			Execute("quit");
		}
		
		[Test, Timeout(1000)]
		public void SingleDeadlineCanBeAdded()
		{
			// Arrange
			Execute("show");

			Execute("add project secrets");
			Execute("add task secrets Eat more donuts.");
			Execute("add task secrets Destroy all humans.");
			// Act
			Execute("deadline 1 01-02-2025");
			Execute("show");

			ReadLines("secrets\n    [ ] 1: Eat more donuts.\n    [ ] 2: Destroy all humans.\n");
			Execute("quit");
		}

		[Test, Timeout(1000)]
		public void PastDeadlineCannotBeAdded()
		{
			// Arrange
			Execute("show");

			Execute("add project secrets");
			Execute("add task secrets Eat more donuts.");
			Execute("add task secrets Destroy all humans.");

			// Act
			Execute("deadline 1 01-02-1994");
			ReadLines("Deadline cannot be added; is in the past");
			Execute("quit");
		}
		
		// [Test, Timeout(1000)]
		// public void DeadlineCannotBeSetOnNonexistingId()
		// {
		// 	// Arrange
		// 	Execute("show");
		//
		// 	Execute("add project secrets");
		// 	Execute("add task secrets Eat more donuts.");
		// 	Execute("add task secrets Destroy all humans.");
		//
		// 	// Act
		// 	Execute("deadline 10 01-02-1994");
		// 	
		// 	// Assert
		// 	ReadLines("Deadline cannot be added; id does not exist");
		// 	Execute("quit");
		// }

		[Test, Timeout(1000)]
		public void TodayShowsTaskWithDeadlineToday()
		{
			// Arrange
			Execute("show");

			Execute("add project secrets");
			Execute("add task secrets Eat more donuts.");
			Execute("add task secrets Destroy all humans.");
			// Act
			Execute("deadline 1 22-08-2024");
			Execute("today");

			ReadLines("secrets\n    [ ] 1: Eat more donuts.\n");
			Execute("quit");
		}	
		
		[Test, Timeout(1000)]
		public void TodayShowsTasksWithDeadlineToday()
		{
			// Arrange
			Execute("show");

			Execute("add project secrets");
			Execute("add task secrets Eat more donuts.");
			Execute("add task secrets Destroy all humans.");
			// Act
			Execute("deadline 1 22-08-2024");
			Execute("deadline 2 22-08-2024");
			Execute("today");

			ReadLines("secrets\n    [ ] 1: Eat more donuts.\n    [ ] 2: Destroy all humans.\n");
			Execute("quit");
		}	

		private void Execute(string command)
		{
			Read(PROMPT);
			Write(command);
		}

		private void Read(string expectedOutput)
		{
			var length = expectedOutput.Length;
			var actualOutput = console.RetrieveOutput(expectedOutput.Length);
			Assert.AreEqual(expectedOutput, actualOutput);
		}

		private void ReadLines(params string[] expectedOutput)
		{
			foreach (var line in expectedOutput)
			{
				Read(line + Environment.NewLine);
			}
		}

		private void Write(string input)
		{
			console.SendInput(input + Environment.NewLine);
		}
	}
}
