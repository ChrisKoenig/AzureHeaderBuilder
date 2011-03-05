using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

namespace AzureHeaderBuilder.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public RelayCommand BuildHeaderCommand { get; private set; }

        public RelayCommand ExecuteQueryCommand { get; private set; }

        public MainViewModel()
        {
            BuildHeaderCommand = new RelayCommand(() => CalculateHeader());
            ExecuteQueryCommand = new RelayCommand(() => ExecuteQuery());
            AccountName = "chkoenigdemo";
            SharedKey = "sxqcsJTpcnp5p2QUU44RF28yVEogr2hC9KQq8tL27ANUmHR/y2RgQB3HVXTROrH3N6N3FYTb3cjQKwapujXZFQ==";
            Query = "Family()";
        }

        private void CalculateHeader()
        {
            //String urlPath = String.Format("{0}(PartitionKey='{1}',RowKey='{2}')", tableName, partitionKey, rowKey);
            this.FullURL = String.Format("http://{0}.table.core.windows.net/{1}", AccountName, Query);

            String dateInRfc1123Format = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
            String canonicalizedResource = String.Format("/{0}/{1}", AccountName, Query);
            String stringToSign = String.Format(
                  "{0}\n\n\n{1}\n{2}",
                  "GET",
                  dateInRfc1123Format,
                  canonicalizedResource);
            String authorizationHeader = CreateAuthorizationHeader(stringToSign);

            Dictionary<String, String> httpHeaders = new Dictionary<string, string>();
            httpHeaders.Add("x-ms-date", dateInRfc1123Format);
            httpHeaders.Add("x-ms-version", "2009-09-19");
            httpHeaders.Add("Authorization", authorizationHeader);
            httpHeaders.Add("Accept-Charset", "UTF-8");
            httpHeaders.Add("Accept", "application/atom+xml, application/xml");
            httpHeaders.Add("DataServiceVersion", "1.0;NetFx");
            httpHeaders.Add("MaxDataServiceVersion", "1.0;NetFx");

            StringBuilder headers = new StringBuilder();
            int counter = 0;
            foreach (var item in httpHeaders)
            {
                if (counter > 0)
                    headers.Append("\r\n");
                counter++;
                headers.AppendFormat("{0}: {1}", item.Key, item.Value);
            }
            this.Header = headers.ToString();
        }

        private String CreateAuthorizationHeader(String canonicalizedString)
        {
            String signature = string.Empty;
            using (HMACSHA256 hmacSha256 = new HMACSHA256(Convert.FromBase64String(SharedKey)))
            {
                Byte[] dataToHmac = System.Text.Encoding.UTF8.GetBytes(canonicalizedString);
                signature = Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
            }

            String authorizationHeader = String.Format(
                   CultureInfo.InvariantCulture,
                   "{0} {1}:{2}",
                   "SharedKey",
                   AccountName,
                   signature);

            return authorizationHeader;
        }

        private void ExecuteQuery()
        {
            String dateInRfc1123Format = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
            String canonicalizedResource = String.Format("/{0}/{1}", AccountName, Query);
            String stringToSign = String.Format(
                  "{0}\n\n\n{1}\n{2}",
                  "GET",
                  dateInRfc1123Format,
                  canonicalizedResource);
            String authorizationHeader = CreateAuthorizationHeader(stringToSign);

            Uri uri = new Uri(this.FullURL);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Headers["x-ms-date"] = dateInRfc1123Format;
            request.Headers["x-ms-version"] = "2009-09-19";
            request.Headers["Authorization"] = authorizationHeader;
            request.Headers["Accept-Charset"] = "UTF-8";
            request.Accept = "application/atom+xml,application/xml";
            request.Headers["DataServiceVersion"] = "1.0;NetFx";
            request.Headers["MaxDataServiceVersion"] = "1.0;NetFx";

            var iar = request.BeginGetResponse((cb) =>
            {
                var response = request.EndGetResponse(cb);
                Stream dataStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    string responseFromServer = reader.ReadToEnd();
                    DispatcherHelper.CheckBeginInvokeOnUI(() => this.QueryResults = responseFromServer);
                }
            }, null);
        }

        #region AccountName property

        public const string AccountNamePropertyName = "AccountName";

        private string _accountName = String.Empty;

        public string AccountName
        {
            get
            {
                return _accountName;
            }

            set
            {
                if (_accountName == value)
                {
                    return;
                }

                var oldValue = _accountName;
                _accountName = value;
                RaisePropertyChanged(AccountNamePropertyName);
            }
        }

        #endregion AccountName property

        #region SharedKey property

        public const string SharedKeyPropertyName = "SharedKey";

        private string _sharedKey = String.Empty;

        public string SharedKey
        {
            get
            {
                return _sharedKey;
            }

            set
            {
                if (_sharedKey == value)
                {
                    return;
                }

                var oldValue = _sharedKey;
                _sharedKey = value;
                RaisePropertyChanged(SharedKeyPropertyName);
            }
        }

        #endregion SharedKey property

        #region Query property

        public const string QueryPropertyName = "Query";

        private string _query = String.Empty;

        public string Query
        {
            get
            {
                return _query;
            }

            set
            {
                if (_query == value)
                {
                    return;
                }

                var oldValue = _query;
                _query = value;
                RaisePropertyChanged(QueryPropertyName);
            }
        }

        #endregion Query property

        #region FullURL property

        public const string FullURLPropertyName = "FullURL";

        private string _fullUrl = String.Empty;

        public string FullURL
        {
            get
            {
                return _fullUrl;
            }

            set
            {
                if (_fullUrl == value)
                {
                    return;
                }

                var oldValue = _fullUrl;
                _fullUrl = value;
                RaisePropertyChanged(FullURLPropertyName);
            }
        }

        #endregion FullURL property

        #region Header property

        public const string HeaderPropertyName = "Header";

        private string _header = String.Empty;

        public string Header
        {
            get
            {
                return _header;
            }

            set
            {
                if (_header == value)
                {
                    return;
                }

                var oldValue = _header;
                _header = value;
                RaisePropertyChanged(HeaderPropertyName);
            }
        }

        #endregion Header property

        #region QueryResults property

        public const string QueryResultsPropertyName = "QueryResults";

        private string _queryResults = String.Empty;

        public string QueryResults
        {
            get
            {
                return _queryResults;
            }

            set
            {
                if (_queryResults == value)
                {
                    return;
                }

                var oldValue = _queryResults;
                _queryResults = value;
                RaisePropertyChanged(QueryResultsPropertyName);
            }
        }

        #endregion QueryResults property
    }
}