﻿using System;
using System.Collections.Generic;
using System.Linq;
using BulkExtensionsIssue354.Models;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace BulkExtensionsIssue354
{
    class Program
    {
        static readonly BulkConfig BulkConfig = new BulkConfig
        {
            CalculateStats = true,
            SetOutputIdentity = true,
            PreserveInsertOrder = true
        };
        
        static void Main(string[] args)
        {
            Console.WriteLine(" --- Issue #354 EFCore.BulkExtensions --- ");
            var db = new DbContext();
            
            Console.WriteLine(" - Cleaning up...");
            db.Database.EnsureDeleted();
            
            Console.WriteLine(" - Migrating...");
            db.Database.Migrate();
            
            Console.WriteLine(" - Adding IDs...");
            var idCs = new List<IdC>
            {
                new IdC("Cake"),
                new IdC("Cookie"),
                new IdC("Biscuit")
            };
            db.IdCs.AddRange(idCs);
            db.SaveChanges();
            
            Console.WriteLine(" - Bulk inserting data... try #1");
            BulkInsertData(db);
            Console.WriteLine(" - Bulk inserting data... try #2");
            BulkInsertData(db); // TODO: ERROR OCCURS HERE
            Console.WriteLine(" - Bulk inserting data... try #3");
            BulkInsertData(db);
        }

        static void BulkInsertData(DbContext db)
        {
            var entityAs = new List<EntityA>
            {
                new EntityA("Sample 1"),
                new EntityA("Sample 2"),
                new EntityA("Sample 3"),
                new EntityA("Sample 4")
            };
            var entityBs = new List<EntityB>
            {
                new EntityB(),
                new EntityB(),
                new EntityB(),
                new EntityB()
            };
            var entityCs = new List<EntityC>
            {
                new EntityC(1),
                new EntityC(2),
                new EntityC(3),
                new EntityC(4),
                new EntityC(5),
                new EntityC(6),
                new EntityC(7),
                new EntityC(8),
                new EntityC(9),
                new EntityC(10),
                new EntityC(11),
                new EntityC(12)
            };
            
            db.BulkInsert(entityAs, BulkConfig);
            Console.WriteLine(" - Inserted EntityAs.");
            for (var i = 0; i < entityAs.Count; i++)
            {
                entityBs[i].Id = -entityAs.Count + i;
                entityBs[i].EntityAId = entityAs[i].Id;
            }
            // Shuffle these to check the preserved insert order
            entityBs = entityBs.OrderBy(_ => Guid.NewGuid()).ToList();
            
            db.BulkInsert(entityBs, BulkConfig);
            db.BulkRead(entityBs, new BulkConfig { UpdateByProperties = new List<string> { nameof(EntityB.EntityAId) }});
            Console.WriteLine(" - Inserted EntityBs.");
            for (var i = 0; i < entityBs.Count; i++)
            {
                entityCs[i * 3].EntityAId = entityBs[i].EntityAId;
                entityCs[i * 3 + 1].EntityAId = entityBs[i].EntityAId;
                entityCs[i * 3 + 2].EntityAId = entityBs[i].EntityAId;
                entityCs[i * 3].EntityBId = entityBs[i].Id;
                entityCs[i * 3 + 1].EntityBId = entityBs[i].Id;
                entityCs[i * 3 + 2].EntityBId = entityBs[i].Id;
                entityCs[i * 3].Id = 1;
                entityCs[i * 3 + 1].Id = 2;
                entityCs[i * 3 + 2].Id = 3;
            }
            // Shuffle these to check the preserved insert order
            entityCs = entityCs.OrderBy(_ => Guid.NewGuid()).ToList();
            
            db.BulkInsert(entityCs, BulkConfig);
            Console.WriteLine(" - Inserted EntityCs.");
        }
    }
}