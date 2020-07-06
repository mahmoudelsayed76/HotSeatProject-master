using Cointeco;
using Microsoft.Azure.Cosmos;
using SpotOn.DAL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpotOn.Base
{
    public class CosmosHelper
    {

        #region Static Initializer

        private static CosmosHelper _cosmosHelper;
        private static CosmosClient _cosmosClient;
        private static Database _database;

        /// <summary>
        /// initialize the Single instance  
        /// </summary>
        /// <param name="EndpointUri"></param>
        /// <param name="PrimaryKey"></param>
        /// <param name="databaseId"></param>
        /// <returns></returns>
        public static bool Initialize(string EndpointUri, string PrimaryKey, string databaseId)
        {
            try
            {
                _cosmosHelper = new CosmosHelper();
                _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

                // Link to database 
                var awaitableTask = _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId).ConfigureAwait(false);
                _database = awaitableTask.GetAwaiter().GetResult();

                // setup autoincrment table 
                _cosmosHelper.InitAutoIncrement();

                return true;

            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("CosmosHelper.Initialize() Exception:  {ex}", ex.Message);
            }
            return false;
        }
        #endregion

        #region Auto Increment Feature 
        // this container holds the integers for auto-increment
        private const string AUTO_INCREMENT_CONTAINER_ID = "_auto_inc_";
        private const string AUTO_INC_ID = "_auto_inc_.1";

        // when we need an ID, we just grab it from this variable 
        private static int _nextUnusedId = 0;

        // if _nextUnusedId == _maxUnusedId , we need to request more from the AutoInc Table 
        private static int _maxUnusedId = 0;

        // always start at 100 
        private static int _idStart = 100;

        // with a batch of 10 
        private static int _idBatchSize = 10;

        /// <summary>
        /// return the next ID
        /// </summary>
        private int NextId
        {
            get
            {
                int retryCount = 5;
                while (_nextUnusedId >= _maxUnusedId && retryCount > 0)
                {
                    if (!this.ReserveNextAutoIncrementBatch())
                    {
                        // if ReserveNextAutoIncrementBatch() fails, retry after 0.5 s
                        Thread.Sleep(500);
                        retryCount--;
                        if (retryCount <= 0)
                            throw new Exception("NextId - unable to call ReserveNextAutoIncrementBatch() successfully. Retries exhausted.");
                    }
                }
                _nextUnusedId++;
                return _nextUnusedId;
            }
        }

        /// <summary>
        /// setup the auto increment table 
        /// </summary>
        private void InitAutoIncrement()
        {
            try
            {
                // connect to the container 
                var container = this.ConnectContainer(AUTO_INCREMENT_CONTAINER_ID);
                //  string id = "_auto_inc_.1";
                ItemResponse<AutoIncrement> itemResponse = null;

                try
                {
                    // see if the table  exists 
                    var awaitableTask = container.ReadItemAsync<AutoIncrement>($"{AUTO_INC_ID}", new PartitionKey($"{AUTO_INC_ID}")).ConfigureAwait(false);
                    itemResponse = awaitableTask.GetAwaiter().GetResult();

                    // it does - just get the max id
                    var ai = itemResponse.Resource;
                    _maxUnusedId = ai.MaxId;
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound) // it doesn't - create it.
                {
                    _maxUnusedId = _idStart;
                    var ai = new AutoIncrement() { id = AUTO_INC_ID, MaxId = _maxUnusedId };
                    var awaitableTask = container.CreateItemAsync<AutoIncrement>(ai, new PartitionKey(AUTO_INC_ID)).ConfigureAwait(false);
                    itemResponse = awaitableTask.GetAwaiter().GetResult();
                }

                // doing this will cause the NextId function to trigger ReserveNextAutoIncrementBatch() if Id's are needed
                _nextUnusedId = _maxUnusedId;
                CommonBase.Logger.Information("CosmosHelper.InitAutoIncrement() Max-UnusedId = {id}", _maxUnusedId);

            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("CosmosHelper.InitAutoIncrement() Exception:  {ex}", ex.Message);
            }

        }

#if DEBUG

        public void TestAutoIncrement()
        {
            for (int i = 0; i < 100; i++)
            {
                CommonBase.Logger.Information("Next Id = {id}", NextId);
            }

        }
#endif 
        /// <summary>
        /// retrieves a batch of auto-increment numbers and updates the globals, 
        /// _nextUnusedId and _maxUnusedId
        /// </summary>
        private bool ReserveNextAutoIncrementBatch()
        {
            try
            {
                // connect to the container 
                var container = this[AUTO_INCREMENT_CONTAINER_ID];
                ItemResponse<AutoIncrement> itemResponse = null;

                // update the max used id by adding the batch size 
                var newMax = _maxUnusedId + _idBatchSize;
                var ai = new AutoIncrement() { id = AUTO_INC_ID, MaxId = newMax };

                var awaitableTask = container.UpsertItemAsync<AutoIncrement>(ai, new PartitionKey(AUTO_INC_ID)).ConfigureAwait(false);
                itemResponse = awaitableTask.GetAwaiter().GetResult();

                if (itemResponse.StatusCode == HttpStatusCode.OK)
                {
                    // update globals 
                    _maxUnusedId = newMax;
                }
                return true;
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("CosmosHelper.ReserveNextAutoIncrementBatch() Exception:  {ex}", ex.Message);
            }
            return false;
        }

        #endregion

        public static CosmosHelper Instance
        {
            get
            {
                if (_cosmosHelper is null)
                    throw new Exception("Call Initialize() before accesssing [CosmosHelperInstance]");
                return _cosmosHelper;
            }
        }

        /// <summary>
        /// every time a container is connected to in ConnectContainer(), it is added here
        /// </summary>
        private Dictionary<string, Container> _containers;

        public Container this[string containerId]
        {
            get
            {
                if (_containers.ContainsKey(containerId))
                    return _containers[containerId];
                else
                    return null;
            }
        }

        public Container ConnectContainer(string containerId, string partitionKey = null)
        {
            try
            {
                // setup a dictionary of containers
                if (_containers is null)
                    _containers = new Dictionary<string, Container>(10);

                partitionKey ??= "/id";
                if (!partitionKey.StartsWith("/")) partitionKey = $"/{partitionKey}";
                var awaitableTask = _database.CreateContainerIfNotExistsAsync(containerId, partitionKey ?? "/id").ConfigureAwait(false);
                var container = awaitableTask.GetAwaiter().GetResult();
                _containers.Add(containerId, container);
                return container;
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("CosmosHelper.GetSert() Exception:  {ex}", ex.Message);
            }
            return null;
        }

        /// <summary>
        ///  Update or insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="containerId"></param>
        /// <param name="item"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public T Getsert<T>(string containerId, T item, string partitionKey = null) where T : IModel
        {
            ItemResponse<T> itemResponse = null;
            string stringId = Guid.NewGuid().ToString();
            partitionKey ??= stringId;
            try
            {
                // Read the item to see if it exists.  
                var awaitableTask = this[containerId].ReadItemAsync<T>(stringId, new PartitionKey(partitionKey)).ConfigureAwait(false);
                itemResponse = awaitableTask.GetAwaiter().GetResult();
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                var awaitableTask = this[containerId].CreateItemAsync<T>(item, new PartitionKey(partitionKey));
                itemResponse = awaitableTask.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("CosmosHelper.Upsert() Exception:  {ex}", ex.Message);
            }

            if (itemResponse != null)
                return itemResponse.Resource;
            else
                return default;
        }

        public T Insert<T>(string containerId, T item, string partitionKey = null) where T : IModel
        {
            T retVal = default;

            // get auto-incremented id, even if one is supplied (this is to ensure that items are unique) 
            item.Id = NextId;
            try
            {
                var awaitableTask = this[containerId].CreateItemAsync<T>(item, new PartitionKey(item.Id.ToString()));
                ItemResponse<T> itemResponse = awaitableTask.GetAwaiter().GetResult();
                retVal = itemResponse.Resource;
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("CosmosHelper.Insert() Exception:  {ex}", ex.Message);
            }
            return retVal;
        }

        public List<T> Query<T>(string containerId, string sqlQueryText) where T : IModel
        {
            List<T> items = new List<T>();
            try
            {
                // var sqlQueryText = "SELECT * FROM c WHERE c.LastName = 'Andersen'";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                var container = this[containerId];
                FeedIterator<T> queryResultSetIterator = container.GetItemQueryIterator<T>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    var awaitableTask = queryResultSetIterator.ReadNextAsync().ConfigureAwait(false);
                    FeedResponse<T> currentResultSet = awaitableTask.GetAwaiter().GetResult();
                    foreach (T result in currentResultSet)
                    {
                        items.Add(result);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBase.Logger.Error("CosmosHelper.Upsert() Exception:  {ex}", ex.Message);
            }
            return items;
        }

    }
}
