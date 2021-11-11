using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SLGatewayClient.Data
{
    public class ParcelDetails
    {
        public IEnumerable<ParcelDetailsParams> ValidParams { get; set; } = new List<ParcelDetailsParams>();
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Guid OwnerKey { get; set; }
        public Guid GroupKey { get; set; }
        public int Area { get; set; }
        public Guid ParcelKey { get; set; }
        public bool CanSeeAvatars { get; set; }

        public static ParcelDetails FromParams(IEnumerable<ParcelDetailsParams> @params, IEnumerable<JsonElement> data)
        {
            var parcelDetails = new ParcelDetails
            {
                ValidParams = @params
            };
            var paramsList = @params.ToList();
            var dataList = data.ToList();
            for (var i = 0; i < paramsList.Count; i++)
            {
                var param = paramsList[i];
                var value = dataList[i];

                switch (param)
                {
                    case ParcelDetailsParams.Name:
                        parcelDetails.Name = value.GetString() ?? "";
                        break;
                    case ParcelDetailsParams.Description:
                        parcelDetails.Description = value.GetString() ?? "";
                        break;
                    case ParcelDetailsParams.Area:
                        parcelDetails.Area = value.GetInt32();
                        break;
                    case ParcelDetailsParams.ParcelKey:
                        parcelDetails.ParcelKey = value.GetGuid();
                        break;
                    case ParcelDetailsParams.GroupKey:
                        parcelDetails.GroupKey = value.GetGuid();
                        break;
                    case ParcelDetailsParams.OwnerKey:
                        parcelDetails.OwnerKey = value.GetGuid();
                        break;
                    case ParcelDetailsParams.CanSeeAvatars:
                        parcelDetails.CanSeeAvatars = value.GetBoolean();
                        break;
                }
            }

            return parcelDetails;
        }
    }
}
