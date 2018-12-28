﻿using CQELight.EventStore.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.EFCore.Common
{
    class ArchiveEventStoreDbContext : DbContext
    {
        #region Ctor

        public ArchiveEventStoreDbContext(DbContextOptions contextOptions)
            : base(contextOptions)
        {
        }

        #endregion

        #region Overriden methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new EventArchiveEntityTypeConfiguration());
        }

        #endregion

    }
}
