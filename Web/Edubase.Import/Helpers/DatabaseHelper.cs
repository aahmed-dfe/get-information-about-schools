﻿using Edubase.Data.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edubase.Common;
using MoreLinq;
using Microsoft.SqlServer.Types;

namespace Edubase.Import.Helpers
{
    public static class DatabaseHelperExtensions
    {
        /// <summary>
        /// Generates data tables from Entity Framework entities
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Dictionary<Type, DataTable> GenerateDataTables(this ApplicationDbContext context)
        {
            const string SQL = "SELECT TOP 0 * FROM ";
            var retVal = new Dictionary<Type, DataTable>();
            var mappings = context.GetTable2TypeMappings();
            var connection = context.Database.Connection as SqlConnection;

            mappings.ForEach(x =>
            {
                var sql = string.Concat(SQL, x.Value);
                var dataTable = new DataTable() { TableName = x.Value };
                using (var adapter = new SqlDataAdapter(sql, connection))
                    adapter.Fill(dataTable);

                var geoColumn = dataTable.Columns.Cast<DataColumn>().Where(c => c.DataType.Name.Contains("SqlGeography")).FirstOrDefault();
                if (geoColumn != null) geoColumn.DataType = typeof(SqlGeography);

                retVal.Append(x.Key, dataTable);
            });

            return retVal;
        }

        public static Dictionary<Type, string> GetTable2TypeMappings(this ApplicationDbContext dataContext)
        {
            var retVal = new Dictionary<Type, string>();
            var metadata = ((IObjectContextAdapter)dataContext).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityTypes = metadata
                .GetItems<EntityType>(DataSpace.OSpace);

            foreach (var entityType in entityTypes)
            {
                var type = objectItemCollection.GetClrType(entityType);

                // Get the entity set that uses this entity type
                var entitySet = metadata
                    .GetItems<EntityContainer>(DataSpace.CSpace)
                    .Single()
                    .EntitySets
                    .Single(s => s.ElementType.Name == entityType.Name);

                // Find the mapping between conceptual and storage model for this entity set
                var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

                // Find the storage entity set (table) that the entity is mapped
                var table = mapping
                    .EntityTypeMappings.Single()
                    .Fragments.Single()
                    .StoreEntitySet;

                // Return the table name from the storage entity set
                var name = (string)table.MetadataProperties["Table"].Value ?? table.Name;

                retVal.Add(type, name);
            }

            return retVal;
        }

        
        public static DataTable Get<T>(this Dictionary<Type, DataTable> dictionary) => dictionary.Get(typeof(T));
    }
    
}