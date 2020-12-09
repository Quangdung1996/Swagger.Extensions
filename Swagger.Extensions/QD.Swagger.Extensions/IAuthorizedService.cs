using System;
using System.Collections.Generic;
using System.Text;

namespace QD.Swagger.Extensions
{
    public interface IAuthorizedService
    {
        AuthorizedUser Get();

        void SignIn(AuthorizedUser authorizedUser);
    }
}
