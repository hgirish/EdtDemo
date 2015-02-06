using System;
using System.Configuration;
using System.Web.Services.Protocols;
using RateWebServiceClient.RateServiceWebReference;

namespace EdtDemo
{
    class Program
    {
        static void Main()
        {
            RateRequest request = CreateRateRequest();

            var service = new RateService();

            string productId = ConfigurationManager.AppSettings["productID"];
            if (productId == "Production")
            {
                service.Url = "https://ws.fedex.com:443/web-services/rate";
            }



            try
            {
                var reply = service.getRates(request);
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    ShowRateReply(reply);
                }
                ShowNotifications(reply);
            }
            catch (SoapException e)
            {
                Console.WriteLine(e.Detail.InnerText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Press any key to quit!");
            Console.ReadKey();
        }

        private static RateRequest CreateRateRequest()
        {
            // Build a RateRequest
            var request = new RateRequest
            {
                WebAuthenticationDetail = new WebAuthenticationDetail
                {
                    UserCredential = new WebAuthenticationCredential
                    {
                        Key = ConfigurationManager.AppSettings["key"],
                        Password = ConfigurationManager.AppSettings["password"]
                    }
                },
                ClientDetail =
                    new ClientDetail
                    {
                        AccountNumber = ConfigurationManager.AppSettings["accountnumber"],
                        MeterNumber = ConfigurationManager.AppSettings["meternumber"]
                    },
                TransactionDetail = new TransactionDetail { CustomerTransactionId = "***Rate Request using VC#***" },
                Version = new VersionId(),
                ReturnTransitAndCommit = true,
                ReturnTransitAndCommitSpecified = true
            };


            SetShipmentDetails(request);


            return request;
        }

        private static void SetShipmentDetails(RateRequest request)
        {
            request.RequestedShipment = new RequestedShipment
            {
                ShipTimestamp = DateTime.Now,
                ShipTimestampSpecified = true,
                DropoffType = DropoffType.REGULAR_PICKUP,
                ServiceType = ServiceType.INTERNATIONAL_PRIORITY,
                ServiceTypeSpecified = true,
                PackagingType = PackagingType.YOUR_PACKAGING,
                PackagingTypeSpecified = true,RateRequestTypes = new RateRequestType[]
                {
                    RateRequestType.LIST, RateRequestType.PREFERRED
                }
            };
            SetOrigin(request);
            SetDestination(request);
            SetPackageLineItems(request);
            SetCustomDetails(request);
            request.RequestedShipment.TotalInsuredValue =
                new Money { Amount = 100, Currency = "USD" };
            request.RequestedShipment.PackageCount = "1";
        }

        private static void SetOrigin(RateRequest request)
        {
            request.RequestedShipment.Shipper = new Party
            {
                Address = new Address
                {
                    StreetLines = new[] { "SHIPPER ADDRESS LINE 1" },
                    City = "COLLIERVILLE",
                    StateOrProvinceCode = "TN",
                    PostalCode = "38017",
                    CountryCode = "US"
                }
            };
        }

        private static void SetDestination(RateRequest request)
        {
            request.RequestedShipment.Recipient = new Party
            {
                Address = new Address
                {
                    StreetLines = new[] { "RECIPIENT ADDRESS LINE 1" },
                    City = "Dublin",
                    StateOrProvinceCode = "IE",
                    PostalCode = "4",
                    CountryCode = "IE"
                }
            };
        }

        private static void SetCustomDetails(RateRequest request)
        {
            request.RequestedShipment.EdtRequestType = EdtRequestType.ALL;
            request.RequestedShipment.EdtRequestTypeSpecified = true;
            var customClearanceDetail = new CustomsClearanceDetail
            {
                //CustomsValue = new Money
                //{
                //    Amount = 100m,
                //    Currency = "USD",
                //    AmountSpecified = true
                //},
                DutiesPayment = new Payment
                {
                    PaymentType = PaymentType.SENDER,
                    PaymentTypeSpecified = true
                },
                Commodities = new[]
                {
                    new Commodity
                    {
                        HarmonizedCode = "392330100000",
                        Name = "Plastic Bottle",
                        Description = "Plastic Bottle",
                        CountryOfManufacture = "CN",
                        Weight = new Weight
                        {
                            Units = WeightUnits.LB,
                            Value = 2
                        },
                        Quantity = 1,
                        QuantityUnits = "Number",
                        //UnitPrice = new Money
                        //{
                        //    Currency = "USD",
                        //    Amount = 100m
                        //},
                        CustomsValue = new Money
                        {
                            Currency = "USD",
                            Amount = 100m,
                            AmountSpecified = true
                        },
                        NumberOfPieces = "1",
                        AdditionalMeasures = new []{new Measure {QuantitySpecified = false}}
                        
                    }
                },
                CommercialInvoice = new CommercialInvoice()
                {
                    FreightCharge =
                        new Money { Amount = 100m, AmountSpecified = true, Currency = "USD" }
                }


            };


            request.RequestedShipment.CustomsClearanceDetail = customClearanceDetail;
        }



        private static void SetPackageLineItems(RateRequest request)
        {
            // request.RequestedShipment.PackageCount = "1";
            var lineItem = new RequestedPackageLineItem
            {
                // SequenceNumber = "1",
                GroupPackageCount = "1",
                Weight = new Weight
                {
                    Units = WeightUnits.LB,
                    UnitsSpecified = true,
                    Value = 2.0M,
                    ValueSpecified = true
                },
                Dimensions = new Dimensions
                {
                    Length = "10",
                    Width = "13",
                    Height = "4",
                    Units = LinearUnits.IN,
                    UnitsSpecified = true
                },
                // InsuredValue = new Money {Amount = 0, Currency = "USD"}
            };


            request.RequestedShipment.RequestedPackageLineItems = new[]
            {
                lineItem
            };

        }

        private static void ShowRateReply(RateReply reply)
        {
            Console.WriteLine("RateReply details:");
            foreach (RateReplyDetail rateReplyDetail in reply.RateReplyDetails)
            {
                if (rateReplyDetail.ServiceTypeSpecified)
                    Console.WriteLine("Service Type: {0}", rateReplyDetail.ServiceType);
                if (rateReplyDetail.PackagingTypeSpecified)
                    Console.WriteLine("Packaging Type: {0}", rateReplyDetail.PackagingType);
                Console.WriteLine();
                foreach (RatedShipmentDetail shipmentDetail in rateReplyDetail.RatedShipmentDetails)
                {
                    ShowShipmentRateDetails(shipmentDetail);
                    Console.WriteLine();
                }
                ShowDeliveryDetails(rateReplyDetail);
                ShowEdtDetail(rateReplyDetail);
                Console.WriteLine("**********************************************************");
            }
        }

        private static void ShowEdtDetail(RateReplyDetail rateReplyDetail)
        {
            var rsd = rateReplyDetail.RatedShipmentDetails;
            if (rsd == null) throw new ArgumentNullException("rateReplyDetail");
            foreach (var ratedShipmentDetail in rsd)
            {
                if (ratedShipmentDetail != null)
                {
                    var srd = ratedShipmentDetail.ShipmentRateDetail;
                    var dat = srd.DutiesAndTaxes;
                    if (dat != null)
                        foreach (var commodityTax in dat)
                        {
                            Console.WriteLine("HTC Code: {0}", commodityTax.HarmonizedCode);
                            foreach (var tax in commodityTax.Taxes)
                            {
                                if (tax != null)
                                    Console.WriteLine("{0} {1} : {2} {3}",
                                     tax.Name,
                                     tax.Description.Replace("&amp;", "").Replace("nbsp;", ""),
                                     tax.Amount.Amount,
                                tax.Amount.Currency
                                        );
                            }
                        }
                }
            }
        }

        private static void ShowShipmentRateDetails(RatedShipmentDetail shipmentDetail)
        {
            if (shipmentDetail == null) return;
            if (shipmentDetail.ShipmentRateDetail == null) return;
            ShipmentRateDetail rateDetail = shipmentDetail.ShipmentRateDetail;
            Console.WriteLine("--- Shipment Rate Detail ---");
            //
            Console.WriteLine("RateType: {0} ", rateDetail.RateType);
            if (rateDetail.TotalBillingWeight != null) Console.WriteLine("Total Billing Weight: {0} {1}", rateDetail.TotalBillingWeight.Value, shipmentDetail.ShipmentRateDetail.TotalBillingWeight.Units);
            if (rateDetail.TotalBaseCharge != null) Console.WriteLine("Total Base Charge: {0} {1}", rateDetail.TotalBaseCharge.Amount, rateDetail.TotalBaseCharge.Currency);
            if (rateDetail.TotalFreightDiscounts != null) Console.WriteLine("Total Freight Discounts: {0} {1}", rateDetail.TotalFreightDiscounts.Amount, rateDetail.TotalFreightDiscounts.Currency);
            if (rateDetail.TotalSurcharges != null) Console.WriteLine("Total Surcharges: {0} {1}", rateDetail.TotalSurcharges.Amount, rateDetail.TotalSurcharges.Currency);
            if (rateDetail.Surcharges != null)
            {
                // Individual surcharge for each package
                foreach (Surcharge surcharge in rateDetail.Surcharges)
                    Console.WriteLine(" {0} surcharge {1} {2}", surcharge.SurchargeType, surcharge.Amount.Amount, surcharge.Amount.Currency);
            }
            if (rateDetail.TotalNetCharge != null) Console.WriteLine("Total Net Charge: {0} {1}", rateDetail.TotalNetCharge.Amount, rateDetail.TotalNetCharge.Currency);
        }

        private static void ShowDeliveryDetails(RateReplyDetail rateDetail)
        {
            if (rateDetail.DeliveryTimestampSpecified)
                Console.WriteLine("Delivery timestamp: " + rateDetail.DeliveryTimestamp);
            if (rateDetail.TransitTimeSpecified)
                Console.WriteLine("Transit time: " + rateDetail.TransitTime);
        }

        private static void ShowNotifications(RateReply reply)
        {
            Console.WriteLine("Notifications");
            for (int i = 0; i < reply.Notifications.Length; i++)
            {
                Notification notification = reply.Notifications[i];
                Console.WriteLine("Notification no. {0}", i);
                Console.WriteLine(" Severity: {0}", notification.Severity);
                Console.WriteLine(" Code: {0}", notification.Code);
                Console.WriteLine(" Message: {0}", notification.Message);
                Console.WriteLine(" Source: {0}", notification.Source);
            }
        }


    }
}
