using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantBot.Models
{

    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;

        private IMobileServiceTable<myNewTable> myTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://restaurantmsa.azurewebsites.net/");
            this.myTable = this.client.GetTable<myNewTable>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task AddTable(myNewTable tempTable)
        {
            await this.myTable.InsertAsync(tempTable);
        }

        public async Task<List<myNewTable>> GetMyTables()
        {
            return await this.myTable.ToListAsync();
        }



        public async Task DeleteTable(myNewTable tempTable)
        {
            await this.myTable.DeleteAsync(tempTable);
        }

    }
}