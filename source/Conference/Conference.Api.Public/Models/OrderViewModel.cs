﻿// ==============================================================================================================
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

namespace Conference.Api.Public.Models
{
    using System;
    using System.Collections.Generic;

    public class OrderViewModel
    {
        public OrderViewModel()
        {
            this.Items = new List<OrderItemViewModel>();
        }

        public Guid OrderId { get; set; }

        public int OrderVersion { get; set; }

        public Guid ConferenceId { get; set; }

        public string ConferenceCode { get; set; }

        public string ConferenceName { get; set; }

        public IList<OrderItemViewModel> Items { get; set; }

        public long ReservationExpirationDate { get; set; }
    }
}
