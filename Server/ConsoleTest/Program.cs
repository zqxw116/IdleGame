using GameDB;
using Microsoft.EntityFrameworkCore;

namespace ConsoleTest
{
    // Create / Read / Update / Delete 해보기
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create
            using (GameDbContext db = new GameDbContext())
            {
                TestDb testDb = new TestDb();
                testDb.Name = "Jnote";

                db.Tests.Add(testDb);
                db.SaveChanges();
            }

            // Read
            using (GameDbContext db = new GameDbContext())
            {
                TestDb? testDb = db.Tests.FirstOrDefault(d => d.Name == "Jnote");
                if (testDb != null)
                {
                    int count = testDb.TestDbId;
                }
            }

            // Update
            using (GameDbContext db = new GameDbContext())
            {
                TestDb? testDb = db.Tests.FirstOrDefault(d => d.Name == "Jnote");
                if (testDb != null)
                {
                    testDb.Name = "jjjJnote";
                    db.SaveChanges();
                }
            }


            // Delete
            using (GameDbContext db = new GameDbContext())
            {
                // 1. 직접 삭제
                TestDb? testDb = db.Tests.FirstOrDefault(d => d.Name == "Jnote");
                if (testDb != null)
                {
                    db.Tests.Remove(testDb);
                    db.SaveChanges();
                }

                // 2. Entity Tracking
                {
                    TestDb? testDb2 = new TestDb();
                    testDb2.TestDbId = 1;

                    var entry = db.Tests.Entry(testDb2);
                    entry.State = EntityState.Deleted;
                    db.SaveChanges();
                }

                // 3. EF Core 7
                {
                    db.Tests.Where(a => a.Name.Contains("Jnote")).ExecuteDelete();
                }
            }
        }
    }
}
