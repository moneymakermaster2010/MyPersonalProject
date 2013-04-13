using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Common;
using System.Data;
using DLIDBDataModels;
using System.Data.Entity.Validation;

namespace Repositories.Base
{
    public class RepositoryBase<T> : IDisposable where T : class 
    {
        private DbContext m_DLIDBContext;

        public DbContext DLIDBContext
        {
            get
            {
                if (m_DLIDBContext == null)
                {
                    m_DLIDBContext = new DownloadsFromDLIDatabaseEntities();
                }
                if (m_DLIDBContext.Database.Connection.State == ConnectionState.Closed)
                {
                    m_DLIDBContext.Database.Connection.Open();
                }
                return m_DLIDBContext;
            }
        }

        public T GetByID(int entityID)
        {
            return DLIDBContext.Set<T>().Find(entityID);
        }

        public List<T> GetAll()
        {
            return DLIDBContext.Set<T>().ToList();
        }
	
        public int Create(T entityToCreate)
        {
            DLIDBContext.Set<T>().Add(entityToCreate);
            return SaveChanges();
        }

        public int Update(T entityToUpdate)
        {
            DLIDBContext.Set<T>().Attach(entityToUpdate);
            return SaveChanges();
        }

        private int SaveChanges()
        {
            int recordsAffected;
            try
            {
                recordsAffected = DLIDBContext.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                string validationErrorMessage = string.Empty;
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {

                        validationErrorMessage = string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                throw new Exception(validationErrorMessage);
            }
            return recordsAffected;
        }

        public void Dispose()
        {
            DLIDBContext.Dispose();
        }
    }
}
