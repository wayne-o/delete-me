// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://go.microsoft.com/fwlink/p/?LinkID=258575
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

namespace MigrationToV3
{
    using System.Data.Entity;
    using System.Data.SqlClient;
    using Registration.Database;
    using Registration.ReadModel.Implementation;

    /// <summary>
    /// This initializer automatically creates the new UndispatchedMessages introduced in V3. 
    /// Database initializers in Entity Framework run only once per AppDomain, so this code 
    /// has very little impact even if it continues to be run on subsequent releases. 
    /// It is kept in a separate project so that further releases can easily detect what
    /// code paths aren't needed to run anymore.
    /// This also means it's a no-downtime migration.
    /// </summary>
    internal class RegistrationProcessManagerDbInitializer : IDatabaseInitializer<RegistrationProcessManagerDbContext>
    {
        public void InitializeDatabase(RegistrationProcessManagerDbContext context)
        {
            // Note that we only update the table if the column doesn't exist, so this 
            // can safely run with already upgraded databases.

            try
            {
                context.Database.ExecuteSqlCommand(@"
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ConferencePayments' AND TABLE_NAME = 'ThirdPartyProcessorPayments' AND COLUMN_NAME = 'StripeData')
    ALTER TABLE [ConferencePayments].[ThirdPartyProcessorPayments]
    ADD [StripeData] VARCHAR(MAX)  
");
            }
            catch (SqlException e)
            {
                if (e.Number != 2705)
                {
                    throw;
                }
            }
        }
    }
}