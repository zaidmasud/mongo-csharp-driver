/* Copyright 2010-2012 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace MongoDB.DriverUnitTests.GridFS
{
    [TestFixture]
    public class MongoGridFSTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS = database.GridFS;
                gridFS.Chunks.RemoveAll();
                gridFS.Chunks.ResetIndexCache();
                gridFS.Files.RemoveAll();
            }
        }

        [Test]
        public void TestConstructorFeezesSettings()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);

                var settings = new MongoGridFSSettings(database);
                Assert.IsFalse(settings.IsFrozen);
                var gridFS = new MongoGridFS(database, settings);
                Assert.IsTrue(gridFS.Settings.IsFrozen);
            }
        }

        [Test]
        public void TestCopyTo()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.AreEqual(0, gridFS.Chunks.Count());
                Assert.AreEqual(0, gridFS.Files.Count());

                var contents = "Hello World";
                var bytes = Encoding.UTF8.GetBytes(contents);
                var uploadStream = new MemoryStream(bytes);
                var createOptions = new MongoGridFSCreateOptions
                {
                    Aliases = new[] { "HelloWorld", "HelloUniverse" },
                    ChunkSize = gridFS.Settings.ChunkSize,
                    ContentType = "text/plain",
                    Id = ObjectId.GenerateNewId(),
                    Metadata = new BsonDocument { { "a", 1 }, { "b", 2 } },
                    UploadDate = DateTime.UtcNow
                };
                var fileInfo = gridFS.Upload(uploadStream, "HelloWorld.txt", createOptions);
                var copyInfo = fileInfo.CopyTo("HelloWorld2.txt");
                Assert.AreEqual(2, gridFS.Chunks.Count());
                Assert.AreEqual(2, gridFS.Files.Count());
                Assert.IsNull(copyInfo.Aliases);
                Assert.AreEqual(fileInfo.ChunkSize, copyInfo.ChunkSize);
                Assert.AreEqual(fileInfo.ContentType, copyInfo.ContentType);
                Assert.AreNotEqual(fileInfo.Id, copyInfo.Id);
                Assert.AreEqual(fileInfo.Length, copyInfo.Length);
                Assert.AreEqual(fileInfo.MD5, copyInfo.MD5);
                Assert.AreEqual(fileInfo.Metadata, copyInfo.Metadata);
                Assert.AreEqual("HelloWorld2.txt", copyInfo.Name);
                Assert.AreEqual(fileInfo.UploadDate, copyInfo.UploadDate);
            }
        }

        [Test]
        public void TestAppendText()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                Assert.IsFalse(gridFS.Exists("HelloWorld.txt"));
                using (var writer = gridFS.AppendText("HelloWorld.txt"))
                {
                    Assert.IsFalse(writer.BaseStream.CanRead);
                    Assert.IsTrue(writer.BaseStream.CanSeek);
                    Assert.IsTrue(writer.BaseStream.CanWrite);
                    writer.Write("Hello");
                }
                Assert.IsTrue(gridFS.Exists("HelloWorld.txt"));
                using (var writer = gridFS.AppendText("HelloWorld.txt"))
                {
                    writer.Write(" World");
                }
                var memoryStream = new MemoryStream();
                gridFS.Download(memoryStream, "HelloWorld.txt");
                var bytes = memoryStream.ToArray();
                Assert.AreEqual(0xEF, bytes[0]); // the BOM
                Assert.AreEqual(0xBB, bytes[1]);
                Assert.AreEqual(0xBF, bytes[2]);
                var text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                Assert.AreEqual("Hello World", text);
            }
        }

        [Test]
        public void TestDeleteByFileId()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.AreEqual(0, gridFS.Chunks.Count());
                Assert.AreEqual(0, gridFS.Files.Count());

                var fileInfo = UploadHelloWorld(database);
                Assert.AreEqual(1, gridFS.Chunks.Count());
                Assert.AreEqual(1, gridFS.Files.Count());

                gridFS.DeleteById(fileInfo.Id);
                Assert.AreEqual(0, gridFS.Chunks.Count());
                Assert.AreEqual(0, gridFS.Files.Count());
            }
        }

        [Test]
        public void TestDeleteByFileName()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.AreEqual(0, gridFS.Chunks.Count());
                Assert.AreEqual(0, gridFS.Files.Count());

                UploadHelloWorld(database);
                Assert.AreEqual(1, gridFS.Chunks.Count());
                Assert.AreEqual(1, gridFS.Files.Count());

                gridFS.Delete("HelloWorld.txt");
                Assert.AreEqual(0, gridFS.Chunks.Count());
                Assert.AreEqual(0, gridFS.Files.Count());
            }
        }

        [Test]
        public void TestDeleteAll()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.AreEqual(0, gridFS.Chunks.Count());
                Assert.AreEqual(0, gridFS.Files.Count());
            }
        }

        [Test]
        public void TestDownload()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                var fileInfo = UploadHelloWorld(database);

                var downloadStream = new MemoryStream();
                gridFS.Download(downloadStream, fileInfo);
                var downloadedBytes = downloadStream.ToArray();
                var downloadedContents = Encoding.UTF8.GetString(downloadedBytes);
                Assert.AreEqual("Hello World", downloadedContents);
            }
        }

        [Test]
        public void TestDownloadDontVerifyMD5()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                var fileInfo = UploadHelloWorld(database, false);

                var settings = new MongoGridFSSettings(database) { VerifyMD5 = false };
                gridFS = database.GetGridFS(settings);
                var downloadStream = new MemoryStream();
                gridFS.Download(downloadStream, fileInfo);
                var downloadedBytes = downloadStream.ToArray();
                var downloadedContents = Encoding.UTF8.GetString(downloadedBytes);
                Assert.AreEqual("Hello World", downloadedContents);
            }
        }

        [Test]
        public void TestDownloadTwoChunks()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                var contents = new string('x', 256 * 1024) + new string('y', 256 * 1024);
                var bytes = Encoding.UTF8.GetBytes(contents);
                var stream = new MemoryStream(bytes);
                var fileInfo = gridFS.Upload(stream, "TwoChunks.txt");
                Assert.AreEqual(2 * fileInfo.ChunkSize, fileInfo.Length);
                Assert.AreEqual(2, gridFS.Chunks.Count());
                Assert.AreEqual(1, gridFS.Files.Count());

                var downloadStream = new MemoryStream();
                gridFS.Download(downloadStream, fileInfo);
                var downloadedBytes = downloadStream.ToArray();
                var downloadedContents = Encoding.UTF8.GetString(downloadedBytes);
                Assert.AreEqual(contents, downloadedContents);
            }
        }

        [Test]
        public void TestExists()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.IsFalse(gridFS.Exists("HelloWorld.txt"));

                var fileInfo = UploadHelloWorld(database);
                Assert.IsTrue(gridFS.Exists("HelloWorld.txt"));
                Assert.IsTrue(gridFS.ExistsById(fileInfo.Id));
            }
        }

        [Test]
        public void TestFindAll()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.IsFalse(gridFS.Exists("HelloWorld.txt"));

                var fileInfo = UploadHelloWorld(database);
                foreach (var foundInfo in gridFS.FindAll())
                {
                    Assert.AreEqual(fileInfo, foundInfo);
                }
            }
        }

        [Test]
        public void TestFindByName()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.IsFalse(gridFS.Exists("HelloWorld.txt"));

                var fileInfo = UploadHelloWorld(database);
                foreach (var foundInfo in gridFS.Find("HelloWorld.txt"))
                {
                    Assert.AreEqual(fileInfo, foundInfo);
                }
            }
        }

        [Test]
        public void TestFindOneById()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.IsFalse(gridFS.Exists("HelloWorld.txt"));

                var fileInfo = UploadHelloWorld(database);
                var foundInfo = gridFS.FindOneById(fileInfo.Id);
                Assert.AreEqual(fileInfo, foundInfo);
            }
        }

        [Test]
        public void TestFindOneByName()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.IsFalse(gridFS.Exists("HelloWorld.txt"));

                var fileInfo = UploadHelloWorld(database);
                var foundInfo = gridFS.FindOne("HelloWorld.txt");
                Assert.AreEqual(fileInfo, foundInfo);
            }
        }

        [Test]
        public void TestFindOneNewest()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.IsFalse(gridFS.Exists("HelloWorld.txt"));

                var fileInfo1 = UploadHelloWorld(database);
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
                var fileInfo2 = UploadHelloWorld(database);
                var foundInfo = gridFS.FindOne("HelloWorld.txt", -1);
                Assert.AreEqual(fileInfo2, foundInfo);
            }
        }

        [Test]
        public void TestFindOneOldest()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.IsFalse(gridFS.Exists("HelloWorld.txt"));

                var fileInfo1 = UploadHelloWorld(database);
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
                var fileInfo2 = UploadHelloWorld(database);
                var foundInfo = gridFS.FindOne("HelloWorld.txt", 1);
                Assert.AreEqual(fileInfo1, foundInfo);
            }
        }

        [Test]
        public void TestMoveTo()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.AreEqual(0, gridFS.Chunks.Count());
                Assert.AreEqual(0, gridFS.Files.Count());

                var contents = "Hello World";
                var bytes = Encoding.UTF8.GetBytes(contents);
                var uploadStream = new MemoryStream(bytes);
                var fileInfo = gridFS.Upload(uploadStream, "HelloWorld.txt");
                Assert.AreEqual(1, gridFS.Chunks.Count());
                Assert.AreEqual(1, gridFS.Files.Count());

                gridFS.MoveTo("HelloWorld.txt", "HelloWorld2.txt");
                Assert.AreEqual(1, gridFS.Chunks.Count());
                Assert.AreEqual(1, gridFS.Files.Count());
                var movedInfo = gridFS.FindOne("HelloWorld2.txt");
                Assert.AreEqual("HelloWorld2.txt", movedInfo.Name);
                Assert.AreEqual(fileInfo.Id, movedInfo.Id);
            }
        }

        [Test]
        public void TestSetAliases()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                var fileInfo = UploadHelloWorld(database);
                Assert.IsNull(fileInfo.Aliases);

                var aliases = new string[] { "a", "b" };
                gridFS.SetAliases(fileInfo, aliases);
                fileInfo.Refresh();
                Assert.IsTrue(aliases.SequenceEqual(fileInfo.Aliases));

                gridFS.SetAliases(fileInfo, null);
                fileInfo.Refresh();
                Assert.IsNull(fileInfo.Aliases);
            }
        }

        [Test]
        public void TestSetContentType()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                var fileInfo = UploadHelloWorld(database);
                Assert.IsNull(fileInfo.ContentType);

                gridFS.SetContentType(fileInfo, "text/plain");
                fileInfo.Refresh();
                Assert.AreEqual("text/plain", fileInfo.ContentType);

                gridFS.SetContentType(fileInfo, null);
                fileInfo.Refresh();
                Assert.IsNull(fileInfo.ContentType);
            }
        }

        [Test]
        public void TestSetMetadata()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                var fileInfo = UploadHelloWorld(database);
                Assert.IsNull(fileInfo.Metadata);

                var metadata = new BsonDocument { { "a", 1 }, { "b", 2 } };
                gridFS.SetMetadata(fileInfo, metadata);
                fileInfo.Refresh();
                Assert.AreEqual(metadata, fileInfo.Metadata);

                gridFS.SetMetadata(fileInfo, null);
                fileInfo.Refresh();
                Assert.IsNull(fileInfo.Metadata);
            }
        }

        [Test]
        public void TestUpload()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var gridFS = database.GridFS;

                gridFS.Delete(Query.Null);
                Assert.AreEqual(0, gridFS.Chunks.Count());
                Assert.AreEqual(0, gridFS.Files.Count());

                var contents = "Hello World";
                var bytes = Encoding.UTF8.GetBytes(contents);
                var uploadStream = new MemoryStream(bytes);
                var createOptions = new MongoGridFSCreateOptions
                {
                    Aliases = new[] { "HelloWorld", "HelloUniverse" },
                    ChunkSize = gridFS.Settings.ChunkSize,
                    ContentType = "text/plain",
                    Id = ObjectId.GenerateNewId(),
                    Metadata = new BsonDocument { { "a", 1 }, { "b", 2 } },
                    UploadDate = DateTime.UtcNow
                };
                var fileInfo = gridFS.Upload(uploadStream, "HelloWorld.txt", createOptions);
                Assert.AreEqual(1, gridFS.Chunks.Count());
                Assert.AreEqual(1, gridFS.Files.Count());
                Assert.IsTrue(createOptions.Aliases.SequenceEqual(fileInfo.Aliases));
                Assert.AreEqual(createOptions.ChunkSize, fileInfo.ChunkSize);
                Assert.AreEqual(createOptions.ContentType, fileInfo.ContentType);
                Assert.AreEqual(createOptions.Id, fileInfo.Id);
                Assert.AreEqual(11, fileInfo.Length);
                Assert.IsTrue(!string.IsNullOrEmpty(fileInfo.MD5));
                Assert.AreEqual(createOptions.Metadata, fileInfo.Metadata);
                Assert.AreEqual("HelloWorld.txt", fileInfo.Name);
                Assert.AreEqual(createOptions.UploadDate.AddTicks(-(createOptions.UploadDate.Ticks % 10000)), fileInfo.UploadDate);
            }
        }

        private MongoGridFSFileInfo UploadHelloWorld(MongoDatabase database)
        {
            return UploadHelloWorld(database, true);
        }

        private MongoGridFSFileInfo UploadHelloWorld(MongoDatabase database, bool verifyMD5)
        {
            var settings = new MongoGridFSSettings(database) { VerifyMD5 = verifyMD5 };
            var gridFS = database.GetGridFS(settings);
            var bytes = Encoding.UTF8.GetBytes("Hello World");
            var stream = new MemoryStream(bytes);
            return gridFS.Upload(stream, "HelloWorld.txt");
        }
    }
}
