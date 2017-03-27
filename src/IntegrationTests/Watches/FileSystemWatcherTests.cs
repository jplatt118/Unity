﻿using GitHub.Unity;
using NUnit.Framework;
using System.Threading;
using FluentAssertions;
using NCrunch.Framework;

namespace IntegrationTests
{
    [TestFixture]
    class FileSystemWatcherTests : BaseIntegrationTest
    {
        [Test, Isolated]
        public void WatchesADirectoryTree()
        {
            int expected = 9;
            int count = 0;
            var platform = new Platform(new DefaultEnvironment(), FileSystem, new TestUIDispatcher());
            var watcher = platform.FileSystemWatchFactory.GetOrCreate(TestBasePath, true);

            watcher.Created += f =>
            {
                Logger.Debug("Created {0}", f);
                count++;
            };

            watcher.Enable = true;

            var files = new NPath[]
            {
                "file1",
                "dir1/dir1/file1",
                "dir1/dir1/dir1/file1",
                "dir1/dir1/dir2/file1",
                "dir1/dir1/dir2/file2",
            };

            CreateFilePaths(files);

            Thread.Sleep(50);

            watcher.Dispose();
            Assert.AreEqual(expected, count);
        }

        [Test, Isolated]
        public void WatchesAFile()
        {
            var createdCount = 0;
            var changedCount = 0;
            var renamedCount = 0;
            var deletedCount = 0;

            var platform = new Platform(new DefaultEnvironment(), FileSystem, new TestUIDispatcher());
            var file = TestBasePath.Combine("file.txt").CreateFile("foobar");
            var watcher = platform.FileSystemWatchFactory.GetOrCreate(file);

            watcher.Created += f =>
            {
                Logger.Debug("Created {0}", f);
                createdCount++;
            };

            watcher.Changed += f =>
            {
                Logger.Debug("Changed {0} {1}", f, f.ReadAllText());
                changedCount++;
            };

            watcher.Renamed += (old, n) =>
            {
                Logger.Debug("Renamed {0} {1}", old, n);
                renamedCount++;
            };

            watcher.Deleted += f =>
            {
                Logger.Debug("Deleted {0}", f);
                deletedCount++;
            };

            watcher.Enable = true;

            file.WriteAllText("FOOBAR");
            Thread.Sleep(120);

            //http://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice
            changedCount.Should().BeLessOrEqualTo(2);
            createdCount.Should().Be(0);
            renamedCount.Should().Be(0);
            deletedCount.Should().Be(0);

            watcher.Dispose();
        }
    }
}
