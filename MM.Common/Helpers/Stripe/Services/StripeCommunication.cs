/* ------------------------------------------------------------------------------------

CREATED BY          :       Variya Sandip.
CREATED DATE        :       17-Fab-2021.
DESCRIPTION         :       This file for the only stripe communication, we are connecting all stripe related API here.

 -------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Text;
using Models.Requests.Stripe;
using Stripe;

namespace MM.Common.Helpers.Stripe.Services
{
    public static class StripeCommunication
    {
        static string APIkey = "sk_test_LS9vOorMhSjxAdIN3DGZXuJC";



        #region Customer       
        public static Customer CreateCustomer(string MobileNumber)
        {
            var objCustomerCreateOptions = new CustomerCreateOptions();
            Customer objCustomer = new Customer();
            StripeConfiguration.ApiKey = APIkey;
            try
            {
                objCustomerCreateOptions.Description = "Payment for Kiwi Wallet";
                objCustomerCreateOptions.Phone = MobileNumber;
                var customerService = new CustomerService();
                objCustomer = customerService.Create(objCustomerCreateOptions);

            }
            catch (Exception ex)
            {
                objCustomer = null;
            }

            return objCustomer;
        }

        public static Customer RetrieveCustomer(string CustomerId)
        {
            Customer objCustomer = new Customer();
            StripeConfiguration.ApiKey = APIkey;
            try
            {
                var customerService = new CustomerService();
                objCustomer = customerService.Get(CustomerId);
            }
            catch (Exception ex)
            {
                objCustomer = null;
            }
            return objCustomer;
        }

        public static bool? DeleteCustomer(string CustomerId)
        {
            bool? IsDeleted = false;
            StripeConfiguration.ApiKey = APIkey;
            try
            {
                Customer objCustomer = new Customer();
                var customerService = new CustomerService();
                objCustomer = customerService.Delete(CustomerId);
                IsDeleted = objCustomer.Deleted == null ? false : objCustomer.Deleted;
            }
            catch (Exception ex)
            {
                IsDeleted = false;
            }
            return IsDeleted;
        }
        #endregion Customer

        #region Card
        public static Card CreateCard(CreateCardRequest objCreateCard)
        {
            Card objCard = new Card();
            Customer objCustomer = new Customer();
            StripeConfiguration.ApiKey = APIkey;
            try
            {
                if (string.IsNullOrEmpty(objCreateCard.CustomerId))
                {
                    objCustomer = CreateCustomer(objCreateCard.MobileNumber);
                    objCreateCard.CustomerId = objCustomer.Id;
                }


                var objTokenCreateOptions = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    {
                        Number = objCreateCard.Number,
                        ExpMonth = objCreateCard.ExpMonth,
                        ExpYear = objCreateCard.ExpYear,
                        Cvc = objCreateCard.Cvc,
                    },
                };

                var serviceTokenService = new TokenService();
                Token stripeNewCardToken = serviceTokenService.Create(objTokenCreateOptions);
                var CardCreateOptions = new CardCreateOptions
                {
                    Source = stripeNewCardToken.Id,
                };
                var service = new CardService();
                objCard = service.Create(objCreateCard.CustomerId, CardCreateOptions);
            }
            catch (Exception ex)
            {
                objCard = null;
            }
            return objCard;
        }

        public static Card RetrieveCard(string CustomerId, string CardId)
        {
            Card objCard = new Card();
            Customer objCustomer = new Customer();
            StripeConfiguration.ApiKey = APIkey;
            try
            {
                var service = new CardService();
                objCard = service.Get(CustomerId, CardId);
            }
            catch (Exception ex)
            {

                objCard = null;
            }
            return objCard;
        }

        public static bool? DeleteCard(string CustomerId, string CardId)
        {
            bool? IsDeleted = false;
            Card objCard = new Card();
            Customer objCustomer = new Customer();
            StripeConfiguration.ApiKey = APIkey;
            try
            {
                var service = new CardService();
                objCard = service.Delete(CustomerId, CardId);
                IsDeleted = objCard.Deleted == null ? false : objCard.Deleted;
            }
            catch (Exception ex)
            {
                IsDeleted = false;
            }
            return IsDeleted;
        }
        #endregion Card


        #region Charge

        public static Charge CreateCharge(string CustomerId, string CardId, long PayableAmount)
        {
            StripeConfiguration.ApiKey = APIkey;
            Charge charge = new Charge();
            var chargeService = new ChargeService();
            try
            {
                var stripeChargeOptions = new ChargeCreateOptions
                {
                    Amount = PayableAmount,
                    Description = "Payment for insurance - Kiwi wallet",
                    Currency = "inr",
                    Customer = CustomerId,
                    Source = CardId,
                    Capture = false
                };
                charge = chargeService.Create(stripeChargeOptions);
            }
            catch (Exception ex)
            {
            }
            return charge;
        }

        public static Charge CaptureCharge(string ChargeId, long PayableAmount)
        {
            StripeConfiguration.ApiKey = APIkey;
            Charge charge = new Charge();
            var chargeService = new ChargeService();
            try
            {
                var stripeChargeOptions = new ChargeCaptureOptions
                {
                    Amount = PayableAmount,
                };
                charge = chargeService.Capture(ChargeId, stripeChargeOptions, null);
            }
            catch (Exception ex)
            {
            }
            return charge;
        }


        public static Charge CreateDirectChargeFromBank(string BankToken, long PayableAmount)
        {
            StripeConfiguration.ApiKey = "sk_test_baKNB3vTmiyNUqkEAvtLWP9Z"; //APIkey;
            Charge charge = new Charge();
            var chargeService = new ChargeService();
            try
            {

                var options = new ChargeCreateOptions
                {
                    Amount = PayableAmount,
                    Description = "Direct Payment from bank - Kiwi wallet",
                    Currency = "usd",
                    //Customer = BankToken,
                    Source = BankToken
                };

                charge = chargeService.Create(options);
            }
            catch (Exception ex)
            {
                charge = null;
            }
            return charge;
        }

        #endregion Charge
    }
}
