using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Models;
using Moq;
using Services;
using Xunit;

namespace Application_Test.ServiceTest
{
    public class LoginServiceTest
    {
        private Mock<UserService> _userMock;
        private Mock<LoginService> _loginMock;
        private Mock<IMapper> _mapperMock;

        public LoginServiceTest()
        {
            _userMock = new Mock<UserService>();
            _loginMock = new Mock<LoginService>();
            _mapperMock = new Mock<IMapper>();
        }

        //[Fact]
        //public async Task<bool> ValidateUser_ReturnsSuccess()
        //{
        //    var user = new User()
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        UserName = "test user",
        //        Password = "test user"
        //    };

        //    var validateUser = new User();

        //    _userMock.
        //}
    }
}
