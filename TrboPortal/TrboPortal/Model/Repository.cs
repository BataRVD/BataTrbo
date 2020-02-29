﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrboPortal.Controllers;

namespace TrboPortal.Model
{
    public class Repository
    {
        /// <summary>
        /// Generic insert or update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void InsertOrUpdate<T>(T entity) where T : class
        {
            using var db = new DatabaseContext();
            if (db.Entry(entity).State == EntityState.Detached)
                db.Set<T>().Add(entity);

            // If an immediate save is needed, can be slow though
            // if iterating through many entities:
            db.SaveChanges();
        }

        /// <summary>
        /// Insert or update Radio (based on key)
        /// </summary>
        /// <param name="settings"></param>
        public static void InsertOrUpdate(List<Radio> settings)
        {
            using var context = new DatabaseContext();
            foreach (var s in settings)
            {
                context.Entry(s).State = s.RadioId == 0 ? EntityState.Added : EntityState.Modified;
            }

            context.SaveChanges();
        }

    }
}