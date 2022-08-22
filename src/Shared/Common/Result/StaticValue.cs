using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Result
{
    public static class StaticValue
    {
        #region return message
        public static string _error = "errors";
        public static string _productNotFound = "Product not found";
        public static string _imageNotFound = "Image not found";
        public static string _basketNotFound = "Basket not found";
        public static string _discountNotFound = "Discount not found";
        public static string _discountAddError = "an error occurred while adding";
        public static string _imageEmpty = "Image is empty";
        public static string _emailorPasswordIncorrect = "Email veya Şifreniz Yanlış";
        public static string _basketCouldNot = "Basket could not update or save";
        #endregion

        #region return code
        public static int _notFoundId = 404;
        public static int _badRequest = 400;
        public static int _couldNot = 500;
        public static int _successReturnModelId = 200;
        public static int _successReturnNotModelId = 204;
        #endregion
    }
}
