/*
* LICENSE: https://raw.github.com/apimash/StarterKits/master/LicenseTerms-SampleApps%20.txt
*/
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using XPlatformCloudKit.Models;
using XPlatformCloudKit.Services;

namespace XPlatformCloudKit.DataServices
{
    public class AzureMobileService : IDataService
    {
        private IMobileServiceTable<Item> itemsTable;
        private List<Item> Items;
        public MobileServiceClient MobileServiceClient;

        public AzureMobileService()
        {
            if (AppSettings.EnableAzureMobileService)
            {
                try
                {
                    MobileServiceClient = new MobileServiceClient(
                                     AppSettings.MobileServiceAddress,
                                     AppSettings.MobileServiceApplicationKey
                            );

                    itemsTable = MobileServiceClient.GetTable<Item>();

                    if (AppSettings.CreateInitialSchemaForAzureMobileService)
                        CreateInitialSchema();
                }
                catch
                {
                    ServiceLocator.MessageService.ShowErrorAsync("Error connecting to Azure Movile Service. Make sure the Service Address and Application Key are correct.", "Application Error");
                }
            }
        }

        public async Task<List<Item>> GetItems()
        {
            try
            {
                Items = await itemsTable.Take(1000).ToListAsync();

                //Fix issue #27 - Search unable to handle NULL Azure database table value
                foreach(var item in Items)
                {
                    item.UrlSource = new UrlSource { Url = AppSettings.MobileServiceAddress, Group = item.Group, Type = "AzureMobileService" };

                    Type type = item.GetType();

                    if (type != typeof(string))
                        continue;

                    var properties = type.GetRuntimeProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(item, null) == null)
                            property.SetValue(item, string.Empty);
                    }
                }

                return Items;
            }
            catch(Exception e)
            {
                ServiceLocator.MessageService.ShowErrorAsync("Error when retrieving items from AzureMobileService", "Application Error");
                return Items;
            }
        }

        /// <summary>
        /// Creates Initial Schema by Inserting Dummy Record(s) into Items Table
        /// <remarks>
        /// Be sure your Mobile Service has a table created named "Item"
        /// This method only needs to be successfully ran once, ever in the life of the application.
        /// </remarks>
        /// </summary>
        private async void CreateInitialSchema()
        {
            await itemsTable.InsertAsync(new Item 
                                                {   
                                                    Title = "schemaGeneratingDummyData",
                                                    Subtitle = "schemaGeneratingDummyData",
                                                    Description = "schemaGeneratingDummyData", 
                                                    Group = "schemaGeneratingDummyData", 
                                                    Image = "schemaGeneratingDummyData", 
                                                });

            var schemaGeneratingDummyData = await itemsTable.Where(item => item.Title == "schemaGeneratingDummyData").ToEnumerableAsync();

            foreach (var schemaGeneratingDummyDataEntry in schemaGeneratingDummyData)
            {
                await itemsTable.DeleteAsync(schemaGeneratingDummyDataEntry);
                await ServiceLocator.MessageService.ShowErrorAsync("Successfully Generated Schema, set \"CreateInitialSchemaForAzureMobileService\" to false in AppSettings.cs", "Schema Generation Enabled");
            }
        }

    }
}
