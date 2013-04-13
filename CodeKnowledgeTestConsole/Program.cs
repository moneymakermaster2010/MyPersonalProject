using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DLIDBDataModels;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeKnowledgeTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (var entities = new DownloadsFromDLIDatabaseEntities())
            //{
            //    User user = entities.Users.Find(2);
            //    if (user == null)
            //    {
            //        var user2 = new User();
            //        user2.UserName = "Suresh";
            //        entities.SaveChanges();
            //    }
            //}

            using (var entities = new MyDBContext())
            {
                TableA table1 = new TableA();
                table1.TableAId = 1;
                table1.Name = "MyFirstTable";
                //table1.TableBs = new List<TableB>();
                entities.Set<TableA>().Add(table1);
                entities.SaveChanges();
            }
        }

        
    }

    public class MyDBContext : DbContext
    {
        public DbSet<TableA> TableAs { get; set; }
        //public DbSet<TableB> TableBs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<MyDBContext>(new DropCreateDatabaseIfModelChanges<MyDBContext>());
            //base.OnModelCreating(modelBuilder);
        }
    }

    [Table("TableA")]
    public class TableA
    {
        //[Key]
        public int TableAId { get; set; }
        public String Name { get; set; }
        public String CreatedByUser { get; set; }

        public ICollection<TableB> TableBs { get; set; }
    }

    [Table("TableB")]
    public class TableB
    {
        //[Key]
        public int TableBID { get; set; }
        //[ForeignKey("TableA")]
        public int TableAID { get; set; }
        public String Name { get; set; }

        public TableA TableA { get; set; }
    }
}
